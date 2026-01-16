using System;
using UnityEngine;

namespace CautionPotion.Microgames
{
    /// <summary>
    /// 미니게임 최상??매니?�
    /// 모든 미니게임???�명주기�?관리합?�다.
    /// </summary>
    public class MicrogameManager : MonoBehaviour
    {
        [Header("미니게임 ?�리??목록")]
        [SerializeField] private GameObject[] microgamePrefabs;
        
        [Header("?�정")]
        [SerializeField] private bool destroyOnEnd = true; // 게임 종료 ???�스?�스 ?�괴 ?��?
        
        /// <summary>
        /// ?�재 ?�행 중인 미니게임 ?�스?�스
        /// </summary>
        private GameObject currentMicrogameInstance;
        
        /// <summary>
        /// ?�재 ?�행 중인 미니게임??IMicrogame 컴포?�트
        /// </summary>
        private IMicrogame currentMicrogame;
        
        /// <summary>
        /// 미니게임 결과 ?�벤??(?��??�서 구독 가??
        /// </summary>
        public event Action<bool> OnMicrogameResult;
        
        /// <summary>
        /// ?�재 미니게임???�행 중인지 ?��?
        /// </summary>
        public bool IsMicrogameRunning => currentMicrogameInstance != null && currentMicrogameInstance.activeSelf;
        
        /// <summary>
        /// ?�덱?�로 미니게임???�작?�니??
        /// </summary>
        /// <param name="index">?�리??배열 ?�덱??/param>
        /// <param name="difficulty">?�이??(1~3)</param>
        /// <param name="speed">배속 (1.0f ?�상)</param>
        public void StartMicrogame(int index, int difficulty, float speed)
        {
            if (index < 0 || index >= microgamePrefabs.Length)
            {
                Debug.LogError($"[MicrogameManager] ?�효?��? ?��? ?�덱?? {index} (배열 ?�기: {microgamePrefabs.Length})");
                return;
            }
            
            if (microgamePrefabs[index] == null)
            {
                Debug.LogError($"[MicrogameManager] ?�덱??{index}???�리?�이 null?�니??");
                return;
            }
            
            StartMicrogame(microgamePrefabs[index], difficulty, speed);
        }
        
        /// <summary>
        /// ?�리?�으�?미니게임???�작?�니??
        /// </summary>
        /// <param name="prefab">미니게임 ?�리??/param>
        /// <param name="difficulty">?�이??(1~3)</param>
        /// <param name="speed">배속 (1.0f ?�상)</param>
        public void StartMicrogame(GameObject prefab, int difficulty, float speed)
        {
            if (prefab == null)
            {
                Debug.LogError("[MicrogameManager] ?�리?�이 null?�니??");
                return;
            }
            
            // ?�이??�??�도 검�?
            difficulty = Mathf.Clamp(difficulty, 1, 3);
            speed = Mathf.Max(1.0f, speed);
            
            // 기존 미니게임???�행 중이�?종료
            if (IsMicrogameRunning)
            {
                Debug.LogWarning("[MicrogameManager] ?��? ?�행 중인 미니게임???�습?�다. 기존 미니게임??종료?�니??");
                EndCurrentMicrogame();
            }
            
            // ?�리???�스?�스??
            currentMicrogameInstance = Instantiate(prefab);
            currentMicrogameInstance.name = prefab.name + "_Instance";
            
            // IMicrogame 컴포?�트 찾기
            currentMicrogame = currentMicrogameInstance.GetComponent<IMicrogame>();
            if (currentMicrogame == null)
            {
                Debug.LogError($"[MicrogameManager] ?�리??'{prefab.name}'??IMicrogame ?�터?�이?��? 구현??컴포?�트가 ?�습?�다.");
                Destroy(currentMicrogameInstance);
                currentMicrogameInstance = null;
                return;
            }
            
            // 결과 ?�벤??구독
            currentMicrogame.OnResultReported = HandleMicrogameResult;
            
            // 게임 ?�작
            Debug.Log($"[MicrogameManager] 미니게임 ?�작: {prefab.name} (?�이?? {difficulty}, 배속: {speed})");
            currentMicrogame.OnGameStart(difficulty, speed);
        }
        
        /// <summary>
        /// ?�덤 미니게임???�작?�니??
        /// </summary>
        /// <param name="difficulty">?�이??(1~3)</param>
        /// <param name="speed">배속 (1.0f ?�상)</param>
        public void StartRandomMicrogame(int difficulty, float speed)
        {
            if (microgamePrefabs == null || microgamePrefabs.Length == 0)
            {
                Debug.LogError("[MicrogameManager] 미니게임 ?�리?�이 ?�정?��? ?�았?�니??");
                return;
            }
            
            int randomIndex = UnityEngine.Random.Range(0, microgamePrefabs.Length);
            StartMicrogame(randomIndex, difficulty, speed);
        }
        
        /// <summary>
        /// ?�재 ?�행 중인 미니게임??종료?�니??
        /// </summary>
        public void EndCurrentMicrogame()
        {
            if (currentMicrogameInstance == null)
            {
                return;
            }
            
            // 결과 ?�벤??구독 ?�제
            if (currentMicrogame != null)
            {
                currentMicrogame.OnResultReported = null;
            }
            
            // ?�스?�스 ?�리
            if (destroyOnEnd)
            {
                Destroy(currentMicrogameInstance);
            }
            else
            {
                currentMicrogameInstance.SetActive(false);
            }
            
            currentMicrogameInstance = null;
            currentMicrogame = null;
            
            Debug.Log("[MicrogameManager] 미니게임 종료");
        }
        
        /// <summary>
        /// 미니게임 결과�?처리?�니??
        /// </summary>
        /// <param name="success">?�공 ?��?</param>
        private void HandleMicrogameResult(bool success)
        {
            Debug.Log($"[MicrogameManager] 미니게임 결과: {(success ? "?�공" : "?�패")}");
            
            // ?��? ?�벤???�출
            OnMicrogameResult?.Invoke(success);
            
            // 미니게임 종료
            EndCurrentMicrogame();
        }
        
        /// <summary>
        /// 컴포?�트가 ?�괴?????�리 ?�업
        /// </summary>
        private void OnDestroy()
        {
            EndCurrentMicrogame();
        }
    }
}
