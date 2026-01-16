using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pansori.Microgames
{
    /// <summary>
    /// 미니게임 최상단 매니저
    /// 모든 미니게임의 생명주기를 관리합니다.
    /// </summary>
    public class MicrogameManager : MonoBehaviour
    {
        [Header("미니게임 프리팹 목록")]
        [SerializeField] private GameObject[] microgamePrefabs;
        
        [Header("게임 정보")]
        [SerializeField] private int maxLives = 3; // 최대 목숨 (초기값)
        [SerializeField] private int currentLives = 3; // 현재 남은 목숨
        [SerializeField] private int currentStage = 1; // 현재 스테이지
        [SerializeField] private float infoDisplayDuration = 2f; // 정보 표시 시간 (초)
        [SerializeField] private MicrogameInfoUI infoUI; // 정보 UI 컴포넌트 참조
        
        /// <summary>
        /// 소모된 목숨 개수
        /// </summary>
        private int consumedLives = 0;
        
        /// <summary>
        /// 프리팹과 인스턴스 매핑 (풀)
        /// </summary>
        private Dictionary<GameObject, GameObject> microgameInstances;
        
        /// <summary>
        /// 풀 오브젝트들의 부모 Transform
        /// </summary>
        private Transform poolParent;
        
        /// <summary>
        /// 현재 실행 중인 미니게임 인스턴스
        /// </summary>
        private GameObject currentMicrogameInstance;
        
        /// <summary>
        /// 현재 실행 중인 미니게임의 IMicrogame 컴포넌트
        /// </summary>
        private IMicrogame currentMicrogame;
        
        /// <summary>
        /// 미니게임 결과 이벤트 (외부에서 구독 가능)
        /// </summary>
        public event Action<bool> OnMicrogameResult;
        
        /// <summary>
        /// 현재 미니게임이 실행 중인지 여부
        /// </summary>
        public bool IsMicrogameRunning => currentMicrogameInstance != null && currentMicrogameInstance.activeSelf;
        
        /// <summary>
        /// 초기화 - 프리팹 풀 생성
        /// </summary>
        private void Awake()
        {
            InitializePool();
        }
        
        /// <summary>
        /// 프리팹 풀을 초기화합니다.
        /// 모든 프리팹을 미리 인스턴스화하여 풀에 저장합니다.
        /// </summary>
        private void InitializePool()
        {
            microgameInstances = new Dictionary<GameObject, GameObject>();
            
            // 풀 부모 오브젝트 생성
            GameObject poolObj = new GameObject("MicrogamePool");
            poolObj.transform.SetParent(transform);
            poolParent = poolObj.transform;
            
            // 모든 프리팹을 미리 인스턴스화
            if (microgamePrefabs != null)
            {
                foreach (GameObject prefab in microgamePrefabs)
                {
                    if (prefab != null)
                    {
                        GameObject instance = Instantiate(prefab, poolParent);
                        instance.name = prefab.name + "_Instance";
                        instance.SetActive(false);
                        microgameInstances[prefab] = instance;
                        Debug.Log($"[MicrogameManager] 프리팹 풀에 추가: {prefab.name}");
                    }
                }
            }
            
            Debug.Log($"[MicrogameManager] 프리팹 풀 초기화 완료 - 총 {microgameInstances.Count}개 인스턴스");
        }
        
        /// <summary>
        /// 프리팹에 해당하는 인스턴스를 풀에서 가져옵니다.
        /// </summary>
        /// <param name="prefab">프리팹</param>
        /// <returns>인스턴스 (없으면 null)</returns>
        private GameObject GetOrCreateInstance(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }
            
            // 풀에서 인스턴스 찾기
            if (microgameInstances != null && microgameInstances.TryGetValue(prefab, out GameObject instance))
            {
                return instance;
            }
            
            // 풀에 없으면 경고 (초기화되지 않은 프리팹일 수 있음)
            Debug.LogWarning($"[MicrogameManager] 풀에서 인스턴스를 찾을 수 없습니다: {prefab.name}. 풀이 초기화되지 않았을 수 있습니다.");
            return null;
        }
        
        /// <summary>
        /// 현재 남은 목숨 (외부에서 설정 가능)
        /// </summary>
        public int CurrentLives
        {
            get => currentLives;
            set
            {
                currentLives = Mathf.Max(0, value);
                consumedLives = maxLives - currentLives;
                
                // UI 업데이트
                if (infoUI != null)
                {
                    infoUI.UpdateLivesDisplay(maxLives, consumedLives);
                }
            }
        }
        
        /// <summary>
        /// 최대 목숨 (외부에서 설정 가능)
        /// </summary>
        public int MaxLives
        {
            get => maxLives;
            set
            {
                maxLives = Mathf.Max(1, value);
                currentLives = maxLives - consumedLives;
                
                // UI 업데이트
                if (infoUI != null)
                {
                    infoUI.UpdateLivesDisplay(maxLives, consumedLives);
                }
            }
        }
        
        /// <summary>
        /// 현재 스테이지 (외부에서 설정 가능)
        /// </summary>
        public int CurrentStage
        {
            get => currentStage;
            set => currentStage = Mathf.Max(1, value);
        }
        
        /// <summary>
        /// 등록된 미니게임 프리팹 개수
        /// </summary>
        public int MicrogamePrefabCount => microgamePrefabs?.Length ?? 0;
        
        /// <summary>
        /// 인덱스로 미니게임 프리팹 가져오기
        /// </summary>
        /// <param name="index">인덱스</param>
        /// <returns>프리팹 (없으면 null)</returns>
        public GameObject GetMicrogamePrefab(int index)
        {
            if (microgamePrefabs == null || index < 0 || index >= microgamePrefabs.Length)
            {
                return null;
            }
            return microgamePrefabs[index];
        }
        
        /// <summary>
        /// 인덱스로 미니게임 이름 가져오기
        /// </summary>
        /// <param name="index">인덱스</param>
        /// <returns>게임 이름 (없으면 빈 문자열)</returns>
        public string GetMicrogameName(int index)
        {
            GameObject prefab = GetMicrogamePrefab(index);
            if (prefab == null) return string.Empty;
            
            // 풀에서 인스턴스 가져와서 이름 확인
            if (microgameInstances != null && microgameInstances.TryGetValue(prefab, out GameObject instance))
            {
                IMicrogame microgame = instance.GetComponent<IMicrogame>();
                if (microgame is MicrogameBase microgameBase)
                {
                    return microgameBase.currentGameName;
                }
            }
            
            return prefab.name;
        }
        
        /// <summary>
        /// 랜덤 미니게임 인덱스 가져오기
        /// </summary>
        /// <returns>랜덤 인덱스 (-1이면 프리팹 없음)</returns>
        public int GetRandomMicrogameIndex()
        {
            if (microgamePrefabs == null || microgamePrefabs.Length == 0)
            {
                return -1;
            }
            return UnityEngine.Random.Range(0, microgamePrefabs.Length);
        }
        
        /// <summary>
        /// 인덱스로 미니게임을 시작합니다.
        /// </summary>
        /// <param name="index">프리팹 배열 인덱스</param>
        /// <param name="difficulty">난이도 (1~3)</param>
        /// <param name="speed">배속 (1.0f 이상)</param>
        public void StartMicrogame(int index, int difficulty, float speed)
        {
            if (index < 0 || index >= microgamePrefabs.Length)
            {
                Debug.LogError($"[MicrogameManager] 유효하지 않은 인덱스: {index} (배열 크기: {microgamePrefabs.Length})");
                return;
            }
            
            if (microgamePrefabs[index] == null)
            {
                Debug.LogError($"[MicrogameManager] 인덱스 {index}의 프리팹이 null입니다.");
                return;
            }
            
            StartMicrogame(microgamePrefabs[index], difficulty, speed);
        }
        
        /// <summary>
        /// 프리팹으로 미니게임을 시작합니다.
        /// </summary>
        /// <param name="prefab">미니게임 프리팹</param>
        /// <param name="difficulty">난이도 (1~3)</param>
        /// <param name="speed">배속 (1.0f 이상)</param>
        public void StartMicrogame(GameObject prefab, int difficulty, float speed)
        {
            if (prefab == null)
            {
                Debug.LogError("[MicrogameManager] 프리팹이 null입니다.");
                return;
            }
            
            // 난이도 및 속도 검증
            difficulty = Mathf.Clamp(difficulty, 1, 3);
            speed = Mathf.Max(1.0f, speed);
            
            // 기존 미니게임이 실행 중이면 종료
            if (IsMicrogameRunning)
            {
                Debug.LogWarning("[MicrogameManager] 이미 실행 중인 미니게임이 있습니다. 기존 미니게임을 종료합니다.");
                EndCurrentMicrogame();
            }
            
            // 풀에서 인스턴스 가져오기
            currentMicrogameInstance = GetOrCreateInstance(prefab);
            if (currentMicrogameInstance == null)
            {
                Debug.LogError($"[MicrogameManager] 프리팹 '{prefab.name}'의 인스턴스를 풀에서 가져올 수 없습니다.");
                return;
            }
            
            // 인스턴스 비활성화 (정보 표시 전까지)
            currentMicrogameInstance.SetActive(false);
            
            // IMicrogame 컴포넌트 찾기
            currentMicrogame = currentMicrogameInstance.GetComponent<IMicrogame>();
            if (currentMicrogame == null)
            {
                Debug.LogError($"[MicrogameManager] 프리팹 '{prefab.name}'에 IMicrogame 인터페이스를 구현한 컴포넌트가 없습니다.");
                currentMicrogameInstance = null;
                return;
            }
            
            // 결과 이벤트 구독
            currentMicrogame.OnResultReported = HandleMicrogameResult;
            
            // 게임 이름 가져오기
            string gameName = prefab.name;
            if (currentMicrogame is MicrogameBase)
            {
                gameName = ((MicrogameBase)currentMicrogame).currentGameName;
            }
            
            // 정보 UI 표시 (자동 숨김 시간 전달)
            if (infoUI != null)
            {
                infoUI.ShowInfoWithLives(gameName, maxLives, consumedLives, currentStage, infoDisplayDuration);
            }
            
            // 코루틴으로 정보 표시 후 게임 시작
            StartCoroutine(StartMicrogameWithInfo(prefab, difficulty, speed));
        }
        
        /// <summary>
        /// 정보 표시 후 게임을 시작하는 코루틴
        /// </summary>
        private IEnumerator StartMicrogameWithInfo(GameObject prefab, int difficulty, float speed)
        {
            // 정보 표시 시간 대기 (MicrogameInfoUI가 자동으로 숨김)
            yield return new WaitForSeconds(infoDisplayDuration);
            
            // 프리팹 활성화 및 게임 시작
            if (currentMicrogameInstance != null && currentMicrogame != null)
            {
                currentMicrogameInstance.SetActive(true);
                
                Debug.Log($"[MicrogameManager] 미니게임 시작: {prefab.name} (난이도: {difficulty}, 배속: {speed})");
                currentMicrogame.OnGameStart(difficulty, speed);
            }
        }
        
        /// <summary>
        /// 랜덤 미니게임을 시작합니다.
        /// </summary>
        /// <param name="difficulty">난이도 (1~3)</param>
        /// <param name="speed">배속 (1.0f 이상)</param>
        public void StartRandomMicrogame(int difficulty, float speed)
        {
            if (microgamePrefabs == null || microgamePrefabs.Length == 0)
            {
                Debug.LogError("[MicrogameManager] 미니게임 프리팹이 설정되지 않았습니다.");
                return;
            }
            
            int randomIndex = UnityEngine.Random.Range(0, microgamePrefabs.Length);
            StartMicrogame(randomIndex, difficulty, speed);
        }
        
        /// <summary>
        /// 현재 실행 중인 미니게임을 종료합니다.
        /// </summary>
        public void EndCurrentMicrogame()
        {
            if (currentMicrogameInstance == null)
            {
                return;
            }
            
            // 결과 이벤트 구독 해제
            if (currentMicrogame != null)
            {
                currentMicrogame.OnResultReported = null;
            }
            
            // 인스턴스 비활성화 (풀로 반환)
            currentMicrogameInstance.SetActive(false);
            // ResetGameState는 OnDisable에서 자동 호출됨
            
            currentMicrogameInstance = null;
            currentMicrogame = null;
            
            Debug.Log("[MicrogameManager] 미니게임 종료 (풀로 반환)");
        }
        
        /// <summary>
        /// 미니게임 결과를 처리합니다.
        /// </summary>
        /// <param name="success">성공 여부</param>
        private void HandleMicrogameResult(bool success)
        {
            Debug.Log($"[MicrogameManager] 미니게임 결과: {(success ? "성공" : "실패")}");
            
            // 외부 이벤트 호출
            OnMicrogameResult?.Invoke(success);
            
            // 실패 시 목숨 감소
            if (!success)
            {
                DecreaseLife();
            }
            
            // 미니게임 종료
            EndCurrentMicrogame();
        }
        
        /// <summary>
        /// 목숨을 감소시킵니다.
        /// </summary>
        private void DecreaseLife()
        {
            consumedLives++;
            currentLives = maxLives - consumedLives;
            
            // UI 업데이트
            if (infoUI != null)
            {
                infoUI.UpdateLivesDisplay(maxLives, consumedLives);
            }
            
            Debug.Log($"[MicrogameManager] 목숨 감소 - 남은 목숨: {currentLives}, 소모된 목숨: {consumedLives}");
            
            // 목숨이 0이면 게임오버
            if (currentLives <= 0)
            {
                if (infoUI != null)
                {
                    infoUI.ShowGameOver();
                }
                Debug.Log("[MicrogameManager] 게임오버 - 모든 목숨을 소모했습니다.");
            }
        }
        
        /// <summary>
        /// 목숨을 초기화합니다. (게임 재시작 시 사용)
        /// </summary>
        public void ResetLives()
        {
            consumedLives = 0;
            currentLives = maxLives;
            
            // UI 업데이트
            if (infoUI != null)
            {
                infoUI.UpdateLivesDisplay(maxLives, consumedLives);
                infoUI.HideGameOver();
            }
            
            Debug.Log($"[MicrogameManager] 목숨 초기화 - 목숨: {currentLives}");
        }
        
        /// <summary>
        /// 컴포넌트가 파괴될 때 정리 작업
        /// </summary>
        private void OnDestroy()
        {
            EndCurrentMicrogame();
            
            // 풀의 모든 인스턴스 정리
            if (microgameInstances != null)
            {
                foreach (var kvp in microgameInstances)
                {
                    if (kvp.Value != null)
                    {
                        Destroy(kvp.Value);
                    }
                }
                microgameInstances.Clear();
            }
            
            // 풀 부모 오브젝트 정리
            if (poolParent != null)
            {
                Destroy(poolParent.gameObject);
            }
            
            Debug.Log("[MicrogameManager] 프리팹 풀 정리 완료");
        }
    }
}
