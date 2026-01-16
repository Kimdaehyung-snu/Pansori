using UnityEngine;
using UnityEditor;
using CautionPotion.Microgames;

namespace CautionPotion.Microgames.Editor
{
    /// <summary>
    /// 미니게임 프리팹 검증 도구
    /// 프리팹이 미니게임 규격을 준수하는지 확인합니다.
    /// </summary>
    public class MicrogameValidator : EditorWindow
    {
        private GameObject prefabToValidate;
        private Vector2 scrollPosition;
        private ValidationResult lastResult;
        
        [System.Serializable]
        private class ValidationResult
        {
            public bool isValid = true;
            public System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
            public System.Collections.Generic.List<string> warnings = new System.Collections.Generic.List<string>();
            public System.Collections.Generic.List<string> info = new System.Collections.Generic.List<string>();
        }
        
        [MenuItem("Tools/Microgames/Validate Prefab")]
        public static void ShowWindow()
        {
            GetWindow<MicrogameValidator>("Microgame Validator");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("미니게임 프리팹 검증 도구", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 프리팹 선택
            prefabToValidate = (GameObject)EditorGUILayout.ObjectField(
                "검증할 프리팹",
                prefabToValidate,
                typeof(GameObject),
                false
            );
            
            EditorGUILayout.Space();
            
            // 검증 버튼
            if (GUILayout.Button("검증 시작", GUILayout.Height(30)))
            {
                if (prefabToValidate == null)
                {
                    EditorUtility.DisplayDialog("오류", "프리팹을 선택해주세요.", "확인");
                    return;
                }
                
                ValidatePrefab(prefabToValidate);
            }
            
            EditorGUILayout.Space();
            
            // 결과 표시
            if (lastResult != null)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                // 전체 상태
                EditorGUILayout.Space();
                GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel);
                statusStyle.fontSize = 14;
                if (lastResult.isValid)
                {
                    statusStyle.normal.textColor = Color.green;
                    GUILayout.Label("✓ 검증 통과", statusStyle);
                }
                else
                {
                    statusStyle.normal.textColor = Color.red;
                    GUILayout.Label("✗ 검증 실패", statusStyle);
                }
                
                EditorGUILayout.Space();
                
                // 오류 표시
                if (lastResult.errors.Count > 0)
                {
                    EditorGUILayout.LabelField("오류:", EditorStyles.boldLabel);
                    foreach (string error in lastResult.errors)
                    {
                        EditorGUILayout.HelpBox(error, MessageType.Error);
                    }
                }
                
                // 경고 표시
                if (lastResult.warnings.Count > 0)
                {
                    EditorGUILayout.LabelField("경고:", EditorStyles.boldLabel);
                    foreach (string warning in lastResult.warnings)
                    {
                        EditorGUILayout.HelpBox(warning, MessageType.Warning);
                    }
                }
                
                // 정보 표시
                if (lastResult.info.Count > 0)
                {
                    EditorGUILayout.LabelField("정보:", EditorStyles.boldLabel);
                    foreach (string info in lastResult.info)
                    {
                        EditorGUILayout.HelpBox(info, MessageType.Info);
                    }
                }
                
                EditorGUILayout.EndScrollView();
            }
        }
        
        /// <summary>
        /// 프리팹 검증 수행
        /// </summary>
        private void ValidatePrefab(GameObject prefab)
        {
            lastResult = new ValidationResult();
            
            // 프리팹이 실제로 프리팹인지 확인
            if (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
            {
                lastResult.errors.Add("선택한 오브젝트가 프리팹이 아닙니다.");
                lastResult.isValid = false;
                return;
            }
            
            // 1. Transform 초기값 확인
            if (prefab.transform.localPosition != Vector3.zero)
            {
                lastResult.warnings.Add($"루트 Transform의 위치가 (0,0,0)이 아닙니다: {prefab.transform.localPosition}");
            }
            
            if (prefab.transform.localRotation != Quaternion.identity)
            {
                lastResult.warnings.Add($"루트 Transform의 회전이 (0,0,0)이 아닙니다: {prefab.transform.localRotation.eulerAngles}");
            }
            
            if (prefab.transform.localScale != Vector3.one)
            {
                lastResult.warnings.Add($"루트 Transform의 스케일이 (1,1,1)이 아닙니다: {prefab.transform.localScale}");
            }
            
            // 2. IMicrogame 구현 확인
            IMicrogame microgame = prefab.GetComponent<IMicrogame>();
            if (microgame == null)
            {
                // 하위 오브젝트에서도 찾기
                microgame = prefab.GetComponentInChildren<IMicrogame>();
            }
            
            if (microgame == null)
            {
                lastResult.errors.Add("IMicrogame 인터페이스를 구현한 컴포넌트가 없습니다.");
                lastResult.isValid = false;
            }
            else
            {
                lastResult.info.Add($"IMicrogame 구현 확인: {microgame.GetType().Name}");
                
                // MicrogameBase 상속 확인
                if (microgame is MicrogameBase)
                {
                    lastResult.info.Add("MicrogameBase를 상속받고 있습니다.");
                }
                else
                {
                    lastResult.warnings.Add("MicrogameBase를 상속받지 않고 있습니다. 권장사항: MicrogameBase 상속");
                }
            }
            
            // 3. ResetGameState 메서드 확인 (MicrogameBase인 경우)
            if (microgame is MicrogameBase)
            {
                var baseType = typeof(MicrogameBase);
                var resetMethod = baseType.GetMethod("ResetGameState", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (resetMethod != null && resetMethod.IsAbstract)
                {
                    // 추상 메서드이므로 구현되어 있어야 함
                    var implementationType = microgame.GetType();
                    var implementedMethod = implementationType.GetMethod("ResetGameState",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (implementedMethod == null || implementedMethod == resetMethod)
                    {
                        lastResult.errors.Add("ResetGameState() 메서드가 구현되지 않았습니다.");
                        lastResult.isValid = false;
                    }
                    else
                    {
                        lastResult.info.Add("ResetGameState() 메서드 구현 확인됨");
                    }
                }
            }
            
            // 4. 필수 컴포넌트 확인 (선택사항)
            if (prefab.GetComponent<MicrogameTimer>() == null)
            {
                lastResult.info.Add("MicrogameTimer 컴포넌트가 없습니다. (선택사항)");
            }
            
            if (prefab.GetComponent<MicrogameInputHandler>() == null)
            {
                lastResult.info.Add("MicrogameInputHandler 컴포넌트가 없습니다. (선택사항)");
            }
            
            if (prefab.GetComponent<MicrogameUILayer>() == null)
            {
                lastResult.info.Add("MicrogameUILayer 컴포넌트가 없습니다. (선택사항)");
            }
            
            // 5. 씬 로드 사용 확인 (정적 분석은 어려우므로 경고만)
            lastResult.info.Add("참고: 씬 로드 사용 여부는 런타임에서만 확인 가능합니다.");
            
            Debug.Log($"[MicrogameValidator] 검증 완료 - {(lastResult.isValid ? "통과" : "실패")}");
        }
    }
}
