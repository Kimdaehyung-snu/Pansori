using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Pansori.Microgames.Editor
{
    /// <summary>
    /// GameFlow UI 자동 설정 마법사
    /// 메뉴에서 실행하여 GameFlow 시스템에 필요한 모든 UI 요소를 자동으로 생성합니다.
    /// </summary>
    public class GameFlowSetupWizard : EditorWindow
    {
        private MicrogameManager microgameManager;
        private Color backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        private Color accentColor = new Color(0.9f, 0.7f, 0.3f, 1f);
        
        [MenuItem("Tools/Microgame System/GameFlow 설정 마법사")]
        public static void ShowWindow()
        {
            GetWindow<GameFlowSetupWizard>("GameFlow 설정");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("GameFlow 설정 마법사", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "이 마법사는 판소리 게임 플로우에 필요한 모든 UI 요소를 자동으로 생성합니다.\n\n" +
                "생성되는 요소:\n" +
                "• GameFlowManager (게임 흐름 관리)\n" +
                "• GameScreens Canvas (메인/준비/승리/패배 화면)\n" +
                "• PansoriSceneUI Canvas (판소리 씬)",
                MessageType.Info);
            
            EditorGUILayout.Space(10);
            
            microgameManager = (MicrogameManager)EditorGUILayout.ObjectField(
                "MicrogameManager", microgameManager, typeof(MicrogameManager), true);
            
            backgroundColor = EditorGUILayout.ColorField("배경색", backgroundColor);
            accentColor = EditorGUILayout.ColorField("강조색", accentColor);
            
            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("GameFlow 시스템 생성", GUILayout.Height(40)))
            {
                CreateGameFlowSystem();
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "MicrogameManager를 지정하지 않으면 씬에서 자동으로 찾거나 새로 생성합니다.",
                MessageType.None);
        }
        
        private void CreateGameFlowSystem()
        {
            // MicrogameManager 찾기 또는 생성
            if (microgameManager == null)
            {
                microgameManager = FindObjectOfType<MicrogameManager>();
            }
            
            if (microgameManager == null)
            {
                GameObject managerObj = new GameObject("MicrogameManager");
                microgameManager = managerObj.AddComponent<MicrogameManager>();
                Undo.RegisterCreatedObjectUndo(managerObj, "Create MicrogameManager");
                Debug.Log("[GameFlowSetupWizard] MicrogameManager 생성됨");
            }
            
            // GameFlowManager 생성
            GameObject gameFlowObj = new GameObject("GameFlowManager");
            GameFlowManager gameFlowManager = gameFlowObj.AddComponent<GameFlowManager>();
            Undo.RegisterCreatedObjectUndo(gameFlowObj, "Create GameFlowManager");
            
            // GameScreens Canvas 생성
            GameObject gameScreensCanvas = CreateGameScreensCanvas(gameFlowManager);
            
            // PansoriSceneUI Canvas 생성
            GameObject pansoriCanvas = CreatePansoriSceneCanvas();
            
            // 참조 연결
            SerializedObject gameFlowSO = new SerializedObject(gameFlowManager);
            gameFlowSO.FindProperty("microgameManager").objectReferenceValue = microgameManager;
            gameFlowSO.FindProperty("pansoriSceneUI").objectReferenceValue = pansoriCanvas.GetComponent<PansoriSceneUI>();
            gameFlowSO.FindProperty("gameScreens").objectReferenceValue = gameScreensCanvas.GetComponent<GameScreens>();
            gameFlowSO.ApplyModifiedProperties();
            
            // GameScreens에 GameFlowManager 참조 연결
            SerializedObject gameScreensSO = new SerializedObject(gameScreensCanvas.GetComponent<GameScreens>());
            gameScreensSO.FindProperty("gameFlowManager").objectReferenceValue = gameFlowManager;
            gameScreensSO.ApplyModifiedProperties();
            
            // 선택
            Selection.activeGameObject = gameFlowObj;
            
            Debug.Log("[GameFlowSetupWizard] GameFlow 시스템 생성 완료!");
            EditorUtility.DisplayDialog("완료", "GameFlow 시스템이 성공적으로 생성되었습니다!", "확인");
        }
        
        private GameObject CreateGameScreensCanvas(GameFlowManager gameFlowManager)
        {
            // Canvas 생성
            GameObject canvasObj = new GameObject("GameScreensCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            GameScreens gameScreens = canvasObj.AddComponent<GameScreens>();
            
            // 메인 메뉴 패널
            GameObject mainMenuPanel = CreatePanel(canvasObj.transform, "MainMenuPanel", backgroundColor);
            GameObject titleText = CreateTextElement(mainMenuPanel.transform, "TitleText", "울려라! 판소리", 72, accentColor);
            SetRectTransform(titleText, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(800, 100));
            
            GameObject startButton = CreateButton(mainMenuPanel.transform, "StartButton", "시작", accentColor);
            SetRectTransform(startButton, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(300, 80));
            
            // 준비 패널
            GameObject readyPanel = CreatePanel(canvasObj.transform, "ReadyPanel", backgroundColor);
            GameObject readyText = CreateTextElement(readyPanel.transform, "ReadyText", "준비!", 120, accentColor);
            SetRectTransform(readyText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 200));
            
            // 승리 패널
            GameObject victoryPanel = CreatePanel(canvasObj.transform, "VictoryPanel", new Color(0.1f, 0.2f, 0.1f, 1f));
            GameObject victoryText = CreateTextElement(victoryPanel.transform, "VictoryText", "축하합니다!", 72, Color.green);
            SetRectTransform(victoryText, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(800, 100));
            
            GameObject victoryScoreText = CreateTextElement(victoryPanel.transform, "VictoryScoreText", "총 20회 승리!", 48, Color.white);
            SetRectTransform(victoryScoreText, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(600, 80));
            
            GameObject victoryRestartButton = CreateButton(victoryPanel.transform, "VictoryRestartButton", "다시 시작", accentColor);
            SetRectTransform(victoryRestartButton, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(300, 80));
            
            // 게임오버 패널
            GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0.2f, 0.1f, 0.1f, 1f));
            GameObject gameOverText = CreateTextElement(gameOverPanel.transform, "GameOverText", "게임 오버", 72, Color.red);
            SetRectTransform(gameOverText, new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(800, 100));
            
            GameObject gameOverScoreText = CreateTextElement(gameOverPanel.transform, "GameOverScoreText", "승리: 0회 / 패배: 4회", 48, Color.white);
            SetRectTransform(gameOverScoreText, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(600, 80));
            
            GameObject gameOverRestartButton = CreateButton(gameOverPanel.transform, "GameOverRestartButton", "다시 시작", accentColor);
            SetRectTransform(gameOverRestartButton, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(300, 80));
            
            // SerializedObject로 참조 연결
            SerializedObject so = new SerializedObject(gameScreens);
            so.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
            so.FindProperty("startButton").objectReferenceValue = startButton.GetComponent<Button>();
            so.FindProperty("titleText").objectReferenceValue = titleText.GetComponent<TMP_Text>();
            so.FindProperty("readyPanel").objectReferenceValue = readyPanel;
            so.FindProperty("readyText").objectReferenceValue = readyText.GetComponent<TMP_Text>();
            so.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
            so.FindProperty("victoryText").objectReferenceValue = victoryText.GetComponent<TMP_Text>();
            so.FindProperty("victoryScoreText").objectReferenceValue = victoryScoreText.GetComponent<TMP_Text>();
            so.FindProperty("victoryRestartButton").objectReferenceValue = victoryRestartButton.GetComponent<Button>();
            so.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
            so.FindProperty("gameOverText").objectReferenceValue = gameOverText.GetComponent<TMP_Text>();
            so.FindProperty("gameOverScoreText").objectReferenceValue = gameOverScoreText.GetComponent<TMP_Text>();
            so.FindProperty("gameOverRestartButton").objectReferenceValue = gameOverRestartButton.GetComponent<Button>();
            so.ApplyModifiedProperties();
            
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create GameScreensCanvas");
            
            return canvasObj;
        }
        
        private GameObject CreatePansoriSceneCanvas()
        {
            // Canvas 생성
            GameObject canvasObj = new GameObject("PansoriSceneCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            PansoriSceneUI pansoriUI = canvasObj.AddComponent<PansoriSceneUI>();
            
            // 판소리 씬 패널
            Color pansoriBackground = new Color(0.9f, 0.85f, 0.75f, 1f);
            GameObject pansoriPanel = CreatePanel(canvasObj.transform, "PansoriPanel", pansoriBackground);
            
            // 배경 이미지 (패널의 Image 컴포넌트 사용)
            Image backgroundImage = pansoriPanel.GetComponent<Image>();
            
            // 명령 텍스트
            GameObject commandText = CreateTextElement(pansoriPanel.transform, "CommandText", "XX해라!", 96, new Color(0.3f, 0.2f, 0.1f));
            SetRectTransform(commandText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 150));
            
            // 반응 텍스트
            GameObject reactionText = CreateTextElement(pansoriPanel.transform, "ReactionText", "얼쑤!", 120, new Color(0.2f, 0.6f, 0.2f));
            SetRectTransform(reactionText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 200));
            
            // SerializedObject로 참조 연결
            SerializedObject so = new SerializedObject(pansoriUI);
            so.FindProperty("pansoriPanel").objectReferenceValue = pansoriPanel;
            so.FindProperty("backgroundImage").objectReferenceValue = backgroundImage;
            so.FindProperty("commandText").objectReferenceValue = commandText.GetComponent<TMP_Text>();
            so.FindProperty("reactionText").objectReferenceValue = reactionText.GetComponent<TMP_Text>();
            so.ApplyModifiedProperties();
            
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create PansoriSceneCanvas");
            
            return canvasObj;
        }
        
        private GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image image = panel.AddComponent<Image>();
            image.color = color;
            
            return panel;
        }
        
        private GameObject CreateTextElement(Transform parent, string name, string text, int fontSize, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            
            textObj.AddComponent<RectTransform>();
            
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.color = color;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontStyle = FontStyles.Bold;
            
            return textObj;
        }
        
        private GameObject CreateButton(Transform parent, string name, string text, Color color)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            
            buttonObj.AddComponent<RectTransform>();
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = color;
            
            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.2f;
            colors.pressedColor = color * 0.8f;
            button.colors = colors;
            
            // 버튼 텍스트
            GameObject textObj = CreateTextElement(buttonObj.transform, "Text", text, 36, Color.white);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return buttonObj;
        }
        
        private void SetRectTransform(GameObject obj, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.anchoredPosition = anchoredPosition;
                rect.sizeDelta = sizeDelta;
            }
        }
    }
}
