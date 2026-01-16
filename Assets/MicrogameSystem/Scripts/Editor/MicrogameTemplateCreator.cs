using UnityEngine;
using UnityEditor;
using System.IO;
using Pansori.Microgames;

namespace Pansori.Microgames.Editor
{
    /// <summary>
    /// 미니게임 템플릿 생성 마법사
    /// 새 미니게임을 쉽게 생성할 수 있도록 도와줍니다.
    /// </summary>
    public class MicrogameTemplateCreator : EditorWindow
    {
        private string gameName = "MG_NewGame_01";
        private string gameDescription = "새 미니게임";
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/Microgames/Create New Microgame")]
        public static void ShowWindow()
        {
            GetWindow<MicrogameTemplateCreator>("Create Microgame");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("미니게임 생성 마법사", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 미니게임 이름 입력
            EditorGUILayout.LabelField("미니게임 이름:", EditorStyles.boldLabel);
            gameName = EditorGUILayout.TextField("이름 (예: MG_Jump_01)", gameName);
            EditorGUILayout.HelpBox("이름은 MG_로 시작하는 것을 권장합니다.", MessageType.Info);
            
            EditorGUILayout.Space();
            
            // 설명 입력
            EditorGUILayout.LabelField("설명 (선택사항):", EditorStyles.boldLabel);
            gameDescription = EditorGUILayout.TextArea(gameDescription, GUILayout.Height(60));
            
            EditorGUILayout.Space();
            
            // 생성할 항목 체크박스
            EditorGUILayout.LabelField("생성할 항목:", EditorStyles.boldLabel);
            bool createScripts = EditorGUILayout.Toggle("스크립트 파일", true);
            bool createPrefab = EditorGUILayout.Toggle("프리팹", true);
            bool createFolders = EditorGUILayout.Toggle("폴더 구조 (Scripts, Prefabs, Arts, Audios)", true);
            
            EditorGUILayout.Space();
            
            // 경로 표시
            string targetPath = Path.Combine("Assets", "MicrogameSystem", "Games", gameName);
            EditorGUILayout.LabelField("생성 경로:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(targetPath, EditorStyles.wordWrappedLabel);
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            // 생성 버튼
            if (GUILayout.Button("미니게임 생성", GUILayout.Height(40)))
            {
                if (string.IsNullOrEmpty(gameName))
                {
                    EditorUtility.DisplayDialog("오류", "미니게임 이름을 입력해주세요.", "확인");
                    return;
                }
                
                CreateMicrogame(gameName, gameDescription, createScripts, createPrefab, createFolders);
            }
        }
        
        /// <summary>
        /// 미니게임 생성
        /// </summary>
        private void CreateMicrogame(string name, string description, bool createScripts, bool createPrefab, bool createFolders)
        {
            string basePath = Path.Combine("Assets", "MicrogameSystem", "Games", name);
            
            try
            {
                // 폴더 구조 생성
                if (createFolders)
                {
                    CreateFolderStructure(basePath);
                }
                
                // 스크립트 생성
                if (createScripts)
                {
                    CreateScripts(basePath, name, description);
                }
                
                // 프리팹 생성
                if (createPrefab)
                {
                    CreatePrefab(basePath, name);
                }
                
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("완료", 
                    $"미니게임 '{name}'이(가) 성공적으로 생성되었습니다!\n\n경로: {basePath}", 
                    "확인");
                
                // 생성된 폴더 선택
                Object folder = AssetDatabase.LoadAssetAtPath<Object>(basePath);
                if (folder != null)
                {
                    Selection.activeObject = folder;
                    EditorGUIUtility.PingObject(folder);
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("오류", 
                    $"미니게임 생성 중 오류가 발생했습니다:\n\n{e.Message}", 
                    "확인");
                Debug.LogError($"[MicrogameTemplateCreator] 오류: {e}");
            }
        }
        
        /// <summary>
        /// 폴더 구조 생성
        /// </summary>
        private void CreateFolderStructure(string basePath)
        {
            string[] folders = { "Scripts", "Prefabs", "Arts", "Audios" };
            
            foreach (string folder in folders)
            {
                string folderPath = Path.Combine(basePath, folder);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    AssetDatabase.ImportAsset(folderPath);
                }
            }
        }
        
        /// <summary>
        /// 스크립트 생성
        /// </summary>
        private void CreateScripts(string basePath, string name, string description)
        {
            string scriptsPath = Path.Combine(basePath, "Scripts");
            
            // 매니저 스크립트 생성
            string managerScript = GenerateManagerScript(name, description);
            string managerPath = Path.Combine(scriptsPath, $"{name}_Manager.cs");
            File.WriteAllText(managerPath, managerScript);
            
            AssetDatabase.ImportAsset(managerPath);
        }
        
        /// <summary>
        /// 매니저 스크립트 생성
        /// </summary>
        private string GenerateManagerScript(string name, string description)
        {
            return $@"using UnityEngine;
using Pansori.Microgames;

namespace Pansori.Microgames.Games
{{
    /// <summary>
    /// {description}
    /// 
    /// TODO: 게임 설명을 여기에 작성하세요.
    /// </summary>
    public class {name}_Manager : MicrogameBase
    {{
        [Header(""게임 오브젝트"")]
        // TODO: 게임 오브젝트 참조를 추가하세요
        
        [Header(""게임 설정"")]
        // TODO: 게임 설정 변수를 추가하세요
        
        [Header(""헬퍼 컴포넌트"")]
        [SerializeField] private MicrogameTimer timer;
        [SerializeField] private MicrogameInputHandler inputHandler;
        [SerializeField] private MicrogameUILayer uiLayer;
        
        protected override void Awake()
        {{
            base.Awake();
            
            // TODO: 초기화 로직을 추가하세요
        }}
        
        public override void OnGameStart(int difficulty, float speed)
        {{
            base.OnGameStart(difficulty, speed);
            
            // TODO: 게임 시작 로직을 추가하세요
            
            // 타이머 시작 예시
            if (timer != null)
            {{
                timer.StartTimer(5f, speed);
                timer.OnTimerEnd += OnTimeUp;
            }}
            
            // 입력 핸들러 이벤트 구독 예시
            if (inputHandler != null)
            {{
                inputHandler.OnKeyPressed += HandleKeyPress;
            }}
        }}
        
        private void HandleKeyPress(KeyCode key)
        {{
            // TODO: 키 입력 처리 로직을 추가하세요
        }}
        
        private void OnTimeUp()
        {{
            // TODO: 시간 초과 처리 로직을 추가하세요
            ReportResult(false); // 또는 true
        }}
        
        private void OnSuccess()
        {{
            ReportResult(true);
        }}
        
        private void OnFailure()
        {{
            ReportResult(false);
        }}
        
        protected override void ResetGameState()
        {{
            // TODO: 모든 오브젝트를 초기 상태로 리셋하는 로직을 추가하세요
            
            // 타이머 중지
            if (timer != null)
            {{
                timer.Stop();
                timer.OnTimerEnd -= OnTimeUp;
            }}
            
            // 입력 핸들러 이벤트 구독 해제
            if (inputHandler != null)
            {{
                inputHandler.OnKeyPressed -= HandleKeyPress;
            }}
        }}
    }}
}}
";
        }
        
        /// <summary>
        /// 프리팹 생성
        /// </summary>
        private void CreatePrefab(string basePath, string name)
        {
            string prefabsPath = Path.Combine(basePath, "Prefabs");
            
            // 빈 게임 오브젝트 생성
            GameObject prefabObject = new GameObject(name);
            
            // 매니저 스크립트 추가
            string managerScriptName = $"{name}_Manager";
            System.Type managerType = System.Type.GetType($"Pansori.Microgames.Games.{managerScriptName}, Assembly-CSharp");
            if (managerType != null)
            {
                prefabObject.AddComponent(managerType);
            }
            else
            {
                Debug.LogWarning($"[MicrogameTemplateCreator] {managerScriptName} 스크립트를 찾을 수 없습니다. 프리팹에 수동으로 추가해주세요.");
            }
            
            // 헬퍼 컴포넌트 추가 (선택사항)
            GameObject timerObj = new GameObject("Timer");
            timerObj.transform.SetParent(prefabObject.transform);
            timerObj.AddComponent<MicrogameTimer>();
            
            GameObject inputObj = new GameObject("InputHandler");
            inputObj.transform.SetParent(prefabObject.transform);
            inputObj.AddComponent<MicrogameInputHandler>();
            
            GameObject uiObj = new GameObject("UILayer");
            uiObj.transform.SetParent(prefabObject.transform);
            uiObj.AddComponent<MicrogameUILayer>();
            
            // 프리팹으로 저장
            string prefabPath = Path.Combine(prefabsPath, $"{name}.prefab");
            PrefabUtility.SaveAsPrefabAsset(prefabObject, prefabPath);
            
            // 임시 오브젝트 삭제
            DestroyImmediate(prefabObject);
            
            AssetDatabase.ImportAsset(prefabPath);
        }
    }
}
