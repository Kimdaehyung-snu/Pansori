using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pansori.Microgames;
using Pansori.Microgames.Games;

/// <summary>
/// 의좋은 형재 미니게임
/// 명령어: 던져라!
/// 
/// 1인칭 시점에서 쌀가마니를 드래그하여 형에게 던지는 게임
/// 플레이어가 형보다 쌀가마니를 적게 가지고 있어야 승리
/// 
/// [UI 기반 드래그 시스템]
/// - playerRiceBagStack의 각 쌀가마니에 DraggableRiceBagUI 컴포넌트가 있어야 함
/// - 스택의 쌀가마니를 직접 드래그하여 던질 수 있음
/// </summary>
public class Jaewon_GAME_2Manager : MicrogameBase
{
    [Header("의좋은 형재 설정")]
    [SerializeField] private float gameDuration = 5f;
    [SerializeField] private int initialRiceBags = 5;
    
    [Header("던지기 설정")]
    [Tooltip("화면 y좌표 비율 (0~1). 이 값 이상으로 드래그하면 던지기 판정")]
    [SerializeField] [Range(0f, 1f)] private float throwThresholdRatio = 0.5f;
    [SerializeField] private float throwAnimationDuration = 0.3f;
    
    [Header("형 AI 설정")]
    [SerializeField] private float hyungThrowInterval = 1.5f;
    
    [Header("UI 참조 - 게임 영역")]
    [SerializeField] private RectTransform gameCanvas;
    [SerializeField] private RectTransform playerRiceBagStack;
    [SerializeField] private RectTransform hyungRiceBagStack;
    [SerializeField] private RectTransform hyungCharacter;
    [SerializeField] private RectTransform flyingRiceBag;
    
    [Header("UI 참조 - 카운트 표시")]
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text hyungCountText;
    
    [Header("결과 애니메이션 참조")]
    [SerializeField] private GameObject successLeftArm;
    [SerializeField] private GameObject successRightArm;
    [SerializeField] private GameObject failHyungCelebration;
    [SerializeField] private TMP_Text failText;
    
    [Header("결과 애니메이션 설정")]
    [SerializeField] private float armRaiseHeight = 300f;
    [SerializeField] private float hyungBounceHeight = 50f;
    [SerializeField] private float hyungBounceSpeed = 8f;
    
    [Header("성공 애니메이션 스프라이트")]
    [SerializeField] private Sprite[] winSprites; // brothers_player_win_0, brothers_player_win_1
    [SerializeField] private float spriteSwapInterval = 0.5f;
    
    [Header("실패 애니메이션 스프라이트")]
    [SerializeField] private Sprite[] loseSprites; // brothers_bro_win_0 ~ 3
    [SerializeField] private float loseSpriteSwapInterval = 0.4f;
    
    [Header("형 던지기 애니메이션 스프라이트")]
    [SerializeField] private Sprite[] hyungThrowSprites; // brothers_bro_throw_0 ~ 2
    [SerializeField] private float hyungThrowSpriteInterval = 0.15f;
    
    [Header("형 던지기 입체 효과")]
    [SerializeField] private float hyungThrowScalePunch = 1.2f; // 던질 때 스케일 최대값
    [SerializeField] private float hyungThrowForwardMove = 50f; // 던질 때 전진 거리 (Y 방향)
    
    [Header("헬퍼 컴포넌트")]
    [SerializeField] private MicrogameTimer timer;
    
    // 게임 상태 변수
    private int playerRiceBags;
    private int hyungRiceBags;
    private float hyungThrowTimer;
    
    // 드래그 가능한 쌀가마니 목록
    private List<DraggableRiceBagUI> draggableRiceBags = new List<DraggableRiceBagUI>();
    
    // 애니메이션 상태
    private bool isThrowingAnimation = false;
    
    // 양팔 Image 컴포넌트 캐싱
    private Image leftArmImage;
    private Image rightArmImage;
    
    // 형 캐릭터 Image 컴포넌트 캐싱
    private Image hyungCharacterImage;
    
    /// <summary>
    /// 이 게임의 표시 이름
    /// </summary>
    public override string currentGameName => "던져라!";
    // Jaewon_GAME_1.cs에서
    public override string controlDescription => "쌀가마니를 드래그하여 형에게 던지세요!";
    protected override void Awake()
    {
        base.Awake();
        
        // 결과 애니메이션 요소 초기 숨김
        HideResultElements();
        
        // 플레이어 스택에서 드래그 가능한 쌀가마니 컴포넌트 수집
        CollectDraggableRiceBags();
        
        // 양팔 Image 컴포넌트 캐싱
        if (successLeftArm != null)
            leftArmImage = successLeftArm.GetComponent<Image>();
        if (successRightArm != null)
            rightArmImage = successRightArm.GetComponent<Image>();
        
        // 형 캐릭터 Image 컴포넌트 캐싱
        if (hyungCharacter != null)
            hyungCharacterImage = hyungCharacter.GetComponent<Image>();
    }
    
    /// <summary>
    /// playerRiceBagStack의 자식들에서 DraggableRiceBagUI 컴포넌트 수집
    /// </summary>
    private void CollectDraggableRiceBags()
    {
        draggableRiceBags.Clear();
        
        if (playerRiceBagStack == null) return;
        
        foreach (Transform child in playerRiceBagStack)
        {
            var draggable = child.GetComponent<DraggableRiceBagUI>();
            if (draggable != null)
            {
                draggableRiceBags.Add(draggable);
            }
        }
        
        Debug.Log($"[Jaewon_GAME_2] 드래그 가능한 쌀가마니 {draggableRiceBags.Count}개 발견");
    }
    
    /// <summary>
    /// 게임 시작 시 호출
    /// </summary>
    public override void OnGameStart(int difficulty, float speed)
    {
        base.OnGameStart(difficulty, speed);
        
        // 게임 상태 초기화
        playerRiceBags = initialRiceBags;
        hyungRiceBags = initialRiceBags;
        hyungThrowTimer = hyungThrowInterval / speed;
        isThrowingAnimation = false;
        
        // UI 업데이트
        UpdateCountUI();
        
        // 시각적 요소 초기화
        SetupInitialState();
        
        // 드래그 이벤트 구독
        SubscribeDragEvents();
        
        // 타이머 시작 (속도 무관하게 5초 고정)
        if (timer != null)
        {
            timer.StartTimer(gameDuration, 1f); // speed 1로 고정
            timer.OnTimerEnd += OnTimeUp;
        }
        
        Debug.Log($"[Jaewon_GAME_2] 게임 시작 - 난이도: {difficulty}, 속도: {speed}");
        Debug.Log($"[Jaewon_GAME_2] 형 던지기 주기: {hyungThrowInterval / speed}초");
    }
    
    /// <summary>
    /// 드래그 이벤트 구독
    /// </summary>
    private void SubscribeDragEvents()
    {
        foreach (var draggable in draggableRiceBags)
        {
            draggable.OnThrowAttempt += HandleThrowAttempt;
            draggable.OnDragStarted += HandleDragStarted;
        }
    }
    
    /// <summary>
    /// 드래그 이벤트 해제
    /// </summary>
    private void UnsubscribeDragEvents()
    {
        foreach (var draggable in draggableRiceBags)
        {
            draggable.OnThrowAttempt -= HandleThrowAttempt;
            draggable.OnDragStarted -= HandleDragStarted;
        }
    }
    
    /// <summary>
    /// 게임 시작 시 초기 상태 설정
    /// </summary>
    private void SetupInitialState()
    {
        // 날아가는 쌀가마니 비활성화
        if (flyingRiceBag != null)
        {
            flyingRiceBag.gameObject.SetActive(false);
        }
        
        // 결과 애니메이션 요소 숨김
        HideResultElements();
        
        // 형 캐릭터 초기화
        if (hyungCharacter != null)
        {
            hyungCharacter.gameObject.SetActive(true);
        }
        
        // 쌀가마니 스택 시각적 업데이트
        UpdateRiceBagVisuals();
        
        // 드래그 가능한 쌀가마니 상태 초기화
        foreach (var draggable in draggableRiceBags)
        {
            draggable.ResetState();
        }
    }
    
    /// <summary>
    /// 결과 애니메이션 요소 숨김
    /// </summary>
    private void HideResultElements()
    {
        if (successLeftArm != null) successLeftArm.SetActive(false);
        if (successRightArm != null) successRightArm.SetActive(false);
        if (failHyungCelebration != null) failHyungCelebration.SetActive(false);
        if (failText != null) failText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (isGameEnded) return;
        
        // 형 AI - 주기적으로 쌀가마니 던지기
        UpdateHyungAI();
    }
    
    /// <summary>
    /// 형 AI 업데이트
    /// </summary>
    private void UpdateHyungAI()
    {
        if (isThrowingAnimation) return;
        
        hyungThrowTimer -= Time.deltaTime;
        
        if (hyungThrowTimer <= 0)
        {
            // 형이 쌀가마니를 가지고 있으면 던지기
            if (hyungRiceBags > 0)
            {
                StartCoroutine(HyungThrowRiceBag());
            }
            
            // 타이머 리셋 (속도 반영)
            hyungThrowTimer = hyungThrowInterval / currentSpeed;
        }
    }
    
    /// <summary>
    /// 형이 쌀가마니를 던지는 코루틴
    /// </summary>
    private IEnumerator HyungThrowRiceBag()
    {
        hyungRiceBags--;
        playerRiceBags++;
        UpdateCountUI();
        UpdateRiceBagVisuals();
        
        Debug.Log($"[Jaewon_GAME_2] 형이 쌀가마니를 던짐! 플레이어: {playerRiceBags}, 형: {hyungRiceBags}");
        
        // 형 던지기 스프라이트 + 입체 효과 애니메이션
        Vector2 hyungOriginalPos = hyungCharacter != null ? hyungCharacter.anchoredPosition : Vector2.zero;
        Vector3 hyungOriginalScale = hyungCharacter != null ? hyungCharacter.localScale : Vector3.one;
        
        if (hyungThrowSprites != null && hyungThrowSprites.Length > 0 && hyungCharacterImage != null)
        {
            float totalDuration = hyungThrowSprites.Length * hyungThrowSpriteInterval;
            float elapsed = 0f;
            int currentSpriteIndex = 0;
            float nextSpriteTime = 0f;
            
            while (elapsed < totalDuration)
            {
                // 스프라이트 교체
                if (elapsed >= nextSpriteTime && currentSpriteIndex < hyungThrowSprites.Length)
                {
                    hyungCharacterImage.sprite = hyungThrowSprites[currentSpriteIndex];
                    currentSpriteIndex++;
                    nextSpriteTime += hyungThrowSpriteInterval;
                }
                
                // 진행률 계산
                float t = elapsed / totalDuration;
                
                // 스케일 펀치 (커졌다가 원래대로)
                float scaleMultiplier = 1f + Mathf.Sin(t * Mathf.PI) * (hyungThrowScalePunch - 1f);
                hyungCharacter.localScale = hyungOriginalScale * scaleMultiplier;
                
                // 전진 이동 (앞으로 갔다가 원래대로)
                float forwardOffset = Mathf.Sin(t * Mathf.PI) * hyungThrowForwardMove;
                hyungCharacter.anchoredPosition = hyungOriginalPos + new Vector2(0, forwardOffset);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 원래 상태 복원
            hyungCharacter.localScale = hyungOriginalScale;
            hyungCharacter.anchoredPosition = hyungOriginalPos;
        }
        
        // 날아오는 애니메이션 (UI 좌표계) - 시작 위치를 hyungCharacter 중심으로 설정
        if (flyingRiceBag != null && hyungCharacter != null && playerRiceBagStack != null)
        {
            flyingRiceBag.gameObject.SetActive(true);
            flyingRiceBag.localScale = Vector3.one;
            
            // hyungCharacter의 실제 위치에서 시작
            Vector2 startPos = hyungCharacter.anchoredPosition;
            Vector2 endPos = playerRiceBagStack.anchoredPosition;
            
            float duration = 0.3f;
            float elapsed = 0f;
            float arcHeight = Vector2.Distance(startPos, endPos) * 0.2f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 포물선 이동
                Vector2 pos = Vector2.Lerp(startPos, endPos, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
                flyingRiceBag.anchoredPosition = pos;
                
                // 스케일 증가 (가까워지는 느낌)
                float scale = Mathf.Lerp(0.5f, 1f, t);
                flyingRiceBag.localScale = new Vector3(scale, scale, 1f);
                
                yield return null;
            }
            
            flyingRiceBag.gameObject.SetActive(false);
        }
    }
    
    #region 드래그 이벤트 처리
    
    /// <summary>
    /// 드래그 시작 처리
    /// </summary>
    private void HandleDragStarted(DraggableRiceBagUI draggable)
    {
        Debug.Log($"[Jaewon_GAME_2] 쌀가마니 드래그 시작: {draggable.name}");
    }
    
    /// <summary>
    /// 던지기 시도 처리
    /// </summary>
    private void HandleThrowAttempt(DraggableRiceBagUI draggable, float yRatio)
    {
        if (isGameEnded || isThrowingAnimation) 
        {
            draggable.ReturnToOriginalPosition();
            return;
        }
        
        if (playerRiceBags <= 0)
        {
            draggable.ReturnToOriginalPosition();
            return;
        }
        
        Debug.Log($"[Jaewon_GAME_2] 던지기 시도 - Y비율: {yRatio:F2}, 임계값: {throwThresholdRatio}");
        
        // 던지기 판정
        if (yRatio >= throwThresholdRatio)
        {
            // 성공 - 형에게 던지기
            StartCoroutine(ThrowRiceBagToHyung(draggable));
        }
        else
        {
            // 실패 - 원래 위치로 복귀
            draggable.ReturnToOriginalPosition();
        }
    }
    
    /// <summary>
    /// 플레이어가 형에게 쌀가마니 던지기
    /// </summary>
    private IEnumerator ThrowRiceBagToHyung(DraggableRiceBagUI draggable)
    {
        isThrowingAnimation = true;
        
        playerRiceBags--;
        hyungRiceBags++;
        UpdateCountUI();
        
        Debug.Log($"[Jaewon_GAME_2] 플레이어가 쌀가마니를 던짐! 플레이어: {playerRiceBags}, 형: {hyungRiceBags}");
        
        // 던지기 목표 위치 (형 영역)
        Vector2 targetPos = hyungCharacter != null ? hyungCharacter.anchoredPosition : new Vector2(0, 300);
        
        // 던지기 애니메이션
        bool animationComplete = false;
        draggable.AnimateThrowToTarget(targetPos, throwAnimationDuration, () =>
        {
            animationComplete = true;
        });
        
        // 애니메이션 완료 대기
        while (!animationComplete)
        {
            yield return null;
        }
        
        // 쌀가마니 비활성화 후 원래 부모로 복귀
        draggable.ReturnToOriginalParent();
        draggable.ResetState();
        
        // 시각적 업데이트
        UpdateRiceBagVisuals();
        
        isThrowingAnimation = false;
    }
    
    #endregion
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateCountUI()
    {
        if (playerCountText != null)
        {
            playerCountText.text = playerRiceBags.ToString();
        }
        
        if (hyungCountText != null)
        {
            hyungCountText.text = hyungRiceBags.ToString();
        }
    }
    
    /// <summary>
    /// 쌀가마니 스택 시각적 업데이트
    /// </summary>
    private void UpdateRiceBagVisuals()
    {
        // 플레이어 쌀가마니 스택 업데이트
        if (playerRiceBagStack != null)
        {
            for (int i = 0; i < playerRiceBagStack.childCount; i++)
            {
                var child = playerRiceBagStack.GetChild(i);
                bool isActive = i < playerRiceBags;
                child.gameObject.SetActive(isActive);
                
                // 드래그 가능 여부 설정 (활성화된 것만 드래그 가능)
                var draggable = child.GetComponent<DraggableRiceBagUI>();
                if (draggable != null)
                {
                    draggable.IsDraggable = isActive;
                }
            }
        }
        
        // 형 쌀가마니 스택 업데이트
        if (hyungRiceBagStack != null)
        {
            for (int i = 0; i < hyungRiceBagStack.childCount; i++)
            {
                hyungRiceBagStack.GetChild(i).gameObject.SetActive(i < hyungRiceBags);
            }
        }
    }
    
    /// <summary>
    /// 시간 종료 시 호출
    /// </summary>
    private void OnTimeUp()
    {
        if (isGameEnded) return;
        
        Debug.Log($"[Jaewon_GAME_2] 시간 종료! 플레이어: {playerRiceBags}, 형: {hyungRiceBags}");
        
        // 승패 판정: 플레이어가 형보다 쌀가마니를 적게 가지고 있으면 승리
        bool success = playerRiceBags < hyungRiceBags;
        
        if (success)
        {
            OnSuccess();
        }
        else
        {
            OnFailure();
        }
    }
    
    /// <summary>
    /// 성공 처리
    /// </summary>
    private void OnSuccess()
    {
        Debug.Log("[Jaewon_GAME_2] 성공! 동생이 더 많이 양보했습니다.");
        ReportResultWithAnimation(true);
    }
    
    /// <summary>
    /// 실패 처리
    /// </summary>
    private void OnFailure()
    {
        Debug.Log("[Jaewon_GAME_2] 실패! 형보다 쌀가마니가 많거나 같습니다.");
        ReportResultWithAnimation(false);
    }
    
    /// <summary>
    /// 커스텀 결과 애니메이션 (오버라이드)
    /// </summary>
    protected override void PlayResultAnimation(bool success, Action onComplete = null)
    {
        if (success)
        {
            StartCoroutine(PlaySuccessAnimation(onComplete));
        }
        else
        {
            StartCoroutine(PlayFailureAnimation(onComplete));
        }
    }
    
    /// <summary>
    /// 성공 애니메이션: 1인칭 시점에서 양팔 환호 + 스프라이트 교체
    /// </summary>
    private IEnumerator PlaySuccessAnimation(Action onComplete)
    {
        Debug.Log("[Jaewon_GAME_2] 성공 애니메이션 시작");
        
        // 양팔 활성화
        if (successLeftArm != null) successLeftArm.SetActive(true);
        if (successRightArm != null) successRightArm.SetActive(true);
        
        // 초기 스프라이트 설정
        if (winSprites != null && winSprites.Length >= 2)
        {
            if (leftArmImage != null) leftArmImage.sprite = winSprites[0];
            if (rightArmImage != null) rightArmImage.sprite = winSprites[1];
        }
        
        // 스프라이트 교체 코루틴 시작 (병렬)
        Coroutine spriteSwapCoroutine = StartCoroutine(AnimateSpriteSwap());
        
        // 팔 올리기 애니메이션
        RectTransform leftArmRect = successLeftArm?.GetComponent<RectTransform>();
        RectTransform rightArmRect = successRightArm?.GetComponent<RectTransform>();
        
        Vector2 leftStartPos = leftArmRect != null ? leftArmRect.anchoredPosition : Vector2.zero;
        Vector2 rightStartPos = rightArmRect != null ? rightArmRect.anchoredPosition : Vector2.zero;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 이징: EaseOutBack
            float easeT = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
            
            if (leftArmRect != null)
            {
                leftArmRect.anchoredPosition = leftStartPos + new Vector2(0, armRaiseHeight * easeT);
            }
            if (rightArmRect != null)
            {
                rightArmRect.anchoredPosition = rightStartPos + new Vector2(0, armRaiseHeight * easeT);
            }
            
            yield return null;
        }
        
        // 잠시 유지 (스프라이트 교체 계속 진행)
        yield return new WaitForSeconds(0.8f);
        
        // 스프라이트 교체 코루틴 중지
        if (spriteSwapCoroutine != null)
        {
            StopCoroutine(spriteSwapCoroutine);
        }
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// 스프라이트 교체 애니메이션 (양팔이 서로 반대 스프라이트를 0.5초마다 교대)
    /// </summary>
    private IEnumerator AnimateSpriteSwap()
    {
        if (winSprites == null || winSprites.Length < 2) yield break;
        
        int currentIndex = 0;
        while (true)
        {
            // 왼팔: currentIndex, 오른팔: 반대 인덱스
            if (leftArmImage != null)
                leftArmImage.sprite = winSprites[currentIndex % winSprites.Length];
            if (rightArmImage != null)
                rightArmImage.sprite = winSprites[(currentIndex + 1) % winSprites.Length];
            
            yield return new WaitForSeconds(spriteSwapInterval);
            currentIndex++;
        }
    }
    
    /// <summary>
    /// 실패 애니메이션: hyungCharacter에 스프라이트 순환 표시
    /// </summary>
    private IEnumerator PlayFailureAnimation(Action onComplete)
    {
        Debug.Log("[Jaewon_GAME_2] 실패 애니메이션 시작");
        
        // hyungCharacter 활성화
        if (hyungCharacter != null) hyungCharacter.gameObject.SetActive(true);
        
        // failHyungCelebration 활성화
        if (failHyungCelebration != null) failHyungCelebration.SetActive(true);
        
        // 스프라이트 교체 애니메이션 (전체 4개 스프라이트 순환)
        if (loseSprites != null && loseSprites.Length > 0 && hyungCharacterImage != null)
        {
            float totalDuration = 1.6f; // 약 4회 순환
            float elapsed = 0f;
            int currentIndex = 0;
            float nextSwapTime = 0f;
            
            while (elapsed < totalDuration)
            {
                if (elapsed >= nextSwapTime)
                {
                    hyungCharacterImage.sprite = loseSprites[currentIndex % loseSprites.Length];
                    currentIndex++;
                    nextSwapTime += loseSpriteSwapInterval;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// 게임 상태 초기화 (재사용을 위해 필수 구현)
    /// </summary>
    protected override void ResetGameState()
    {
        playerRiceBags = initialRiceBags;
        hyungRiceBags = initialRiceBags;
        hyungThrowTimer = hyungThrowInterval;
        isThrowingAnimation = false;
        
        // 타이머 중지 및 이벤트 해제
        if (timer != null)
        {
            timer.Stop();
            timer.OnTimerEnd -= OnTimeUp;
        }
        
        // 드래그 이벤트 해제
        UnsubscribeDragEvents();
        
        // 드래그 가능한 쌀가마니 상태 초기화
        foreach (var draggable in draggableRiceBags)
        {
            draggable.ResetState();
        }
        
        // 결과 애니메이션 요소 숨김
        HideResultElements();
        
        // UI 초기화
        UpdateCountUI();
    }
    
    /// <summary>
    /// 게임 종료 시 호출
    /// </summary>
    protected override void OnGameEnd()
    {
        base.OnGameEnd();
        
        // 모든 드래그 비활성화
        foreach (var draggable in draggableRiceBags)
        {
            draggable.IsDraggable = false;
        }
    }
}
