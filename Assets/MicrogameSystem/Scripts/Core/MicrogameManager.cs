using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pansori.Microgames
{
    /// <summary>
    /// 미니게임 최상단 매니저
    /// 모든 미니게임의 생명주기를 관리합니다.
    /// 셔플, 통계 추적, 자동 스캔 기능을 지원합니다.
    /// </summary>
    public class MicrogameManager : MonoBehaviour
    {
        [Header("시스템 설정")]
        [SerializeField] private MicrogameSystemSettings settings;
        
        [Header("미니게임 프리팹 목록")]
        [SerializeField] private List<GameObject> microgamePrefabs = new List<GameObject>();
        
        [Header("게임 정보")]
        [SerializeField] private int maxLives = 4;
        [SerializeField] private int currentLives = 4;
        [SerializeField] private int currentStage = 1;
        [SerializeField] private float infoDisplayDuration = 2f;
        [SerializeField] private MicrogameInfoUI infoUI;
        
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

        #region Shuffle System
        
        /// <summary>
        /// 셔플된 인덱스 큐 (중복 방지용)
        /// </summary>
        private Queue<int> shuffledIndices = new Queue<int>();
        
        /// <summary>
        /// 최근 플레이한 게임 인덱스 히스토리 (연속 중복 방지)
        /// </summary>
        private Queue<int> recentPlayedHistory = new Queue<int>();
        
        #endregion

        #region Statistics
        
        /// <summary>
        /// 게임별 플레이 횟수
        /// </summary>
        private Dictionary<int, int> playCountByIndex = new Dictionary<int, int>();
        
        /// <summary>
        /// 게임별 성공 횟수
        /// </summary>
        private Dictionary<int, int> successCountByIndex = new Dictionary<int, int>();
        
        /// <summary>
        /// 마지막으로 플레이한 게임 인덱스
        /// </summary>
        private int lastPlayedIndex = -1;

        /// <summary>
        /// 총 플레이 횟수
        /// </summary>
        public int TotalPlayCount { get; private set; }

        /// <summary>
        /// 총 성공 횟수
        /// </summary>
        public int TotalSuccessCount { get; private set; }

        /// <summary>
        /// 총 실패 횟수
        /// </summary>
        public int TotalFailureCount => TotalPlayCount - TotalSuccessCount;

        #endregion

        #region Events
        
        /// <summary>
        /// 미니게임 결과 이벤트 (외부에서 구독 가능)
        /// </summary>
        public event Action<bool> OnMicrogameResult;
        
        /// <summary>
        /// 미니게임 시작 이벤트 (인덱스, 난이도, 속도)
        /// </summary>
        public event Action<int, int, float> OnMicrogameStarted;
        
        /// <summary>
        /// 목숨 변경 이벤트 (현재 목숨, 최대 목숨)
        /// </summary>
        public event Action<int, int> OnLivesChanged;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// 현재 미니게임이 실행 중인지 여부
        /// </summary>
        public bool IsMicrogameRunning => currentMicrogameInstance != null && currentMicrogameInstance.activeSelf;
        
        /// <summary>
        /// 현재 남은 목숨 (외부에서 설정 가능)
        /// </summary>
        public int CurrentLives
        {
            get => currentLives;
            set
            {
                int oldLives = currentLives;
                currentLives = Mathf.Max(0, value);
                consumedLives = maxLives - currentLives;
                
                if (oldLives != currentLives)
                {
                    OnLivesChanged?.Invoke(currentLives, maxLives);
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
                OnLivesChanged?.Invoke(currentLives, maxLives);
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
        public int MicrogamePrefabCount => microgamePrefabs?.Count ?? 0;
        
        /// <summary>
        /// 시스템 설정
        /// </summary>
        public MicrogameSystemSettings Settings => settings;

        /// <summary>
        /// 현재 실행 중인 미니게임 인덱스
        /// </summary>
        public int CurrentMicrogameIndex => lastPlayedIndex;

        /// <summary>
        /// 현재 실행 중인 미니게임 이름
        /// </summary>
        public string CurrentMicrogameName
        {
            get
            {
                if (currentMicrogameInstance == null) return string.Empty;
                if (currentMicrogame is MicrogameBase microgameBase)
                {
                    return microgameBase.currentGameName;
                }
                return currentMicrogameInstance.name;
            }
        }
        
        #endregion
        
        /// <summary>
        /// 초기화 - 프리팹 풀 생성
        /// </summary>
        private void Awake()
        {
            ApplySettings();
            InitializePool();
        }

        /// <summary>
        /// 설정을 적용합니다.
        /// </summary>
        private void ApplySettings()
        {
            if (settings != null)
            {
                maxLives = settings.MaxLives;
                currentLives = maxLives;
                infoDisplayDuration = settings.InfoDisplayDuration;
            }
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
        /// 프리팹 목록을 설정합니다. (에디터/런타임에서 사용)
        /// </summary>
        /// <param name="prefabs">프리팹 목록</param>
        public void SetMicrogamePrefabs(List<GameObject> prefabs)
        {
            microgamePrefabs = prefabs ?? new List<GameObject>();
            
            // 풀이 이미 초기화되어 있으면 재초기화
            if (microgameInstances != null)
            {
                ClearPool();
                InitializePool();
            }
        }

        /// <summary>
        /// 프리팹을 목록에 추가합니다.
        /// </summary>
        /// <param name="prefab">추가할 프리팹</param>
        public void AddMicrogamePrefab(GameObject prefab)
        {
            if (prefab == null) return;
            if (microgamePrefabs.Contains(prefab)) return;
            
            microgamePrefabs.Add(prefab);
            
            // 풀에도 추가
            if (microgameInstances != null && poolParent != null)
            {
                GameObject instance = Instantiate(prefab, poolParent);
                instance.name = prefab.name + "_Instance";
                instance.SetActive(false);
                microgameInstances[prefab] = instance;
            }
        }

        /// <summary>
        /// 프리팹 목록을 초기화합니다.
        /// </summary>
        public void ClearMicrogamePrefabs()
        {
            ClearPool();
            microgamePrefabs.Clear();
        }

        /// <summary>
        /// 풀을 정리합니다.
        /// </summary>
        private void ClearPool()
        {
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
        }
        
        /// <summary>
        /// 프리팹에 해당하는 인스턴스를 풀에서 가져옵니다.
        /// </summary>
        private GameObject GetOrCreateInstance(GameObject prefab)
        {
            if (prefab == null) return null;
            
            if (microgameInstances != null && microgameInstances.TryGetValue(prefab, out GameObject instance))
            {
                return instance;
            }
            
            Debug.LogWarning($"[MicrogameManager] 풀에서 인스턴스를 찾을 수 없습니다: {prefab.name}");
            return null;
        }

        #region Microgame Selection
        
        /// <summary>
        /// 인덱스로 미니게임 프리팹 가져오기
        /// </summary>
        public GameObject GetMicrogamePrefab(int index)
        {
            if (microgamePrefabs == null || index < 0 || index >= microgamePrefabs.Count)
            {
                return null;
            }
            return microgamePrefabs[index];
        }
        
        /// <summary>
        /// 인덱스로 미니게임 이름 가져오기
        /// </summary>
        public string GetMicrogameName(int index)
        {
            GameObject prefab = GetMicrogamePrefab(index);
            if (prefab == null) return string.Empty;
            
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
        /// 인덱스로 미니게임 조작법 설명 가져오기
        /// </summary>
        public string GetMicrogameControlDescription(int index)
        {
            GameObject prefab = GetMicrogamePrefab(index);
            if (prefab == null) return string.Empty;
            
            if (microgameInstances != null && microgameInstances.TryGetValue(prefab, out GameObject instance))
            {
                IMicrogame microgame = instance.GetComponent<IMicrogame>();
                if (microgame is MicrogameBase microgameBase)
                {
                    return microgameBase.controlDescription;
                }
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// 랜덤 미니게임 인덱스 가져오기 (셔플 시스템 적용)
        /// </summary>
        public int GetRandomMicrogameIndex()
        {
            if (microgamePrefabs == null || microgamePrefabs.Count == 0)
            {
                return -1;
            }

            // 셔플이 비활성화된 경우 또는 설정이 없는 경우 단순 랜덤
            if (settings == null || !settings.EnableShuffle)
            {
                return UnityEngine.Random.Range(0, microgamePrefabs.Count);
            }

            // 셔플된 인덱스가 없으면 새로 생성
            if (shuffledIndices.Count == 0)
            {
                GenerateShuffledIndices();
            }

            // 셔플된 인덱스에서 하나 꺼내기
            int selectedIndex = shuffledIndices.Dequeue();

            // 연속 중복 방지 체크
            int maxAttempts = microgamePrefabs.Count;
            int attempts = 0;
            while (recentPlayedHistory.Contains(selectedIndex) && attempts < maxAttempts)
            {
                // 다시 큐에 넣고 다른 것 선택
                shuffledIndices.Enqueue(selectedIndex);
                
                if (shuffledIndices.Count == 0)
                {
                    GenerateShuffledIndices();
                }
                
                selectedIndex = shuffledIndices.Dequeue();
                attempts++;
            }

            return selectedIndex;
        }

        /// <summary>
        /// 셔플된 인덱스 배열을 생성합니다.
        /// </summary>
        private void GenerateShuffledIndices()
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < microgamePrefabs.Count; i++)
            {
                indices.Add(i);
            }

            // Fisher-Yates 셔플
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int temp = indices[i];
                indices[i] = indices[j];
                indices[j] = temp;
            }

            shuffledIndices.Clear();
            foreach (int index in indices)
            {
                shuffledIndices.Enqueue(index);
            }
        }

        /// <summary>
        /// 플레이 히스토리에 인덱스를 추가합니다.
        /// </summary>
        private void AddToPlayHistory(int index)
        {
            if (settings == null) return;

            recentPlayedHistory.Enqueue(index);
            
            // 히스토리 크기 유지
            while (recentPlayedHistory.Count > settings.ShuffleHistorySize)
            {
                recentPlayedHistory.Dequeue();
            }
        }

        #endregion

        #region Game Start/End
        
        /// <summary>
        /// 인덱스로 미니게임을 시작합니다.
        /// </summary>
        public void StartMicrogame(int index, int difficulty, float speed)
        {
            if (index < 0 || index >= microgamePrefabs.Count)
            {
                Debug.LogError($"[MicrogameManager] 유효하지 않은 인덱스: {index} (배열 크기: {microgamePrefabs.Count})");
                return;
            }
            
            if (microgamePrefabs[index] == null)
            {
                Debug.LogError($"[MicrogameManager] 인덱스 {index}의 프리팹이 null입니다.");
                return;
            }
            
            StartMicrogame(microgamePrefabs[index], index, difficulty, speed);
        }

        /// <summary>
        /// 프리팹으로 미니게임을 시작합니다.
        /// </summary>
        public void StartMicrogame(GameObject prefab, int difficulty, float speed)
        {
            int index = microgamePrefabs.IndexOf(prefab);
            StartMicrogame(prefab, index, difficulty, speed);
        }
        
        /// <summary>
        /// 미니게임을 시작합니다. (내부용)
        /// </summary>
        private void StartMicrogame(GameObject prefab, int index, int difficulty, float speed)
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
            
            // IMicrogame 컴포넌트 찾기
            currentMicrogame = currentMicrogameInstance.GetComponent<IMicrogame>();
            if (currentMicrogame == null)
            {
                Debug.LogError($"[MicrogameManager] 프리팹 '{prefab.name}'에 IMicrogame 인터페이스를 구현한 컴포넌트가 없습니다.");
                currentMicrogameInstance = null;
                return;
            }
            
            // 통계 업데이트
            lastPlayedIndex = index;
            TotalPlayCount++;
            
            if (!playCountByIndex.ContainsKey(index))
            {
                playCountByIndex[index] = 0;
            }
            playCountByIndex[index]++;
            
            // 플레이 히스토리 추가
            AddToPlayHistory(index);
            
            // 결과 이벤트 구독
            currentMicrogame.OnResultReported = HandleMicrogameResult;
            
            // 인스턴스 활성화
            currentMicrogameInstance.SetActive(true);
            
            // 미니게임 BGM 재생 (속도 반영)
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayMicrogameBGM(prefab.name, speed);
            }
            
            Debug.Log($"[MicrogameManager] 미니게임 시작: {prefab.name} (난이도: {difficulty}, 배속: {speed})");
            
            // 시작 이벤트 발생
            OnMicrogameStarted?.Invoke(index, difficulty, speed);
            
            // 게임 시작 호출
            currentMicrogame.OnGameStart(difficulty, speed);
        }
        
        /// <summary>
        /// 랜덤 미니게임을 시작합니다.
        /// </summary>
        public void StartRandomMicrogame(int difficulty, float speed)
        {
            if (microgamePrefabs == null || microgamePrefabs.Count == 0)
            {
                Debug.LogError("[MicrogameManager] 미니게임 프리팹이 설정되지 않았습니다.");
                return;
            }
            
            int randomIndex = GetRandomMicrogameIndex();
            StartMicrogame(randomIndex, difficulty, speed);
        }
        
        /// <summary>
        /// 현재 실행 중인 미니게임을 종료합니다.
        /// </summary>
        public void EndCurrentMicrogame()
        {
            if (currentMicrogameInstance == null) return;
            
            // 결과 이벤트 구독 해제
            if (currentMicrogame != null)
            {
                currentMicrogame.OnResultReported = null;
            }
            
            // 미니게임 BGM 정지
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopMicrogameBGM();
            }
            
            // 인스턴스 비활성화 (풀로 반환)
            currentMicrogameInstance.SetActive(false);
            
            currentMicrogameInstance = null;
            currentMicrogame = null;
            
            Debug.Log("[MicrogameManager] 미니게임 종료 (풀로 반환)");
        }
        
        /// <summary>
        /// 미니게임 결과를 처리합니다.
        /// </summary>
        private void HandleMicrogameResult(bool success)
        {
            Debug.Log($"[MicrogameManager] 미니게임 결과: {(success ? "성공" : "실패")}");
            
            // 통계 업데이트
            if (success)
            {
                TotalSuccessCount++;
                if (lastPlayedIndex >= 0)
                {
                    if (!successCountByIndex.ContainsKey(lastPlayedIndex))
                    {
                        successCountByIndex[lastPlayedIndex] = 0;
                    }
                    successCountByIndex[lastPlayedIndex]++;
                }
            }
            
            // 결과 사운드 재생
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayMicrogameResultSound(success);
            }
            
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
            
            OnLivesChanged?.Invoke(currentLives, maxLives);
            
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
            
            if (infoUI != null)
            {
                infoUI.HideGameOver();
            }
            
            OnLivesChanged?.Invoke(currentLives, maxLives);
            
            Debug.Log($"[MicrogameManager] 목숨 초기화 - 목숨: {currentLives}");
        }

        /// <summary>
        /// 모든 통계를 초기화합니다.
        /// </summary>
        public void ResetStatistics()
        {
            TotalPlayCount = 0;
            TotalSuccessCount = 0;
            playCountByIndex.Clear();
            successCountByIndex.Clear();
            lastPlayedIndex = -1;
            shuffledIndices.Clear();
            recentPlayedHistory.Clear();
            
            Debug.Log("[MicrogameManager] 통계 초기화");
        }

        /// <summary>
        /// 특정 게임의 플레이 횟수를 가져옵니다.
        /// </summary>
        public int GetPlayCount(int index)
        {
            return playCountByIndex.TryGetValue(index, out int count) ? count : 0;
        }

        /// <summary>
        /// 특정 게임의 성공 횟수를 가져옵니다.
        /// </summary>
        public int GetSuccessCount(int index)
        {
            return successCountByIndex.TryGetValue(index, out int count) ? count : 0;
        }

        /// <summary>
        /// 특정 게임의 성공률을 가져옵니다.
        /// </summary>
        public float GetSuccessRate(int index)
        {
            int playCount = GetPlayCount(index);
            if (playCount == 0) return 0f;
            return (float)GetSuccessCount(index) / playCount;
        }

        #endregion

        #region Debug Methods

        /// <summary>
        /// 디버그용: 현재 게임을 강제로 성공 처리합니다.
        /// </summary>
        public void DebugForceSuccess()
        {
            if (currentMicrogame != null)
            {
                HandleMicrogameResult(true);
            }
        }

        /// <summary>
        /// 디버그용: 현재 게임을 강제로 실패 처리합니다.
        /// </summary>
        public void DebugForceFailure()
        {
            if (currentMicrogame != null)
            {
                HandleMicrogameResult(false);
            }
        }

        #endregion
        
        /// <summary>
        /// 컴포넌트가 파괴될 때 정리 작업
        /// </summary>
        private void OnDestroy()
        {
            EndCurrentMicrogame();
            ClearPool();
            
            if (poolParent != null)
            {
                Destroy(poolParent.gameObject);
            }
            
            Debug.Log("[MicrogameManager] 프리팹 풀 정리 완료");
        }
    }
}