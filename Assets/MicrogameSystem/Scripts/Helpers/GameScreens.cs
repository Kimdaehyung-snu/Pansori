using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pansori.Microgames
{
    /// <summary>
    /// 게임 화면 UI 관리
    /// 메인 메뉴, 준비 화면, 승리 화면, 패배 화면을 관리합니다.
    /// </summary>
    public class GameScreens : MonoBehaviour
    {
        [Header("메인 메뉴")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text titleText;
        
        [Header("준비 화면")]
        [SerializeField] private GameObject readyPanel;
        [SerializeField] private TMP_Text readyText;
        [SerializeField] private TMP_Text controlDescriptionText; // 조작법 설명 텍스트
        [SerializeField] private string readyMessage = "준비!";
        [SerializeField] private string startMessage = "시작!";
        
        [Header("승리 화면")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TMP_Text victoryText;
        [SerializeField] private TMP_Text victoryScoreText;
        [SerializeField] private Button victoryRestartButton;
        
        [Header("패배 화면")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text gameOverText;
        [SerializeField] private TMP_Text gameOverScoreText;
        [SerializeField] private Button gameOverRestartButton;
        
        [Header("애니메이션 설정")]
        [SerializeField] private float textScaleAnimDuration = 0.3f;
        [SerializeField] private float textScaleAmount = 1.3f;
        
        [Header("참조")]
        [SerializeField] private GameFlowManager gameFlowManager;
        
        private Canvas canvas;
        private Coroutine currentCoroutine;
        
        private void Awake()
        {
            SetupCanvas();
            SetupButtons();
            HideAllScreens();
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
            canvas.sortingOrder = 100; // 다른 UI보다 위에 표시
            
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
        /// 버튼 이벤트 설정
        /// </summary>
        private void SetupButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }
            
            if (victoryRestartButton != null)
            {
                victoryRestartButton.onClick.AddListener(OnRestartButtonClicked);
            }
            
            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
            }
        }
        
        /// <summary>
        /// 시작 버튼 클릭 처리
        /// </summary>
        private void OnStartButtonClicked()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.StartGame();
            }
            else
            {
                Debug.LogWarning("[GameScreens] GameFlowManager 참조가 없습니다.");
            }
        }
        
        /// <summary>
        /// 재시작 버튼 클릭 처리
        /// </summary>
        private void OnRestartButtonClicked()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.RestartGame();
            }
            else
            {
                Debug.LogWarning("[GameScreens] GameFlowManager 참조가 없습니다.");
            }
        }
        
        /// <summary>
        /// 모든 화면 숨기기
        /// </summary>
        public void HideAllScreens()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }
            
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }
            
            if (readyPanel != null)
            {
                readyPanel.SetActive(false);
            }
            
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// 메인 메뉴 표시
        /// </summary>
        public void ShowMainMenu()
        {
            HideAllScreens();
            
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
            
            if (titleText != null)
            {
                titleText.text = "울려라! 판소리";
            }
            
            Debug.Log("[GameScreens] 메인 메뉴 표시");
        }
        
        /// <summary>
        /// 준비 화면 표시
        /// </summary>
        /// <param name="duration">표시 시간</param>
        /// <param name="onComplete">완료 콜백</param>
        /// <param name="controlDescription">조작법 설명 (선택)</param>
        public void ShowReadyScreen(float duration, Action onComplete, string controlDescription = "")
        {
            HideAllScreens();
            
            if (readyPanel != null)
            {
                readyPanel.SetActive(true);
            }
            
            // 조작법 설명 표시
            if (controlDescriptionText != null)
            {
                if (!string.IsNullOrEmpty(controlDescription))
                {
                    controlDescriptionText.text = controlDescription;
                    controlDescriptionText.gameObject.SetActive(true);
                }
                else
                {
                    controlDescriptionText.gameObject.SetActive(false);
                }
            }
            
            currentCoroutine = StartCoroutine(ReadySequenceCoroutine(duration, onComplete));
            
            Debug.Log($"[GameScreens] 준비 화면 표시 - 조작법: {controlDescription}");
        }
        
        /// <summary>
        /// 준비 화면 시퀀스 코루틴
        /// </summary>
        private IEnumerator ReadySequenceCoroutine(float totalDuration, Action onComplete)
        {
            float halfDuration = totalDuration * 0.5f;
            
            // "준비!" 표시
            if (readyText != null)
            {
                readyText.text = readyMessage;
                readyText.gameObject.SetActive(true);
                
                // 스케일 애니메이션
                yield return StartCoroutine(ScalePunchEffect(readyText.rectTransform, textScaleAmount, textScaleAnimDuration));
            }
            
            yield return new WaitForSeconds(halfDuration - textScaleAnimDuration);
            
            // "시작!" 표시
            if (readyText != null)
            {
                readyText.text = startMessage;
                
                // 스케일 애니메이션
                yield return StartCoroutine(ScalePunchEffect(readyText.rectTransform, textScaleAmount, textScaleAnimDuration));
            }
            
            yield return new WaitForSeconds(halfDuration - textScaleAnimDuration);
            
            // 준비 화면 숨기기
            if (readyPanel != null)
            {
                readyPanel.SetActive(false);
            }
            
            // 조작법 텍스트도 숨기기
            if (controlDescriptionText != null)
            {
                controlDescriptionText.gameObject.SetActive(false);
            }
            
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 승리 화면 표시
        /// </summary>
        /// <param name="winCount">승리 횟수</param>
        public void ShowVictoryScreen(int winCount)
        {
            HideAllScreens();
            
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
            
            if (victoryText != null)
            {
                victoryText.text = "축하합니다!";
            }
            
            if (victoryScoreText != null)
            {
                victoryScoreText.text = $"총 {winCount}회 승리!";
            }
            
            Debug.Log($"[GameScreens] 승리 화면 표시 - 승리 횟수: {winCount}");
        }
        
        /// <summary>
        /// 패배 화면 표시
        /// </summary>
        /// <param name="winCount">승리 횟수</param>
        /// <param name="loseCount">패배 횟수</param>
        public void ShowGameOverScreen(int winCount, int loseCount)
        {
            HideAllScreens();
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            
            if (gameOverText != null)
            {
                gameOverText.text = "게임 오버";
            }
            
            if (gameOverScoreText != null)
            {
                gameOverScoreText.text = $"승리: {winCount}회 / 패배: {loseCount}회";
            }
            
            Debug.Log($"[GameScreens] 패배 화면 표시 - 승리: {winCount}, 패배: {loseCount}");
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
                // EaseOutQuad 적용
                t = 1f - (1f - t) * (1f - t);
                target.localScale = Vector3.Lerp(originalScale, punchScale, t);
                yield return null;
            }
            
            // 원래대로 돌아오는 단계
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                // EaseInQuad 적용
                t = t * t;
                target.localScale = Vector3.Lerp(punchScale, originalScale, t);
                yield return null;
            }
            
            target.localScale = originalScale;
        }
        
        /// <summary>
        /// 현재 활성화된 화면 가져오기
        /// </summary>
        public string GetActiveScreen()
        {
            if (mainMenuPanel != null && mainMenuPanel.activeSelf) return "MainMenu";
            if (readyPanel != null && readyPanel.activeSelf) return "Ready";
            if (victoryPanel != null && victoryPanel.activeSelf) return "Victory";
            if (gameOverPanel != null && gameOverPanel.activeSelf) return "GameOver";
            return "None";
        }
    }
}
