using UnityEngine;
using UnityEditor;
using System.IO;
using Pansori.Microgames;

namespace Pansori.Microgames.Editor
{
    /// <summary>
    /// 마이크로게임 템플릿 생성 도구
    /// 새 마이크로게임 폴더, 스크립트 템플릿, 프리팹을 자동으로 생성합니다.
    /// </summary>
    public class MicrogameTemplateCreator : EditorWindow
    {
        private string gameName = "MG_NewGame_01";
        private string gameDescription = "새 미니게임";
        private string gameCommand = "행동해라!";
        
        private bool createScripts = true;
        private bool createPrefab = true;
        private bool createFolders = true;
        private bool addHelperComponents = true;
        private bool addTimerComponent = true;
        private bool addInputHandler = true;
        
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/Microgames/Create New Microgame", false, 30)]
        public static void ShowWindow()
        {
            var window = GetWindow<MicrogameTemplateCreator>("Create Microgame");
            window.minSize = new Vector2(400, 450);
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            GUILayout.Label("새 마이크로게임 생성", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "새 마이크로게임을 위한 폴더 구조, 스크립트, 프리팹을 자동으로 생성합니다.\n" +
                "MicrogameBase를 상속한 매니저 스크립트가 포함됩니다.",
                MessageType.Info);
            
            EditorGUILayout.Space();
            
            // 기본 정보
            EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
            gameName = EditorGUILayout.TextField("게임 이름 (영문)", gameName);
            gameDescription = EditorGUILayout.TextField("게임 설명 (한글)", gameDescription);
            gameCommand = EditorGUILayout.TextField("게임 명령어", gameCommand);
            
            if (!IsValidName(gameName))
            {
                EditorGUILayout.HelpBox("게임 이름은 영문, 숫자, 언더스코어(_)만 사용 가능합니다.", MessageType.Warning);
            }
            
            EditorGUILayout.Space();
            
            // 생성 옵션
            EditorGUILayout.LabelField("생성 옵션", EditorStyles.boldLabel);
            createFolders = EditorGUILayout.Toggle("폴더 구조 생성", createFolders);
            createScripts = EditorGUILayout.Toggle("스크립트 생성", createScripts);
            createPrefab = EditorGUILayout.Toggle("프리팹 생성", createPrefab);
            
            EditorGUILayout.Space();
            
            // 헬퍼 컴포넌트 옵션
            EditorGUILayout.LabelField("헬퍼 컴포넌트", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(!createPrefab);
            addHelperComponents = EditorGUILayout.Toggle("기본 헬퍼 추가", addHelperComponents);
            
            EditorGUI.BeginDisabledGroup(!addHelperComponents);
            addTimerComponent = EditorGUILayout.Toggle("  타이머 컴포넌트", addTimerComponent);
            addInputHandler = EditorGUILayout.Toggle("  입력 핸들러", addInputHandler);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // 미리보기
            EditorGUILayout.LabelField("생성될 파일 미리보기", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            
            string basePath = "Assets/MicrogameSystem/Games/" + gameName;
            
            if (createFolders)
            {
                EditorGUILayout.TextField("폴더", basePath);
                EditorGUILayout.TextField("스크립트 폴더", basePath + "/Scripts");
                EditorGUILayout.TextField("프리팹 폴더", basePath + "/Prefabs");
                EditorGUILayout.TextField("아트 폴더", basePath + "/Art");
            }
            
            if (createScripts)
            {
                EditorGUILayout.TextField("매니저 스크립트", basePath + "/Scripts/" + gameName + "Manager.cs");
            }
            
            if (createPrefab)
            {
                EditorGUILayout.TextField("프리팹", basePath + "/Prefabs/" + gameName + ".prefab");
            }
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
            
            // 생성 버튼
            EditorGUI.BeginDisabledGroup(!IsValidName(gameName));
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("마이크로게임 생성", GUILayout.Height(40)))
            {
                CreateMicrogame();
            }
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndScrollView();
        }
        
        private bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            
            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '_') return false;
            }
            
            return true;
        }
        
        private void CreateMicrogame()
        {
            string basePath = "Assets/MicrogameSystem/Games/" + gameName;
            
            try
            {
                // 1. 폴더 생성
                if (createFolders)
                {
                    CreateFolderStructure(basePath);
                }
                
                // 2. 스크립트 생성
                if (createScripts)
                {
                    CreateManagerScript(basePath);
                }
                
                AssetDatabase.Refresh();
                
                // 3. 프리팹 생성 (스크립트 컴파일 후)
                if (createPrefab)
                {
                    // 스크립트가 컴파일될 때까지 기다려야 할 수 있음
                    EditorApplication.delayCall += () =>
                    {
                        CreatePrefabAsset(basePath);
                        
                        EditorUtility.DisplayDialog("완료",
                            "마이크로게임이 생성되었습니다!\n\n" +
                            "위치: " + basePath + "\n\n" +
                            "다음 단계:\n" +
                            "1. " + gameName + "Manager.cs 스크립트 편집\n" +
                            "2. 프리팹에 게임 요소 추가\n" +
                            "3. Tools > Microgames > Scan Prefabs로 등록",
                            "확인");
                    };
                }
                else
                {
                    EditorUtility.DisplayDialog("완료",
                        "마이크로게임 템플릿이 생성되었습니다!\n\n" +
                        "위치: " + basePath,
                        "확인");
                }
                
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("오류", "생성 중 오류가 발생했습니다:\n" + e.Message, "확인");
                Debug.LogError("[MicrogameTemplateCreator] 오류: " + e);
            }
        }
        
        private void CreateFolderStructure(string basePath)
        {
            // 기본 폴더가 없으면 생성
            if (!Directory.Exists("Assets/MicrogameSystem/Games"))
            {
                Directory.CreateDirectory("Assets/MicrogameSystem/Games");
            }
            
            // 게임별 폴더 생성
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            
            Directory.CreateDirectory(basePath + "/Scripts");
            Directory.CreateDirectory(basePath + "/Prefabs");
            Directory.CreateDirectory(basePath + "/Art");
            
            Debug.Log("[MicrogameTemplateCreator] 폴더 구조 생성 완료: " + basePath);
        }
        
        private void CreateManagerScript(string basePath)
        {
            string scriptPath = basePath + "/Scripts/" + gameName + "Manager.cs";
            
            string scriptContent = GenerateManagerScript();
            
            File.WriteAllText(scriptPath, scriptContent);
            
            Debug.Log("[MicrogameTemplateCreator] 매니저 스크립트 생성: " + scriptPath);
        }
        
        private string GenerateManagerScript()
        {
            string template = @"using System;
using UnityEngine;
using Pansori.Microgames;

/// <summary>
/// {DESCRIPTION}
/// 명령어: {COMMAND}
/// </summary>
public class {NAME}Manager : MicrogameBase
{
    [Header(""{DESCRIPTION} 설정"")]
    [SerializeField] private float gameDuration = 5f;
    
    // 게임 고유 변수들
    private float timer;
    private bool hasSucceeded = false;
    
    /// <summary>
    /// 이 게임의 표시 이름
    /// </summary>
    public override string currentGameName => ""{COMMAND}"";
    
    /// <summary>
    /// 게임 시작 시 호출
    /// </summary>
    public override void OnGameStart(int difficulty, float speed)
    {
        base.OnGameStart(difficulty, speed);
        
        // 게임 상태 초기화
        timer = gameDuration / speed;
        hasSucceeded = false;
        
        // TODO: 게임 초기화 로직 추가
        Debug.Log($""[{NAME}] 게임 시작 - 난이도: {difficulty}, 속도: {speed}"");
    }
    
    private void Update()
    {
        if (isGameEnded) return;
        
        // 타이머 업데이트
        timer -= Time.deltaTime;
        
        // 시간 초과 시 실패
        if (timer <= 0)
        {
            OnTimeOut();
            return;
        }
        
        // TODO: 게임 로직 추가
        // 예시: 승리 조건 확인
        // if (승리조건)
        // {
        //     OnSuccess();
        // }
    }
    
    /// <summary>
    /// 성공 처리
    /// </summary>
    private void OnSuccess()
    {
        if (isGameEnded) return;
        
        hasSucceeded = true;
        Debug.Log(""[{NAME}] 성공!"");
        
        // 결과 애니메이션과 함께 보고
        ReportResultWithAnimation(true);
    }
    
    /// <summary>
    /// 실패 처리 (시간 초과)
    /// </summary>
    private void OnTimeOut()
    {
        if (isGameEnded) return;
        
        Debug.Log(""[{NAME}] 시간 초과!"");
        
        // 결과 애니메이션과 함께 보고
        ReportResultWithAnimation(false);
    }
    
    /// <summary>
    /// 게임 상태 초기화 (재사용을 위해 필수 구현)
    /// </summary>
    protected override void ResetGameState()
    {
        timer = gameDuration;
        hasSucceeded = false;
        
        // TODO: 게임 요소들 초기 상태로 복원
    }
    
    /// <summary>
    /// 게임 종료 시 호출
    /// </summary>
    protected override void OnGameEnd()
    {
        base.OnGameEnd();
        
        // TODO: 게임 종료 시 정리 작업
    }
}
";
            // 플레이스홀더 치환
            template = template.Replace("{NAME}", gameName);
            template = template.Replace("{DESCRIPTION}", gameDescription);
            template = template.Replace("{COMMAND}", gameCommand);
            
            return template;
        }
        
        private void CreatePrefabAsset(string basePath)
        {
            string prefabPath = basePath + "/Prefabs/" + gameName + ".prefab";
            
            // 게임 오브젝트 생성
            GameObject gameObject = new GameObject(gameName);
            
            // 매니저 스크립트 추가 시도
            string managerTypeName = gameName + "Manager";
            System.Type managerType = null;
            
            // 모든 어셈블리에서 타입 검색
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                managerType = assembly.GetType(managerTypeName);
                if (managerType != null) break;
            }
            
            if (managerType != null)
            {
                gameObject.AddComponent(managerType);
            }
            else
            {
                Debug.LogWarning("[MicrogameTemplateCreator] 매니저 스크립트를 찾을 수 없습니다. 수동으로 추가해주세요: " + managerTypeName);
            }
            
            // 헬퍼 컴포넌트 추가
            if (addHelperComponents)
            {
                if (addTimerComponent)
                {
                    gameObject.AddComponent<MicrogameTimer>();
                }
                
                if (addInputHandler)
                {
                    gameObject.AddComponent<MicrogameInputHandler>();
                }
            }
            
            // 프리팹으로 저장
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
            
            // 임시 오브젝트 삭제
            DestroyImmediate(gameObject);
            
            // 생성된 프리팹 선택
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            
            Debug.Log("[MicrogameTemplateCreator] 프리팹 생성 완료: " + prefabPath);
        }
    }
}