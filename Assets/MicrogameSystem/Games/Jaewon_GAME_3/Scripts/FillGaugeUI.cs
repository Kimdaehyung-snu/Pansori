using UnityEngine;
using UnityEngine.UI;

namespace Pansori.Microgames.Games
{
    /// <summary>
    /// 채움률 게이지 UI
    /// 세로 슬라이더 형태로 현재 채움률을 표시합니다.
    /// </summary>
    public class FillGaugeUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Slider fillSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private RectTransform targetMarker;
        
        [Header("색상 설정")]
        [SerializeField] private Color normalColor = new Color(0.3f, 0.5f, 0.8f, 1f); // 파란색
        [SerializeField] private Color successColor = new Color(0.2f, 0.8f, 0.3f, 1f); // 녹색
        [SerializeField] private Color warningColor = new Color(0.8f, 0.6f, 0.2f, 1f); // 주황색 (거의 다 채움)
        
        [Header("애니메이션 설정")]
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private float pulseSpeed = 4f;
        [SerializeField] private bool pulseOnSuccess = true;
        
        private float targetValue = 0f;
        private float currentValue = 0f;
        private float requiredPercentage = 0.6f;
        private bool isSuccess = false;
        
        private void Awake()
        {
            // Slider 자동 찾기
            if (fillSlider == null)
            {
                fillSlider = GetComponent<Slider>();
            }
            
            if (fillSlider != null)
            {
                // 세로 슬라이더로 설정 (아래에서 위로)
                fillSlider.direction = Slider.Direction.BottomToTop;
                fillSlider.interactable = false;
                
                // Fill Image 가져오기
                if (fillImage == null && fillSlider.fillRect != null)
                {
                    fillImage = fillSlider.fillRect.GetComponent<Image>();
                }
            }
            
            // 초기 상태
            SetValue(0f, true);
        }
        
        /// <summary>
        /// 목표 비율 설정 (마커 위치 조정)
        /// </summary>
        /// <param name="percentage">0~1 범위</param>
        public void SetTargetPercentage(float percentage)
        {
            requiredPercentage = Mathf.Clamp01(percentage);
            UpdateTargetMarker();
        }
        
        /// <summary>
        /// 타겟 마커 위치 업데이트
        /// </summary>
        private void UpdateTargetMarker()
        {
            if (targetMarker == null || fillSlider == null) return;
            
            // 슬라이더의 높이를 기준으로 마커 위치 계산
            RectTransform sliderRect = fillSlider.GetComponent<RectTransform>();
            float sliderHeight = sliderRect.rect.height;
            
            // 마커 위치를 목표 비율에 맞게 설정
            Vector2 markerPos = targetMarker.anchoredPosition;
            markerPos.y = -sliderHeight * 0.5f + sliderHeight * requiredPercentage;
            targetMarker.anchoredPosition = markerPos;
        }
        
        /// <summary>
        /// 채움률 값 설정
        /// </summary>
        /// <param name="ratio">0~1 범위의 비율</param>
        /// <param name="instant">즉시 적용 여부</param>
        public void SetValue(float ratio, bool instant = false)
        {
            targetValue = Mathf.Clamp01(ratio);
            
            if (instant)
            {
                currentValue = targetValue;
                ApplyValue(currentValue);
            }
            
            // 성공 상태 확인
            bool wasSuccess = isSuccess;
            isSuccess = targetValue >= requiredPercentage;
            
            // 성공 전환 시 색상 즉시 변경
            if (isSuccess && !wasSuccess)
            {
                UpdateColor();
            }
        }
        
        private void Update()
        {
            // 부드러운 값 변화
            if (!Mathf.Approximately(currentValue, targetValue))
            {
                currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * smoothSpeed);
                
                // 근사치에 도달하면 스냅
                if (Mathf.Abs(currentValue - targetValue) < 0.001f)
                {
                    currentValue = targetValue;
                }
                
                ApplyValue(currentValue);
            }
            
            // 성공 시 펄스 애니메이션
            if (isSuccess && pulseOnSuccess && fillImage != null)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                Color targetColor = Color.Lerp(successColor, Color.white, pulse * 0.3f);
                fillImage.color = targetColor;
            }
        }
        
        /// <summary>
        /// 값 적용
        /// </summary>
        private void ApplyValue(float value)
        {
            if (fillSlider != null)
            {
                fillSlider.value = value;
            }
            
            UpdateColor();
        }
        
        /// <summary>
        /// 색상 업데이트
        /// </summary>
        private void UpdateColor()
        {
            if (fillImage == null) return;
            
            if (isSuccess)
            {
                fillImage.color = successColor;
            }
            else if (currentValue >= requiredPercentage * 0.8f)
            {
                // 목표의 80% 이상이면 경고색
                fillImage.color = warningColor;
            }
            else
            {
                fillImage.color = normalColor;
            }
        }
        
        /// <summary>
        /// 리셋
        /// </summary>
        public void ResetGauge()
        {
            SetValue(0f, true);
            isSuccess = false;
            
            if (fillImage != null)
            {
                fillImage.color = normalColor;
            }
        }
        
        /// <summary>
        /// 현재 성공 여부
        /// </summary>
        public bool IsSuccess => isSuccess;
        
        /// <summary>
        /// 현재 채움률
        /// </summary>
        public float CurrentValue => currentValue;
    }
}