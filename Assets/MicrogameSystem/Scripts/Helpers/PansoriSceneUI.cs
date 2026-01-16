using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pansori.Microgames
{
    /// <summary>
    /// 판소리 씬 UI 및 연출 관리
    /// 마이크로게임 사이에 표시되는 판소리 무대를 관리합니다.
    /// 게임 정보(목숨, 스테이지)도 함께 표시합니다.
    /// </summary>
    public class PansoriSceneUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private GameObject pansoriPanel; // 판소리 씬 패널
        [SerializeField] private Image backgroundImage; // 배경 이미지
        [SerializeField] private TMP_Text commandText; // "XX해라!" 명령 텍스트
        [SerializeField] private TMP_Text reactionText; // 환호/야유 반응 텍스트
        
        [Header("게임 정보 UI")]
        [SerializeField] private TMP_Text livesText; // 목숨 텍스트
        [SerializeField] private TMP_Text stageText; // 스테이지 텍스트
        [SerializeField] private Transform livesContainer; // 목숨 스프라이트 컨테이너
        
        [Header("목숨 스프라이트 설정")]
        [SerializeField] private Sprite lifeSprite; // 목숨 스프라이트
        [SerializeField] private Sprite consumedLifeSprite; // 소모된 목숨 스프라이트 (선택사항)
        [SerializeField] private float lifeSpriteSpacing = 10f; // 스프라이트 간 간격
        [SerializeField] private Vector2 lifeSpriteSize = new Vector2(50, 50); // 스프라이트 크기
        
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
        
        /// <summary>
        /// 생성된 목숨 스프라이트 리스트
        /// </summary>
        private List<GameObject> lifeSpriteObjects = new List<GameObject>();
        
        private void Awake()
        {
            SetupCanvas();
            SetupLivesContainer();
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
            canvas.sortingOrder = 50;
            
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
        /// 목숨 컨테이너를 설정합니다.
        /// </summary>
        private void SetupLivesContainer()
        {
            // 컨테이너가 없으면 자동 생성
            if (livesContainer == null)
            {
                GameObject containerObj = new GameObject("LivesContainer");
                containerObj.transform.SetParent(transform, false);
                livesContainer = containerObj.transform;
                
                RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                rectTransform.anchoredPosition = new Vector2(0, -50);
                rectTransform.sizeDelta = new Vector2(500, 100);
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
            
            // 게임 정보 UI 숨기기
            HideGameInfo();
            
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }
        
        /// <summary>
        /// 게임 정보 UI 숨기기
        /// </summary>
        private void HideGameInfo()
        {
            if (livesText != null)
            {
                livesText.gameObject.SetActive(false);
            }
            
            if (stageText != null)
            {
                stageText.gameObject.SetActive(false);
            }
            
            ClearLifeSprites();
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
        /// "XX해라!" 명령 표시 (기존 메서드 - 호환성 유지)
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
        /// "XX해라!" 명령과 게임 정보를 함께 표시
        /// </summary>
        /// <param name="gameName">게임 이름</param>
        /// <param name="totalLives">총 목숨</param>
        /// <param name="consumedLives">소모된 목숨</param>
        /// <param name="stage">현재 스테이지</param>
        /// <param name="delay">표시 전 대기 시간</param>
        /// <param name="onComplete">완료 콜백</param>
        public void ShowCommandWithInfo(string gameName, int totalLives, int consumedLives, int stage, float delay, Action onComplete)
        {
            Show();
            
            // 게임 정보 표시
            UpdateLivesDisplay(totalLives, consumedLives);
            UpdateStageDisplay(stage);
            
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
            
            // 게임 정보 숨기기
            HideGameInfo();
            
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
        /// 목숨 표시 업데이트
        /// </summary>
        /// <param name="totalLives">총 목숨</param>
        /// <param name="consumedLives">소모된 목숨</param>
        public void UpdateLivesDisplay(int totalLives, int consumedLives)
        {
            // 텍스트 업데이트
            if (livesText != null)
            {
                int remainingLives = totalLives - consumedLives;
                livesText.text = $"목숨: {remainingLives}";
                livesText.gameObject.SetActive(true);
            }
            
            // 스프라이트 업데이트
            ClearLifeSprites();
            
            if (lifeSprite == null || livesContainer == null)
            {
                return;
            }
            
            // 총 목숨만큼 스프라이트 생성
            for (int i = 0; i < totalLives; i++)
            {
                GameObject spriteObj = CreateLifeSprite(i, totalLives);
                
                // 우측에서부터 consumedLives개는 소모된 상태로 표시
                bool isConsumed = i >= (totalLives - consumedLives);
                SetLifeSpriteState(spriteObj, isConsumed);
                
                lifeSpriteObjects.Add(spriteObj);
            }
            
            Debug.Log($"[PansoriSceneUI] 목숨 표시 업데이트 - 총: {totalLives}, 소모: {consumedLives}");
        }
        
        /// <summary>
        /// 스테이지 표시 업데이트
        /// </summary>
        /// <param name="stage">현재 스테이지</param>
        public void UpdateStageDisplay(int stage)
        {
            if (stageText != null)
            {
                stageText.text = $"스테이지: {stage}";
                stageText.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// 목숨 스프라이트를 생성합니다.
        /// </summary>
        private GameObject CreateLifeSprite(int index, int totalLives)
        {
            GameObject spriteObj = new GameObject($"Life_{index}");
            spriteObj.transform.SetParent(livesContainer, false);
            
            Image image = spriteObj.AddComponent<Image>();
            image.sprite = lifeSprite;
            
            RectTransform rectTransform = spriteObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = lifeSpriteSize;
            
            // 좌측부터 나열 (중앙 정렬)
            float startX = -(totalLives - 1) * (lifeSpriteSize.x + lifeSpriteSpacing) / 2f;
            float xPos = startX + index * (lifeSpriteSize.x + lifeSpriteSpacing);
            rectTransform.anchoredPosition = new Vector2(xPos, 0);
            
            return spriteObj;
        }
        
        /// <summary>
        /// 목숨 스프라이트의 상태를 설정합니다.
        /// </summary>
        private void SetLifeSpriteState(GameObject spriteObj, bool isConsumed)
        {
            Image image = spriteObj.GetComponent<Image>();
            if (image == null)
                return;
            
            if (isConsumed)
            {
                // 소모된 목숨 처리
                if (consumedLifeSprite != null)
                {
                    image.sprite = consumedLifeSprite;
                }
                else
                {
                    // 스프라이트가 없으면 반투명 처리
                    Color color = image.color;
                    color.a = 0.3f;
                    image.color = color;
                }
            }
            else
            {
                // 정상 목숨
                image.sprite = lifeSprite;
                Color color = image.color;
                color.a = 1.0f;
                image.color = color;
            }
        }
        
        /// <summary>
        /// 기존 목숨 스프라이트를 모두 제거합니다.
        /// </summary>
        private void ClearLifeSprites()
        {
            foreach (GameObject spriteObj in lifeSpriteObjects)
            {
                if (spriteObj != null)
                {
                    Destroy(spriteObj);
                }
            }
            lifeSpriteObjects.Clear();
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
