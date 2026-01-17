using System;
using UnityEngine;
using Pansori.Microgames;

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
        [SerializeField] RectTransform nammaeRect; // 남매
        [SerializeField] RectTransform successTarget; // (판정용 목표위치)
        [SerializeField] RectTransform gameOverTarget; // (게임오버 판정용 목표위치)
        [SerializeField] AudioClip moveSoundClip;

        [Header("게임 설정")] 
        [SerializeField] float friction = 10f; // 마찰(값↑ = 덜 미끄러짐)
        [SerializeField] float nammaePushForce = 3f; // 스페이스 1회: 버티는 힘(속도 감소량)
        [SerializeField] float gravityPushForce = 15.5f; // 중력이 계속 미는 힘(초당 가속)

        [SerializeField] float maxSpeed = 30f; // 속도 제한

        private Vector2 startPos; // 시작 위치(리셋용)
        private float velocity; // 현재 x축 속도

        private bool isBlocking; // 실제 게임 진행 중 여부

        /// <summary>
        /// 현재 게임 이름
        /// </summary>
        public override string currentGameName => "올라가라!";

        [Header("헬퍼 컴포넌트")] 
        [SerializeField] private MicrogameTimer timer;
        [SerializeField] private MicrogameInputHandler inputHandler;
        [SerializeField] private MicrogameUILayer uiLayer;

        protected override void Awake()
        {
            base.Awake();

            velocity = 0f;
            isBlocking = false;

            if (nammaeRect != null)
            {
                startPos = nammaeRect.position;
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

            // 중력 힘 적용: 매 프레임 오른쪽으로 가속(속도 누적)
            velocity -= gravityPushForce * Time.deltaTime;
            // 속도 제한: 너무 빨라지는 것 방지
            velocity = Mathf.Clamp(velocity, -maxSpeed, maxSpeed);

            // 위치 갱신: 현재 속도로 y축 이동
            Vector2 p = nammaeRect.position;
            p.y += velocity * Time.deltaTime;
            nammaeRect.position = p;
        }

        private void FixedUpdate()
        {
            if (isGameEnded || isBlocking == false)
            {
                return;
            }
            

            if (DetectHelper.CheckCollisionEnterUI(nammaeRect, successTarget)) // 목표 지점 도달
            {
                isBlocking = false;
                velocity = 0f;
                OnSuccess();
            }
            else if (DetectHelper.CheckCollisionEnterUI(nammaeRect, gameOverTarget)) // 밀려날 수 있는 최대 거리 벗어남
            {
                isBlocking = false;
                velocity = 0f;
                OnFailure();
            }
        }
        


        public override void OnGameStart(int difficulty, float speed)
        {
            isBlocking = true; //  밀려나기 시작

            base.OnGameStart(difficulty, speed);

            float speedMultiplier = Mathf.Max(0.1f, speed); // 최소 0.1배속

            if (nammaeRect == null || successTarget == null)
            {
                Debug.LogError("RectTransform 참조가 비었습니다.");
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
            if (isGameEnded)
            {
                return;
            }

            if (key != KeyCode.Space)
            {
                return;
            }

            velocity += nammaePushForce;

            SoundManager.Instance.SFXPlay("NammaeMove", moveSoundClip);
        }

        private void OnTimeUp()
        {
            if (DetectHelper.CheckCollisionEnterUI(nammaeRect, successTarget))
            {
                isBlocking = false;
                velocity = 0f;
                ReportResult(true);
                return;
            }

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
            isBlocking = false;
            velocity = 0f;
            nammaeRect.position = startPos;

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
