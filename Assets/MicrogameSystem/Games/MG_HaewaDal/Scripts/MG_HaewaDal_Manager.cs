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
        [SerializeField] private GameObject successResult;
        [SerializeField] private GameObject failResult;

        [SerializeField] private AudioClip successTiger;
        [SerializeField] private AudioClip successRope;
        [SerializeField] private AudioClip failTiger;
        [SerializeField] private AudioClip failRope;
        
        [Header("게임 설정")] 
        
        
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
        public override string currentGameName => "올바른 동앗줄을 골라라!";
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
            //초기화
            successResult.SetActive(false);
            failResult.SetActive(false);
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
            successResult.SetActive(true);
            //사운드
            SoundManager.Instance.SFXPlay(successRope.name,successRope);
            SoundManager.Instance.SFXPlay(successTiger.name,successTiger);
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
            failResult.SetActive(true);
            //사운드
            SoundManager.Instance.SFXPlay(failRope.name,failRope);
            SoundManager.Instance.SFXPlay(failTiger.name,failTiger);
            // 결과 표시 유지
            yield return new WaitForSeconds(resultDisplayDelay);
            // 완료 콜백
            onComplete?.Invoke();
        }
    }
}
