using UnityEngine;

namespace CautionPotion.Microgames
{
    /// <summary>
    /// 미니게임 베이???�래??
    /// 모든 미니게임?� ???�래?��? ?�속받아 구현?�는 것을 권장?�니??
    /// </summary>
    public abstract class MicrogameBase : MonoBehaviour, IMicrogame
    {
        /// <summary>
        /// 결과 ?�달 ?�벤??
        /// </summary>
        public System.Action<bool> OnResultReported { get; set; }
        
        /// <summary>
        /// 게임??종료?�었?��? ?��? (중복 결과 보고 방�?)
        /// </summary>
        protected bool isGameEnded = false;
        
        /// <summary>
        /// ?�재 ?�이??(1~3)
        /// </summary>
        protected int currentDifficulty = 1;
        
        /// <summary>
        /// ?�재 배속 (1.0f ?�상)
        /// </summary>
        protected float currentSpeed = 1.0f;
        
        /// <summary>
        /// Awake 메서??(?�버?�이??가??
        /// ?�브 ?�래?�에??초기??로직??추�??????�습?�다.
        /// </summary>
        protected virtual void Awake()
        {
            // ?�브 ?�래?�에???�요???�버?�이??
        }
        
        /// <summary>
        /// 게임 ?�작 ??매니?�가 ?�출?�니??
        /// </summary>
        /// <param name="difficulty">?�이??(1~3)</param>
        /// <param name="speed">배속 (1.0f ?�상)</param>
        public virtual void OnGameStart(int difficulty, float speed)
        {
            isGameEnded = false;
            currentDifficulty = difficulty;
            currentSpeed = speed;
            
            Debug.Log($"[MicrogameBase] 게임 ?�작 - ?�이?? {difficulty}, 배속: {speed}");
        }
        
        /// <summary>
        /// 결과�?매니?�?�게 보고?�니??
        /// </summary>
        /// <param name="success">true: ?�공 / false: ?�패</param>
        protected void ReportResult(bool success)
        {
            if (isGameEnded)
            {
                Debug.LogWarning($"[MicrogameBase] ?��? 종료??게임?�서 결과�?보고?�려�??�도?�습?�다. (?�공: {success})");
                return;
            }
            
            isGameEnded = true;
            OnGameEnd();
            OnResultReported?.Invoke(success);
            
            Debug.Log($"[MicrogameBase] 결과 보고 - ?�공: {success}");
        }
        
        /// <summary>
        /// 게임 종료 ???�출?�니?? (?�버?�이??가??
        /// </summary>
        protected virtual void OnGameEnd()
        {
            // ?�브 ?�래?�에???�요???�버?�이??
        }
        
        /// <summary>
        /// 게임 ?�태�?초기값으�?리셋?�니?? (추상 메서??- 반드??구현 ?�요)
        /// </summary>
        protected abstract void ResetGameState();
        
        /// <summary>
        /// ?�브?�트가 비활?�화?????�동?�로 ?�태�?리셋?�니??
        /// </summary>
        private void OnDisable()
        {
            ResetGameState();
        }
    }
}
