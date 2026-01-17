using System;
using UnityEngine;
using Pansori.Microgames;
using TMPro;

namespace Pansori.Microgames.Games
{
    /// <summary>
    /// 부러진 제비의 다리를 올바른 방향으로 돌려놓아라!
    /// 
    /// TODO: 게임 설명을 여기에 작성하세요.
    /// </summary>
    public class MG02_FixLeg_Manager : MicrogameBase
    {
        [Header("게임 오브젝트")]
        // TODO: 게임 오브젝트 참조를 추가하세요
        [SerializeField] private RectTransform legTransform;
        [SerializeField] private RectTransform canvasTransform;
        [SerializeField] private TMP_Text timerText; // 남은 시간 표시 UI
        [SerializeField] private GameObject successResultPanel;
        [SerializeField] private GameObject failResultPanel;
        [SerializeField] private RectTransform rotateAreaRect;
        [SerializeField] private GameObject legGuideLineGameObject;
        [SerializeField] private AudioClip legWheelSound;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip failSound;
        
        [Header("게임 설정")]
        // TODO: 게임 설정 변수를 추가하세요
        [SerializeField] private float successAngleCondition = 10f;
        
        [Header("결과 연출 설정")]
        [SerializeField] private bool useCustomResultAnimation = true; // 커스텀 결과 연출 사용 여부
        [SerializeField] private float resultDisplayDelay = 0.5f; // 결과 표시 전 연출 시간

        [Header("헬퍼 컴포넌트")]
        [SerializeField] private MicrogameTimer timer;
        [SerializeField] private MicrogameInputHandler inputHandler;
        [SerializeField] private MicrogameUILayer uiLayer;
        
        /// <summary>
        /// 현재 게임 이름
        /// </summary>
        public override string currentGameName => "고쳐라!";
        public override string controlDescription => "제비 다리를 드래그해 기준선에 맞추세요!";
        
        private bool isDragging = false;
        private bool gameCleared = false;
        private Quaternion initialRotation; // 초기 회전값 저장
        private float angleOffset;

        
        protected override void Awake()
        {
            base.Awake();
            
            // TODO: 초기화 로직을 추가하세요
            initialRotation = legTransform.rotation;
            
        }
        
        public override void OnGameStart(int difficulty, float speed)
        {
            base.OnGameStart(difficulty, speed);
            
            // TODO: 게임 시작 로직을 추가하세요
            legTransform.rotation = initialRotation;
            gameCleared = false;
            // 타이머 시작 예시
            if (timer != null)
            {
                timer.StartTimer(5f, speed);
                timer.OnTimerEnd += OnTimeUp;
                UpdateTimerUI(); // 초기 시간 표시
            }
            
            // 입력 핸들러 이벤트 구독 예시
            if (inputHandler != null)
            {
                inputHandler.OnMouseDragStart += HandleDragStart;
                inputHandler.OnMouseDrag += HandleDrag;
                inputHandler.OnMouseDragEnd += HandleDragEnd;
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

        private void HandleDragStart(Vector3 startPos)
        {
            angleOffset = legTransform.eulerAngles.z - GetMouseAngle(startPos);
            if (legGuideLineGameObject.activeSelf)
            {
                legGuideLineGameObject.SetActive(false);    
            }
            SoundManager.Instance.SFXPlay(legWheelSound.name,legWheelSound); 
        }
        
        // 2. 드래그 중: 회전 로직 실행
        private void HandleDrag(Vector3 startPos, Vector3 currentPos)
        {
            if (gameCleared)
            {
                return;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(rotateAreaRect, Input.mousePosition, null))
            {
                RotateLegToMouse(currentPos);
            }
    
        }

        private void HandleDragEnd(Vector3 endPos)
        {
            if (gameCleared)
            {
                return;
            }
            CheckHealed();
        }



        void CheckHealed()
        {
            //현재 각도 확인
            float currentZ = legTransform.eulerAngles.z;
            Debug.Log($"currentZ : {currentZ}");
            
            // 0~360도를 -180~180도로 변환 (판정 편의성)
            
            // 오차 범위 n도 이내면 성공

            if (-20f <Mathf.Abs(currentZ)&& Mathf.Abs(currentZ)< 20f) 
            {
                Debug.Log("제비 다리 치료 완료! 🩹");
            
                // 성공 시 각도를 0으로 딱 맞춰주기
                legTransform.rotation = Quaternion.Euler(0, 0, 0);
            
                // 더 이상 드래그 안 되게
                gameCleared = true; 
                
                // 목표 달성 성공 처리
                OnSuccess();
            }
        }

        void RotateLegToMouse(Vector3 currentPos)
        {
            // 오프셋 더해서 회전 적용
            float currentMouseAngle = GetMouseAngle(currentPos);
            legTransform.rotation = Quaternion.Euler(0, 0, currentMouseAngle + angleOffset);
        }
      
        private void OnTimeUp()
        {
            // TODO: 시간 초과 처리 로직을 추가하세요
            if (gameCleared==false)
            {
                OnFailure();
            }
        }
        
        private void OnSuccess()
        {
   
            if (useCustomResultAnimation && useResultAnimation)
            {
                ReportResultWithAnimation(true);
            }
            else
            {
                ReportResult(true);
            }
        }
        
        private void OnFailure()
        {
            if (useCustomResultAnimation && useResultAnimation)
            {
                ReportResultWithAnimation(false);
            }
            else
            {
                ReportResult(false);
            }
        }
        
        protected override void ResetGameState()
        {
            // TODO: 모든 오브젝트를 초기 상태로 리셋하는 로직을 추가하세요
            successResultPanel.SetActive(false);
            failResultPanel.SetActive(false);
            legGuideLineGameObject.SetActive(true);
            
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
                inputHandler.OnMouseDrag += HandleDrag;
            }
        }

        /// <summary>
        /// 결과 애니메이션을 오버라이드하여 게임별 커스텀 연출을 추가합니다.
        /// </summary>
        protected override void PlayResultAnimation(bool success, System.Action onComplete = null)
        {
            if (success)
            {
                // 성공 시: 성공 패널 열기
                Debug.Log("[Jaewon_GAME_1] 성공 커스텀 연출 시작");
                StartCoroutine(PlaySuccessResultAnimation(onComplete));
            }
            else
            {
                // 실패 시: 실패 패널 열기
                Debug.Log("[Jaewon_GAME_1] 실패 커스텀 연출 시작");
                StartCoroutine(PlayFailureResultAnimation(onComplete));
            }
        }

        /// <summary>
        /// 성공 결과 애니메이션
        /// </summary>
        private System.Collections.IEnumerator PlaySuccessResultAnimation(System.Action onComplete)
        {
            //패널열기
            successResultPanel.SetActive(true);
            //사운드재생
            SoundManager.Instance.SFXPlay(successSound.name, successSound);
            // 결과 표시 유지
            yield return new WaitForSeconds(resultDisplayDelay);
            // 완료 콜백
            onComplete?.Invoke();
        }

        /// <summary>
        /// 실패 결과 애니메이션 
        /// </summary>
        private System.Collections.IEnumerator PlayFailureResultAnimation(System.Action onComplete)
        {
            //패널열기
            failResultPanel.SetActive(true);
            //사운드재생
            SoundManager.Instance.SFXPlay(failSound.name, failSound);
            // 결과 표시 유지
            yield return new WaitForSeconds(resultDisplayDelay);
            // 완료 콜백
            onComplete?.Invoke();
        }
        
        // 마우스 위치를 입력받아 다리와의 각도(도)를 반환하는 함수
        private float GetMouseAngle(Vector3 targetPosition)
        {
            Vector3 mouseWorldPos;
    
            // 스크린 좌표 -> 월드 좌표 변환 
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPosition);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasTransform, screenPos, null, out mouseWorldPos
            );

            // 각도 계산 (Atan2)
            Vector3 direction = mouseWorldPos - legTransform.position;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
    }
}
