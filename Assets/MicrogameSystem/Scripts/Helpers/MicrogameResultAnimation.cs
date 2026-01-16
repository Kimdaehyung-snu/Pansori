using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pansori.Microgames
{
    /// <summary>
    /// 마이크로게임 결과 애니메이션 타입
    /// </summary>
    public enum ResultAnimationType
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

    /// <summary>
    /// 마이크로게임 결과 애니메이션 헬퍼 클래스
    /// 클리어/실패 시 다양한 시각적 피드백을 제공합니다.
    /// </summary>
    public class MicrogameResultAnimation : MonoBehaviour
    {
        [Header("결과 UI 요소")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Image resultImage;
        [SerializeField] private Image backgroundOverlay;

        [Header("클리어 설정")]
        [SerializeField] private string winText = "성공!";
        [SerializeField] private Color winTextColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color winBackgroundColor = new Color(0.7f, 1f, 0.7f, 0.5f);
        [SerializeField] private Sprite winSprite;
        [SerializeField] private ResultAnimationType winAnimationType = ResultAnimationType.ScalePunch;

        [Header("실패 설정")]
        [SerializeField] private string loseText = "실패...";
        [SerializeField] private Color loseTextColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color loseBackgroundColor = new Color(1f, 0.7f, 0.7f, 0.5f);
        [SerializeField] private Sprite loseSprite;
        [SerializeField] private ResultAnimationType loseAnimationType = ResultAnimationType.Shake;

        [Header("애니메이션 설정")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private float displayDuration = 0.8f;
        [SerializeField] private float scalePunchAmount = 1.3f;
        [SerializeField] private float shakeIntensity = 10f;
        [SerializeField] private int shakeCount = 5;
        [SerializeField] private float rotationAmount = 360f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("대상 오브젝트 (선택적)")]
        [SerializeField] private Transform[] winTargets;  // 클리어 시 애니메이션할 오브젝트들
        [SerializeField] private Transform[] loseTargets; // 실패 시 애니메이션할 오브젝트들

        [Header("Unity Animator 연동 (선택적)")]
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private string winAnimationTrigger = "Win";
        [SerializeField] private string loseAnimationTrigger = "Lose";

        /// <summary>
        /// 애니메이션 완료 이벤트
        /// </summary>
        public event Action OnAnimationComplete;

        /// <summary>
        /// 현재 애니메이션 코루틴
        /// </summary>
        private Coroutine currentCoroutine;

        /// <summary>
        /// 애니메이션이 재생 중인지 여부
        /// </summary>
        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            // 초기 상태: 결과 패널 숨김
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
            if (backgroundOverlay != null)
            {
                backgroundOverlay.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 결과 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="success">성공 여부</param>
        /// <param name="onComplete">완료 콜백</param>
        public void PlayResultAnimation(bool success, Action onComplete = null)
        {
            if (IsPlaying)
            {
                StopCurrentAnimation();
            }

            IsPlaying = true;
            currentCoroutine = StartCoroutine(PlayResultAnimationCoroutine(success, onComplete));
        }

        /// <summary>
        /// 결과 애니메이션 코루틴
        /// </summary>
        private IEnumerator PlayResultAnimationCoroutine(bool success, Action onComplete)
        {
            // 설정 가져오기
            string text = success ? winText : loseText;
            Color textColor = success ? winTextColor : loseTextColor;
            Color bgColor = success ? winBackgroundColor : loseBackgroundColor;
            Sprite sprite = success ? winSprite : loseSprite;
            ResultAnimationType animType = success ? winAnimationType : loseAnimationType;
            Transform[] targets = success ? winTargets : loseTargets;

            // Unity Animator 트리거 (있으면)
            if (characterAnimator != null)
            {
                string trigger = success ? winAnimationTrigger : loseAnimationTrigger;
                characterAnimator.SetTrigger(trigger);
            }

            // 결과 UI 설정
            SetupResultUI(text, textColor, bgColor, sprite);

            // 배경 오버레이 페이드인
            if (backgroundOverlay != null)
            {
                backgroundOverlay.gameObject.SetActive(true);
                yield return StartCoroutine(FadeImage(backgroundOverlay, 0f, bgColor.a, animationDuration * 0.3f));
            }

            // 결과 패널 표시
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            // 메인 애니메이션
            Transform mainTarget = resultPanel != null ? resultPanel.transform : transform;
            yield return StartCoroutine(PlayAnimation(mainTarget, animType));

            // 추가 타겟 애니메이션 (병렬)
            if (targets != null && targets.Length > 0)
            {
                foreach (Transform target in targets)
                {
                    if (target != null)
                    {
                        StartCoroutine(PlayAnimation(target, animType));
                    }
                }
            }

            // 표시 유지
            yield return new WaitForSeconds(displayDuration);

            // 페이드아웃
            if (resultPanel != null && resultText != null)
            {
                yield return StartCoroutine(FadeText(resultText, 1f, 0f, animationDuration * 0.3f));
            }

            if (backgroundOverlay != null)
            {
                yield return StartCoroutine(FadeImage(backgroundOverlay, bgColor.a, 0f, animationDuration * 0.3f));
                backgroundOverlay.gameObject.SetActive(false);
            }

            // 결과 패널 숨기기
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            IsPlaying = false;

            // 완료 콜백
            onComplete?.Invoke();
            OnAnimationComplete?.Invoke();

            Debug.Log($"[MicrogameResultAnimation] 결과 애니메이션 완료: {(success ? "성공" : "실패")}");
        }

        /// <summary>
        /// 결과 UI를 설정합니다.
        /// </summary>
        private void SetupResultUI(string text, Color textColor, Color bgColor, Sprite sprite)
        {
            if (resultText != null)
            {
                resultText.text = text;
                resultText.color = textColor;
            }

            if (resultImage != null)
            {
                if (sprite != null)
                {
                    resultImage.sprite = sprite;
                    resultImage.gameObject.SetActive(true);
                }
                else
                {
                    resultImage.gameObject.SetActive(false);
                }
            }

            if (backgroundOverlay != null)
            {
                Color overlayColor = bgColor;
                overlayColor.a = 0f; // 시작은 투명
                backgroundOverlay.color = overlayColor;
            }
        }

        /// <summary>
        /// 애니메이션을 재생합니다.
        /// </summary>
        private IEnumerator PlayAnimation(Transform target, ResultAnimationType animType)
        {
            if (target == null) yield break;

            switch (animType)
            {
                case ResultAnimationType.ScalePunch:
                    yield return StartCoroutine(ScalePunchAnimation(target));
                    break;
                case ResultAnimationType.Rotate:
                    yield return StartCoroutine(RotateAnimation(target));
                    break;
                case ResultAnimationType.ColorFlash:
                    yield return StartCoroutine(ColorFlashAnimation(target));
                    break;
                case ResultAnimationType.Shake:
                    yield return StartCoroutine(ShakeAnimation(target));
                    break;
                case ResultAnimationType.FadeInOut:
                    yield return StartCoroutine(FadeInOutAnimation(target));
                    break;
                case ResultAnimationType.SlideIn:
                    yield return StartCoroutine(SlideInAnimation(target));
                    break;
                case ResultAnimationType.Bounce:
                    yield return StartCoroutine(BounceAnimation(target));
                    break;
                case ResultAnimationType.None:
                default:
                    // 애니메이션 없음
                    break;
            }
        }

        /// <summary>
        /// 스케일 펀치 애니메이션
        /// </summary>
        private IEnumerator ScalePunchAnimation(Transform target)
        {
            Vector3 originalScale = target.localScale;
            Vector3 punchScale = originalScale * scalePunchAmount;

            float halfDuration = animationDuration * 0.5f;

            // 커지기
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsed / halfDuration);
                target.localScale = Vector3.Lerp(originalScale, punchScale, t);
                yield return null;
            }

            // 원래대로
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsed / halfDuration);
                target.localScale = Vector3.Lerp(punchScale, originalScale, t);
                yield return null;
            }

            target.localScale = originalScale;
        }

        /// <summary>
        /// 회전 애니메이션
        /// </summary>
        private IEnumerator RotateAnimation(Transform target)
        {
            Quaternion originalRotation = target.localRotation;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                float angle = Mathf.Lerp(0f, rotationAmount, animationCurve.Evaluate(t));
                target.localRotation = originalRotation * Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }

            target.localRotation = originalRotation;
        }

        /// <summary>
        /// 색상 플래시 애니메이션
        /// </summary>
        private IEnumerator ColorFlashAnimation(Transform target)
        {
            Image image = target.GetComponent<Image>();
            SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();

            if (image != null)
            {
                Color originalColor = image.color;
                Color flashColor = Color.white;

                yield return StartCoroutine(FlashColor(
                    (c) => image.color = c,
                    originalColor,
                    flashColor,
                    animationDuration
                ));

                image.color = originalColor;
            }
            else if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                Color flashColor = Color.white;

                yield return StartCoroutine(FlashColor(
                    (c) => spriteRenderer.color = c,
                    originalColor,
                    flashColor,
                    animationDuration
                ));

                spriteRenderer.color = originalColor;
            }
        }

        /// <summary>
        /// 색상 플래시 헬퍼
        /// </summary>
        private IEnumerator FlashColor(Action<Color> setColor, Color original, Color flash, float duration)
        {
            float halfDuration = duration * 0.5f;
            float elapsed = 0f;

            // 플래시 색상으로
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                setColor(Color.Lerp(original, flash, t));
                yield return null;
            }

            // 원래 색상으로
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                setColor(Color.Lerp(flash, original, t));
                yield return null;
            }

            setColor(original);
        }

        /// <summary>
        /// 흔들림 애니메이션
        /// </summary>
        private IEnumerator ShakeAnimation(Transform target)
        {
            Vector3 originalPosition = target.localPosition;
            float elapsed = 0f;
            float shakeDuration = animationDuration / shakeCount;

            for (int i = 0; i < shakeCount; i++)
            {
                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
                    UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
                    0f
                );

                elapsed = 0f;
                while (elapsed < shakeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / shakeDuration;
                    // 감쇠 효과
                    float damping = 1f - ((float)i / shakeCount);
                    target.localPosition = originalPosition + randomOffset * damping * (1f - t);
                    yield return null;
                }
            }

            target.localPosition = originalPosition;
        }

        /// <summary>
        /// 페이드 인/아웃 애니메이션
        /// </summary>
        private IEnumerator FadeInOutAnimation(Transform target)
        {
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }

            float halfDuration = animationDuration * 0.5f;

            // 페이드 인
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, halfDuration));

            // 잠시 대기
            yield return new WaitForSeconds(displayDuration * 0.5f);

            // 페이드 아웃
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, halfDuration));
        }

        /// <summary>
        /// 슬라이드 인 애니메이션
        /// </summary>
        private IEnumerator SlideInAnimation(Transform target)
        {
            Vector3 originalPosition = target.localPosition;
            Vector3 startPosition = originalPosition + new Vector3(500f, 0f, 0f); // 오른쪽에서 시작
            target.localPosition = startPosition;

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsed / animationDuration);
                target.localPosition = Vector3.Lerp(startPosition, originalPosition, t);
                yield return null;
            }

            target.localPosition = originalPosition;
        }

        /// <summary>
        /// 바운스 애니메이션
        /// </summary>
        private IEnumerator BounceAnimation(Transform target)
        {
            Vector3 originalScale = target.localScale;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;

                // 바운스 곡선: sin을 이용한 감쇠 진동
                float bounce = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t) * (scalePunchAmount - 1f);
                float scale = 1f + bounce;

                target.localScale = originalScale * scale;
                yield return null;
            }

            target.localScale = originalScale;
        }

        /// <summary>
        /// 이미지 페이드
        /// </summary>
        private IEnumerator FadeImage(Image image, float from, float to, float duration)
        {
            Color color = image.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                color.a = Mathf.Lerp(from, to, t);
                image.color = color;
                yield return null;
            }

            color.a = to;
            image.color = color;
        }

        /// <summary>
        /// 텍스트 페이드
        /// </summary>
        private IEnumerator FadeText(TMP_Text text, float from, float to, float duration)
        {
            Color color = text.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                color.a = Mathf.Lerp(from, to, t);
                text.color = color;
                yield return null;
            }

            color.a = to;
            text.color = color;
        }

        /// <summary>
        /// CanvasGroup 페이드
        /// </summary>
        private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            group.alpha = to;
        }

        /// <summary>
        /// 현재 애니메이션을 중지합니다.
        /// </summary>
        public void StopCurrentAnimation()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            IsPlaying = false;

            // UI 정리
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
            if (backgroundOverlay != null)
            {
                backgroundOverlay.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 대상 오브젝트에 간단한 성공/실패 이펙트를 적용합니다.
        /// UI가 없는 경우에도 사용 가능합니다.
        /// </summary>
        /// <param name="target">대상 Transform</param>
        /// <param name="success">성공 여부</param>
        /// <param name="onComplete">완료 콜백</param>
        public void PlayQuickEffect(Transform target, bool success, Action onComplete = null)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                return;
            }

            ResultAnimationType animType = success ? winAnimationType : loseAnimationType;
            StartCoroutine(PlayQuickEffectCoroutine(target, animType, onComplete));
        }

        private IEnumerator PlayQuickEffectCoroutine(Transform target, ResultAnimationType animType, Action onComplete)
        {
            yield return StartCoroutine(PlayAnimation(target, animType));
            onComplete?.Invoke();
        }

        /// <summary>
        /// 화면 전체를 플래시합니다.
        /// </summary>
        /// <param name="success">성공 여부 (색상 결정)</param>
        public void FlashScreen(bool success)
        {
            if (backgroundOverlay != null)
            {
                Color flashColor = success ? winBackgroundColor : loseBackgroundColor;
                StartCoroutine(ScreenFlashCoroutine(flashColor));
            }
        }

        private IEnumerator ScreenFlashCoroutine(Color flashColor)
        {
            backgroundOverlay.gameObject.SetActive(true);

            Color startColor = flashColor;
            startColor.a = 0.8f;
            backgroundOverlay.color = startColor;

            yield return StartCoroutine(FadeImage(backgroundOverlay, 0.8f, 0f, 0.3f));

            backgroundOverlay.gameObject.SetActive(false);
        }
    }
}