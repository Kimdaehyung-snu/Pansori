using Pansori.Microgames;
using System;
using System.Collections;
using UnityEngine;
using URandom = UnityEngine.Random;

namespace Pansori.Microgames.Games
{
    /// <summary>
    /// 심청전에서, 심청이 인당수에 빠지는 장면을 재현한 마이크로 게임
    /// 
    /// - 배가 화면 오른쪽으로 자동 이동
    /// - 용궁(목표)은 화면 기준 중앙~오른쪽 구간의 X에서 랜덤 생성
    /// - 플레이어는 Space를 눌러 타이밍을 확정
    /// - Space 입력 순간, 배의 X구간과 용궁의 X구간이 겹치면 성공.
    /// - 실패/시간초과면 실패 처리.
    /// </summary>
    public class MG_IndangsuDive_Manager : MicrogameBase
    {
        [Header("게임 오브젝트")]
        [SerializeField] SpriteRenderer boat;    // 배
        [SerializeField] Transform simcheong;  // 심청이
        [SerializeField] SpriteRenderer dragonPalace;  // 용궁

        [Header("사운드 클립")]
        [SerializeField] AudioClip diveSplashClip;

        [Header("용궁 스폰 설정")]
        [SerializeField, Range(0f, 0.95f)] float rightMarginViewport = 0.05f;   // 화면 오른쪽 여백(뷰포트 기준)
        private Camera cam;

        [Header("배 이동 설정")]
        [SerializeField] float defaultVel;  // 기본 배 속도
        [SerializeField] float maxDis = 15f;
        private float currentVel;

        private Vector2 startBoatPos;   // 시작 위치(리셋용)
        private Vector2 startSimcheongPos;   // 시작 위치(리셋용)
        private bool isMoving;

        [Header("판정 설정")]
        // 판정 윈도우를 넓히거나(+) 좁히기(-) 위한 가산치
        [SerializeField] float extraWindow = 0.0f;

        [Header("판정 설정")]
        [SerializeField] float diveSpeed = 6f;  // 낙하 속도
        [SerializeField] float diveStopEpsilon = 0.01f; // 도착 판정 오차

        [Header("헬퍼 컴포넌트")]
        [SerializeField] private MicrogameTimer timer;
        [SerializeField] private MicrogameInputHandler inputHandler;
        [SerializeField] private MicrogameUILayer uiLayer;

        /// <summary>
        /// 이 게임의 표시 이름
        /// </summary>
        public override string currentGameName => "뛰어내려라!";

        public override string controlDescription => "용궁에 맞춰\n스페이스바를\n누르세요!";

        protected override void Awake()
        {
            base.Awake();

            startBoatPos = boat.transform.position;
            startSimcheongPos = simcheong.position;
            isMoving = false;
            cam = Camera.main;
        }

        private void Update()
        {
            if (isGameEnded || isMoving == false)
            {
                return;
            }

            Vector2 p = boat.transform.position;
            p.x += currentVel * Time.deltaTime;
            boat.transform.position = p;
        }

        private void FixedUpdate()
        {
            if (isGameEnded || isMoving == false)
            {
                return;
            }

            if (boat.transform.position.x - startBoatPos.x >= maxDis)
            {
                isMoving = false;
                OnFailure();
            }
        }

        public override void OnGameStart(int difficulty, float speed)
        {
            SpawnDragonPalace();

            isMoving = true;
            currentVel = defaultVel * Mathf.Pow(4, speed) * difficulty;

            SoundManager.Instance.PlayMicrogameBGM("MG_IndangsuDive");  // 배경음 재생

            base.OnGameStart(difficulty, speed);
            
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
            if (isGameEnded)
            {
                return;
            }

            if (key != KeyCode.Space)
            {
                return;
            }

            isMoving = false;
            SoundManager.Instance.SFXPlay("DiveSplash", diveSplashClip);

            if (IsOverlapping())    // 용궁과 배의 X 좌표가 겹치는지 확인
            {
                OnSuccess();
            }
            else
            {
                OnFailure();
            }
        }
        
        private void OnTimeUp()
        {
            ReportResult(false);
        }
        
        private void OnSuccess()
        {
            ReportResultWithAnimation(true);
        }
        
        private void OnFailure()
        {
            ReportResultWithAnimation(false);
        }

        protected override void PlayResultAnimation(bool success, Action onComplete = null)
        {
            StartCoroutine(ResultAnimationCoroutine(success, onComplete));
        }

        /// <summary>
        /// 결과 연출 코루틴
        /// - 심청이 낙하 후 성공 여부에 따른 애니메이션 실행
        /// </summary>
        /// <param name="success"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        private IEnumerator ResultAnimationCoroutine(bool success, Action onComplete)
        {
            // 연출 시작 전 텀
            yield return new WaitForSeconds(0.2f);

            // 심청이 낙하 애니메이션 실행
            while (isGameEnded == true && simcheong != null)
            {
                Vector3 p = simcheong.position;
                float newY = Mathf.MoveTowards(p.y, dragonPalace.transform.position.y, diveSpeed * Time.deltaTime);
                simcheong.position = new Vector3(p.x, newY, p.z);

                if (Mathf.Abs(simcheong.position.y - dragonPalace.transform.position.y) <= diveStopEpsilon)
                    break;

                yield return null;
            }

            if (success)
            {
                // TODO... 성공 애니메이션 실행
            }
            else
            {
                // TODO... 실패 애니메이션 실행
            }

            yield return new WaitForSeconds(0.8f);

            onComplete?.Invoke();
        }

        protected override void ResetGameState()
        {
            boat.transform.position = startBoatPos;
            simcheong.position = startSimcheongPos;
            currentVel = defaultVel;
            isMoving = false;

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

        /// <summary>
        /// 용궁을 화면 기준 중앙 ~ 오른쪽 X 범위에서 랜덤 스폰하는 메서드
        /// - Y축과 Z축은 고정
        /// </summary>
        private void SpawnDragonPalace()
        {
            if (dragonPalace == null)
            {
                return;
            }

            // 뷰포트 x: 0.5(가운데) ~ 1.0-여백(오른쪽)
            float vxMin = 0.5f;
            float vxMax = Mathf.Clamp01(1f - rightMarginViewport);
            float vx = URandom.Range(vxMin, vxMax);

            // 화면에 대응되는 스크린 y 계산
            Vector3 baseWorld = new Vector3(0f, dragonPalace.transform.position.y, 0f);
            Vector3 baseScreen = cam.WorldToScreenPoint(baseWorld);

            Vector3 spawnScreen = new Vector3(vx * Screen.width, baseScreen.y, 0);
            Vector3 spawnWorld = cam.ScreenToWorldPoint(spawnScreen);

            spawnWorld.y = dragonPalace.transform.position.y;      // Y를 확실히 고정
            spawnWorld.z = 0; // 기존 Z 유지(2D면 보통 0)

            dragonPalace.transform.position = spawnWorld;
        }

        /// <summary>
        /// X축 기준으로 구간 겹침을 판정
        /// - 배와 용궁의 SpriteRenderer.bounds에서
        /// - X min/max를 뽑아 1D overlap 검사
        /// </summary>
        /// <returns></returns>
        private bool IsOverlapping()
        {
            if (boat == null || dragonPalace == null)
            {
                return false;
            }

            Bounds a = boat.bounds;
            Bounds b = dragonPalace.bounds;

            float aMin = a.min.x - extraWindow;
            float aMax = a.max.x + extraWindow;

            float bMin = b.min.x - extraWindow;
            float bMax = b.max.x + extraWindow;

            // 1D 구간 겹침
            return aMin <= bMax && bMin <= aMax;
        }

        private bool IsOutOfScreenX(Transform t)
        {
            if (t == null || cam == null)
            { 
                return false;
            }

            Vector3 v = cam.WorldToViewportPoint(t.position);
            return v.x < 0f || v.x > 1f;
        }
    }
}
