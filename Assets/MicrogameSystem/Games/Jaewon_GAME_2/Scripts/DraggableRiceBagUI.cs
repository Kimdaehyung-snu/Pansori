using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pansori.Microgames.Games
{
    /// <summary>
    /// UI 기반 드래그 가능한 쌀가마니 컴포넌트
    /// playerRiceBagStack의 각 쌀가마니에 추가하여 드래그 기능을 제공합니다.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableRiceBagUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("설정")]
        [Tooltip("드래그 중 투명도")]
        [SerializeField] private float dragAlpha = 0.8f;
        
        /// <summary>
        /// 드래그 종료 시 호출되는 이벤트
        /// (던진 쌀가마니, 화면 y좌표 비율)
        /// </summary>
        public event Action<DraggableRiceBagUI, float> OnThrowAttempt;
        
        /// <summary>
        /// 드래그 시작 시 호출되는 이벤트
        /// </summary>
        public event Action<DraggableRiceBagUI> OnDragStarted;
        
        /// <summary>
        /// 드래그 취소/복귀 시 호출되는 이벤트
        /// </summary>
        public event Action<DraggableRiceBagUI> OnDragCancelled;
        
        // 컴포넌트 캐시
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas parentCanvas;
        private Image image;
        
        // 드래그 상태
        private Vector2 originalPosition;
        private Vector2 originalAnchorMin;
        private Vector2 originalAnchorMax;
        private Vector2 originalPivot;
        private Transform originalParent;
        private int originalSiblingIndex;
        private bool isDragging = false;
        
        /// <summary>
        /// 드래그 가능 여부
        /// </summary>
        public bool IsDraggable { get; set; } = true;
        
        /// <summary>
        /// 현재 드래그 중인지 여부
        /// </summary>
        public bool IsDragging => isDragging;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            image = GetComponent<Image>();
            
            // CanvasGroup이 없으면 추가
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        private void Start()
        {
            // 부모 Canvas 찾기
            parentCanvas = GetComponentInParent<Canvas>();
        }
        
        /// <summary>
        /// 드래그 시작
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsDraggable) return;
            
            isDragging = true;
            
            // 원래 위치, 앵커, 피벗, 부모 저장
            originalPosition = rectTransform.anchoredPosition;
            originalAnchorMin = rectTransform.anchorMin;
            originalAnchorMax = rectTransform.anchorMax;
            originalPivot = rectTransform.pivot;
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            
            // 현재 스크린 위치 저장 (부모 변경 전)
            Vector2 screenPosition = eventData.position;
            
            // 드래그 중에는 최상위로 이동하여 다른 UI 위에 표시
            if (parentCanvas != null)
            {
                transform.SetParent(parentCanvas.transform);
                transform.SetAsLastSibling();
                
                // 앵커와 피벗을 중앙으로 설정 (좌표 계산 단순화)
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                
                // Screen Space Overlay 모드일 경우 카메라가 null
                Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
                
                // 마우스 위치를 Canvas 로컬 좌표로 변환
                RectTransform canvasRect = parentCanvas.transform as RectTransform;
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    cam,
                    out localPoint);
                rectTransform.anchoredPosition = localPoint;
            }
            
            // 드래그 중 시각적 피드백
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false; // 다른 UI와 충돌 방지
            
            OnDragStarted?.Invoke(this);
            
            Debug.Log($"[DraggableRiceBagUI] 드래그 시작: {gameObject.name}");
        }
        
        /// <summary>
        /// 드래그 중
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            // 마우스/터치 위치로 이동
            if (parentCanvas != null)
            {
                RectTransform canvasRect = parentCanvas.transform as RectTransform;
                Vector2 localPoint;
                
                // Screen Space Overlay 모드일 경우 카메라가 null
                Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
                
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    eventData.position,
                    cam,
                    out localPoint))
                {
                    rectTransform.anchoredPosition = localPoint;
                }
            }
        }
        
        /// <summary>
        /// 드래그 종료
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            isDragging = false;
            
            // 시각적 피드백 복원
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            
            // 화면 y좌표 비율 계산
            float screenHeight = Screen.height;
            float yRatio = eventData.position.y / screenHeight;
            
            Debug.Log($"[DraggableRiceBagUI] 드래그 종료: {gameObject.name}, Y비율: {yRatio:F2}");
            
            // 던지기 시도 이벤트 발생 (매니저에서 판정)
            OnThrowAttempt?.Invoke(this, yRatio);
        }
        
        /// <summary>
        /// 원래 위치로 복귀
        /// </summary>
        public void ReturnToOriginalPosition(float duration = 0.2f)
        {
            // 원래 부모로 복귀
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            
            // 원래 앵커와 피벗 복원
            rectTransform.anchorMin = originalAnchorMin;
            rectTransform.anchorMax = originalAnchorMax;
            rectTransform.pivot = originalPivot;
            
            if (duration <= 0)
            {
                rectTransform.anchoredPosition = originalPosition;
            }
            else
            {
                StartCoroutine(AnimateReturn(duration));
            }
            
            OnDragCancelled?.Invoke(this);
        }
        
        /// <summary>
        /// 복귀 애니메이션
        /// </summary>
        private System.Collections.IEnumerator AnimateReturn(float duration)
        {
            Vector2 startPos = rectTransform.anchoredPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // EaseOutQuad
                t = 1f - (1f - t) * (1f - t);
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
                yield return null;
            }
            
            rectTransform.anchoredPosition = originalPosition;
        }
        
        /// <summary>
        /// 던지기 성공 시 호출 - 형에게 날아가는 애니메이션
        /// </summary>
        public void AnimateThrowToTarget(Vector2 targetPosition, float duration, Action onComplete)
        {
            StartCoroutine(AnimateThrow(targetPosition, duration, onComplete));
        }
        
        /// <summary>
        /// 던지기 애니메이션 코루틴
        /// </summary>
        private System.Collections.IEnumerator AnimateThrow(Vector2 targetPosition, float duration, Action onComplete)
        {
            Vector2 startPos = rectTransform.anchoredPosition;
            float elapsed = 0f;
            
            // 포물선 높이 계산
            float arcHeight = Vector2.Distance(startPos, targetPosition) * 0.3f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 선형 보간 위치
                Vector2 linearPos = Vector2.Lerp(startPos, targetPosition, t);
                
                // 포물선 효과 추가
                float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
                linearPos.y += arc;
                
                rectTransform.anchoredPosition = linearPos;
                
                // 스케일 감소 (멀어지는 느낌)
                float scale = Mathf.Lerp(1f, 0.5f, t);
                rectTransform.localScale = new Vector3(scale, scale, 1f);
                
                yield return null;
            }
            
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 원래 부모로 복귀 (위치는 유지)
        /// </summary>
        public void ReturnToOriginalParent()
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            
            // 원래 앵커와 피벗 복원
            rectTransform.anchorMin = originalAnchorMin;
            rectTransform.anchorMax = originalAnchorMax;
            rectTransform.pivot = originalPivot;
        }
        
        /// <summary>
        /// 상태 초기화
        /// </summary>
        public void ResetState()
        {
            isDragging = false;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            rectTransform.localScale = Vector3.one;
            IsDraggable = true;
        }
    }
}
