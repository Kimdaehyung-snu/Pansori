using UnityEngine;
using UnityEngine.UI;
using Pansori.Microgames;
using TMPro;

namespace Pansori.Microgames.Games
{
    /// <summary>
    /// 흥부 톱질 미니게임
    /// 좌우 화살표를 번갈아 눌러 박을 타는 게임
    /// </summary>
    public class Jaewon_GAME_1 : MicrogameBase
    {
        [Header("캐릭터 스프라이트")]
        [SerializeField] private GameObject heungbu; // 흥부
        [SerializeField] private GameObject heungbuWife; // 흥부 아내
        [SerializeField] private GameObject nolbu; // 놀부 (실패 시 표시)
        
        [Header("톱질 오브젝트")]
        [SerializeField] private Transform saw; // 톱
        [SerializeField] private GameObject gourdWhole; // 온전한 박
        [SerializeField] private GameObject gourdBroken; // 깨진 박
        
        [Header("게임 설정")]
        [SerializeField] private int targetClicks = 10; // 목표 클릭 횟수
        [SerializeField] private float sawMoveDistance = 0.5f; // 톱 좌우 이동 거리
        [SerializeField] private float gourdRisePerClick = 0.1f; // 클릭당 박 상승 거리
        [SerializeField] private float sawMoveSpeed = 10f; // 톱 이동 속도
        
        [Header("UI")]
        [SerializeField] private TMP_Text clickCountText; // 클릭 횟수 표시 UI
        [SerializeField] private TMP_Text timerText; // 남은 시간 표시 UI
        
        /// <summary>
        /// 현재 게임 이름
        /// </summary>
        public override string currentGameName => "톱질";
        
        /// <summary>
        /// 현재 클릭 횟수
        /// </summary>
        private int clickCount = 0;
        
        /// <summary>
        /// 마지막으로 누른 방향 (true: 오른쪽, false: 왼쪽)
        /// </summary>
        private bool? lastDirection = null;
        
        /// <summary>
        /// 톱의 목표 위치
        /// </summary>
        private Vector3 sawTargetPosition;
        
        /// <summary>
        /// 톱의 시작 위치
        /// </summary>
        private Vector3 sawStartPosition;
        
        /// <summary>
        /// 박의 시작 위치
        /// </summary>
        private Vector3 gourdStartPosition;
        
        [Header("헬퍼 컴포넌트")]
        [SerializeField] private MicrogameTimer timer;
        [SerializeField] private MicrogameInputHandler inputHandler;
        [SerializeField] private MicrogameUILayer uiLayer;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 시작 위치 저장
            if (saw != null)
            {
                sawStartPosition = saw.localPosition;
                sawTargetPosition = sawStartPosition;
            }
            
            if (gourdWhole != null)
            {
                gourdStartPosition = gourdWhole.transform.localPosition;
            }
        }
        
        public override void OnGameStart(int difficulty, float speed)
        {
            base.OnGameStart(difficulty, speed);
            
            // 상태 초기화
            clickCount = 0;
            lastDirection = null;
            UpdateClickCountUI();
            
            // 스프라이트 초기 상태 설정
            SetupInitialState();
            
            // 타이머 시작
            if (timer != null)
            {
                timer.StartTimer(5f, speed);
                timer.OnTimerEnd += OnTimeUp;
                UpdateTimerUI();
            }
            
            // 입력 핸들러 이벤트 구독
            if (inputHandler != null)
            {
                inputHandler.OnKeyPressed += HandleKeyPress;
            }
        }
        
        /// <summary>
        /// 게임 시작 시 초기 스프라이트 상태 설정
        /// </summary>
        private void SetupInitialState()
        {
            // 흥부, 흥부 아내 표시
            if (heungbu != null) heungbu.SetActive(true);
            if (heungbuWife != null) heungbuWife.SetActive(true);
            
            // 놀부 숨김
            if (nolbu != null) nolbu.SetActive(false);
            
            // 온전한 박 표시, 깨진 박 숨김
            if (gourdWhole != null) gourdWhole.SetActive(true);
            if (gourdBroken != null) gourdBroken.SetActive(false);
            
            // 톱과 박 위치 초기화
            if (saw != null)
            {
                saw.localPosition = sawStartPosition;
                sawTargetPosition = sawStartPosition;
            }
            
            if (gourdWhole != null)
            {
                gourdWhole.transform.localPosition = gourdStartPosition;
            }
        }
        
        private void HandleKeyPress(KeyCode key)
        {
            // 이미 종료된 경우 무시
            if (isGameEnded)
                return;
            
            // 좌우 화살표만 처리
            bool isRightArrow = (key == KeyCode.RightArrow);
            bool isLeftArrow = (key == KeyCode.LeftArrow);
            
            if (!isRightArrow && !isLeftArrow)
                return;
            
            bool currentDirection = isRightArrow;
            
            // 번갈아 입력 체크 (첫 입력이거나 이전과 다른 방향)
            if (lastDirection == null || lastDirection.Value != currentDirection)
            {
                // 유효한 입력
                clickCount++;
                lastDirection = currentDirection;
                
                // 톱 이동
                MoveSaw(currentDirection);
                
                // 박 상승
                MoveGourdUp();
                
                UpdateClickCountUI();
                
                Debug.Log($"[Jaewon_GAME_1] 유효 입력! 방향: {(currentDirection ? "오른쪽" : "왼쪽")}, 횟수: {clickCount}/{targetClicks}");
                
                // 목표 달성 시 성공 처리
                if (clickCount >= targetClicks)
                {
                    OnSuccess();
                }
            }
            else
            {
                // 같은 방향 연속 입력은 무시
                Debug.Log($"[Jaewon_GAME_1] 같은 방향 입력 무시: {(currentDirection ? "오른쪽" : "왼쪽")}");
            }
        }
        
        /// <summary>
        /// 톱을 좌우로 이동
        /// </summary>
        private void MoveSaw(bool moveRight)
        {
            if (saw == null) return;
            
            float offsetX = moveRight ? sawMoveDistance : -sawMoveDistance;
            sawTargetPosition = new Vector3(
                sawStartPosition.x + offsetX,
                sawStartPosition.y,
                sawStartPosition.z
            );
        }
        
        /// <summary>
        /// 박을 위로 이동
        /// </summary>
        private void MoveGourdUp()
        {
            if (gourdWhole == null) return;
            
            Vector3 currentPos = gourdWhole.transform.localPosition;
            gourdWhole.transform.localPosition = new Vector3(
                currentPos.x,
                currentPos.y + gourdRisePerClick,
                currentPos.z
            );
        }
        
        /// <summary>
        /// 클릭 횟수 UI 업데이트
        /// </summary>
        private void UpdateClickCountUI()
        {
            if (clickCountText != null)
            {
                clickCountText.text = $"{clickCount}/{targetClicks}";
            }
            Debug.Log($"[Jaewon_GAME_1] 클릭 횟수: {clickCount}/{targetClicks}");
        }
        
        /// <summary>
        /// 남은 시간 UI 업데이트
        /// </summary>
        private void UpdateTimerUI()
        {
            if (timerText != null && timer != null)
            {
                float remainingTime = timer.GetRemainingTime();
                timerText.text = $"남은 시간: {remainingTime:F1}초";
            }
        }
        
        private void Update()
        {
            // 게임이 진행 중일 때만 업데이트
            if (!isGameEnded)
            {
                // 시간 UI 업데이트
                if (timer != null && timer.IsRunning)
                {
                    UpdateTimerUI();
                }
                
                // 톱 부드러운 이동
                if (saw != null)
                {
                    saw.localPosition = Vector3.Lerp(
                        saw.localPosition,
                        sawTargetPosition,
                        Time.deltaTime * sawMoveSpeed
                    );
                }
            }
        }
        
        private void OnTimeUp()
        {
            // 시간 초과 시 실패 처리
            if (clickCount < targetClicks)
            {
                OnFailure();
            }
        }
        
        private void OnSuccess()
        {
            Debug.Log("[Jaewon_GAME_1] 성공!");
            
            // 박이 깨지는 연출
            if (gourdWhole != null) gourdWhole.SetActive(false);
            if (gourdBroken != null) gourdBroken.SetActive(true);
            
            // 흥부 부부 유지 (이미 표시 중)
            
            ReportResult(true);
        }
        
        private void OnFailure()
        {
            Debug.Log("[Jaewon_GAME_1] 실패!");
            
            // 흥부 부부 숨기고 놀부 표시
            if (heungbu != null) heungbu.SetActive(false);
            if (heungbuWife != null) heungbuWife.SetActive(false);
            if (nolbu != null) nolbu.SetActive(true);
            
            ReportResult(false);
        }
        
        protected override void ResetGameState()
        {
            // 상태 리셋
            clickCount = 0;
            lastDirection = null;
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
            
            // 스프라이트 초기 상태로 복원
            SetupInitialState();
        }
    }
}
