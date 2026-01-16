using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using Pansori.Microgames;

namespace Pansori.Microgames.Editor
{
    /// <summary>
    /// MicrogameManager 테스트 에디터
    /// Unity 에디터에서 미니게임을 쉽게 테스트할 수 있도록 도와줍니다.
    /// </summary>
    public class MicrogameManagerTester : EditorWindow
    {
        private MicrogameManager manager;
        private SerializedObject serializedManager;
        private SerializedProperty prefabsProperty;
        
        private int selectedDifficulty = 1;
        private float selectedSpeed = 1.0f;
        private int selectedPrefabIndex = 0;
        
        private Vector2 scrollPosition;
        private Vector2 logScrollPosition;
        
        private List<LogEntry> resultLogs = new List<LogEntry>();
        private const int MAX_LOG_ENTRIES = 20;
        
        private bool autoFindManager = true;
        
        [System.Serializable]
        private class LogEntry
        {
            public string message;
            public bool isSuccess;
            public string timestamp;
            
            public LogEntry(string msg, bool success)
            {
                message = msg;
                isSuccess = success;
                timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            }
        }
        
        [MenuItem("Tools/Microgames/Test Microgame Manager")]
        public static void ShowWindow()
        {
            GetWindow<MicrogameManagerTester>("Microgame Manager Tester");
        }
        
        private void OnEnable()
        {
            // 씬에서 자동으로 Manager 찾기
            if (autoFindManager)
            {
                FindManagerInScene();
            }
            
            // 에디터 업데이트 구독하여 실시간 상태 업데이트
            EditorApplication.update += OnEditorUpdate;
        }
        
        private void OnDisable()
        {
            // 이벤트 구독 해제
            if (manager != null)
            {
                manager.OnMicrogameResult -= OnMicrogameResult;
            }
            
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private void OnEditorUpdate()
        {
            // 매 프레임 UI 업데이트를 위해 Repaint 호출
            Repaint();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("MicrogameManager 테스트 도구", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Manager 선택 영역
            DrawManagerSelection();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            if (manager == null)
            {
                EditorGUILayout.HelpBox("MicrogameManager를 찾을 수 없습니다. 씬에 MicrogameManager 컴포넌트가 있는지 확인하세요.", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                return;
            }
            
            // 프리팹 목록 영역
            DrawPrefabList();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            // 설정 영역
            DrawSettings();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            // 제어 버튼 영역
            DrawControlButtons();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            // 상태 표시 영역
            DrawStatusDisplay();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            
            // 결과 로그 영역
            DrawResultLog();
            
            EditorGUILayout.EndScrollView();
        }
        
        /// <summary>
        /// Manager 선택 영역 그리기
        /// </summary>
        private void DrawManagerSelection()
        {
            EditorGUILayout.LabelField("Manager 선택", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            manager = (MicrogameManager)EditorGUILayout.ObjectField("MicrogameManager", manager, typeof(MicrogameManager), true);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateSerializedObject();
            }
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("씬에서 찾기"))
            {
                FindManagerInScene();
            }
            
            autoFindManager = EditorGUILayout.Toggle("자동 찾기", autoFindManager);
            EditorGUILayout.EndHorizontal();
            
            if (manager != null)
            {
                EditorGUILayout.HelpBox($"Manager 찾음: {manager.name}", MessageType.Info);
            }
        }
        
        /// <summary>
        /// 프리팹 목록 영역 그리기
        /// </summary>
        private void DrawPrefabList()
        {
            EditorGUILayout.LabelField("프리팹 목록", EditorStyles.boldLabel);
            
            if (serializedManager == null || prefabsProperty == null)
            {
                UpdateSerializedObject();
            }
            
            if (prefabsProperty == null)
            {
                EditorGUILayout.HelpBox("프리팹 배열을 찾을 수 없습니다.", MessageType.Warning);
                return;
            }
            
            int arraySize = prefabsProperty.arraySize;
            EditorGUILayout.LabelField($"프리팹 개수: {arraySize}");
            
            EditorGUILayout.Space();
            
            // 배열 크기 조정
            EditorGUI.BeginChangeCheck();
            int newSize = EditorGUILayout.IntField("배열 크기", arraySize);
            if (EditorGUI.EndChangeCheck() && newSize >= 0)
            {
                prefabsProperty.arraySize = newSize;
                serializedManager.ApplyModifiedProperties();
            }
            
            EditorGUILayout.Space();
            
            // 프리팹 목록 표시
            for (int i = 0; i < arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                SerializedProperty element = prefabsProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element, new GUIContent($"프리팹 {i}"), true);
                
                if (element.objectReferenceValue != null)
                {
                    GUI.enabled = !manager.IsMicrogameRunning;
                    if (GUILayout.Button("시작", GUILayout.Width(50)))
                    {
                        StartMicrogameAtIndex(i);
                    }
                    GUI.enabled = true;
                }
                else
                {
                    EditorGUILayout.HelpBox("null", MessageType.Warning);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            if (serializedManager != null)
            {
                serializedManager.ApplyModifiedProperties();
            }
        }
        
        /// <summary>
        /// 설정 영역 그리기
        /// </summary>
        private void DrawSettings()
        {
            EditorGUILayout.LabelField("게임 설정", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("난이도:", GUILayout.Width(60));
            selectedDifficulty = EditorGUILayout.IntSlider(selectedDifficulty, 1, 3);
            EditorGUILayout.LabelField(selectedDifficulty.ToString(), GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("배속:", GUILayout.Width(60));
            selectedSpeed = EditorGUILayout.Slider(selectedSpeed, 1.0f, 5.0f);
            EditorGUILayout.LabelField(selectedSpeed.ToString("F1"), GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// 제어 버튼 영역 그리기
        /// </summary>
        private void DrawControlButtons()
        {
            EditorGUILayout.LabelField("제어", EditorStyles.boldLabel);
            
            bool isRunning = manager.IsMicrogameRunning;
            
            EditorGUI.BeginDisabledGroup(isRunning);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("인덱스로 시작", GUILayout.Height(30)))
            {
                StartMicrogameAtIndex(selectedPrefabIndex);
            }
            
            if (GUILayout.Button("랜덤 시작", GUILayout.Height(30)))
            {
                StartRandomMicrogame();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(!isRunning);
            
            if (GUILayout.Button("강제 종료", GUILayout.Height(30)))
            {
                ForceEndMicrogame();
            }
            
            EditorGUI.EndDisabledGroup();
        }
        
        /// <summary>
        /// 상태 표시 영역 그리기
        /// </summary>
        private void DrawStatusDisplay()
        {
            EditorGUILayout.LabelField("현재 상태", EditorStyles.boldLabel);
            
            bool isRunning = manager.IsMicrogameRunning;
            
            GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel);
            statusStyle.fontSize = 14;
            
            if (isRunning)
            {
                statusStyle.normal.textColor = Color.green;
                EditorGUILayout.LabelField("상태: 실행 중", statusStyle);
                
                // 현재 실행 중인 미니게임 정보 표시
                if (manager != null)
                {
                    // 리플렉션을 사용하여 현재 인스턴스 정보 가져오기
                    var instanceField = typeof(MicrogameManager).GetField("currentMicrogameInstance", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (instanceField != null)
                    {
                        GameObject instance = instanceField.GetValue(manager) as GameObject;
                        if (instance != null)
                        {
                            EditorGUILayout.LabelField($"미니게임: {instance.name}");
                        }
                    }
                }
            }
            else
            {
                statusStyle.normal.textColor = Color.gray;
                EditorGUILayout.LabelField("상태: 대기 중", statusStyle);
            }
        }
        
        /// <summary>
        /// 결과 로그 영역 그리기
        /// </summary>
        private void DrawResultLog()
        {
            EditorGUILayout.LabelField("결과 로그", EditorStyles.boldLabel);
            
            if (resultLogs.Count == 0)
            {
                EditorGUILayout.HelpBox("로그가 없습니다.", MessageType.Info);
                return;
            }
            
            logScrollPosition = EditorGUILayout.BeginScrollView(logScrollPosition, GUILayout.Height(150));
            
            // 최신 로그부터 표시
            for (int i = resultLogs.Count - 1; i >= 0; i--)
            {
                LogEntry entry = resultLogs[i];
                
                GUIStyle logStyle = new GUIStyle(EditorStyles.label);
                logStyle.wordWrap = true;
                
                Color originalColor = GUI.color;
                GUI.color = entry.isSuccess ? Color.green : Color.red;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{entry.timestamp}]", GUILayout.Width(70));
                EditorGUILayout.LabelField(entry.message, logStyle);
                EditorGUILayout.EndHorizontal();
                
                GUI.color = originalColor;
            }
            
            EditorGUILayout.EndScrollView();
            
            if (GUILayout.Button("로그 지우기"))
            {
                resultLogs.Clear();
            }
        }
        
        /// <summary>
        /// 씬에서 Manager 찾기
        /// </summary>
        private void FindManagerInScene()
        {
            manager = FindObjectOfType<MicrogameManager>();
            
            if (manager != null)
            {
                UpdateSerializedObject();
                SubscribeToEvents();
                Debug.Log($"[MicrogameManagerTester] Manager 찾음: {manager.name}");
            }
            else
            {
                Debug.LogWarning("[MicrogameManagerTester] 씬에서 MicrogameManager를 찾을 수 없습니다.");
            }
        }
        
        /// <summary>
        /// SerializedObject 업데이트
        /// </summary>
        private void UpdateSerializedObject()
        {
            if (manager != null)
            {
                serializedManager = new SerializedObject(manager);
                prefabsProperty = serializedManager.FindProperty("microgamePrefabs");
            }
            else
            {
                serializedManager = null;
                prefabsProperty = null;
            }
        }
        
        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (manager != null)
            {
                manager.OnMicrogameResult += OnMicrogameResult;
            }
        }
        
        /// <summary>
        /// 인덱스로 미니게임 시작
        /// </summary>
        private void StartMicrogameAtIndex(int index)
        {
            if (manager == null)
            {
                Debug.LogError("[MicrogameManagerTester] Manager가 없습니다.");
                return;
            }
            
            if (prefabsProperty == null || index < 0 || index >= prefabsProperty.arraySize)
            {
                Debug.LogError($"[MicrogameManagerTester] 유효하지 않은 인덱스: {index}");
                return;
            }
            
            SerializedProperty element = prefabsProperty.GetArrayElementAtIndex(index);
            GameObject prefab = element.objectReferenceValue as GameObject;
            
            if (prefab == null)
            {
                Debug.LogError($"[MicrogameManagerTester] 인덱스 {index}의 프리팹이 null입니다.");
                return;
            }
            
            selectedPrefabIndex = index;
            manager.StartMicrogame(index, selectedDifficulty, selectedSpeed);
            AddLog($"미니게임 시작: {prefab.name} (난이도: {selectedDifficulty}, 배속: {selectedSpeed:F1})", true);
        }
        
        /// <summary>
        /// 랜덤 미니게임 시작
        /// </summary>
        private void StartRandomMicrogame()
        {
            if (manager == null)
            {
                Debug.LogError("[MicrogameManagerTester] Manager가 없습니다.");
                return;
            }
            
            manager.StartRandomMicrogame(selectedDifficulty, selectedSpeed);
            AddLog($"랜덤 미니게임 시작 (난이도: {selectedDifficulty}, 배속: {selectedSpeed:F1})", true);
        }
        
        /// <summary>
        /// 미니게임 강제 종료
        /// </summary>
        private void ForceEndMicrogame()
        {
            if (manager == null)
            {
                return;
            }
            
            manager.EndCurrentMicrogame();
            AddLog("미니게임 강제 종료", false);
        }
        
        /// <summary>
        /// 미니게임 결과 콜백
        /// </summary>
        private void OnMicrogameResult(bool success)
        {
            string message = success ? "성공" : "실패";
            AddLog($"미니게임 결과: {message}", success);
        }
        
        /// <summary>
        /// 로그 추가
        /// </summary>
        private void AddLog(string message, bool isSuccess)
        {
            resultLogs.Add(new LogEntry(message, isSuccess));
            
            // 최대 개수 초과 시 오래된 로그 제거
            while (resultLogs.Count > MAX_LOG_ENTRIES)
            {
                resultLogs.RemoveAt(0);
            }
        }
    }
}
