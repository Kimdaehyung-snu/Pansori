using UnityEngine;

namespace Pansori.Microgames
{
    /// <summary>
    /// 결과 애니메이션 설정 ScriptableObject
    /// 클리어/실패 시 애니메이션 파라미터를 정의합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ResultAnimationSettings", menuName = "Pansori/Result Animation Settings")]
    public class ResultAnimationSettings : ScriptableObject
    {
        /// <summary>
        /// 설정용 애니메이션 타입 (MicrogameResultAnimation과 동일)
        /// </summary>
        public enum AnimationType
        {
            None,           // 애니메이션 없음
            ScalePunch,     // 스케일 펀치
            Rotate,         // 회전
            ColorFlash,     // 색상 플래시
            Shake,          // 흔들림
            FadeInOut,      // 페이드 인/아웃
            SlideIn,        // 슬라이드 인
            Bounce          // 바운스
        }

        [Header("클리어 설정")]
        [Tooltip("클리어 시 표시할 텍스트")]
        public string winText = "성공!";
        
        [Tooltip("클리어 텍스트 색상")]
        public Color winTextColor = new Color(0.2f, 0.8f, 0.2f);
        
        [Tooltip("클리어 시 배경 오버레이 색상")]
        public Color winBackgroundColor = new Color(0.7f, 1f, 0.7f, 0.5f);
        
        [Tooltip("클리어 시 표시할 스프라이트 (선택적)")]
        public Sprite winSprite;
        
        [Tooltip("클리어 시 애니메이션 타입")]
        public AnimationType winAnimationType = AnimationType.ScalePunch;
        
        [Tooltip("클리어 시 재생할 사운드 (선택적)")]
        public AudioClip winSound;

        [Header("실패 설정")]
        [Tooltip("실패 시 표시할 텍스트")]
        public string loseText = "실패...";
        
        [Tooltip("실패 텍스트 색상")]
        public Color loseTextColor = new Color(0.8f, 0.2f, 0.2f);
        
        [Tooltip("실패 시 배경 오버레이 색상")]
        public Color loseBackgroundColor = new Color(1f, 0.7f, 0.7f, 0.5f);
        
        [Tooltip("실패 시 표시할 스프라이트 (선택적)")]
        public Sprite loseSprite;
        
        [Tooltip("실패 시 애니메이션 타입")]
        public AnimationType loseAnimationType = AnimationType.Shake;
        
        [Tooltip("실패 시 재생할 사운드 (선택적)")]
        public AudioClip loseSound;

        [Header("애니메이션 타이밍")]
        [Tooltip("애니메이션 재생 시간 (초)")]
        [Range(0.1f, 2f)]
        public float animationDuration = 0.5f;
        
        [Tooltip("결과 표시 유지 시간 (초)")]
        [Range(0.1f, 3f)]
        public float displayDuration = 0.8f;
        
        [Tooltip("결과 보고 전 대기 시간 (초) - 연출 후 결과 전달까지")]
        [Range(0f, 2f)]
        public float resultReportDelay = 0.3f;

        [Header("스케일 펀치 설정")]
        [Tooltip("스케일 펀치 크기 배율")]
        [Range(1f, 2f)]
        public float scalePunchAmount = 1.3f;

        [Header("흔들림 설정")]
        [Tooltip("흔들림 강도 (픽셀)")]
        [Range(1f, 50f)]
        public float shakeIntensity = 10f;
        
        [Tooltip("흔들림 횟수")]
        [Range(1, 20)]
        public int shakeCount = 5;

        [Header("회전 설정")]
        [Tooltip("회전 각도")]
        [Range(0f, 720f)]
        public float rotationAmount = 360f;

        [Header("애니메이션 커브")]
        [Tooltip("애니메이션 보간 커브")]
        public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("판소리 씬 연동")]
        [Tooltip("판소리 씬에서 캐릭터 애니메이션을 사용할지 여부")]
        public bool useCharacterAnimation = true;
        
        [Tooltip("클리어 시 Unity Animator 트리거 이름")]
        public string winAnimatorTrigger = "Win";
        
        [Tooltip("실패 시 Unity Animator 트리거 이름")]
        public string loseAnimatorTrigger = "Lose";

        [Header("화면 효과")]
        [Tooltip("클리어 시 화면 플래시 효과 사용")]
        public bool useWinScreenFlash = true;
        
        [Tooltip("실패 시 화면 플래시 효과 사용")]
        public bool useLoseScreenFlash = true;
        
        [Tooltip("실패 시 화면 흔들림 효과 사용")]
        public bool useLoseScreenShake = true;
        
        [Tooltip("화면 흔들림 강도")]
        [Range(1f, 30f)]
        public float screenShakeIntensity = 5f;

        /// <summary>
        /// 결과에 따른 텍스트를 반환합니다.
        /// </summary>
        public string GetResultText(bool success)
        {
            return success ? winText : loseText;
        }

        /// <summary>
        /// 결과에 따른 텍스트 색상을 반환합니다.
        /// </summary>
        public Color GetTextColor(bool success)
        {
            return success ? winTextColor : loseTextColor;
        }

        /// <summary>
        /// 결과에 따른 배경 색상을 반환합니다.
        /// </summary>
        public Color GetBackgroundColor(bool success)
        {
            return success ? winBackgroundColor : loseBackgroundColor;
        }

        /// <summary>
        /// 결과에 따른 스프라이트를 반환합니다.
        /// </summary>
        public Sprite GetSprite(bool success)
        {
            return success ? winSprite : loseSprite;
        }

        /// <summary>
        /// 결과에 따른 애니메이션 타입을 반환합니다.
        /// </summary>
        public AnimationType GetAnimationType(bool success)
        {
            return success ? winAnimationType : loseAnimationType;
        }

        /// <summary>
        /// 결과에 따른 사운드 클립을 반환합니다.
        /// </summary>
        public AudioClip GetSound(bool success)
        {
            return success ? winSound : loseSound;
        }

        /// <summary>
        /// 결과에 따른 Animator 트리거 이름을 반환합니다.
        /// </summary>
        public string GetAnimatorTrigger(bool success)
        {
            return success ? winAnimatorTrigger : loseAnimatorTrigger;
        }

        /// <summary>
        /// 화면 플래시 효과 사용 여부를 반환합니다.
        /// </summary>
        public bool ShouldUseScreenFlash(bool success)
        {
            return success ? useWinScreenFlash : useLoseScreenFlash;
        }
    }
}