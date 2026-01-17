using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private Button practiceButton;
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
        
        [Header("연습 모드 선택 화면")]
        [SerializeField] private GameObject practiceSelectPanel;
        [SerializeField] private TMP_Text practiceSelectTitleText;
        [SerializeField] private Transform practiceGridContainer;
        [SerializeField] private Button practiceBackButton;
        [SerializeField] private GameObject practiceButtonPrefab;
        
        [Header("연습 모드 힌트")]
        [SerializeField] private GameObject practiceHintPanel;
        [SerializeField] private TMP_Text practiceHintText;
        [SerializeField] private string practiceHintMessage = "ESC를 눌러 메인화면으로";
        
        [Header("연습 모드 UI 설정")]
        [SerializeField] private Vector2 practiceButtonSize = new Vector2(200, 250);
        [SerializeField] private Vector2 practiceGridSpacing = new Vector2(20, 20);
        [SerializeField] private int practiceGridColumns = 3;
        
        [Header("애니메이션 설정")]
        [SerializeField] private float textScaleAnimDuration = 0.3f;
        [SerializeField] private float textScaleAmount = 1.3f;
        
        [Header("참조")]
        [SerializeField] private GameFlowManager gameFlowManager;
        
        private Canvas canvas;
        private Coroutine currentCoroutine;
        private List<GameObject> practiceButtons = new List<GameObject>();
        
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
            
            if (practiceButton != null)
            {
                practiceButton.onClick.AddListener(OnPracticeButtonClicked);
            }
            
            if (victoryRestartButton != null)
            {
                victoryRestartButton.onClick.AddListener(OnRestartButtonClicked);
            }
            
            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
            }
            
            if (practiceBackButton != null)
            {
                practiceBackButton.onClick.AddListener(OnPracticeBackButtonClicked);
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
        /// 연습 모드 버튼 클릭 처리
        /// </summary>
        private void OnPracticeButtonClicked()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.OpenPracticeSelect();
            }
            else
            {
                Debug.LogWarning("[GameScreens] GameFlowManager 참조가 없습니다.");
            }
        }
        
        /// <summary>
        /// 연습 모드 뒤로가기 버튼 클릭 처리
        /// </summary>
        private void OnPracticeBackButtonClicked()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.ChangeState(GameState.MainMenu);
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
            
            if (practiceSelectPanel != null)
            {
                practiceSelectPanel.SetActive(false);
            }
            
            // 연습 모드 힌트는 별도로 관리 (HideAllScreens에서 숨기지 않음)
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
            if (practiceSelectPanel != null && practiceSelectPanel.activeSelf) return "PracticeSelect";
            return "None";
        }
        
        #region 연습 모드 UI
        
        /// <summary>
        /// 연습 모드 선택 화면 표시
        /// </summary>
        public void ShowPracticeSelectScreen()
        {
            HideAllScreens();
            
            // 연습 모드 힌트 숨기기
            ShowPracticeHint(false);
            
            // 패널이 없으면 동적 생성
            if (practiceSelectPanel == null)
            {
                CreatePracticeSelectPanel();
            }
            
            // 미니게임 버튼 생성
            PopulatePracticeButtons();
            
            if (practiceSelectPanel != null)
            {
                practiceSelectPanel.SetActive(true);
            }
            
            if (practiceSelectTitleText != null)
            {
                practiceSelectTitleText.text = "연습할 게임을 선택하세요";
            }
            
            Debug.Log("[GameScreens] 연습 모드 선택 화면 표시");
        }
        
        /// <summary>
        /// 연습 모드 선택 패널 동적 생성
        /// </summary>
        private void CreatePracticeSelectPanel()
        {
            // 패널 생성
            GameObject panelObj = new GameObject("PracticeSelectPanel");
            panelObj.transform.SetParent(transform, false);
            practiceSelectPanel = panelObj;
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;
            
            // 배경 이미지
            Image bgImage = panelObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            // 타이틀 텍스트
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -50);
            titleRect.sizeDelta = new Vector2(800, 80);
            
            practiceSelectTitleText = titleObj.AddComponent<TextMeshProUGUI>();
            practiceSelectTitleText.text = "연습할 게임을 선택하세요";
            practiceSelectTitleText.fontSize = 48;
            practiceSelectTitleText.alignment = TextAlignmentOptions.Center;
            practiceSelectTitleText.color = Color.white;
            
            // 그리드 컨테이너
            GameObject gridObj = new GameObject("GridContainer");
            gridObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridRect.anchorMax = new Vector2(0.5f, 0.5f);
            gridRect.pivot = new Vector2(0.5f, 0.5f);
            gridRect.anchoredPosition = new Vector2(0, 0);
            gridRect.sizeDelta = new Vector2(
                practiceGridColumns * practiceButtonSize.x + (practiceGridColumns - 1) * practiceGridSpacing.x,
                3 * practiceButtonSize.y + 2 * practiceGridSpacing.y
            );
            
            practiceGridContainer = gridRect;
            
            GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = practiceButtonSize;
            grid.spacing = practiceGridSpacing;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = practiceGridColumns;
            grid.childAlignment = TextAnchor.MiddleCenter;
            
            // 뒤로가기 버튼
            GameObject backBtnObj = new GameObject("BackButton");
            backBtnObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform backRect = backBtnObj.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0f);
            backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0, 50);
            backRect.sizeDelta = new Vector2(200, 60);
            
            Image backBtnImage = backBtnObj.AddComponent<Image>();
            backBtnImage.color = new Color(0.3f, 0.3f, 0.35f, 1f);
            
            practiceBackButton = backBtnObj.AddComponent<Button>();
            practiceBackButton.onClick.AddListener(OnPracticeBackButtonClicked);
            
            // 뒤로가기 버튼 텍스트
            GameObject backTextObj = new GameObject("Text");
            backTextObj.transform.SetParent(backBtnObj.transform, false);
            
            RectTransform backTextRect = backTextObj.AddComponent<RectTransform>();
            backTextRect.anchorMin = Vector2.zero;
            backTextRect.anchorMax = Vector2.one;
            backTextRect.sizeDelta = Vector2.zero;
            backTextRect.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI backText = backTextObj.AddComponent<TextMeshProUGUI>();
            backText.text = "뒤로 가기";
            backText.fontSize = 28;
            backText.alignment = TextAlignmentOptions.Center;
            backText.color = Color.white;
            
            Debug.Log("[GameScreens] 연습 모드 선택 패널 동적 생성 완료");
        }
        
        /// <summary>
        /// 연습 모드 버튼 채우기
        /// </summary>
        private void PopulatePracticeButtons()
        {
            // 기존 버튼 제거
            ClearPracticeButtons();
            
            if (practiceGridContainer == null || gameFlowManager == null || gameFlowManager.MicrogameManager == null)
            {
                Debug.LogWarning("[GameScreens] 연습 모드 버튼을 생성할 수 없습니다.");
                return;
            }
            
            MicrogameManager mgManager = gameFlowManager.MicrogameManager;
            int count = mgManager.MicrogamePrefabCount;
            
            for (int i = 0; i < count; i++)
            {
                CreatePracticeButton(i, mgManager);
            }
            
            Debug.Log($"[GameScreens] 연습 모드 버튼 {count}개 생성 완료");
        }
        
        /// <summary>
        /// 개별 연습 모드 버튼 생성
        /// </summary>
        private void CreatePracticeButton(int index, MicrogameManager mgManager)
        {
            // 버튼 오브젝트
            GameObject btnObj = new GameObject($"PracticeButton_{index}");
            btnObj.transform.SetParent(practiceGridContainer, false);
            
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = practiceButtonSize;
            
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.25f, 0.25f, 0.3f, 1f);
            
            Button btn = btnObj.AddComponent<Button>();
            int capturedIndex = index;
            btn.onClick.AddListener(() => OnPracticeGameSelected(capturedIndex));
            
            // 버튼 색상 설정
            ColorBlock colors = btn.colors;
            colors.normalColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            colors.highlightedColor = new Color(0.35f, 0.35f, 0.45f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.25f, 1f);
            colors.selectedColor = new Color(0.35f, 0.35f, 0.45f, 1f);
            btn.colors = colors;
            
            // 썸네일 이미지
            GameObject thumbnailObj = new GameObject("Thumbnail");
            thumbnailObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform thumbnailRect = thumbnailObj.AddComponent<RectTransform>();
            thumbnailRect.anchorMin = new Vector2(0.1f, 0.3f);
            thumbnailRect.anchorMax = new Vector2(0.9f, 0.95f);
            thumbnailRect.sizeDelta = Vector2.zero;
            thumbnailRect.anchoredPosition = Vector2.zero;
            
            Image thumbnailImage = thumbnailObj.AddComponent<Image>();
            thumbnailImage.color = new Color(0.4f, 0.4f, 0.45f, 1f);
            thumbnailImage.preserveAspect = true;
            
            // 썸네일 스프라이트 설정
            GameObject prefab = mgManager.GetMicrogamePrefab(index);
            if (prefab != null)
            {
                MicrogameBase microgameBase = prefab.GetComponent<MicrogameBase>();
                if (microgameBase == null)
                {
                    // 인스턴스에서 찾기
                    var instances = FindObjectsOfType<MicrogameBase>(true);
                    foreach (var instance in instances)
                    {
                        if (instance.gameObject.name.Contains(prefab.name))
                        {
                            microgameBase = instance;
                            break;
                        }
                    }
                }
                
                if (microgameBase != null && microgameBase.ThumbnailSprite != null)
                {
                    thumbnailImage.sprite = microgameBase.ThumbnailSprite;
                    thumbnailImage.color = Color.white;
                }
            }
            
            // 게임 이름 텍스트
            GameObject nameObj = new GameObject("GameName");
            nameObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0.25f);
            nameRect.sizeDelta = Vector2.zero;
            nameRect.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = mgManager.GetMicrogameName(index);
            nameText.fontSize = 20;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;
            nameText.enableWordWrapping = true;
            nameText.overflowMode = TextOverflowModes.Ellipsis;
            
            practiceButtons.Add(btnObj);
        }
        
        /// <summary>
        /// 연습 모드 버튼 제거
        /// </summary>
        private void ClearPracticeButtons()
        {
            foreach (GameObject btn in practiceButtons)
            {
                if (btn != null)
                {
                    Destroy(btn);
                }
            }
            practiceButtons.Clear();
        }
        
        /// <summary>
        /// 연습 모드 게임 선택 처리
        /// </summary>
        private void OnPracticeGameSelected(int index)
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.StartPracticeMode(index);
            }
            
            Debug.Log($"[GameScreens] 연습 모드 게임 선택: {index}");
        }
        
        /// <summary>
        /// 연습 모드 힌트 표시/숨기기
        /// </summary>
        /// <param name="show">표시 여부</param>
        public void ShowPracticeHint(bool show)
        {
            // 힌트 패널이 없으면 동적 생성
            if (practiceHintPanel == null && show)
            {
                CreatePracticeHintPanel();
            }
            
            if (practiceHintPanel != null)
            {
                practiceHintPanel.SetActive(show);
            }
            
            if (show && practiceHintText != null)
            {
                practiceHintText.text = practiceHintMessage;
            }
        }
        
        /// <summary>
        /// 연습 모드 힌트 패널 동적 생성
        /// </summary>
        private void CreatePracticeHintPanel()
        {
            // 힌트 패널 생성
            GameObject hintObj = new GameObject("PracticeHintPanel");
            hintObj.transform.SetParent(transform, false);
            practiceHintPanel = hintObj;
            
            RectTransform hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0f, 1f);
            hintRect.anchorMax = new Vector2(0f, 1f);
            hintRect.pivot = new Vector2(0f, 1f);
            hintRect.anchoredPosition = new Vector2(20, -20);
            hintRect.sizeDelta = new Vector2(350, 50);
            
            // 배경 이미지
            Image bgImage = hintObj.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.7f);
            
            // 힌트 텍스트
            GameObject textObj = new GameObject("HintText");
            textObj.transform.SetParent(hintObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-20, 0);
            textRect.anchoredPosition = Vector2.zero;
            
            practiceHintText = textObj.AddComponent<TextMeshProUGUI>();
            practiceHintText.text = practiceHintMessage;
            practiceHintText.fontSize = 24;
            practiceHintText.alignment = TextAlignmentOptions.MidlineLeft;
            practiceHintText.color = Color.white;
            
            Debug.Log("[GameScreens] 연습 모드 힌트 패널 동적 생성 완료");
        }
        
        #endregion
    }
}
