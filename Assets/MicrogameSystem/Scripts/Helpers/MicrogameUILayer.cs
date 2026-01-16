using UnityEngine;
using UnityEngine.UI;

namespace Pansori.Microgames
{
    /// <summary>
    /// 미니게임 전용 UI 레이어 관리 헬퍼 컴포넌트
    /// Canvas를 자동으로 생성하고 관리합니다.
    /// </summary>
    public class MicrogameUILayer : MonoBehaviour
    {
        [Header("UI 설정")]
        [SerializeField] private int sortOrder = 100; // Canvas 정렬 순서
        [SerializeField] private bool worldSpace = false; // 월드 스페이스 여부
        
        /// <summary>
        /// 생성된 Canvas
        /// </summary>
        private Canvas canvas;
        
        /// <summary>
        /// Canvas의 RectTransform
        /// </summary>
        private RectTransform rectTransform;
        
        /// <summary>
        /// Canvas의 GraphicRaycaster
        /// </summary>
        private GraphicRaycaster raycaster;
        
        private void Awake()
        {
            SetupCanvas();
        }
        
        /// <summary>
        /// Canvas를 설정합니다.
        /// </summary>
        private void SetupCanvas()
        {
            // Canvas가 없으면 생성
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            
            // Canvas 설정
            canvas.renderMode = worldSpace ? RenderMode.WorldSpace : RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            
            // CanvasScaler 추가 (스크린 스페이스인 경우)
            if (!worldSpace)
            {
                CanvasScaler scaler = GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    scaler = gameObject.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.matchWidthOrHeight = 0.5f;
                }
            }
            
            // GraphicRaycaster 추가
            raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // RectTransform 설정
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            
            if (!worldSpace)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }
        
        /// <summary>
        /// UI 요소를 추가합니다.
        /// </summary>
        /// <param name="uiElement">추가할 UI 요소</param>
        public void AddUIElement(GameObject uiElement)
        {
            if (uiElement == null)
            {
                Debug.LogWarning("[MicrogameUILayer] UI 요소가 null입니다.");
                return;
            }
            
            uiElement.transform.SetParent(transform, false);
        }
        
        /// <summary>
        /// UI 요소를 제거합니다.
        /// </summary>
        /// <param name="uiElement">제거할 UI 요소</param>
        public void RemoveUIElement(GameObject uiElement)
        {
            if (uiElement == null)
            {
                return;
            }
            
            if (uiElement.transform.parent == transform)
            {
                Destroy(uiElement);
            }
        }
        
        /// <summary>
        /// 모든 UI 요소를 제거합니다.
        /// </summary>
        public void ClearAllUIElements()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        
        /// <summary>
        /// Canvas의 정렬 순서를 설정합니다.
        /// </summary>
        /// <param name="order">정렬 순서</param>
        public void SetSortOrder(int order)
        {
            sortOrder = order;
            if (canvas != null)
            {
                canvas.sortingOrder = sortOrder;
            }
        }
        
        /// <summary>
        /// Canvas를 가져옵니다.
        /// </summary>
        /// <returns>Canvas 컴포넌트</returns>
        public Canvas GetCanvas()
        {
            return canvas;
        }
        
        /// <summary>
        /// RectTransform을 가져옵니다.
        /// </summary>
        /// <returns>RectTransform 컴포넌트</returns>
        public RectTransform GetRectTransform()
        {
            return rectTransform;
        }
    }
}
