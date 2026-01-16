using System;
using UnityEngine;

namespace CautionPotion.Microgames
{
    /// <summary>
    /// 미니게임 타이머 헬퍼 컴포넌트
    /// 난이도와 속도를 반영한 타이머를 제공합니다.
    /// </summary>
    public class MicrogameTimer : MonoBehaviour
    {
        [Header("타이머 설정")]
        [SerializeField] private float baseDuration = 5f; // 기본 지속 시간 (초)
        
        /// <summary>
        /// 현재 적용된 속도 배율
        /// </summary>
        private float speedMultiplier = 1.0f;
        
        /// <summary>
        /// 남은 시간
        /// </summary>
        private float remainingTime = 0f;
        
        /// <summary>
        /// 타이머가 실행 중인지 여부
        /// </summary>
        private bool isRunning = false;
        
        /// <summary>
        /// 타이머가 일시정지 상태인지 여부
        /// </summary>
        private bool isPaused = false;
        
        /// <summary>
        /// 타이머 종료 이벤트
        /// </summary>
        public event Action OnTimerEnd;
        
        /// <summary>
        /// 타이머 시작
        /// </summary>
        /// <param name="duration">기본 지속 시간 (초) - speed에 자동 반영됨</param>
        /// <param name="speed">속도 배율 (기본값: 1.0f)</param>
        public void StartTimer(float duration, float speed = 1.0f)
        {
            baseDuration = duration;
            speedMultiplier = Mathf.Max(0.1f, speed); // 최소 0.1배속
            remainingTime = baseDuration / speedMultiplier;
            isRunning = true;
            isPaused = false;
            
        }
        
        /// <summary>
        /// 타이머 시작 (현재 설정된 속도 사용)
        /// </summary>
        /// <param name="duration">기본 지속 시간 (초)</param>
        public void StartTimer(float duration)
        {
            StartTimer(duration, speedMultiplier);
        }
        
        /// <summary>
        /// 타이머 중지
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            isPaused = false;
            remainingTime = 0f;
        }
        
        /// <summary>
        /// 타이머 일시정지
        /// </summary>
        public void Pause()
        {
            if (isRunning && !isPaused)
            {
                isPaused = true;
                Debug.Log("[MicrogameTimer] 타이머 일시정지");
            }
        }
        
        /// <summary>
        /// 타이머 재개
        /// </summary>
        public void Resume()
        {
            if (isRunning && isPaused)
            {
                isPaused = false;
                Debug.Log("[MicrogameTimer] 타이머 재개");
            }
        }
        
        /// <summary>
        /// 남은 시간을 반환합니다.
        /// </summary>
        /// <returns>남은 시간 (초)</returns>
        public float GetRemainingTime()
        {
            return Mathf.Max(0f, remainingTime);
        }
        
        /// <summary>
        /// 타이머가 실행 중인지 여부
        /// </summary>
        public bool IsRunning => isRunning && !isPaused;
        
        /// <summary>
        /// 타이머가 일시정지 상태인지 여부
        /// </summary>
        public bool IsPaused => isPaused;
        
        private void Update()
        {
            if (isRunning && !isPaused && remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;
                
                if (remainingTime <= 0f)
                {
                    remainingTime = 0f;
                    isRunning = false;
                    OnTimerEnd?.Invoke();
                    Debug.Log("[MicrogameTimer] 타이머 종료");
                }
            }
        }
        
        /// <summary>
        /// 컴포넌트가 비활성화될 때 타이머 중지
        /// </summary>
        private void OnDisable()
        {
            Stop();
        }
    }
}
