using System;
using UnityEngine;
using Pansori.Microgames;
using TMPro;
using UnityEngine.UI;

namespace Pansori.Microgames.Games
{
    enum Patgi
    {
        Idle,
        Avoid,
        Hit,
        Lose
    }
    enum Conggi
    {
        Idle,
        Sadaggi,
        Lose,
    }
    
    
    /// <summary>
    /// 새 미니게임
    /// 
    /// TODO: 게임 설명을 여기에 작성하세요.
    /// </summary>
    /// TODO: 실패결과를 표시하기위해 이미지전환 기능 추가
    public class MG_Saddagi_Manager : MicrogameBase
    {
        [Header("게임 오브젝트")]
        // TODO: 게임 오브젝트 참조를 추가하세요
        [SerializeField]
        private RectTransform canvasRect;

        [SerializeField] private RectTransform hitAreaRect;
        [SerializeField] private TMP_Text timerText; // 남은 시간 표시 UI
        [SerializeField] private Image attackerImage;
        [SerializeField] private Image victimImage;
        [SerializeField] private GameObject arrowImage;
        [SerializeField] private Slider sadagiGageSlider;
        
        [SerializeField] Animator victimAnimator;
        [SerializeField] Animator attackerAnimator;
        [SerializeField] AudioClip generalSadagiSound;
        [SerializeField] AudioClip strongSadagiSound;
        [SerializeField] AudioClip missSadagiSound;
                
        [Header("게임 설정")]
        // TODO: 게임 설정 변수를 추가하세요
        [Tooltip("판정을 위해 최소 넘어야하는 스피드")]
        [SerializeField]
        private float minSlapSpeed = 1000f;

        [Header("결과 연출 설정")]
        [SerializeField] private bool useCustomResultAnimation = true; // 커스텀 결과 연출 사용 여부
        [SerializeField] private float resultDisplayDelay = 0.5f; // 결과 표시 전 연출 시간

        
        [Tooltip("클리어 위한 싸대기 달성 수")] [SerializeField]
        private float saddagiMaxCount = 5f;

        [Header("헬퍼 컴포넌트")] [SerializeField] private MicrogameTimer timer;
        [SerializeField] private MicrogameInputHandler inputHandler;
        [SerializeField] private MicrogameUILayer uiLayer;

        private bool hasHitTarget = false;

        private Vector3 startCanvasPos;
        private Vector3 prevCanvasPos;
        private float startTime;
        private int saddagiCount = 0;
        private Vector3 attackerPos;
        private Vector3 victimPos;
        
        private float afterHitAttackerTimer = 0f;
        private float afterHitAttackerTimerMax = 0.3f;
        private bool isAttackerIdle = false;
        private float afterHitVictimTimer = 0f;
        private float afterHitVictimTimerMax = 0.3f;
        private bool isVictimIdle = false;
        
        /// <summary>
        /// 현재 게임 이름
        /// </summary>
        public override string currentGameName => "때려라!";
        public override string controlDescription => "팥쥐의 뺨을 향해 마우스를 드래그하세요!";
            
        protected override void Awake()
        {
            base.Awake();

            // TODO: 초기화 로직을 추가하세요
            attackerPos = attackerImage.transform.position;
            victimPos =  victimImage.transform.position;
        }

        public override void OnGameStart(int difficulty, float speed)
        {
            base.OnGameStart(difficulty, speed);

            // TODO: 게임 시작 로직을 추가하세요
            victimImage.sprite = GetSpriteByEnum(Patgi.Idle);
            attackerImage.sprite = GetSpriteByEnum(Conggi.Idle);
            
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
            // 게임이 진행 중일 때만 업데이트
            if (!isGameEnded && timer != null && timer.IsRunning)
            {
                UpdateTimerUI();
                if (!isAttackerIdle)
                {
                    afterHitAttackerTimer+=Time.deltaTime;
                }
                if (afterHitAttackerTimer >= afterHitAttackerTimerMax)
                {
                    attackerImage.sprite =  GetSpriteByEnum(Conggi.Idle);
                }
                
                if (!isVictimIdle)
                {
                    afterHitVictimTimer+=Time.deltaTime;
                }

                if (afterHitVictimTimer >= afterHitVictimTimerMax)
                {
                    victimImage.sprite =  GetSpriteByEnum(Patgi.Idle);
                }
            }
        }

        private void HandleDragStart(Vector3 startPos)
        {
            hasHitTarget = false;
            startTime = Time.time;
            
            //시작좌표 변환 및 저장
            startCanvasPos = CoordinateHelper.GetCanvasWorldPos(startPos,canvasRect);
            prevCanvasPos = startCanvasPos;

            if (arrowImage.activeSelf)
            {
                arrowImage.SetActive(false);    
            }
            
        }

        private void HandleDrag(Vector3 inputStartPos, Vector3 currentPos)
        {
            // 현재 좌표 변환
            Vector3 currentCanvasPos =  CoordinateHelper.GetCanvasWorldPos(currentPos,canvasRect);
            // 아직 때리지 않았다면, 이전 위치와 현재 위치 사이를 검사 (뚫림 방지)
            if (!hasHitTarget)
            {
                CheckHitContinuous(prevCanvasPos, currentCanvasPos);
            }

            // 다음 프레임 계산을 위해 현재 위치를 이전 위치로 갱신
            prevCanvasPos = currentCanvasPos;
        }
        
        private void CheckHitContinuous(Vector3 start, Vector3 end)
        {
            float distance = Vector3.Distance(start, end);
        
            // 캔버스 좌표 기준 20 유닛마다 검사 (적절히 조절 가능)
            int steps = Mathf.CeilToInt(distance / 20f);

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector3 checkPoint = Vector3.Lerp(start, end, t);

                if (IsPointInHitArea(checkPoint))
                {
                    hasHitTarget = true;
                    break; // 한 번이라도 닿으면 루프 종료
                }
            }
        }
        // 특정 포인트(Canvas 평면 위)가 HitArea 안에 있는지 검사
        private bool IsPointInHitArea(Vector3 pointOnCanvas)
        {
            // pointOnCanvas는 현재 캔버스와 동일한 평면에 있는 월드 좌표입니다.
            // HitArea의 로컬 좌표계로 변환하여 사각형(rect) 안에 포함되는지 확인합니다.
        
            Vector3 localPos = hitAreaRect.InverseTransformPoint(pointOnCanvas);
            return hitAreaRect.rect.Contains(localPos);
        }

        private void HandleDragEnd(Vector3 endPos)
        {
            if (!hasHitTarget)
            {
                Debug.Log("빗나감!");
                victimImage.sprite = GetSpriteByEnum(Patgi.Avoid);
                attackerImage.sprite = GetSpriteByEnum(Conggi.Sadaggi);
                return;
            }
            
            //최종위치반환
            Vector3 endCanvasPos = CoordinateHelper.GetCanvasWorldPos(endPos,canvasRect);
            
            //속도계산(거리/시간)
            float distance = Vector3.Distance(startCanvasPos, endCanvasPos);
            float duration = Mathf.Max(Time.time - startTime, 0.001f);
            float speed = distance / duration;
            
            Debug.Log($"최종 속도: {speed:F2}");
            
            if (speed >= minSlapSpeed)
            {
                Debug.Log("‼️찰싹!");

                saddagiCount++;
                UpdateSadagiUI();
                
               
                if (saddagiCount >= saddagiMaxCount)
                {
                    OnSuccess();
                    if (strongSadagiSound != null)
                    {
                        SoundManager.Instance.SFXPlay(strongSadagiSound.ToString(),strongSadagiSound);
                    }
                    return;
                }
                if (generalSadagiSound != null)
                {
                    victimImage.sprite = GetSpriteByEnum(Patgi.Hit);
                    attackerImage.sprite = GetSpriteByEnum(Conggi.Sadaggi);
                    SoundManager.Instance.SFXPlay(generalSadagiSound.ToString(),generalSadagiSound);
                }
            }
            else
            {
                Debug.Log("‼️너무느려요!");
                if (missSadagiSound != null)
                {
                    victimImage.sprite = GetSpriteByEnum(Patgi.Avoid);
                    attackerImage.sprite = GetSpriteByEnum(Conggi.Sadaggi);
                    SoundManager.Instance.SFXPlay(missSadagiSound.ToString(),missSadagiSound);
                }
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

        private void UpdateSadagiUI()
        {
            sadagiGageSlider.value = saddagiCount;
        }

        private void OnTimeUp()
        {
            // TODO: 시간 초과 처리 로직을 추가하세요
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
            // TODO: 모든 오브젝트를 초기 상태로 리셋하는 로직을 추가하세요
            //싸다기카운드 초기화
            saddagiCount = 0;
            UpdateSadagiUI();
            //애니메이터초기화
            victimAnimator.Rebind();
            victimAnimator.enabled = false;
            attackerAnimator.Rebind();
            attackerAnimator.enabled = false;
            //위치 초기화
            attackerImage.transform.position = attackerPos;
            victimImage.transform.position = victimPos;
            //화살표초기화
            arrowImage.SetActive(true);
            
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
                inputHandler.OnMouseDragStart -= HandleDragStart;
                inputHandler.OnMouseDrag -= HandleDrag;
                inputHandler.OnMouseDragEnd -= HandleDragEnd;
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
            // 이미지 변환
            attackerImage.sprite = GetSpriteByEnum(Conggi.Sadaggi);
            victimImage.sprite = GetSpriteByEnum(Patgi.Lose);
            // 애니메이션 켜기
            victimAnimator.enabled = true;
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
            // 이미지 변환
            attackerImage.sprite = GetSpriteByEnum(Conggi.Lose);
            victimImage.sprite = GetSpriteByEnum(Patgi.Idle);
            // 애니메이션 켜기
            attackerAnimator.enabled = true;
            // 결과 표시 유지
            yield return new WaitForSeconds(resultDisplayDelay);
            // 완료 콜백
            onComplete?.Invoke();
        }
        //이넘 따라 다른 리소스를 로드하여 리턴
        private Sprite GetSpriteByEnum(Enum e)
        {
            Sprite sprite = null;
            switch (e)
            {
                case Patgi.Avoid:
                    sprite = Resources.Load<Sprite>("Saddagi/팥쥐_명중실패할때");
                    isVictimIdle = false;
                    break;
                case Patgi.Hit:
                    sprite = Resources.Load<Sprite>("Saddagi/팥쥐_아플때");
                    isVictimIdle = false;
                    break;
                case  Patgi.Lose:
                    sprite = Resources.Load<Sprite>("Saddagi/팥쥐_졌을때");
                    isVictimIdle = false;
                    break;
                case Patgi.Idle:
                    sprite = Resources.Load<Sprite>("Saddagi/팥쥐_Idle");
                    isVictimIdle = true;
                    afterHitVictimTimer = 0f;
                    break;
                case Conggi.Idle:
                    sprite = Resources.Load<Sprite>("Saddagi/콩쥐_Idle");
                    isAttackerIdle = true;
                    afterHitAttackerTimer = 0f;
                    break;
                case Conggi.Lose:
                    sprite = Resources.Load<Sprite>("Saddagi/콩쥐_졌을때");
                    isAttackerIdle = false;
                    break;
                case Conggi.Sadaggi:
                    sprite = Resources.Load<Sprite>("Saddagi/콩쥐_싸다귀");
                    isAttackerIdle = false;
                    break;
            }

            return sprite;
        }

    }
    
}
