using UnityEngine;
using UnityEditor;
using System.IO;
using Pansori.Microgames;

namespace Pansori.Microgames.Editor
{
    /// <summary>
    /// 마이크로게임 씬 원클릭 자동 세팅 마법사
    /// MicrogameManager, GameFlowManager, 모든 UI를 자동으로 생성합니다.
    /// </summary>
    public class MicrogameSceneSetupWizard : EditorWindow
    {
        private MicrogameSystemSettings settings;
        private bool createNewSettings = true;
        private string settingsAssetName = "DefaultSettings";
        
        private Color backgroundColor = new Color(0.9f, 0.85f, 0.75f);
        private Color accentColor = new Color(0.2f, 0.6f, 0.8f);
        
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/Microgames/Scene Setup Wizard", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<MicrogameSceneSetupWizard>("Scene Setup Wizard");
            window.minSize = new Vector2(400, 500);
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            GUILayout.Label("마이크로게임 씬 자동 세팅", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "이 마법사는 마이크로게임 시스템에 필요한 모든 컴포넌트와 UI를 자동으로 생성합니다.\n" +
                "- MicrogameManager\n" +
                "- GameFlowManager\n" +
                "- 메인/준비/승리/패배 화면\n" +
                "- 판소리 씬 UI\n" +
                "- 시스템 설정 ScriptableObject",
                MessageType.Info);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("시스템 설정", EditorStyles.boldLabel);
            
            createNewSettings = EditorGUILayout.Toggle("새 설정 파일 생성", createNewSettings);
            
            if (createNewSettings)
            {
                settingsAssetName = EditorGUILayout.TextField("설정 파일 이름", settingsAssetName);
            }
            else
            {
                settings = (MicrogameSystemSettings)EditorGUILayout.ObjectField(
                    "기존 설정 사용",
                    settings,
                    typeof(MicrogameSystemSettings),
                    false);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("UI 색상 설정", EditorStyles.boldLabel);
            backgroundColor = EditorGUILayout.ColorField("배경색", backgroundColor);
            accentColor = EditorGUILayout.ColorField("강조색", accentColor);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("원클릭 씬 세팅 실행", GUILayout.Height(50)))
            {
                ExecuteSetup();
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("개별 컴포넌트 생성", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("설정 파일만 생성"))
            {
                CreateSettingsAsset();
            }
            if (GUILayout.Button("매니저만 생성"))
            {
                CreateManagers();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("GameScreens UI 생성"))
            {
                CreateGameScreensUI();
            }
            if (GUILayout.Button("PansoriScene UI 생성"))
            {
                CreatePansoriSceneUI();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void ExecuteSetup()
        {
            try
            {
                if (createNewSettings)
                {
                    settings = CreateSettingsAsset();
                }
                
                if (settings == null && !createNewSettings)
                {
                    EditorUtility.DisplayDialog("오류", "설정 파일을 선택하거나 새로 생성해주세요.", "확인");
                    return;
                }
                
                var managers = CreateManagers();
                var gameScreens = CreateGameScreensUI();
                var pansoriUI = CreatePansoriSceneUI();
                
                ConnectReferences(managers.flowManager, managers.microgameManager, gameScreens, pansoriUI);
                
                if (settings != null)
                {
                    ApplySettings(managers.flowManager, managers.microgameManager, settings);
                }
                
                EditorUtility.DisplayDialog("완료", 
                    "마이크로게임 씬 세팅이 완료되었습니다!\n\n" +
                    "다음 단계:\n" +
                    "1. MicrogameManager에 마이크로게임 프리팹 추가\n" +
                    "2. Tools > Microgames > Scan Prefabs로 자동 등록\n" +
                    "3. 플레이 모드에서 테스트",
                    "확인");
                
                Selection.activeGameObject = managers.flowManager.gameObject;
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("오류", "씬 세팅 중 오류가 발생했습니다:\n" + e.Message, "확인");
                Debug.LogError("[MicrogameSceneSetupWizard] 오류: " + e);
            }
        }
        
        private MicrogameSystemSettings CreateSettingsAsset()
        {
            string folderPath = "Assets/MicrogameSystem/Settings";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }
            
            string assetPath = folderPath + "/" + settingsAssetName + ".asset";
            
            var existing = AssetDatabase.LoadAssetAtPath<MicrogameSystemSettings>(assetPath);
            if (existing != null)
            {
                return existing;
            }
            
            var newSettings = ScriptableObject.CreateInstance<MicrogameSystemSettings>();
            AssetDatabase.CreateAsset(newSettings, assetPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log("[MicrogameSceneSetupWizard] 설정 파일 생성: " + assetPath);
            
            return newSettings;
        }
        
        private (GameFlowManager flowManager, MicrogameManager microgameManager) CreateManagers()
        {
            var existingFlow = Object.FindObjectOfType<GameFlowManager>();
            var existingMicrogame = Object.FindObjectOfType<MicrogameManager>();
            
            if (existingFlow != null || existingMicrogame != null)
            {
                if (!EditorUtility.DisplayDialog("확인", 
                    "이미 매니저가 존재합니다. 새로 생성하시겠습니까?\n(기존 매니저는 유지됩니다)",
                    "새로 생성", "기존 사용"))
                {
                    return (existingFlow, existingMicrogame);
                }
            }
            
            GameObject flowObj = new GameObject("GameFlowManager");
            var flowManager = flowObj.AddComponent<GameFlowManager>();
            Undo.RegisterCreatedObjectUndo(flowObj, "Create GameFlowManager");
            
            GameObject microgameObj = new GameObject("MicrogameManager");
            var microgameManager = microgameObj.AddComponent<MicrogameManager>();
            Undo.RegisterCreatedObjectUndo(microgameObj, "Create MicrogameManager");
            
            Debug.Log("[MicrogameSceneSetupWizard] 매니저 생성 완료");
            
            return (flowManager, microgameManager);
        }
        
        private GameScreens CreateGameScreensUI()
        {
            GameObject canvasObj = new GameObject("GameScreensCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            var gameScreens = canvasObj.AddComponent<GameScreens>();
            
            var mainMenuPanel = CreatePanel(canvasObj.transform, "MainMenuPanel", backgroundColor);
            var titleText = CreateText(mainMenuPanel.transform, "TitleText", "울려라! 판소리", 72, accentColor);
            SetRectPosition(titleText, new Vector2(0, 100));
            
            var startButton = CreateButton(mainMenuPanel.transform, "StartButton", "시작", accentColor);
            SetRectPosition(startButton, new Vector2(0, -50));
            
            var readyPanel = CreatePanel(canvasObj.transform, "ReadyPanel", backgroundColor);
            var readyText = CreateText(readyPanel.transform, "ReadyText", "준비!", 96, accentColor);
            readyPanel.SetActive(false);
            
            var victoryPanel = CreatePanel(canvasObj.transform, "VictoryPanel", new Color(0.7f, 1f, 0.7f));
            var victoryText = CreateText(victoryPanel.transform, "VictoryText", "축하합니다!", 72, new Color(0.2f, 0.6f, 0.2f));
            SetRectPosition(victoryText, new Vector2(0, 100));
            
            var victoryScoreText = CreateText(victoryPanel.transform, "VictoryScoreText", "총 20회 승리!", 36, Color.black);
            
            var victoryRestartButton = CreateButton(victoryPanel.transform, "VictoryRestartButton", "다시 시작", accentColor);
            SetRectPosition(victoryRestartButton, new Vector2(0, -100));
            victoryPanel.SetActive(false);
            
            var gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(1f, 0.7f, 0.7f));
            var gameOverText = CreateText(gameOverPanel.transform, "GameOverText", "게임 오버", 72, new Color(0.8f, 0.2f, 0.2f));
            SetRectPosition(gameOverText, new Vector2(0, 100));
            
            var gameOverScoreText = CreateText(gameOverPanel.transform, "GameOverScoreText", "승리: 0회 / 패배: 4회", 36, Color.black);
            
            var gameOverRestartButton = CreateButton(gameOverPanel.transform, "GameOverRestartButton", "다시 시작", accentColor);
            SetRectPosition(gameOverRestartButton, new Vector2(0, -100));
            gameOverPanel.SetActive(false);
            
            SerializedObject serializedScreens = new SerializedObject(gameScreens);
            serializedScreens.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
            serializedScreens.FindProperty("startButton").objectReferenceValue = startButton.GetComponent<UnityEngine.UI.Button>();
            serializedScreens.FindProperty("titleText").objectReferenceValue = titleText.GetComponent<TMPro.TMP_Text>();
            serializedScreens.FindProperty("readyPanel").objectReferenceValue = readyPanel;
            serializedScreens.FindProperty("readyText").objectReferenceValue = readyText.GetComponent<TMPro.TMP_Text>();
            serializedScreens.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
            serializedScreens.FindProperty("victoryText").objectReferenceValue = victoryText.GetComponent<TMPro.TMP_Text>();
            serializedScreens.FindProperty("victoryScoreText").objectReferenceValue = victoryScoreText.GetComponent<TMPro.TMP_Text>();
            serializedScreens.FindProperty("victoryRestartButton").objectReferenceValue = victoryRestartButton.GetComponent<UnityEngine.UI.Button>();
            serializedScreens.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
            serializedScreens.FindProperty("gameOverText").objectReferenceValue = gameOverText.GetComponent<TMPro.TMP_Text>();
            serializedScreens.FindProperty("gameOverScoreText").objectReferenceValue = gameOverScoreText.GetComponent<TMPro.TMP_Text>();
            serializedScreens.FindProperty("gameOverRestartButton").objectReferenceValue = gameOverRestartButton.GetComponent<UnityEngine.UI.Button>();
            serializedScreens.ApplyModifiedProperties();
            
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create GameScreens UI");
            
            Debug.Log("[MicrogameSceneSetupWizard] GameScreens UI 생성 완료");
            
            return gameScreens;
        }
        
        private PansoriSceneUI CreatePansoriSceneUI()
        {
            GameObject canvasObj = new GameObject("PansoriSceneCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            
            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            var pansoriUI = canvasObj.AddComponent<PansoriSceneUI>();
            
            var pansoriPanel = CreatePanel(canvasObj.transform, "PansoriPanel", backgroundColor);
            
            var commandText = CreateText(pansoriPanel.transform, "CommandText", "점프해라!", 72, accentColor);
            commandText.SetActive(false);
            
            var reactionText = CreateText(pansoriPanel.transform, "ReactionText", "얼쑤!", 96, new Color(0.2f, 0.6f, 0.2f));
            reactionText.SetActive(false);
            
            var livesText = CreateText(pansoriPanel.transform, "LivesText", "목숨: 4", 36, Color.black);
            var livesRect = livesText.GetComponent<RectTransform>();
            livesRect.anchorMin = new Vector2(0, 1);
            livesRect.anchorMax = new Vector2(0, 1);
            livesRect.pivot = new Vector2(0, 1);
            livesRect.anchoredPosition = new Vector2(50, -50);
            livesText.SetActive(false);
            
            var stageText = CreateText(pansoriPanel.transform, "StageText", "스테이지: 1", 36, Color.black);
            var stageRect = stageText.GetComponent<RectTransform>();
            stageRect.anchorMin = new Vector2(1, 1);
            stageRect.anchorMax = new Vector2(1, 1);
            stageRect.pivot = new Vector2(1, 1);
            stageRect.anchoredPosition = new Vector2(-50, -50);
            stageText.SetActive(false);
            
            var livesContainer = new GameObject("LivesContainer");
            livesContainer.transform.SetParent(pansoriPanel.transform, false);
            var containerRect = livesContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 1f);
            containerRect.anchorMax = new Vector2(0.5f, 1f);
            containerRect.pivot = new Vector2(0.5f, 1f);
            containerRect.anchoredPosition = new Vector2(0, -50);
            containerRect.sizeDelta = new Vector2(500, 100);
            
            pansoriPanel.SetActive(false);
            
            SerializedObject serializedPansori = new SerializedObject(pansoriUI);
            serializedPansori.FindProperty("pansoriPanel").objectReferenceValue = pansoriPanel;
            serializedPansori.FindProperty("backgroundImage").objectReferenceValue = pansoriPanel.GetComponent<UnityEngine.UI.Image>();
            serializedPansori.FindProperty("commandText").objectReferenceValue = commandText.GetComponent<TMPro.TMP_Text>();
            serializedPansori.FindProperty("reactionText").objectReferenceValue = reactionText.GetComponent<TMPro.TMP_Text>();
            serializedPansori.FindProperty("livesText").objectReferenceValue = livesText.GetComponent<TMPro.TMP_Text>();
            serializedPansori.FindProperty("stageText").objectReferenceValue = stageText.GetComponent<TMPro.TMP_Text>();
            serializedPansori.FindProperty("livesContainer").objectReferenceValue = livesContainer.transform;
            serializedPansori.FindProperty("normalBackgroundColor").colorValue = backgroundColor;
            serializedPansori.FindProperty("successBackgroundColor").colorValue = new Color(0.7f, 1f, 0.7f);
            serializedPansori.FindProperty("failureBackgroundColor").colorValue = new Color(1f, 0.7f, 0.7f);
            serializedPansori.ApplyModifiedProperties();
            
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create PansoriScene UI");
            
            Debug.Log("[MicrogameSceneSetupWizard] PansoriScene UI 생성 완료");
            
            return pansoriUI;
        }
        
        private void ConnectReferences(GameFlowManager flowManager, MicrogameManager microgameManager,
            GameScreens gameScreens, PansoriSceneUI pansoriUI)
        {
            if (flowManager == null) return;
            
            SerializedObject serializedFlow = new SerializedObject(flowManager);
            serializedFlow.FindProperty("microgameManager").objectReferenceValue = microgameManager;
            serializedFlow.FindProperty("gameScreens").objectReferenceValue = gameScreens;
            serializedFlow.FindProperty("pansoriSceneUI").objectReferenceValue = pansoriUI;
            serializedFlow.ApplyModifiedProperties();
            
            if (gameScreens != null)
            {
                SerializedObject serializedScreens = new SerializedObject(gameScreens);
                serializedScreens.FindProperty("gameFlowManager").objectReferenceValue = flowManager;
                serializedScreens.ApplyModifiedProperties();
            }
            
            Debug.Log("[MicrogameSceneSetupWizard] 참조 연결 완료");
        }
        
        private void ApplySettings(GameFlowManager flowManager, MicrogameManager microgameManager, MicrogameSystemSettings settingsRef)
        {
            if (flowManager != null)
            {
                SerializedObject serializedFlow = new SerializedObject(flowManager);
                serializedFlow.FindProperty("settings").objectReferenceValue = settingsRef;
                serializedFlow.ApplyModifiedProperties();
            }
            
            if (microgameManager != null)
            {
                SerializedObject serializedMicrogame = new SerializedObject(microgameManager);
                serializedMicrogame.FindProperty("settings").objectReferenceValue = settingsRef;
                serializedMicrogame.ApplyModifiedProperties();
            }
            
            Debug.Log("[MicrogameSceneSetupWizard] 설정 적용 완료");
        }
        
        private GameObject CreatePanel(Transform parent, string panelName, Color color)
        {
            GameObject panel = new GameObject(panelName);
            panel.transform.SetParent(parent, false);
            
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            var image = panel.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            
            return panel;
        }
        
        private GameObject CreateText(Transform parent, string textName, string text, int fontSize, Color color)
        {
            GameObject textObj = new GameObject(textName);
            textObj.transform.SetParent(parent, false);
            
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(800, 200);
            
            var tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            
            return textObj;
        }
        
        private GameObject CreateButton(Transform parent, string buttonName, string text, Color color)
        {
            GameObject buttonObj = new GameObject(buttonName);
            buttonObj.transform.SetParent(parent, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300, 80);
            
            var image = buttonObj.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            
            var button = buttonObj.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            
            var textObj = CreateText(buttonObj.transform, "Text", text, 36, Color.white);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            return buttonObj;
        }
        
        private void SetRectPosition(GameObject obj, Vector2 position)
        {
            var rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = position;
            }
        }
    }
}