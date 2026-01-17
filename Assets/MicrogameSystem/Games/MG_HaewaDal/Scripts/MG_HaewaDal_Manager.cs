using System;
using UnityEngine;
using Pansori.Microgames;
using TMPro;
using UnityEngine.UI;

namespace Pansori.Microgames.Games
{
    /// <summary>
    /// 새 미니게임
    /// 
    /// TODO: 게임 설명을 여기에 작성하세요.
    /// </summary>
    public class MG_HaewaDal_Manager : MicrogameBase
    {
        [Header("게임 오브젝트")] 
        [SerializeField] private TextMeshProUGUI timerText;

        [SerializeField] private Button rottenRope;
        [SerializeField] private Button goodRope;
        
        [Header("게임 설정")] 

        /// <summary>
        /// 현재 게임 이름
        /// </summary>
        public override string currentGameName => "올바른 동앗줄을 골라라!";

        [Header("헬퍼 컴포넌트")] 
        [SerializeField] private MicrogameTimer timer;
        [SerializeField] private MicrogameInputHandler inputHandler;
        [SerializeField] private MicrogameUILayer uiLayer;
        
        
        protected override void Awake()
        {
            base.Awake();

            
        }

        private void Update()
        {
            if (!isGameEnded && timer != null && timer.IsRunning)
            {
                UpdateTimerUI();
            }
        }
        /// <summary>
        /// 남은 시간 UI 업데이트
        /// </summary>
        private void UpdateTimerUI()
        {
            if (timerText != null && timer != null)
            {
                float remainingTime = timer.GetRemainingTime();
                // 소수점 첫째 자리까지 표시
                timerText.text = $"남은 시간: {remainingTime:F1}초";
            }
        }
        
        

        public override void OnGameStart(int difficulty, float speed)
        {
           
            base.OnGameStart(difficulty, speed);
            

            if (timer != null)
            {
                timer.StartTimer(5f, speed);
                timer.OnTimerEnd += OnTimeUp;
                UpdateTimerUI(); // 초기 시간 표시
            }

            //이벤트구독
            rottenRope.onClick.AddListener(OnRottenRopeClicked);
            goodRope.onClick.AddListener(OnGoodRopeClicked);
        }

        private void OnRottenRopeClicked()
        {
            OnFailure();
        }
        private void OnGoodRopeClicked()
        {
            OnSuccess();
        }
        private void OnTimeUp()
        {
            OnFailure();
        }

        private void OnSuccess()
        {
            ReportResult(true);
        }

        private void OnFailure()
        {
            ReportResult(false);
        }

        protected override void ResetGameState()
        {
            // 타이머 중지
            if (timer != null)
            {
                timer.Stop();
                timer.OnTimerEnd -= OnTimeUp;
            }
            // 타이머 UI 초기화
            if (timerText != null)
            {
                timerText.text = "남은 시간: 0.0초";
            }
            //이벤트구독해제
            rottenRope.onClick.RemoveListener(OnRottenRopeClicked);
            goodRope.onClick.RemoveListener(OnGoodRopeClicked);

        }
    }
}
