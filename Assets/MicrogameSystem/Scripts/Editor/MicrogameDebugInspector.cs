using UnityEngine;
using UnityEditor;
using Pansori.Microgames;

namespace Pansori.Microgames.Editor
{
    /// <summary>
    /// 마이크로게임 실시간 디버그 인스펙터
    /// 플레이 모드에서 게임 상태를 실시간으로 모니터링하고 테스트할 수 있습니다.
    /// </summary>
    public class MicrogameDebugInspector : EditorWindow
    {
        private GameFlowManager flowManager;
        private MicrogameManager microgameManager;
        
        private Vector2 scrollPosition;
        private bool autoRefresh = true;
        private float refreshInterval = 0.1f;
        private double lastRefreshTime;
        
        // 디버그 설정
        private int debugWinCount = 10;
        private int debugLoseCount = 2;
        private float debugSpeed = 1.5f;
        private int debugDifficulty = 1;
        private int selectedMicrogameIndex = 0;
        
        [MenuItem("Tools/Microgames/Debug Inspector", false, 20)]
        public static void ShowWindow()
        {
            var window = GetWindow<MicrogameDebugInspector>("Debug Inspector");
            window.minSize = new Vector2(350, 500);
        }
        
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            FindManagers();
        }
        
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                FindManagers();
            }
        }
        
        private void FindManagers()
        {
            if (flowManager == null)
            {
                flowManager = Object.FindObjectOfType<GameFlowManager>();
            }
            if (microgameManager == null)
            {
                microgameManager = Object.FindObjectOfType<MicrogameManager>();
            }
        }
        
        private void Update()
        {
            if (autoRefresh && Application.isPlaying)
            {
                if (EditorApplication.timeSinceStartup - lastRefreshTime > refreshInterval)
                {
                    Repaint();
                    lastRefreshTime = EditorApplication.timeSinceStartup;
                }
            }
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 헤더
            GUILayout.Label("마이크로게임 디버그 인스펙터", EditorStyles.boldLabel);
            
            // 상태 표시
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("플레이 모드가 아닙니다. 플레이 모드에서 실행해주세요.", MessageType.Warning);
            }
            else
            {
                FindManagers();
            }
            
            EditorGUILayout.Space();
            
            // 자동 새로고침 설정
            EditorGUILayout.BeginHorizontal();
            autoRefresh = EditorGUILayout.Toggle("자동 새로고침", autoRefresh);
            if (!autoRefresh && GUILayout.Button("새로고침", GUILayout.Width(80)))
            {
                FindManagers();
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // 매니저 참조
            EditorGUILayout.LabelField("매니저 참조", EditorStyles.boldLabel);
            flowManager = (GameFlowManager)EditorGUILayout.ObjectField("GameFlowManager", flowManager, typeof(GameFlowManager), true);
            microgameManager = (MicrogameManager)EditorGUILayout.ObjectField("MicrogameManager", microgameManager, typeof(MicrogameManager), true);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // 현재 상태 표시
            DrawCurrentState();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // 디버그 컨트롤
            DrawDebugControls();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // 통계
            DrawStatistics();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawCurrentState()
        {
            EditorGUILayout.LabelField("현재 게임 상태", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            
            if (flowManager != null)
            {
                // 게임 상태
                EditorGUILayout.EnumPopup("게임 상태", flowManager.CurrentState);
                
                // 승리/패배 횟수
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.IntField("승리 횟수", flowManager.WinCount);
                EditorGUILayout.IntField("패배 횟수", flowManager.LoseCount);
                EditorGUILayout.EndHorizontal();
                
                // 현재 속도/난이도
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.FloatField("현재 속도", flowManager.CurrentSpeed);
                EditorGUILayout.IntField("현재 난이도", flowManager.CurrentDifficulty);
                EditorGUILayout.EndHorizontal();
                
                // 스테이지
                EditorGUILayout.IntField("현재 스테이지", flowManager.CurrentStage);
            }
            else
            {
                EditorGUILayout.LabelField("GameFlowManager를 찾을 수 없습니다.", EditorStyles.helpBox);
            }
            
            EditorGUILayout.Space();
            
            if (microgameManager != null)
            {
                // 미니게임 상태
                EditorGUILayout.Toggle("미니게임 실행 중", microgameManager.IsMicrogameRunning);
                EditorGUILayout.TextField("현재 미니게임", microgameManager.CurrentMicrogameName);
                
                // 목숨
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.IntField("현재 목숨", microgameManager.CurrentLives);
                EditorGUILayout.IntField("최대 목숨", microgameManager.MaxLives);
                EditorGUILayout.EndHorizontal();
                
                // 등록된 프리팹 수
                EditorGUILayout.IntField("등록된 미니게임", microgameManager.MicrogamePrefabCount);
            }
            else
            {
                EditorGUILayout.LabelField("MicrogameManager를 찾을 수 없습니다.", EditorStyles.helpBox);
            }
            
            EditorGUI.EndDisabledGroup();
        }
        
        private void DrawDebugControls()
        {
            EditorGUILayout.LabelField("디버그 컨트롤", EditorStyles.boldLabel);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("플레이 모드에서만 사용 가능합니다.", MessageType.Info);
                return;
            }
            
            // 상태 강제 변경
            EditorGUILayout.LabelField("상태 변경", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("메인 메뉴"))
            {
                if (flowManager != null) flowManager.DebugSetState(GameState.MainMenu);
            }
            if (GUILayout.Button("게임 시작"))
            {
                if (flowManager != null) flowManager.StartGame();
            }
            if (GUILayout.Button("승리"))
            {
                if (flowManager != null) flowManager.DebugSetState(GameState.Victory);
            }
            if (GUILayout.Button("게임오버"))
            {
                if (flowManager != null) flowManager.DebugSetState(GameState.GameOver);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 미니게임 결과 강제
            EditorGUILayout.LabelField("미니게임 결과 강제", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("강제 성공", GUILayout.Height(30)))
            {
                if (microgameManager != null) microgameManager.DebugForceSuccess();
            }
            GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
            if (GUILayout.Button("강제 실패", GUILayout.Height(30)))
            {
                if (microgameManager != null) microgameManager.DebugForceFailure();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 값 조정
            EditorGUILayout.LabelField("값 조정", EditorStyles.miniBoldLabel);
            
            EditorGUILayout.BeginHorizontal();
            debugWinCount = EditorGUILayout.IntField("승리 횟수", debugWinCount);
            if (GUILayout.Button("적용", GUILayout.Width(60)))
            {
                if (flowManager != null) flowManager.DebugSetWinCount(debugWinCount);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            debugLoseCount = EditorGUILayout.IntField("패배 횟수", debugLoseCount);
            if (GUILayout.Button("적용", GUILayout.Width(60)))
            {
                if (flowManager != null) flowManager.DebugSetLoseCount(debugLoseCount);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            debugSpeed = EditorGUILayout.Slider("속도", debugSpeed, 1f, 3f);
            if (GUILayout.Button("적용", GUILayout.Width(60)))
            {
                if (flowManager != null) flowManager.DebugSetSpeed(debugSpeed);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 특정 미니게임 시작
            EditorGUILayout.LabelField("미니게임 직접 시작", EditorStyles.miniBoldLabel);
            
            if (microgameManager != null && microgameManager.MicrogamePrefabCount > 0)
            {
                // 미니게임 선택 드롭다운
                string[] gameNames = new string[microgameManager.MicrogamePrefabCount];
                for (int i = 0; i < microgameManager.MicrogamePrefabCount; i++)
                {
                    gameNames[i] = i + ": " + microgameManager.GetMicrogameName(i);
                }
                
                selectedMicrogameIndex = EditorGUILayout.Popup("미니게임 선택", selectedMicrogameIndex, gameNames);
                
                EditorGUILayout.BeginHorizontal();
                debugDifficulty = EditorGUILayout.IntSlider("난이도", debugDifficulty, 1, 3);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("선택한 게임 시작"))
                {
                    microgameManager.StartMicrogame(selectedMicrogameIndex, debugDifficulty, debugSpeed);
                }
                if (GUILayout.Button("랜덤 게임 시작"))
                {
                    microgameManager.StartRandomMicrogame(debugDifficulty, debugSpeed);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("등록된 미니게임이 없습니다.", MessageType.Warning);
            }
        }
        
        private void DrawStatistics()
        {
            EditorGUILayout.LabelField("통계", EditorStyles.boldLabel);
            
            if (microgameManager == null)
            {
                EditorGUILayout.HelpBox("MicrogameManager를 찾을 수 없습니다.", MessageType.Warning);
                return;
            }
            
            EditorGUI.BeginDisabledGroup(true);
            
            // 전체 통계
            EditorGUILayout.IntField("총 플레이 횟수", microgameManager.TotalPlayCount);
            EditorGUILayout.IntField("총 성공 횟수", microgameManager.TotalSuccessCount);
            EditorGUILayout.IntField("총 실패 횟수", microgameManager.TotalFailureCount);
            
            if (microgameManager.TotalPlayCount > 0)
            {
                float successRate = (float)microgameManager.TotalSuccessCount / microgameManager.TotalPlayCount * 100f;
                EditorGUILayout.TextField("성공률", successRate.ToString("F1") + "%");
            }
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
            
            // 게임별 통계
            if (microgameManager.MicrogamePrefabCount > 0)
            {
                EditorGUILayout.LabelField("게임별 통계", EditorStyles.miniBoldLabel);
                
                for (int i = 0; i < microgameManager.MicrogamePrefabCount; i++)
                {
                    int playCount = microgameManager.GetPlayCount(i);
                    if (playCount > 0)
                    {
                        int successCount = microgameManager.GetSuccessCount(i);
                        float rate = microgameManager.GetSuccessRate(i) * 100f;
                        
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(microgameManager.GetMicrogameName(i), GUILayout.Width(150));
                        EditorGUILayout.LabelField(successCount + "/" + playCount + " (" + rate.ToString("F0") + "%)");
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            
            EditorGUILayout.Space();
            
            // 통계 초기화
            if (GUILayout.Button("통계 초기화"))
            {
                if (microgameManager != null)
                {
                    microgameManager.ResetStatistics();
                }
            }
        }
    }
}