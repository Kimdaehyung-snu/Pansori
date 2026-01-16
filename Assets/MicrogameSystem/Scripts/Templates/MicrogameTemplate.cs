using UnityEngine;
using CautionPotion.Microgames;

namespace CautionPotion.Microgames.Templates
{
    /// <summary>
    /// 미니게임 ?�플�??�크립트
    /// ??미니게임??만들 ?????�플릿을 복사?�여 ?�용?�세??
    /// 
    /// ?�용 방법:
    /// 1. ???�일??복사?�여 ???�크립트�?만듭?�다 (?? MG_YourGame_Manager.cs)
    /// 2. ?�래???�름??변경합?�다
    /// 3. ResetGameState() 메서?��? 구현?�니??
    /// 4. OnGameStart() 메서?�에 게임 ?�작 로직??추�??�니??
    /// 5. 게임 로직???�라 ReportResult(true/false)�??�출?�니??
    /// </summary>
    public class MicrogameTemplate : MicrogameBase
    {
        [Header("게임 ?�브?�트")]
        [SerializeField] private GameObject player; // ?�시: ?�레?�어 ?�브?�트
        
        [Header("게임 ?�정")]
        [SerializeField] private float gameDuration = 5f; // ?�시: 게임 지???�간
        
        [Header("?�퍼 컴포?�트")]
        [SerializeField] private MicrogameTimer timer; // ?�?�머 (?�택?�항)
        [SerializeField] private MicrogameInputHandler inputHandler; // ?�력 ?�들??(?�택?�항)
        [SerializeField] private MicrogameUILayer uiLayer; // UI ?�이??(?�택?�항)
        
        // 초기 ?�치 ?�?�용 변?�들
        private Vector3 playerStartPos;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 초기 ?�치 ?�??
            if (player != null)
            {
                playerStartPos = player.transform.position;
            }
            
            // ?�퍼 컴포?�트 ?�동 찾기 (Inspector?�서 ?�정?��? ?��? 경우)
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
            
            // ?�이?�에 ?�른 ?�정 조정 ?�시
            // gameDuration = 5f - (difficulty * 0.5f); // ?�이?��? ?�을?�록 ?�간 ?�축
            
            // ?�?�머 ?�작 (?�는 경우)
            if (timer != null)
            {
                timer.StartTimer(gameDuration, speed);
                timer.OnTimerEnd += OnTimeUp;
            }
            
            // ?�력 ?�들???�벤??구독 (?�는 경우)
            if (inputHandler != null)
            {
                inputHandler.OnKeyPressed += HandleKeyPress;
                inputHandler.OnMouseClick += HandleMouseClick;
            }
            
            // TODO: ?�기??게임 ?�작 로직??추�??�세??
            Debug.Log($"[MicrogameTemplate] 게임 ?�작 - ?�이?? {difficulty}, 배속: {speed}");
        }
        
        /// <summary>
        /// ?�간 초과 처리
        /// </summary>
        private void OnTimeUp()
        {
            ReportResult(false); // ?�간 초과 = ?�패
        }
        
        /// <summary>
        /// ???�력 처리 ?�시
        /// </summary>
        private void HandleKeyPress(KeyCode key)
        {
            // TODO: ???�력 처리 로직 추�?
            if (key == KeyCode.Space)
            {
                // ?�시: ?�페?�스�??�력 처리
            }
        }
        
        /// <summary>
        /// 마우???�릭 처리 ?�시
        /// </summary>
        private void HandleMouseClick(int button, Vector3 worldPos)
        {
            // TODO: 마우???�릭 처리 로직 추�?
        }
        
        /// <summary>
        /// ?�공 조건 ?�성 ???�출
        /// </summary>
        private void OnSuccess()
        {
            ReportResult(true);
        }
        
        /// <summary>
        /// ?�패 조건 ?�성 ???�출
        /// </summary>
        private void OnFailure()
        {
            ReportResult(false);
        }
        
        protected override void ResetGameState()
        {
            // 모든 ?�브?�트�?초기 ?�치�?복원
            if (player != null)
            {
                player.transform.position = playerStartPos;
            }
            
            // ?�?�머 중�?
            if (timer != null)
            {
                timer.Stop();
                timer.OnTimerEnd -= OnTimeUp;
            }
            
            // ?�력 ?�들???�벤??구독 ?�제
            if (inputHandler != null)
            {
                inputHandler.OnKeyPressed -= HandleKeyPress;
                inputHandler.OnMouseClick -= HandleMouseClick;
            }
            
            // TODO: ?�기??추�??�인 리셋 로직??구현?�세??
            // ?? 변??초기?? ?�브?�트 ?�태 리셋 ??
        }
    }
}
