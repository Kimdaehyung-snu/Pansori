using UnityEngine;
using UnityEngine.UI;

namespace CautionPotion.Microgames
{
    /// <summary>
    /// 미니게임 ?�용 UI ?�이??관�??�퍼 컴포?�트
    /// Canvas�??�동?�로 ?�성?�고 관리합?�다.
    /// </summary>
    public class MicrogameUILayer : MonoBehaviour
    {
        [Header("UI ?�정")]
        [SerializeField] private int sortOrder = 100; // Canvas ?�렬 ?�서
        [SerializeField] private bool worldSpace = false; // ?�드 ?�페?�스 ?��?
        
        /// <summary>
        /// ?�성??Canvas
        /// </summary>
        private Canvas canvas;
        
        /// <summary>
        /// Canvas??RectTransform
        /// </summary>
        private RectTransform rectTransform;
        
        /// <summary>
        /// Canvas??GraphicRaycaster
        /// </summary>
        private GraphicRaycaster raycaster;
        
        private void Awake()
        {
            SetupCanvas();
        }
        
        /// <summary>
        /// Canvas�??�정?�니??
        /// </summary>
        private void SetupCanvas()
        {
            // Canvas가 ?�으�??�성
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            
            // Canvas ?�정
            canvas.renderMode = worldSpace ? RenderMode.WorldSpace : RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            
            // CanvasScaler 추�? (?�크�??�페?�스??경우)
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
            
            // GraphicRaycaster 추�?
            raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // RectTransform ?�정
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
        /// UI ?�소�?추�??�니??
        /// </summary>
        /// <param name="uiElement">추�???UI ?�소</param>
        public void AddUIElement(GameObject uiElement)
        {
            if (uiElement == null)
            {
                Debug.LogWarning("[MicrogameUILayer] UI ?�소가 null?�니??");
                return;
            }
            
            uiElement.transform.SetParent(transform, false);
        }
        
        /// <summary>
        /// UI ?�소�??�거?�니??
        /// </summary>
        /// <param name="uiElement">?�거??UI ?�소</param>
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
        /// 모든 UI ?�소�??�거?�니??
        /// </summary>
        public void ClearAllUIElements()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        
        /// <summary>
        /// Canvas???�렬 ?�서�??�정?�니??
        /// </summary>
        /// <param name="order">?�렬 ?�서</param>
        public void SetSortOrder(int order)
        {
            sortOrder = order;
            if (canvas != null)
            {
                canvas.sortingOrder = sortOrder;
            }
        }
        
        /// <summary>
        /// Canvas�?가?�옵?�다.
        /// </summary>
        /// <returns>Canvas 컴포?�트</returns>
        public Canvas GetCanvas()
        {
            return canvas;
        }
        
        /// <summary>
        /// RectTransform??가?�옵?�다.
        /// </summary>
        /// <returns>RectTransform 컴포?�트</returns>
        public RectTransform GetRectTransform()
        {
            return rectTransform;
        }
    }
}
