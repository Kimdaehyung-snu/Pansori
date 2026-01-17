using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Pansori.Microgames.Games
{
    /// <summary>
    /// Texture2D 기반 드로잉 캔버스
    /// 마우스/터치로 먹물을 그리고 채움률을 계산합니다.
    /// </summary>
    public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("드로잉 설정")]
        [SerializeField] private int textureSize = 512;
        [SerializeField] private int brushSize = 15;
        [SerializeField] private Color brushColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        [SerializeField] private float brushHardness = 0.8f;
        
        [Header("먹물 소비 설정")]
        [Tooltip("픽셀당 먹물 소비량 (높을수록 빨리 소모)")]
        [SerializeField] private float inkConsumptionRate = 0.002f;
        
        [Header("채움률 계산 설정")]
        [Tooltip("채움률 업데이트 간격 (초)")]
        [SerializeField] private float fillUpdateInterval = 0.1f;
        
        [Header("마스크 텍스처")]
        [Tooltip("天 한자 마스크 (흰색 = 채워야 할 영역)")]
        [SerializeField] private Texture2D maskTian;
        [Tooltip("地 한자 마스크 (흰색 = 채워야 할 영역)")]
        [SerializeField] private Texture2D maskDi;
        
        [Header("UI 참조")]
        [SerializeField] private RawImage displayImage;
        [SerializeField] private RectTransform drawingArea;
        
        // 내부 변수
        private Texture2D drawTexture;
        private Color[] drawPixels;
        private Texture2D currentMask;
        private Color[] cachedMaskPixels;
        private int cachedMaskPixelCount = 0;
        private bool textureNeedsUpdate = false;
        
        private Vector2 lastDrawPosition;
        private bool isDrawing = false;
        private bool canDraw = true;
        private bool hasValidLastPosition = false;
        
        // 먹물 관련
        private float currentInk = 1f;
        private float maxInk = 1f;
        
        // 총 그린 거리 추적
        private float totalDrawnDistance = 0f;
        
        // 채움률 관련
        private float lastFillUpdateTime = 0f;
        private float currentFillPercentage = 0f;
        
        /// <summary>
        /// 먹물 소비 이벤트 (소비된 양)
        /// </summary>
        public event Action<float> OnInkConsumed;
        
        /// <summary>
        /// 채움률 변경 이벤트 (현재 채움률 0~1)
        /// </summary>
        public event Action<float> OnFillPercentageChanged;
        
        /// <summary>
        /// 드로잉 시작 이벤트
        /// </summary>
        public event Action OnDrawStart;
        
        /// <summary>
        /// 드로잉 종료 이벤트
        /// </summary>
        public event Action OnDrawEnd;
        
        /// <summary>
        /// 현재 먹물 비율 (0~1)
        /// </summary>
        public float InkRatio => maxInk > 0 ? currentInk / maxInk : 0f;
        
        /// <summary>
        /// 현재 채움률 (0~1)
        /// </summary>
        public float FillPercentage => currentFillPercentage;
        
        /// <summary>
        /// 드로잉 가능 여부
        /// </summary>
        public bool CanDraw
        {
            get => canDraw && currentInk > 0.001f;
            set => canDraw = value;
        }
        
        private void Awake()
        {
            InitializeTextures();
        }
        
        /// <summary>
        /// 텍스처 초기화
        /// </summary>
        private void InitializeTextures()
        {
            // 드로잉용 Texture2D 생성
            drawTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
            drawTexture.filterMode = FilterMode.Bilinear;
            drawTexture.wrapMode = TextureWrapMode.Clamp;
            
            // 픽셀 배열 초기화
            drawPixels = new Color[textureSize * textureSize];
            ClearDrawTexture();
            
            // RawImage에 연결
            if (displayImage != null)
            {
                displayImage.texture = drawTexture;
            }
            
            Debug.Log($"[DrawingCanvas] 텍스처 초기화 완료: {textureSize}x{textureSize}");
        }
        
        /// <summary>
        /// 드로잉 텍스처 초기화 (투명)
        /// </summary>
        public void ClearDrawTexture()
        {
            if (drawPixels == null) return;
            
            Color clearColor = new Color(0, 0, 0, 0);
            for (int i = 0; i < drawPixels.Length; i++)
            {
                drawPixels[i] = clearColor;
            }
            
            ApplyTexture();
            totalDrawnDistance = 0f;
            currentFillPercentage = 0f;
        }
        
        /// <summary>
        /// 텍스처 적용
        /// </summary>
        private void ApplyTexture()
        {
            if (drawTexture == null || drawPixels == null) return;
            
            drawTexture.SetPixels(drawPixels);
            drawTexture.Apply();
        }
        
        /// <summary>
        /// 마스크 설정 (天 또는 地)
        /// </summary>
        public void SetMask(bool isTian)
        {
            currentMask = isTian ? maskTian : maskDi;
            string charName = isTian ? "天" : "地";
            
            if (currentMask == null)
            {
                Debug.LogWarning($"[DrawingCanvas] {charName} 마스크가 Inspector에서 할당되지 않았습니다!");
                cachedMaskPixels = null;
                cachedMaskPixelCount = 0;
            }
            else
            {
                Debug.Log($"[DrawingCanvas] {charName} 마스크 설정 시도: {currentMask.name} ({currentMask.width}x{currentMask.height})");
                // 마스크 픽셀 캐싱
                CacheMaskPixels();
            }
        }
        
        /// <summary>
        /// 마스크 픽셀 캐싱 (실시간 계산 최적화)
        /// </summary>
        private void CacheMaskPixels()
        {
            if (currentMask == null) return;
            
            try
            {
                // Read/Write Enabled 확인을 위해 GetPixels 시도
                Color[] maskPixels = currentMask.GetPixels();
                
                // 마스크 크기가 다른 경우 리샘플링
                if (maskPixels.Length != drawPixels.Length)
                {
                    Debug.Log($"[DrawingCanvas] 마스크 리샘플링: {currentMask.width}x{currentMask.height} -> {textureSize}x{textureSize}");
                    cachedMaskPixels = ResampleMask(currentMask, textureSize);
                }
                else
                {
                    cachedMaskPixels = maskPixels;
                }
            }
            catch (UnityException e)
            {
                Debug.LogError($"[DrawingCanvas] 마스크 텍스처를 읽을 수 없습니다! Read/Write Enabled를 활성화하세요. 에러: {e.Message}");
                cachedMaskPixels = null;
                cachedMaskPixelCount = 0;
                return;
            }
            
            // 마스크 픽셀 수 미리 계산 (글씨 영역)
            cachedMaskPixelCount = 0;
            for (int i = 0; i < cachedMaskPixels.Length; i++)
            {
                float maskValue = cachedMaskPixels[i].grayscale;
                if (maskValue > 0.5f || cachedMaskPixels[i].a > 0.5f)
                {
                    cachedMaskPixelCount++;
                }
            }
            
            float maskRatio = (float)cachedMaskPixelCount / cachedMaskPixels.Length * 100f;
            Debug.Log($"[DrawingCanvas] 마스크 캐싱 완료: 글씨 영역 {cachedMaskPixelCount}픽셀 (전체의 {maskRatio:F1}%)");
            
            if (cachedMaskPixelCount == 0)
            {
                Debug.LogWarning("[DrawingCanvas] 마스크에 글씨 영역이 없습니다! 마스크 텍스처가 올바른지 확인하세요.");
            }
        }
        
        /// <summary>
        /// 먹물 초기화
        /// </summary>
        public void InitializeInk(float amount)
        {
            maxInk = amount;
            currentInk = amount;
        }
        
        /// <summary>
        /// 먹물 소비
        /// </summary>
        private bool ConsumeInk(float distance)
        {
            if (currentInk <= 0) return false;
            
            float consumed = distance * inkConsumptionRate;
            currentInk = Mathf.Max(0, currentInk - consumed);
            
            // 소비 이벤트 발생
            OnInkConsumed?.Invoke(consumed);
            
            return currentInk > 0;
        }
        
        #region 이벤트 핸들러
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanDraw) return;
            
            isDrawing = true;
            hasValidLastPosition = false;
            OnDrawStart?.Invoke();
            
            Vector2 localPoint;
            if (GetLocalPoint(eventData.position, out localPoint))
            {
                lastDrawPosition = localPoint;
                hasValidLastPosition = true;
                DrawBrushAt(localPoint);
                textureNeedsUpdate = true;
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDrawing || !CanDraw) return;
            
            Vector2 localPoint;
            if (GetLocalPoint(eventData.position, out localPoint))
            {
                if (hasValidLastPosition)
                {
                    DrawLine(lastDrawPosition, localPoint);
                }
                else
                {
                    DrawBrushAt(localPoint);
                }
                
                lastDrawPosition = localPoint;
                hasValidLastPosition = true;
                textureNeedsUpdate = true;
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (isDrawing)
            {
                isDrawing = false;
                hasValidLastPosition = false;
                OnDrawEnd?.Invoke();
                
                // 최종 텍스처 적용
                if (textureNeedsUpdate)
                {
                    ApplyTexture();
                    textureNeedsUpdate = false;
                }
                
                // 드로잉 종료 시 채움률 업데이트
                UpdateFillPercentage(true);
            }
        }
        
        #endregion
        
        private void LateUpdate()
        {
            // 드로잉 중 텍스처 업데이트
            if (textureNeedsUpdate && isDrawing)
            {
                ApplyTexture();
                textureNeedsUpdate = false;
                
                // 일정 간격으로 채움률 업데이트
                if (Time.time - lastFillUpdateTime >= fillUpdateInterval)
                {
                    UpdateFillPercentage(false);
                    lastFillUpdateTime = Time.time;
                }
            }
        }
        
        /// <summary>
        /// 채움률 업데이트 및 이벤트 발생
        /// </summary>
        private void UpdateFillPercentage(bool forceLog)
        {
            float newFill = CalculateFillPercentageFast();
            
            if (Mathf.Abs(newFill - currentFillPercentage) > 0.001f || forceLog)
            {
                currentFillPercentage = newFill;
                OnFillPercentageChanged?.Invoke(currentFillPercentage);
                
                if (forceLog)
                {
                    if (cachedMaskPixels != null && cachedMaskPixelCount > 0)
                    {
                        Debug.Log($"[DrawingCanvas] 채움률: {currentFillPercentage * 100f:F1}% (마스크와 겹치는 비율)");
                    }
                    else
                    {
                        Debug.Log($"[DrawingCanvas] 채움률: {currentFillPercentage * 100f:F1}% (마스크 없음, 전체 영역 기준 fallback)");
                    }
                }
            }
        }
        
        /// <summary>
        /// 빠른 채움률 계산 (캐시된 마스크 사용)
        /// 글씨(마스크)와 그린 선이 겹치는 비율을 계산합니다.
        /// 계산식: (마스크 영역 ∩ 그린 영역) / 마스크 영역
        /// </summary>
        private float CalculateFillPercentageFast()
        {
            if (drawPixels == null) return 0f;
            
            // 마스크가 없으면 전체 캔버스 대비 그린 영역 비율로 계산 (fallback)
            if (cachedMaskPixels == null || cachedMaskPixelCount == 0)
            {
                int drawnCount = 0;
                for (int i = 0; i < drawPixels.Length; i++)
                {
                    if (drawPixels[i].a > 0.3f)
                    {
                        drawnCount++;
                    }
                }
                // 전체 캔버스의 30%를 채우면 100%로 취급 (마스크 없을 때 fallback)
                float fallbackRatio = (float)drawnCount / drawPixels.Length / 0.3f;
                return Mathf.Clamp01(fallbackRatio);
            }
            
            // 겹치는 픽셀 수 계산 (마스크 영역 ∩ 그린 영역)
            int overlappingPixelCount = 0;
            
            for (int i = 0; i < drawPixels.Length && i < cachedMaskPixels.Length; i++)
            {
                // 마스크의 해당 픽셀이 글씨 영역인지 확인 (흰색 또는 알파가 있는 부분)
                bool isMaskArea = cachedMaskPixels[i].grayscale > 0.5f || cachedMaskPixels[i].a > 0.5f;
                
                // 그린 영역인지 확인
                bool isDrawnArea = drawPixels[i].a > 0.3f;
                
                // 둘 다 해당하면 겹치는 영역
                if (isMaskArea && isDrawnArea)
                {
                    overlappingPixelCount++;
                }
            }
            
            // 겹치는 비율 = 겹치는 픽셀 / 마스크 전체 픽셀
            return (float)overlappingPixelCount / cachedMaskPixelCount;
        }
        
        /// <summary>
        /// 스크린 좌표를 텍스처 로컬 좌표로 변환
        /// </summary>
        private bool GetLocalPoint(Vector2 screenPoint, out Vector2 localPoint)
        {
            localPoint = Vector2.zero;
            
            if (drawingArea == null) return false;
            
            Camera cam = null;
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = canvas.worldCamera;
            }
            
            Vector2 rectLocalPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingArea, screenPoint, cam, out rectLocalPoint))
            {
                Rect rect = drawingArea.rect;
                float normalizedX = (rectLocalPoint.x - rect.x) / rect.width;
                float normalizedY = (rectLocalPoint.y - rect.y) / rect.height;
                
                if (normalizedX < 0 || normalizedX > 1 || normalizedY < 0 || normalizedY > 1)
                {
                    return false;
                }
                
                localPoint.x = normalizedX * textureSize;
                localPoint.y = normalizedY * textureSize;
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 두 점 사이에 선 그리기
        /// </summary>
        private void DrawLine(Vector2 from, Vector2 to)
        {
            float distance = Vector2.Distance(from, to);
            
            if (distance < 0.5f)
            {
                DrawBrushAt(to);
                return;
            }
            
            if (!ConsumeInk(distance))
            {
                return;
            }
            
            totalDrawnDistance += distance;
            
            int steps = Mathf.CeilToInt(distance / (brushSize * 0.3f));
            steps = Mathf.Max(steps, 2);
            
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector2 point = Vector2.Lerp(from, to, t);
                DrawBrushAt(point);
            }
        }
        
        /// <summary>
        /// 특정 위치에 브러시 그리기
        /// </summary>
        private void DrawBrushAt(Vector2 position)
        {
            if (drawPixels == null) return;
            
            int centerX = Mathf.RoundToInt(position.x);
            int centerY = Mathf.RoundToInt(position.y);
            
            for (int offsetY = -brushSize; offsetY <= brushSize; offsetY++)
            {
                for (int offsetX = -brushSize; offsetX <= brushSize; offsetX++)
                {
                    int px = centerX + offsetX;
                    int py = centerY + offsetY;
                    
                    if (px < 0 || px >= textureSize || py < 0 || py >= textureSize)
                        continue;
                    
                    float dist = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);
                    float normalizedDist = dist / brushSize;
                    
                    if (normalizedDist > 1f) continue;
                    
                    float alpha = 1f - Mathf.Pow(normalizedDist, brushHardness);
                    alpha = Mathf.Clamp01(alpha);
                    
                    int index = py * textureSize + px;
                    
                    Color existingColor = drawPixels[index];
                    Color newColor = brushColor;
                    
                    float blendedAlpha = Mathf.Max(existingColor.a, alpha * brushColor.a);
                    
                    drawPixels[index] = new Color(
                        Mathf.Lerp(existingColor.r, newColor.r, alpha),
                        Mathf.Lerp(existingColor.g, newColor.g, alpha),
                        Mathf.Lerp(existingColor.b, newColor.b, alpha),
                        blendedAlpha
                    );
                }
            }
        }
        
        /// <summary>
        /// 마스크 영역 내 채움률 계산 (정밀)
        /// </summary>
        public float CalculateFillPercentage()
        {
            float fill = CalculateFillPercentageFast();
            Debug.Log($"[DrawingCanvas] 채움률: {fill * 100f:F1}%");
            return fill;
        }
        
        /// <summary>
        /// 마스크 텍스처 리샘플링
        /// </summary>
        private Color[] ResampleMask(Texture2D mask, int targetSize)
        {
            Color[] result = new Color[targetSize * targetSize];
            
            for (int y = 0; y < targetSize; y++)
            {
                for (int x = 0; x < targetSize; x++)
                {
                    float u = (float)x / targetSize;
                    float v = (float)y / targetSize;
                    result[y * targetSize + x] = mask.GetPixelBilinear(u, v);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 전체 리셋
        /// </summary>
        public void ResetCanvas()
        {
            ClearDrawTexture();
            currentInk = maxInk;
            isDrawing = false;
            hasValidLastPosition = false;
            currentFillPercentage = 0f;
            OnFillPercentageChanged?.Invoke(0f);
        }
        
        private void OnDestroy()
        {
            if (drawTexture != null)
            {
                UnityEngine.Object.Destroy(drawTexture);
            }
        }
    }
}