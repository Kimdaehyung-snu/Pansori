using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Pansori.Microgames;

namespace Pansori.Microgames.Editor
{
    /// <summary>
    /// 마이크로게임 프리팹 자동 스캔/등록 도구
    /// Games 폴더 내 모든 프리팹을 스캔하여 MicrogameManager에 등록합니다.
    /// </summary>
    public class MicrogamePrefabScanner : EditorWindow
    {
        private MicrogameManager targetManager;
        private string scanFolderPath = "Assets/MicrogameSystem/Games";
        private Vector2 scrollPosition;
        
        private List<ScanResult> scanResults = new List<ScanResult>();
        private bool showValidOnly = false;
        
        private class ScanResult
        {
            public GameObject prefab;
            public string path;
            public bool hasIMicrogame;
            public bool hasMicrogameBase;
            public bool isValid;
            public string validationMessage;
            public bool isSelected;
        }
        
        [MenuItem("Tools/Microgames/Scan Prefabs", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<MicrogamePrefabScanner>("Prefab Scanner");
            window.minSize = new Vector2(500, 400);
        }
        
        private void OnEnable()
        {
            // 씬에서 MicrogameManager 자동 찾기
            FindMicrogameManager();
        }
        
        private void FindMicrogameManager()
        {
            if (targetManager == null)
            {
                targetManager = Object.FindObjectOfType<MicrogameManager>();
            }
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("마이크로게임 프리팹 스캐너", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "지정된 폴더에서 IMicrogame 인터페이스를 구현한 프리팹을 자동으로 스캔합니다.\n" +
                "스캔 결과에서 원하는 프리팹을 선택하여 MicrogameManager에 등록할 수 있습니다.",
                MessageType.Info);
            
            EditorGUILayout.Space();
            
            // 설정 섹션
            EditorGUILayout.LabelField("설정", EditorStyles.boldLabel);
            
            targetManager = (MicrogameManager)EditorGUILayout.ObjectField(
                "대상 MicrogameManager",
                targetManager,
                typeof(MicrogameManager),
                true);
            
            EditorGUILayout.BeginHorizontal();
            scanFolderPath = EditorGUILayout.TextField("스캔 폴더", scanFolderPath);
            if (GUILayout.Button("찾아보기", GUILayout.Width(80)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("스캔할 폴더 선택", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        scanFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 스캔 버튼
            GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);
            if (GUILayout.Button("폴더 스캔", GUILayout.Height(30)))
            {
                ScanFolder();
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            // 결과 섹션
            if (scanResults.Count > 0)
            {
                DrawScanResults();
            }
            else
            {
                EditorGUILayout.HelpBox("스캔 결과가 없습니다. '폴더 스캔' 버튼을 클릭하세요.", MessageType.None);
            }
        }
        
        private void ScanFolder()
        {
            scanResults.Clear();
            
            if (!Directory.Exists(scanFolderPath))
            {
                EditorUtility.DisplayDialog("오류", "지정된 폴더를 찾을 수 없습니다: " + scanFolderPath, "확인");
                return;
            }
            
            // 모든 프리팹 파일 찾기
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { scanFolderPath });
            
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    var result = ValidatePrefab(prefab, path);
                    scanResults.Add(result);
                }
            }
            
            // 유효한 프리팹 우선 정렬
            scanResults.Sort((a, b) =>
            {
                if (a.isValid != b.isValid) return b.isValid.CompareTo(a.isValid);
                return a.prefab.name.CompareTo(b.prefab.name);
            });
            
            int validCount = 0;
            foreach (var result in scanResults)
            {
                if (result.isValid) validCount++;
            }
            
            Debug.Log("[MicrogamePrefabScanner] 스캔 완료 - 총: " + scanResults.Count + "개, 유효: " + validCount + "개");
        }
        
        private ScanResult ValidatePrefab(GameObject prefab, string path)
        {
            var result = new ScanResult
            {
                prefab = prefab,
                path = path,
                isSelected = true
            };
            
            // IMicrogame 인터페이스 확인
            var microgame = prefab.GetComponent<IMicrogame>();
            result.hasIMicrogame = microgame != null;
            
            // MicrogameBase 상속 확인
            var microgameBase = prefab.GetComponent<MicrogameBase>();
            result.hasMicrogameBase = microgameBase != null;
            
            // 유효성 판단
            if (result.hasIMicrogame)
            {
                result.isValid = true;
                if (result.hasMicrogameBase)
                {
                    result.validationMessage = "MicrogameBase 구현";
                }
                else
                {
                    result.validationMessage = "IMicrogame 구현";
                }
            }
            else
            {
                result.isValid = false;
                result.validationMessage = "IMicrogame 미구현";
                result.isSelected = false;
            }
            
            return result;
        }
        
        private void DrawScanResults()
        {
            int validCount = 0;
            int selectedCount = 0;
            
            foreach (var result in scanResults)
            {
                if (result.isValid) validCount++;
                if (result.isSelected) selectedCount++;
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("스캔 결과: " + scanResults.Count + "개 (유효: " + validCount + "개, 선택: " + selectedCount + "개)", EditorStyles.boldLabel);
            showValidOnly = EditorGUILayout.Toggle("유효한 것만 표시", showValidOnly);
            EditorGUILayout.EndHorizontal();
            
            // 전체 선택/해제 버튼
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("유효한 것 모두 선택"))
            {
                foreach (var result in scanResults)
                {
                    if (result.isValid) result.isSelected = true;
                }
            }
            if (GUILayout.Button("선택 해제"))
            {
                foreach (var result in scanResults)
                {
                    result.isSelected = false;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 결과 목록
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            foreach (var result in scanResults)
            {
                if (showValidOnly && !result.isValid) continue;
                
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                
                // 선택 체크박스
                bool newSelected = EditorGUILayout.Toggle(result.isSelected, GUILayout.Width(20));
                if (newSelected != result.isSelected)
                {
                    result.isSelected = newSelected;
                }
                
                // 상태 아이콘
                GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
                if (result.isValid)
                {
                    statusStyle.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
                    GUILayout.Label("✓", statusStyle, GUILayout.Width(20));
                }
                else
                {
                    statusStyle.normal.textColor = new Color(0.8f, 0.2f, 0.2f);
                    GUILayout.Label("✗", statusStyle, GUILayout.Width(20));
                }
                
                // 프리팹 이름
                if (GUILayout.Button(result.prefab.name, EditorStyles.label))
                {
                    Selection.activeObject = result.prefab;
                    EditorGUIUtility.PingObject(result.prefab);
                }
                
                // 상태 메시지
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(result.validationMessage, GUILayout.Width(150));
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            // 등록 버튼
            EditorGUI.BeginDisabledGroup(targetManager == null || selectedCount == 0);
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("선택한 프리팹을 MicrogameManager에 등록 (" + selectedCount + "개)", GUILayout.Height(40)))
            {
                RegisterSelectedPrefabs();
            }
            GUI.backgroundColor = Color.white;
            EditorGUI.EndDisabledGroup();
            
            if (targetManager == null)
            {
                EditorGUILayout.HelpBox("MicrogameManager를 먼저 지정해주세요.", MessageType.Warning);
            }
        }
        
        private void RegisterSelectedPrefabs()
        {
            if (targetManager == null)
            {
                EditorUtility.DisplayDialog("오류", "MicrogameManager가 설정되지 않았습니다.", "확인");
                return;
            }
            
            List<GameObject> selectedPrefabs = new List<GameObject>();
            foreach (var result in scanResults)
            {
                if (result.isSelected && result.isValid)
                {
                    selectedPrefabs.Add(result.prefab);
                }
            }
            
            if (selectedPrefabs.Count == 0)
            {
                EditorUtility.DisplayDialog("알림", "선택된 유효한 프리팹이 없습니다.", "확인");
                return;
            }
            
            // SerializedObject를 통해 프리팹 목록 설정
            SerializedObject serializedManager = new SerializedObject(targetManager);
            SerializedProperty prefabsProperty = serializedManager.FindProperty("microgamePrefabs");
            
            // 기존 목록 확인
            List<GameObject> existingPrefabs = new List<GameObject>();
            for (int i = 0; i < prefabsProperty.arraySize; i++)
            {
                var element = prefabsProperty.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue != null)
                {
                    existingPrefabs.Add(element.objectReferenceValue as GameObject);
                }
            }
            
            // 새 프리팹 추가 (중복 제외)
            int addedCount = 0;
            foreach (var prefab in selectedPrefabs)
            {
                if (!existingPrefabs.Contains(prefab))
                {
                    prefabsProperty.InsertArrayElementAtIndex(prefabsProperty.arraySize);
                    prefabsProperty.GetArrayElementAtIndex(prefabsProperty.arraySize - 1).objectReferenceValue = prefab;
                    addedCount++;
                }
            }
            
            serializedManager.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(targetManager);
            
            string message = addedCount + "개의 프리팹이 등록되었습니다.";
            if (selectedPrefabs.Count - addedCount > 0)
            {
                message += "\n(" + (selectedPrefabs.Count - addedCount) + "개는 이미 등록되어 있음)";
            }
            
            EditorUtility.DisplayDialog("완료", message, "확인");
            
            Debug.Log("[MicrogamePrefabScanner] 프리팹 등록 완료 - " + addedCount + "개 추가");
        }
    }
}