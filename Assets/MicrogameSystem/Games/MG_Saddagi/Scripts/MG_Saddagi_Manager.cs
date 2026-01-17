using UnityEngine;
using Pansori.Microgames;

namespace Pansori.Microgames.Games
{
    /// <summary>
    /// 새 미니게임
    /// 
    /// TODO: 게임 설명을 여기에 작성하세요.
    /// </summary>
    public class MG_Saddagi_Manager : MicrogameBase
    {
        [Header("게임 오브젝트")]
        // TODO: 게임 오브젝트 참조를 추가하세요
        
        [Header("게임 설정")]
        // TODO: 게임 설정 변수를 추가하세요
        
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
            
            // TODO: 게임 시작 로직을 추가하세요
            
            // 타이머 시작 예시
            if (timer != null)
            {
                timer.StartTimer(5f, speed);
                timer.OnTimerEnd += OnTimeUp;
            }
            
            // 입력 핸들러 이벤트 구독 예시
            if (inputHandler != null)
            {
                inputHandler.OnKeyPressed += HandleKeyPress;
            }
        }
        
        private void HandleKeyPress(KeyCode key)
        {
            // TODO: 키 입력 처리 로직을 추가하세요
        }
        
        private void OnTimeUp()
        {
            // TODO: 시간 초과 처리 로직을 추가하세요
            ReportResult(false); // 또는 true
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
            // TODO: 모든 오브젝트를 초기 상태로 리셋하는 로직을 추가하세요
            
            // 타이머 중지
            if (timer != null)
            {
                timer.Stop();
                timer.OnTimerEnd -= OnTimeUp;
            }
            
            // 입력 핸들러 이벤트 구독 해제
            if (inputHandler != null)
            {
                inputHandler.OnKeyPressed -= HandleKeyPress;
            }
        }
    }
}
