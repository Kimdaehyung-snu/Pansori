using UnityEngine;
using Pansori.Microgames;

namespace Pansori.Microgames.Templates
{
    /// <summary>
    /// 미니게임 템플릿 스크립트
    /// 새 미니게임을 만들 때 이 템플릿을 복사하여 사용하세요.
    /// 
    /// 사용 방법:
    /// 1. 이 파일을 복사하여 새 스크립트를 만듭니다 (예: MG_YourGame_Manager.cs)
    /// 2. 클래스 이름을 변경합니다
    /// 3. ResetGameState() 메서드를 구현합니다
    /// 4. OnGameStart() 메서드에 게임 시작 로직을 추가합니다
    /// 5. 게임 로직에 따라 ReportResult(true/false)를 호출합니다
    /// </summary>
    public class MicrogameTemplate : MicrogameBase
    {
        [Header("게임 오브젝트")]
        [SerializeField] private GameObject player; // 예시: 플레이어 오브젝트
        
        [Header("게임 설정")]
        [SerializeField] private float gameDuration = 5f; // 예시: 게임 지속 시간
        
        [Header("헬퍼 컴포넌트")]
        [SerializeField] private MicrogameTimer timer; // 타이머 (선택사항)
        [SerializeField] private MicrogameInputHandler inputHandler; // 입력 핸들러 (선택사항)
        [SerializeField] private MicrogameUILayer uiLayer; // UI 레이어 (선택사항)
        
        // 초기 위치 저장용 변수들
        private Vector3 playerStartPos;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 초기 위치 저장
            if (player != null)
            {
                playerStartPos = player.transform.position;
            }
            
            // 헬퍼 컴포넌트 자동 찾기 (Inspector에서 설정하지 않은 경우)
            if (timer == null)
            {
                timer = GetComponentInChildren<MicrogameTimer>();
            }
            
            if (inputHandler == null)
            {
                inputHandler = GetComponentInChildren<MicrogameInputHandler>();
            }
            
            if (uiLayer == null)
            {
                uiLayer = GetComponentInChildren<MicrogameUILayer>();
            }
        }
        
        public override void OnGameStart(int difficulty, float speed)
        {
            base.OnGameStart(difficulty, speed);
            
            // 난이도에 따른 설정 조정 예시
            // gameDuration = 5f - (difficulty * 0.5f); // 난이도가 높을수록 시간 단축
            
            // 타이머 시작 (있는 경우)
            if (timer != null)
            {
                timer.StartTimer(gameDuration, speed);
                timer.OnTimerEnd += OnTimeUp;
            }
            
            // 입력 핸들러 이벤트 구독 (있는 경우)
            if (inputHandler != null)
            {
                inputHandler.OnKeyPressed += HandleKeyPress;
                inputHandler.OnMouseClick += HandleMouseClick;
            }
            
            // TODO: 여기에 게임 시작 로직을 추가하세요
            Debug.Log($"[MicrogameTemplate] 게임 시작 - 난이도: {difficulty}, 배속: {speed}");
        }
        
        /// <summary>
        /// 시간 초과 처리
        /// </summary>
        private void OnTimeUp()
        {
            ReportResult(false); // 시간 초과 = 실패
        }
        
        /// <summary>
        /// 키 입력 처리 예시
        /// </summary>
        private void HandleKeyPress(KeyCode key)
        {
            // TODO: 키 입력 처리 로직 추가
            if (key == KeyCode.Space)
            {
                // 예시: 스페이스바 입력 처리
            }
        }
        
        /// <summary>
        /// 마우스 클릭 처리 예시
        /// </summary>
        private void HandleMouseClick(int button, Vector3 worldPos)
        {
            // TODO: 마우스 클릭 처리 로직 추가
        }
        
        /// <summary>
        /// 성공 조건 달성 시 호출
        /// </summary>
        private void OnSuccess()
        {
            ReportResult(true);
        }
        
        /// <summary>
        /// 실패 조건 달성 시 호출
        /// </summary>
        private void OnFailure()
        {
            ReportResult(false);
        }
        
        protected override void ResetGameState()
        {
            // 모든 오브젝트를 초기 위치로 복원
            if (player != null)
            {
                player.transform.position = playerStartPos;
            }
            
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
                inputHandler.OnMouseClick -= HandleMouseClick;
            }
            
            // TODO: 여기에 추가적인 리셋 로직을 구현하세요
            // 예: 변수 초기화, 오브젝트 상태 리셋 등
        }
    }
}
