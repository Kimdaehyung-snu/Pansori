using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pansori.Microgames
{
    /// <summary>
    /// 판소리 씬 UI 및 연출 관리
    /// 마이크로게임 사이에 표시되는 판소리 무대를 관리합니다.
    /// </summary>
    public class PansoriSceneUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private GameObject pansoriPanel; // 판소리 씬 패널
        [SerializeField] private Image backgroundImage; // 배경 이미지
        [SerializeField] private TMP_Text commandText; // "XX해라!" 명령 텍스트
        [SerializeField] private TMP_Text reactionText; // 환호/야유 반응 텍스트
        
        [Header("캐릭터 (플레이스홀더)")]
        [SerializeField] private GameObject performerObject; // 소리꾼 오브젝트
        [SerializeField] private GameObject audienceObject; // 관객 오브젝트
        
        [Header("색상 설정")]
        [SerializeField] private Color normalBackgroundColor = new Color(0.9f, 0.85f, 0.75f); // 기본 배경색
        [SerializeField] private Color successBackgroundColor = new Color(0.7f, 1f, 0.7f); // 성공 시 배경색
        [SerializeField] private Color failureBackgroundColor = new Color(1f, 0.7f, 0.7f); // 실패 시 배경색
        
        [Header("텍스트 설정")]
        [SerializeField] private string successReactionText = "얼쑤!"; // 성공 반응 텍스트
        [SerializeField] private string failureReactionText = "에잇..."; // 실패 반응 텍스트
        [SerializeField] private Color successTextColor = new Color(0.2f, 0.6f, 0.2f); // 성공 텍스트 색상
        [SerializeField] private Color failureTextColor = new Color(0.8f, 0.2f, 0.2f); // 실패 텍스트 색상
        
        [Header("애니메이션 설정")]
        [SerializeField] private float commandFadeInDuration = 0.3f; // 명령 텍스트 페이드인 시간
        [SerializeField] private float reactionScalePunchAmount = 1.2f; // 반응 텍스트 스케일 펀치 크기
        
        private Coroutine currentCoroutine;
        private Canvas canvas;
        
        private void Awake()
        {
            SetupCanvas();
            HideAll();
        }
        
        /// <summary>
        /// Canvas 설정
        /// </summary>
        private void SetupCanvas()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50; // MicrogameInfoUI(200) 보다 낮게 설정
            
            // CanvasScaler 설정
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            // GraphicRaycaster 설정
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }
        
        /// <summary>
        /// 모든 UI 숨기기
        /// </summary>
        public void HideAll()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }
            
            if (pansoriPanel != null)
            {
                pansoriPanel.SetActive(false);
            }
            
            if (commandText != null)
            {
                commandText.gameObject.SetActive(false);
            }
            
            if (reactionText != null)
            {
                reactionText.gameObject.SetActive(false);
            }
            
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }
        
        /// <summary>
        /// 판소리 씬 표시
        /// </summary>
        public void Show()
        {
            if (canvas != null)
            {
                canvas.enabled = true;
            }
            
            if (pansoriPanel != null)
            {
                pansoriPanel.SetActive(true);
            }
            
            // 기본 배경색으로 설정
            if (backgroundImage != null)
            {
                backgroundImage.color = normalBackgroundColor;
            }
        }
        
        /// <summary>
        /// "XX해라!" 명령 표시
        /// </summary>
        /// <param name="gameName">게임 이름</param>
        /// <param name="delay">표시 전 대기 시간</param>
        /// <param name="onComplete">완료 콜백</param>
        public void ShowCommand(string gameName, float delay, Action onComplete)
        {
            Show();
            currentCoroutine = StartCoroutine(ShowCommandCoroutine(gameName, delay, onComplete));
        }
        
        /// <summary>
        /// 명령 표시 코루틴
        /// </summary>
        private IEnumerator ShowCommandCoroutine(string gameName, float delay, Action onComplete)
        {
            // 명령 텍스트 숨기기
            if (commandText != null)
            {
                commandText.gameObject.SetActive(false);
            }
            
            // 대기
            yield return new WaitForSeconds(delay);
            
            // 명령 텍스트 표시
            if (commandText != null)
            {
                commandText.text = $"{gameName}해라!";
                commandText.gameObject.SetActive(true);
                
                // 간단한 페이드인 효과
                yield return StartCoroutine(FadeInText(commandText, commandFadeInDuration));
            }
            
            // 약간의 추가 대기 후 콜백
            yield return new WaitForSeconds(0.5f);
            
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 마이크로게임 결과에 따른 반응 표시
        /// </summary>
        /// <param name="success">성공 여부</param>
        /// <param name="duration">표시 시간</param>
        /// <param name="onComplete">완료 콜백</param>
        public void ShowReaction(bool success, float duration, Action onComplete)
        {
            Show();
            currentCoroutine = StartCoroutine(ShowReactionCoroutine(success, duration, onComplete));
        }
        
        /// <summary>
        /// 반응 표시 코루틴
        /// </summary>
        private IEnumerator ShowReactionCoroutine(bool success, float duration, Action onComplete)
        {
            // 명령 텍스트 숨기기
            if (commandText != null)
            {
                commandText.gameObject.SetActive(false);
            }
            
            // 배경색 변경
            if (backgroundImage != null)
            {
                backgroundImage.color = success ? successBackgroundColor : failureBackgroundColor;
            }
            
            // 반응 텍스트 표시
            if (reactionText != null)
            {
                reactionText.text = success ? successReactionText : failureReactionText;
                reactionText.color = success ? successTextColor : failureTextColor;
                reactionText.gameObject.SetActive(true);
                
                // 스케일 펀치 효과
                yield return StartCoroutine(ScalePunchEffect(reactionText.rectTransform, reactionScalePunchAmount, 0.3f));
            }
            
            // 대기
            yield return new WaitForSeconds(duration);
            
            // 반응 텍스트 숨기기
            if (reactionText != null)
            {
                reactionText.gameObject.SetActive(false);
            }
            
            // 배경색 복원
            if (backgroundImage != null)
            {
                backgroundImage.color = normalBackgroundColor;
            }
            
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 텍스트 페이드인 효과
        /// </summary>
        private IEnumerator FadeInText(TMP_Text text, float duration)
        {
            if (text == null) yield break;
            
            Color originalColor = text.color;
            Color startColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                text.color = Color.Lerp(startColor, originalColor, t);
                yield return null;
            }
            
            text.color = originalColor;
        }
        
        /// <summary>
        /// 스케일 펀치 효과
        /// </summary>
        private IEnumerator ScalePunchEffect(RectTransform target, float punchAmount, float duration)
        {
            if (target == null) yield break;
            
            Vector3 originalScale = Vector3.one;
            Vector3 punchScale = originalScale * punchAmount;
            
            float halfDuration = duration * 0.5f;
            
            // 커지는 단계
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                target.localScale = Vector3.Lerp(originalScale, punchScale, t);
                yield return null;
            }
            
            // 원래대로 돌아오는 단계
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                target.localScale = Vector3.Lerp(punchScale, originalScale, t);
                yield return null;
            }
            
            target.localScale = originalScale;
        }
        
        /// <summary>
        /// 현재 표시 상태
        /// </summary>
        public bool IsVisible => canvas != null && canvas.enabled;
    }
}
