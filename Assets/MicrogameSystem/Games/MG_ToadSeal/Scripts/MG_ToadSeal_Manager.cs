using Pansori.Microgames;
using System;
using System.Collections;
using UnityEngine;

namespace Pansori.Microgames.Games
{
    /// <summary>
    /// 콩쥐팥쥐 전래동화를 모티브로 한 
    /// 두꺼비가깨진 장독대에서 흘러나오는 물을 막는 마이크로 게임
    /// 
    /// - 스페이스를 연타 해 목표 지점(장독대 깨진 부분)에 도달하면 즉시 성공
    /// - 제한 시간까지 도착하지 못하거나 maxPushbackDis 이상 멀어지면 실패
    /// </summary>
    public class MG_ToadSeal_Manager : MicrogameBase
    {
        [Header("게임 오브젝트")]
        [SerializeField] Transform toad;    // 두꺼비
        [SerializeField] Transform target;  // 깨진 장독대 부분 (판정용)

        [SerializeField] AudioClip toadClip;
        [SerializeField] AudioClip waterClip;

        [Header("게임 설정")]
        [SerializeField] float friction = 10f;   // 마찰(값↑ = 덜 미끄러짐)

        [SerializeField] float toadPushForce = 3f;   // 스페이스 1회: 왼쪽으로 버티는 힘(속도 감소량)
        [SerializeField] float waterDefaultPushForce = 15.5f;  // 물이 계속 미는 힘(초당 가속)
        [SerializeField] float currentWaterPushForce;

        [SerializeField] float maxSpeed = 30f;      // 속도 제한
        [SerializeField] float maxPushbackDis;      // 밀려날 수 있는 최대 거리(이 이상이면 실패)

        private Vector2 startPos;   // 시작 위치(리셋용)
        private float velocity;     // 현재 x축 속도

        private bool isBlocking;    // 실제 게임 진행 중 여부

        [Header("두꺼비 애니메이션 설정")]
        [SerializeField] Animator toadAnimator;
        [SerializeField] float backAnimHoldTime;
        private float backAnimHoldRemaining = 0f;
        private Coroutine animCorotine;

        /// <summary>
        /// 현재 게임 이름
        /// </summary>
        public override string currentGameName => "막아라!";

        public override string controlDescription => "스페이스바를 눌러 물을 막으세요!";

        [Header("헬퍼 컴포넌트")]
        [SerializeField] private MicrogameTimer timer;
        [SerializeField] private MicrogameInputHandler inputHandler;
        [SerializeField] private MicrogameUILayer uiLayer;

        protected override void Awake()
        {
            base.Awake();

            velocity = 0f;
            isBlocking = false;
            backAnimHoldRemaining = 0f;
            currentWaterPushForce = 0f;

            if (toad != null)
            {
                startPos = toad.position;
            }
        }

        private void Update()
        {
            if (isGameEnded || isBlocking == false)
            {
                return;
            }

            // 마찰 적용: 속도를 0으로 서서히 끌어당겨 미끄러짐(관성) 줄이기
            velocity = Mathf.MoveTowards(velocity, 0f, friction * Time.deltaTime);

            // 물 힘 적용: 매 프레임 오른쪽으로 가속(속도 누적)
            velocity += currentWaterPushForce * Time.deltaTime;
            // 속도 제한: 너무 빨라지는 것 방지
            velocity = Mathf.Clamp(velocity, -maxSpeed, maxSpeed);

            // 위치 갱신: 현재 속도로 x축 이동
            Vector2 p = toad.position;
            p.x += velocity * Time.deltaTime;
            toad.position = p;

            if (backAnimHoldRemaining > 0f)
            {
                backAnimHoldRemaining -= Time.deltaTime;
            }

            bool isPushingBack = backAnimHoldRemaining > 0f;
            toadAnimator.SetBool("IsPushingBack", isPushingBack);
        }

        private void FixedUpdate()
        {
            if (isGameEnded || isBlocking == false)
            {
                return;
            }

            if (toad.position.x <= target.position.x)   // 목표 지점 도달
            {
                isBlocking = false;
                velocity = 0f;
                toad.position = target.position;

                toadAnimator.SetBool("IsPushingBack", false);

                OnSuccess();
            }
            else if (toad.position.x - target.position.x >= maxPushbackDis)    // 밀려날 수 있는 최대 거리 벗어남
            {
                isBlocking = false;
                velocity = 0f;
                OnFailure();
            }
        }

        public override void OnGameStart(int difficulty, float speed)
        {
            isBlocking = true;  // 두꺼비 밀려나기 시작
            // 성공 단계에 따라 물이 미는 힘이 강해짐
            currentWaterPushForce = waterDefaultPushForce + (speed - 1) * difficulty;

            base.OnGameStart(difficulty, speed);

            float speedMultiplier = Mathf.Max(0.1f, speed); // 최소 0.1배속

            if (toad == null || target == null)
            {
                Debug.LogError("[MG_ToadSeal] RectTransform 참조가 비었습니다.");
                ReportResult(false);
                return;
            }

            if (timer != null)
            {
                timer.StartTimer(5f, speed);
                timer.OnTimerEnd += OnTimeUp;
            }

            if (inputHandler != null)
            {
                inputHandler.OnKeyPressed += HandleKeyPress;
            }
        }

        private void HandleKeyPress(KeyCode key)
        {
            if (isGameEnded || isBlocking == false)
            {
                return;
            }

            if (key != KeyCode.Space)
            {
                return;
            }

            velocity -= toadPushForce;
            backAnimHoldRemaining = backAnimHoldTime;

            SoundManager.Instance.SFXPlay("ToadMove", toadClip);
        }

        private void OnTimeUp()
        {
            if (toad.position.x < target.position.x)
            {
                isBlocking = false;
                velocity = 0f;

                ReportResultWithAnimation(true);
                return;
            }

            ReportResultWithAnimation(false); // 또는 true
        }

        private void OnSuccess()
        {
            ReportResultWithAnimation(true);
        }

        private void OnFailure()
        {
            ReportResultWithAnimation(false);
        }

        protected override void ResetGameState()
        {
            isBlocking = false;
            velocity = 0f;
            backAnimHoldRemaining = 0f;
            currentWaterPushForce = 0f;
            toad.position = startPos;

            toadAnimator.SetBool("IsSuccess", false);
            toadAnimator.SetBool("IsFailure", false);

            if (animCorotine != null)
            {
                StopCoroutine(animCorotine);
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
            }
        }

        protected override void PlayResultAnimation(bool success, Action onComplete = null)
        {
            animCorotine = StartCoroutine(ResultAnimationCoroutine(success, onComplete));
        }

        private IEnumerator ResultAnimationCoroutine(bool success, Action onComplete)
        {
            toadAnimator.SetBool("IsPushingBack", false);

            if (success)
            {
                toadAnimator.SetBool("IsSuccess", true);
            }
            else
            {
                toadAnimator.SetBool("IsFailure", true);
            }

            yield return new WaitForSeconds(1.5f);

            onComplete?.Invoke();
        }
    }
}
