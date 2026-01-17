using UnityEngine;
using UnityEngine.UI;

namespace Pansori.Microgames.Games
{
    /// <summary>
    /// 먹물 게이지 UI
    /// 세로 슬라이더 형태로 남은 먹물 양을 표시합니다.
    /// </summary>
    public class InkGaugeUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Slider inkSlider;
        [SerializeField] private Image fillImage;
        
        [Header("색상 설정")]
        [SerializeField] private Color fullColor = new Color(0.1f, 0.1f, 0.1f, 1f); // 검정 (먹물)
        [SerializeField] private Color emptyColor = new Color(0.6f, 0.6f, 0.6f, 1f); // 회색
        [SerializeField] private Color warningColor = new Color(0.8f, 0.2f, 0.2f, 1f); // 빨강 (경고)
        
        [Header("애니메이션 설정")]
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private float warningThreshold = 0.2f; // 20% 이하일 때 경고
        [SerializeField] private bool pulseOnWarning = true;
        [SerializeField] private float pulseSpeed = 3f;
        
        private float targetValue = 1f;
        private float currentValue = 1f;
        private bool isWarning = false;
        
        private void Awake()
        {
            // Slider 자동 찾기
            if (inkSlider == null)
            {
                inkSlider = GetComponent<Slider>();
            }
            
            if (inkSlider != null)
            {
                // 세로 슬라이더로 설정
                inkSlider.direction = Slider.Direction.BottomToTop;
                inkSlider.interactable = false; // 사용자 조작 불가
                
                // Fill Image 가져오기
                if (fillImage == null && inkSlider.fillRect != null)
                {
                    fillImage = inkSlider.fillRect.GetComponent<Image>();
                }
            }
            
            // 초기 상태
            SetValue(1f, true);
        }
        
        /// <summary>
        /// 게이지 값 설정
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
            
            // 경고 상태 확인
            isWarning = targetValue <= warningThreshold;
        }
        
        /// <summary>
        /// 먹물 소비 시 호출
        /// </summary>
        /// <param name="consumedAmount">소비된 양 (0~1 기준으로 변환 필요)</param>
        public void OnInkConsumed(float consumedAmount)
        {
            // DrawingCanvas에서 직접 InkRatio를 설정하도록 수정됨
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
            
            // 경고 펄스 애니메이션
            if (isWarning && pulseOnWarning && fillImage != null)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                fillImage.color = Color.Lerp(warningColor, emptyColor, pulse * 0.5f);
            }
        }
        
        /// <summary>
        /// 값 적용
        /// </summary>
        private void ApplyValue(float value)
        {
            if (inkSlider != null)
            {
                inkSlider.value = value;
            }
            
            // 색상 업데이트
            if (fillImage != null && !isWarning)
            {
                fillImage.color = Color.Lerp(emptyColor, fullColor, value);
            }
        }
        
        /// <summary>
        /// 리셋
        /// </summary>
        public void ResetGauge()
        {
            SetValue(1f, true);
            isWarning = false;
            
            if (fillImage != null)
            {
                fillImage.color = fullColor;
            }
        }
        
        /// <summary>
        /// 경고 임계값 설정
        /// </summary>
        /// <param name="threshold">0~1 범위</param>
        public void SetWarningThreshold(float threshold)
        {
            warningThreshold = Mathf.Clamp01(threshold);
        }
    }
}