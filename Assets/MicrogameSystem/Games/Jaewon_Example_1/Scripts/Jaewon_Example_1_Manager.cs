using UnityEngine;
using UnityEngine.UI;
using CautionPotion.Microgames;
using TMPro;

namespace CautionPotion.Microgames.Games
{
    /// <summary>
    /// 미니게임 예시1
    /// 시간 안에 키보드를 10번 클릭해야 클리어되는 게임
    /// </summary>
    public class Jaewon_Example_1_Manager : MicrogameBase
    {
        [Header("게임 오브젝트")]
        // TODO: 게임 오브젝트 참조를 추가하세요
        
        [Header("게임 설정")]
        [SerializeField] private int targetClicks = 10; // 목표 클릭 횟수
        [SerializeField] private TMP_Text clickCountText; // 클릭 횟수 표시 UI
        [SerializeField] private TMP_Text timerText; // 남은 시간 표시 UI
        
        /// <summary>
        /// 현재 클릭 횟수
        /// </summary>
        private int clickCount = 0;
        
        [Header("헬퍼 컴포넌트")]
        [SerializeField] private MicrogameTimer timer;
        [SerializeField] private MicrogameInputHandler inputHandler;
        [SerializeField] private MicrogameUILayer uiLayer;
        
        protected override void Awake()
        {
            base.Awake();
            // TODO: 초기화 로직을 추가하세요
        }
        
        public override void OnGameStart(int difficulty, float speed)
        {
            base.OnGameStart(difficulty, speed);
            
            // 클릭 카운터 초기화
            clickCount = 0;
            UpdateClickCountUI();
            
            // 타이머 시작
            if (timer != null)
            {
                timer.StartTimer(5f, speed);
                timer.OnTimerEnd += OnTimeUp;
                UpdateTimerUI(); // 초기 시간 표시
            }
            
            // 입력 핸들러 이벤트 구독
            if (inputHandler != null)
            {
                inputHandler.OnKeyPressed += HandleKeyPress;
            }
        }
        
        private void HandleKeyPress(KeyCode key)
        {
            // 이미 종료된 경우 무시
            if (isGameEnded)
                return;
            
            // 클릭 횟수 증가
            clickCount++;
            UpdateClickCountUI();
            
            // 목표 달성 시 성공 처리
            if (clickCount >= targetClicks)
            {
                OnSuccess();
            }
        }
        
        /// <summary>
        /// 클릭 횟수 UI 업데이트
        /// </summary>
        private void UpdateClickCountUI()
        {
            if (clickCountText != null)
            {
                clickCountText.text = $"{clickCount}/{targetClicks} 클릭";
            }
            Debug.Log($"[Jaewon_Example_1] 클릭 횟수: {clickCount}/{targetClicks}");
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
        
        private void Update()
        {
            // 게임이 진행 중일 때만 시간 업데이트
            if (!isGameEnded && timer != null && timer.IsRunning)
            {
                UpdateTimerUI();
            }
        }
        
        private void OnTimeUp()
        {
            // 시간 초과 시 실패 처리
            // 목표 달성하지 못했으면 실패
            if (clickCount < targetClicks)
            {
                OnFailure();
            }
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
            // 클릭 카운터 리셋
            clickCount = 0;
            UpdateClickCountUI();
            
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
            
            // 입력 핸들러 이벤트 구독 해제
            if (inputHandler != null)
            {
                inputHandler.OnKeyPressed -= HandleKeyPress;
            }
        }
    }
}
