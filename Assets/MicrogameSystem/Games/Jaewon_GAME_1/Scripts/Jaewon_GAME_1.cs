using UnityEngine;
using UnityEngine.UI;
using Pansori.Microgames;
using TMPro;

namespace Pansori.Microgames.Games
{
    /// <summary>
    /// 흥부 톱질 미니게임
    /// 좌우 화살표를 번갈아 눌러 박을 타는 게임
    /// 
    /// [와리오웨어 스타일 결과 애니메이션 적용]
    /// - 성공 시: 박이 깨지는 연출 + 화면 플래시 + 결과 애니메이션
    /// - 실패 시: 놀부 등장 + 화면 흔들림 + 결과 애니메이션
    /// </summary>
    public class Jaewon_GAME_1 : MicrogameBase
    {
        [Header("캐릭터 스프라이트")]
        [SerializeField] private GameObject heungbu; // 흥부
        [SerializeField] private GameObject heungbuWife; // 흥부 아내
        [SerializeField] private GameObject nolbu; // 놀부 (실패 시 표시)
        
        [Header("흥부 스프라이트 애니메이션")]
        [SerializeField] private Sprite[] heungbuSprites; // 흥부 스프라이트 배열 (0~6)
        [SerializeField] private Sprite[] wifeSprites; // 아내 스프라이트 배열 (0~6)
        [SerializeField] private float spriteAnimationInterval = 0.15f; // 스프라이트 전환 간격
        
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
        
        [Header("결과 연출 설정")]
        [SerializeField] private bool useCustomResultAnimation = true; // 커스텀 결과 연출 사용 여부
        [SerializeField] private float resultDisplayDelay = 0.5f; // 결과 표시 전 연출 시간
        
        [Header("성공 결과 연출")]
        [SerializeField] private GameObject treasureEffect; // 금은보화 이펙트 (파티클 또는 스프라이트)
        [SerializeField] private Image stampImage; // "참 잘했어요" 도장 이미지
        [SerializeField] private RectTransform stampTargetPosition; // 도장 목표 위치 (우측 하단)
        [SerializeField] private float stampAnimDuration = 0.5f; // 도장 애니메이션 시간
        [SerializeField] private float stampStartScale = 2.0f; // 도장 시작 스케일
        [SerializeField] private float stampEndScale = 1.0f; // 도장 끝 스케일
        [SerializeField] private float stampRotation = -15f; // 도장 회전 각도
        
        [Header("실패 결과 연출")]
        [SerializeField] private float nolbuBounceHeight = 50f; // 놀부 바운스 높이
        [SerializeField] private float nolbuBounceSpeed = 8f; // 놀부 바운스 속도
        [SerializeField] private int nolbuBounceCount = 4; // 놀부 바운스 횟수
        
        /// <summary>
        /// 현재 게임 이름
        /// </summary>
        public override string currentGameName => "톱질 하라!";
        // Jaewon_GAME_1.cs에서
        public override string controlDescription => "A, D를 번갈아 눌러 톱질하세요!";
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
        
        // 스프라이트 애니메이션 관련 변수
        private SpriteRenderer heungbuRenderer;
        private SpriteRenderer wifeRenderer;
        private float spriteAnimTimer = 0f;
        private int spriteFrameIndex = 0; // 0 또는 1 (프레임 전환용)
        
        /// <summary>
        /// 게임 결과 상태 (null: 진행중, true: 성공, false: 실패)
        /// </summary>
        private bool? gameResultState = null;
        
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
            
            // SpriteRenderer 캐시
            if (heungbu != null)
            {
                heungbuRenderer = heungbu.GetComponent<SpriteRenderer>();
            }
            if (heungbuWife != null)
            {
                wifeRenderer = heungbuWife.GetComponent<SpriteRenderer>();
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
            
            // 스프라이트 애니메이션 상태 초기화
            gameResultState = null;
            spriteFrameIndex = 0;
            spriteAnimTimer = 0f;
            
            // 초기 스프라이트 설정 (흥부: 0, 아내: 2)
            SetHeungbuSprite(0);
            SetWifeSprite(2);
        }
        
        private void HandleKeyPress(KeyCode key)
        {
            // 이미 종료된 경우 무시
            if (isGameEnded)
                return;
            
            // 좌우 화살표만 처리
            bool isRightArrow = (key == KeyCode.D);
            bool isLeftArrow = (key == KeyCode.A);
            
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
                
                // 스프라이트 프레임 전환 (입력 시마다 한 루프)
                spriteFrameIndex = (spriteFrameIndex + 1) % 2;
                ApplyCurrentSpriteFrame();
                
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
            // 스프라이트 애니메이션 업데이트 (게임 종료 후에도 계속 재생)
            UpdateSpriteAnimation();
            
            // 게임이 진행 중일 때만 업데이트
            if (!isGameEnded)
            {
                // 시간 UI 업데이트
                if (timer != null && timer.IsRunning)
                {
                    UpdateTimerUI();
                }
                
                // 톱 즉시 이동
                if (saw != null)
                {
                    saw.localPosition = sawTargetPosition;
                }
            }
        }
        
        /// <summary>
        /// 스프라이트 프레임 애니메이션 업데이트 (성공/실패 후 자동 애니메이션용)
        /// </summary>
        private void UpdateSpriteAnimation()
        {
            // 스프라이트 배열이 없으면 무시
            if (heungbuSprites == null || heungbuSprites.Length == 0 ||
                wifeSprites == null || wifeSprites.Length == 0)
                return;
            
            // 게임 진행 중에는 입력 기반 애니메이션 사용 (타이머 애니메이션 비활성화)
            if (gameResultState == null)
                return;
            
            // 성공/실패 후에만 타이머 기반 자동 애니메이션
            spriteAnimTimer += Time.deltaTime;
            
            if (spriteAnimTimer >= spriteAnimationInterval)
            {
                spriteAnimTimer = 0f;
                spriteFrameIndex = (spriteFrameIndex + 1) % 2; // 0과 1 사이 토글
                
                // 현재 상태에 따른 스프라이트 설정
                ApplyCurrentSpriteFrame();
            }
        }
        
        /// <summary>
        /// 현재 게임 상태에 따른 스프라이트 프레임 적용
        /// </summary>
        private void ApplyCurrentSpriteFrame()
        {
            if (gameResultState == true)
            {
                // 성공 상태: 흥부 3→4, 아내 3→4
                int heungbuFrame = spriteFrameIndex == 0 ? 3 : 4;
                int wifeFrame = spriteFrameIndex == 0 ? 3 : 4;
                SetHeungbuSprite(heungbuFrame);
                SetWifeSprite(wifeFrame);
            }
            else if (gameResultState == false)
            {
                // 실패 상태: 흥부 5 고정, 아내 6 고정
                SetHeungbuSprite(5);
                SetWifeSprite(6);
            }
            else
            {
                // 게임 진행 중
                bool isOverHalf = clickCount > targetClicks / 2;
                
                if (isOverHalf)
                {
                    // 클릭수 > 절반: 흥부 0→2, 아내 2→0
                    int heungbuFrame = spriteFrameIndex == 0 ? 0 : 2;
                    int wifeFrame = spriteFrameIndex == 0 ? 0 : 2;
                    SetHeungbuSprite(heungbuFrame);
                    SetWifeSprite(wifeFrame);
                }
                else
                {
                    // 클릭수 <= 절반: 흥부 0→1, 아내 2→1
                    int heungbuFrame = spriteFrameIndex == 0 ? 0 : 1;
                    int wifeFrame = spriteFrameIndex == 0 ? 1 : 2;
                    SetHeungbuSprite(heungbuFrame);
                    SetWifeSprite(wifeFrame);
                }
            }
        }
        
        /// <summary>
        /// 흥부 스프라이트 설정
        /// </summary>
        private void SetHeungbuSprite(int index)
        {
            if (heungbuRenderer != null && heungbuSprites != null && 
                index >= 0 && index < heungbuSprites.Length)
            {
                heungbuRenderer.sprite = heungbuSprites[index];
            }
        }
        
        /// <summary>
        /// 아내 스프라이트 설정
        /// </summary>
        private void SetWifeSprite(int index)
        {
            if (wifeRenderer != null && wifeSprites != null && 
                index >= 0 && index < wifeSprites.Length)
            {
                wifeRenderer.sprite = wifeSprites[index];
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
        
        /// <summary>
        /// 성공 처리 (와리오웨어 스타일 연출 포함)
        /// </summary>
        private void OnSuccess()
        {
            Debug.Log("[Jaewon_GAME_1] 성공!");
            
            // 스프라이트 애니메이션 상태를 성공으로 변경
            gameResultState = true;
            spriteFrameIndex = 0; // 애니메이션 프레임 리셋
            ApplyCurrentSpriteFrame(); // 즉시 스프라이트 적용
            
            // 박이 깨지는 연출
            if (gourdWhole != null) gourdWhole.SetActive(false);
            if (gourdBroken != null) gourdBroken.SetActive(true);
            
            // 흥부 부부 유지 (이미 표시 중)
            
            // 화면 플래시 효과 (성공)
            FlashScreen(true);
            
            // 깨진 박에 이펙트 적용
            if (gourdBroken != null)
            {
                PlayQuickEffect(gourdBroken.transform, true, null);
            }
            
            // 결과 애니메이션을 사용하여 결과 보고
            // 애니메이션이 완료된 후 결과가 매니저에게 전달됩니다
            if (useCustomResultAnimation && useResultAnimation)
            {
                ReportResultWithAnimation(true);
            }
            else
            {
                ReportResult(true);
            }
        }
        
        /// <summary>
        /// 실패 처리 (와리오웨어 스타일 연출 포함)
        /// </summary>
        private void OnFailure()
        {
            Debug.Log("[Jaewon_GAME_1] 실패!");
            
            // 스프라이트 애니메이션 상태를 실패로 변경
            gameResultState = false;
            ApplyCurrentSpriteFrame(); // 즉시 스프라이트 적용 (고정 프레임)
            
            // 흥부 부부 유지 (놀부가 놀리는 연출을 위해)
            // 놀부만 추가로 표시
            if (nolbu != null) nolbu.SetActive(true);
            
            // 화면 플래시 효과 (실패)
            FlashScreen(false);
            
            // 결과 애니메이션을 사용하여 결과 보고
            if (useCustomResultAnimation && useResultAnimation)
            {
                ReportResultWithAnimation(false);
            }
            else
            {
                ReportResult(false);
            }
        }
        
        /// <summary>
        /// 결과 애니메이션을 오버라이드하여 게임별 커스텀 연출을 추가합니다.
        /// </summary>
        protected override void PlayResultAnimation(bool success, System.Action onComplete = null)
        {
            if (success)
            {
                // 성공 시: 금은보화 + 도장 연출
                Debug.Log("[Jaewon_GAME_1] 성공 커스텀 연출 시작");
                StartCoroutine(PlaySuccessResultAnimation(onComplete));
            }
            else
            {
                // 실패 시: 놀부 바운스 연출
                Debug.Log("[Jaewon_GAME_1] 실패 커스텀 연출 시작");
                StartCoroutine(PlayFailureResultAnimation(onComplete));
            }
        }
        
        /// <summary>
        /// 성공 결과 애니메이션 - 금은보화 + 도장
        /// </summary>
        private System.Collections.IEnumerator PlaySuccessResultAnimation(System.Action onComplete)
        {
            // 금은보화 이펙트 표시
            ShowTreasureEffect();
            
            // 잠시 대기 후 도장 애니메이션
            yield return new WaitForSeconds(0.3f);
            
            // 도장 애니메이션 실행
            yield return StartCoroutine(StampAnimation());
            
            // 결과 표시 유지
            yield return new WaitForSeconds(resultDisplayDelay);
            
            // 완료 콜백
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 실패 결과 애니메이션 - 놀부 바운스
        /// </summary>
        private System.Collections.IEnumerator PlayFailureResultAnimation(System.Action onComplete)
        {
            // 놀부 바운스 애니메이션 실행
            yield return StartCoroutine(NolbuBounceAnimation());
            
            // 결과 표시 유지
            yield return new WaitForSeconds(resultDisplayDelay);
            
            // 완료 콜백
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 놀부 바운스 애니메이션 (위아래로 왔다갔다하며 흥부 부부 놀리기)
        /// </summary>
        private System.Collections.IEnumerator NolbuBounceAnimation()
        {
            if (nolbu == null) yield break;
            
            Vector3 originalPosition = nolbu.transform.localPosition;
            float elapsed = 0f;
            float totalDuration = nolbuBounceCount / nolbuBounceSpeed;
            
            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;
                
                // 사인 함수로 위아래 바운스 (감쇠 적용)
                float damping = 1f - (elapsed / totalDuration) * 0.5f; // 점점 약해짐
                float bounceOffset = Mathf.Sin(elapsed * nolbuBounceSpeed * Mathf.PI * 2f) * nolbuBounceHeight * damping;
                
                nolbu.transform.localPosition = new Vector3(
                    originalPosition.x,
                    originalPosition.y + bounceOffset,
                    originalPosition.z
                );
                
                yield return null;
            }
            
            // 원래 위치로 복원
            nolbu.transform.localPosition = originalPosition;
        }
        
        /// <summary>
        /// 도장 찍는 애니메이션 ("참 잘했어요" 도장)
        /// </summary>
        private System.Collections.IEnumerator StampAnimation()
        {
            if (stampImage == null) yield break;
            
            // 도장 활성화
            stampImage.gameObject.SetActive(true);
            
            RectTransform stampRect = stampImage.rectTransform;
            Vector3 targetPosition = stampTargetPosition != null 
                ? stampTargetPosition.anchoredPosition3D 
                : new Vector3(300f, -200f, 0f); // 기본 우측 하단 위치
            
            // 시작 위치 (화면 위에서)
            Vector3 startPosition = targetPosition + new Vector3(0f, 500f, 0f);
            stampRect.anchoredPosition3D = startPosition;
            
            // 시작 스케일과 회전
            stampRect.localScale = Vector3.one * stampStartScale;
            stampRect.localRotation = Quaternion.Euler(0f, 0f, stampRotation + 30f); // 시작 시 더 기울어짐
            
            // 투명도 설정
            Color stampColor = stampImage.color;
            stampColor.a = 0f;
            stampImage.color = stampColor;
            
            float elapsed = 0f;
            
            // 애니메이션 (위에서 내려오며 찍힘)
            while (elapsed < stampAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / stampAnimDuration;
                
                // 이징 함수 (EaseOutBack - 살짝 튕기는 효과)
                float easeT = 1f - Mathf.Pow(1f - t, 3f);
                float bounceT = t < 0.8f ? easeT : easeT + Mathf.Sin((t - 0.8f) * Mathf.PI * 5f) * 0.05f * (1f - t);
                
                // 위치 보간
                stampRect.anchoredPosition3D = Vector3.Lerp(startPosition, targetPosition, bounceT);
                
                // 스케일 보간 (커졌다가 작아짐)
                float scaleT = t < 0.5f 
                    ? Mathf.Lerp(stampStartScale, stampStartScale * 1.2f, t * 2f) 
                    : Mathf.Lerp(stampStartScale * 1.2f, stampEndScale, (t - 0.5f) * 2f);
                stampRect.localScale = Vector3.one * scaleT;
                
                // 회전 보간
                float rotationT = Mathf.Lerp(stampRotation + 30f, stampRotation, easeT);
                stampRect.localRotation = Quaternion.Euler(0f, 0f, rotationT);
                
                // 투명도 (빠르게 나타남)
                stampColor.a = Mathf.Min(1f, t * 3f);
                stampImage.color = stampColor;
                
                yield return null;
            }
            
            // 최종 상태 설정
            stampRect.anchoredPosition3D = targetPosition;
            stampRect.localScale = Vector3.one * stampEndScale;
            stampRect.localRotation = Quaternion.Euler(0f, 0f, stampRotation);
            stampColor.a = 1f;
            stampImage.color = stampColor;
        }
        
        /// <summary>
        /// 금은보화 이펙트 표시
        /// </summary>
        private void ShowTreasureEffect()
        {
            if (treasureEffect != null)
            {
                treasureEffect.SetActive(true);
                
                // 파티클 시스템이면 재생
                ParticleSystem particles = treasureEffect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    particles.Play();
                }
                
                Debug.Log("[Jaewon_GAME_1] 금은보화 이펙트 표시");
            }
        }
        
        /// <summary>
        /// 금은보화 이펙트 숨기기
        /// </summary>
        private void HideTreasureEffect()
        {
            if (treasureEffect != null)
            {
                treasureEffect.SetActive(false);
                
                ParticleSystem particles = treasureEffect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    particles.Stop();
                }
            }
        }
        
        /// <summary>
        /// 도장 숨기기
        /// </summary>
        private void HideStamp()
        {
            if (stampImage != null)
            {
                stampImage.gameObject.SetActive(false);
            }
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
            
            // 결과 연출 이펙트 숨기기
            HideTreasureEffect();
            HideStamp();
            
            // 스프라이트 초기 상태로 복원
            SetupInitialState();
        }
    }
}