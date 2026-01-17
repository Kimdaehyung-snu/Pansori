using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pansori.Microgames;
using Pansori.Microgames.Games;

/// <summary>
/// 전우치전 미니게임
/// 명령어: 그려라!
/// 
/// 두루마리에 한자(天/地)를 먹물로 그리는 게임
/// - 랜덤으로 天 또는 地 한자 선택
/// - 시간 내에 목표 비율만큼 채워야 성공
/// - 먹물은 한정되어 있어 난사 방지
/// </summary>
public class Jaewon_GAME_3Manager : MicrogameBase
{
    [Header("전우치전 설정")]
    [SerializeField] private float gameDuration = 5f;
    
    [Header("성공 조건")]
    [Tooltip("목표 채움률 (0~1, 기본 0.6 = 60%)")]
    [SerializeField] [Range(0.3f, 1f)] private float requiredFillPercentage = 0.6f;
    
    [Header("먹물 설정")]
    [Tooltip("최대 먹물 양")]
    [SerializeField] private float maxInkAmount = 1f;
    
    [Header("난이도별 설정")]
    [SerializeField] private float[] difficultyFillRequirements = { 0.5f, 0.6f, 0.7f };
    [SerializeField] private float[] difficultyInkAmounts = { 1.2f, 1f, 0.8f };
    
    [Header("UI 참조")]
    [SerializeField] private TMP_Text targetCharacterText;
    [SerializeField] private TMP_Text guideText;
    [SerializeField] private Image backgroundImage;
    
    [Header("컴포넌트 참조")]
    [SerializeField] private DrawingCanvas drawingCanvas;
    [SerializeField] private InkGaugeUI inkGauge;
    [SerializeField] private FillGaugeUI fillGauge;
    [SerializeField] private MicrogameTimer timer;
    
    [Header("결과 애니메이션 참조")]
    [SerializeField] private RectTransform scrollImage;
    [SerializeField] private RectTransform dragonImage;
    [SerializeField] private Image flashOverlay;
    [SerializeField] private RectTransform scrollLeftPart;
    [SerializeField] private RectTransform scrollRightPart;
    
    [Header("결과 애니메이션 설정")]
    [SerializeField] private float dragonRiseHeight = 500f;
    [SerializeField] private float dragonRiseDuration = 1f;
    [SerializeField] private float scrollTearDistance = 300f;
    [SerializeField] private float scrollTearDuration = 0.5f;
    
    [Header("붓글씨 사운드")]
    [SerializeField] private AudioClip brushSoundClip; // 붓글씨.mp3
    
    [Header("커스텀 커서")]
    [SerializeField] private Texture2D cursorTexture; // 커서 스프라이트 (Texture2D)
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero; // 커서 클릭 지점 (좌상단 기준)
    
    // 붓글씨 사운드용 AudioSource
    private AudioSource brushAudioSource;
    
    // 게임 상태 변수
    private bool isTian = true; // true: 天, false: 地
    private bool hasSucceeded = false;
    
    // dragonImage 초기 위치 저장
    private Vector2 dragonImageInitialPosition;
    
    // scrollLeftPart, scrollRightPart 초기 위치 저장
    private Vector2 scrollLeftPartInitialPosition;
    private Vector2 scrollRightPartInitialPosition;
    
    /// <summary>
    /// 이 게임의 표시 이름
    /// </summary>
    public override string currentGameName => "그려라!";
    // Jaewon_GAME_1.cs에서
    public override string controlDescription => "마우스로 글자를 그리세요!!";

    protected override void Awake()
    {
        base.Awake();

        // 컴포넌트 자동 탐색
        if (drawingCanvas == null)
        {
            drawingCanvas = GetComponentInChildren<DrawingCanvas>();
        }

        if (inkGauge == null)
        {
            inkGauge = GetComponentInChildren<InkGaugeUI>();
        }

        if (fillGauge == null)
        {
            fillGauge = GetComponentInChildren<FillGaugeUI>();
        }

        if (timer == null)
        {
            timer = GetComponent<MicrogameTimer>();
        }

        // dragonImage 초기 위치 저장
        if (dragonImage != null)
        {
            dragonImageInitialPosition = dragonImage.anchoredPosition;
        }
        
        // scrollLeftPart, scrollRightPart 초기 위치 저장
        if (scrollLeftPart != null)
        {
            scrollLeftPartInitialPosition = scrollLeftPart.anchoredPosition;
        }
        if (scrollRightPart != null)
        {
            scrollRightPartInitialPosition = scrollRightPart.anchoredPosition;
        }

        // 결과 애니메이션 요소 초기 숨김
        HideResultElements();
        
        // 붓글씨 사운드용 AudioSource 생성
        brushAudioSource = gameObject.AddComponent<AudioSource>();
        brushAudioSource.clip = brushSoundClip;
        brushAudioSource.loop = true;
        brushAudioSource.playOnAwake = false;
        brushAudioSource.volume = 0.5f;
    }
    
    /// <summary>
    /// 게임 시작 시 호출
    /// </summary>
    public override void OnGameStart(int difficulty, float speed)
    {
        base.OnGameStart(difficulty, speed);
        
        // 난이도 적용
        ApplyDifficulty(difficulty);
        
        // 게임 상태 초기화
        hasSucceeded = false;
        
        // 랜덤으로 天 또는 地 선택
        isTian = UnityEngine.Random.value > 0.5f;
        
        // UI 업데이트
        SetupTargetCharacter();
        
        // 드로잉 캔버스 초기화
        if (drawingCanvas != null)
        {
            drawingCanvas.SetMask(isTian);
            drawingCanvas.InitializeInk(maxInkAmount);
            drawingCanvas.ResetCanvas();
            drawingCanvas.CanDraw = true;
            
            // 먹물 소비 이벤트 구독
            drawingCanvas.OnInkConsumed += OnInkConsumed;
            
            // 채움률 변경 이벤트 구독
            drawingCanvas.OnFillPercentageChanged += OnFillPercentageChanged;
            
            // 드로잉 시작/종료 이벤트 구독 (붓글씨 사운드)
            drawingCanvas.OnDrawStart += OnDrawingStart;
            drawingCanvas.OnDrawEnd += OnDrawingEnd;
        }
        
        // 먹물 게이지 초기화
        if (inkGauge != null)
        {
            inkGauge.ResetGauge();
        }
        
        // 채움률 게이지 초기화
        if (fillGauge != null)
        {
            fillGauge.ResetGauge();
            fillGauge.SetTargetPercentage(requiredFillPercentage);
        }
        
        // 결과 애니메이션 요소 숨김
        HideResultElements();
        
        // 타이머 시작
        if (timer != null)
        {
            timer.StartTimer(gameDuration, speed);
            timer.OnTimerEnd += OnTimeOut;
        }
        
        // 커스텀 커서 적용
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
        }
        
        Debug.Log($"[Jaewon_GAME_3] 게임 시작 - 한자: {(isTian ? "天" : "地")}, 목표: {requiredFillPercentage * 100f}%, 먹물: {maxInkAmount}");
    }
    
    /// <summary>
    /// 난이도 적용
    /// </summary>
    private void ApplyDifficulty(int difficulty)
    {
        int index = Mathf.Clamp(difficulty - 1, 0, 2);
        
        float prevFillReq = requiredFillPercentage;
        float prevInk = maxInkAmount;
        
        if (difficultyFillRequirements != null && difficultyFillRequirements.Length > index)
        {
            requiredFillPercentage = difficultyFillRequirements[index];
        }
        
        if (difficultyInkAmounts != null && difficultyInkAmounts.Length > index)
        {
            maxInkAmount = difficultyInkAmounts[index];
        }
        
        Debug.Log($"[Jaewon_GAME_3] 난이도 적용 - difficulty:{difficulty}, index:{index}, " +
                  $"목표채움률:{prevFillReq*100f}%→{requiredFillPercentage*100f}%, " +
                  $"먹물:{prevInk}→{maxInkAmount}");
    }
    
    /// <summary>
    /// 목표 한자 UI 설정
    /// </summary>
    private void SetupTargetCharacter()
    {
        if (targetCharacterText != null)
        {
            targetCharacterText.text = isTian ? "天" : "地";
        }
        
        if (guideText != null)
        {
            guideText.text = $"'{(isTian ? "天" : "地")}' 를 그려라!";
        }
    }
    
    /// <summary>
    /// 먹물 소비 이벤트 처리 (경로 길이에 따라 게이지 감소)
    /// </summary>
    private void OnInkConsumed(float amount)
    {
        // 먹물 게이지 UI 업데이트
        if (drawingCanvas != null && inkGauge != null)
        {
            inkGauge.SetValue(drawingCanvas.InkRatio);
        }
    }
    
    /// <summary>
    /// 채움률 변경 이벤트 처리
    /// </summary>
    private void OnFillPercentageChanged(float fillPercentage)
    {
        // 채움률 게이지 UI 업데이트
        if (fillGauge != null)
        {
            fillGauge.SetValue(fillPercentage);
        }
        
        // 목표 채움률 달성 시 즉시 성공 처리
        if (!isGameEnded && fillPercentage >= requiredFillPercentage)
        {
            OnSuccess();
        }
    }
    
    /// <summary>
    /// 드로잉 시작 시 붓글씨 사운드 재생
    /// </summary>
    private void OnDrawingStart()
    {
        if (brushAudioSource != null && brushSoundClip != null && !brushAudioSource.isPlaying)
        {
            brushAudioSource.Play();
        }
    }
    
    /// <summary>
    /// 드로잉 종료 시 붓글씨 사운드 정지
    /// </summary>
    private void OnDrawingEnd()
    {
        if (brushAudioSource != null && brushAudioSource.isPlaying)
        {
            brushAudioSource.Stop();
        }
    }
    
    private void Update()
    {
        if (isGameEnded) return;
    }
    
    /// <summary>
    /// 시간 종료 시 호출
    /// </summary>
    private void OnTimeOut()
    {
        if (isGameEnded) return;
        
        // 채움률 계산
        float fillPercentage = 0f;
        if (drawingCanvas != null)
        {
            fillPercentage = drawingCanvas.CalculateFillPercentage();
        }
        
        // 판정 결과 미리 계산
        bool isSuccess = fillPercentage >= requiredFillPercentage;
        
        Debug.Log($"[Jaewon_GAME_3] 시간 종료! 채움률: {fillPercentage * 100f:F1}% vs 목표: {requiredFillPercentage * 100f}% → {(isSuccess ? "성공" : "실패")}");
        Debug.Log($"[Jaewon_GAME_3] 판정 상세: {fillPercentage:F4} >= {requiredFillPercentage:F4} = {isSuccess}");
        
        // 승패 판정
        if (isSuccess)
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
        if (isGameEnded) return;
        
        hasSucceeded = true;
        Debug.Log("[Jaewon_GAME_3] 성공! 도술 발동!");
        
        // 드로잉 비활성화
        if (drawingCanvas != null)
        {
            drawingCanvas.CanDraw = false;
        }
        
        // 결과 애니메이션과 함께 보고
        ReportResultWithAnimation(true);
    }
    
    /// <summary>
    /// 실패 처리
    /// </summary>
    private void OnFailure()
    {
        if (isGameEnded) return;
        
        Debug.Log("[Jaewon_GAME_3] 실패! 주문 실패...");
        
        // 드로잉 비활성화
        if (drawingCanvas != null)
        {
            drawingCanvas.CanDraw = false;
        }
        
        // 결과 애니메이션과 함께 보고
        ReportResultWithAnimation(false);
    }
    
    /// <summary>
    /// 커스텀 결과 애니메이션 (오버라이드)
    /// </summary>
    protected override void PlayResultAnimation(bool success, Action onComplete = null)
    {
        if (success)
        {
            StartCoroutine(PlayDragonRiseAnimation(onComplete));
        }
        else
        {
            StartCoroutine(PlayScrollTearAnimation(onComplete));
        }
    }
    
    /// <summary>
    /// 성공 애니메이션: 용 승천
    /// </summary>
    private IEnumerator PlayDragonRiseAnimation(Action onComplete)
    {
        Debug.Log("[Jaewon_GAME_3] 용 승천 애니메이션 시작");
        
        // 화면 플래시
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            Color flashColor = new Color(1f, 0.95f, 0.7f, 0f);
            flashOverlay.color = flashColor;
            
            float flashDuration = 0.2f;
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 0.8f, elapsed / flashDuration);
                flashColor.a = alpha;
                flashOverlay.color = flashColor;
                yield return null;
            }
            
            elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.8f, 0f, elapsed / flashDuration);
                flashColor.a = alpha;
                flashOverlay.color = flashColor;
                yield return null;
            }
            
            flashOverlay.gameObject.SetActive(false);
        }
        
        // 용 승천 애니메이션
        if (dragonImage != null)
        {
            dragonImage.gameObject.SetActive(true);
            
            Vector2 startPos = dragonImage.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0, dragonRiseHeight);
            Vector3 startScale = Vector3.one * 0.5f;
            Vector3 endScale = Vector3.one * 1.2f;
            
            float elapsed = 0f;
            
            while (elapsed < dragonRiseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dragonRiseDuration;
                float easeT = 1f - (1f - t) * (1f - t);
                
                dragonImage.anchoredPosition = Vector2.Lerp(startPos, endPos, easeT);
                dragonImage.localScale = Vector3.Lerp(startScale, endScale, easeT);
                
                yield return null;
            }
            
            dragonImage.anchoredPosition = endPos;
            dragonImage.localScale = endScale;
        }
        
        yield return new WaitForSeconds(0.5f);
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// 실패 애니메이션: 두루마리 찢김
    /// </summary>
    private IEnumerator PlayScrollTearAnimation(Action onComplete)
    {
        Debug.Log("[Jaewon_GAME_3] 두루마리 찢김 애니메이션 시작");
        
        // 화면 흔들림
        if (scrollImage != null)
        {
            Vector2 originalPos = scrollImage.anchoredPosition;
            float shakeDuration = 0.3f;
            float shakeIntensity = 10f;
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float x = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
                float y = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
                scrollImage.anchoredPosition = originalPos + new Vector2(x, y);
                yield return null;
            }
            
            scrollImage.anchoredPosition = originalPos;
        }
        
        // 두루마리 좌우 분리 애니메이션
        if (scrollLeftPart != null && scrollRightPart != null)
        {
            scrollLeftPart.gameObject.SetActive(true);
            scrollRightPart.gameObject.SetActive(true);
            
            if (scrollImage != null)
            {
                scrollImage.gameObject.SetActive(false);
            }
            
            Vector2 leftStart = scrollLeftPart.anchoredPosition;
            Vector2 rightStart = scrollRightPart.anchoredPosition;
            Vector2 leftEnd = leftStart + new Vector2(-scrollTearDistance, -50f);
            Vector2 rightEnd = rightStart + new Vector2(scrollTearDistance, -50f);
            
            float elapsed = 0f;
            
            while (elapsed < scrollTearDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scrollTearDuration;
                float easeT = t * t;
                
                scrollLeftPart.anchoredPosition = Vector2.Lerp(leftStart, leftEnd, easeT);
                scrollRightPart.anchoredPosition = Vector2.Lerp(rightStart, rightEnd, easeT);
                
                scrollLeftPart.localRotation = Quaternion.Euler(0, 0, -15f * easeT);
                scrollRightPart.localRotation = Quaternion.Euler(0, 0, 15f * easeT);
                
                yield return null;
            }
        }
        else if (scrollImage != null)
        {
            Vector3 startScale = scrollImage.localScale;
            Vector3 endScale = new Vector3(0.8f, 0.3f, 1f);
            
            float elapsed = 0f;
            
            while (elapsed < scrollTearDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scrollTearDuration;
                scrollImage.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }
        }
        
        yield return new WaitForSeconds(0.3f);
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// 결과 애니메이션 요소 숨김
    /// </summary>
    private void HideResultElements()
    {
        if (dragonImage != null) dragonImage.gameObject.SetActive(false);
        if (flashOverlay != null) flashOverlay.gameObject.SetActive(false);
        if (scrollLeftPart != null) scrollLeftPart.gameObject.SetActive(false);
        if (scrollRightPart != null) scrollRightPart.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 게임 상태 초기화
    /// </summary>
    protected override void ResetGameState()
    {
        hasSucceeded = false;
        
        // 커스텀 커서가 설정된 경우에만 기본 커서로 복원
        if (cursorTexture != null)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        
        // 타이머 중지 및 이벤트 해제
        if (timer != null)
        {
            timer.Stop();
            timer.OnTimerEnd -= OnTimeOut;
        }
        
        // 드로잉 캔버스 이벤트 해제 및 리셋
        if (drawingCanvas != null)
        {
            drawingCanvas.OnInkConsumed -= OnInkConsumed;
            drawingCanvas.OnFillPercentageChanged -= OnFillPercentageChanged;
            drawingCanvas.OnDrawStart -= OnDrawingStart;
            drawingCanvas.OnDrawEnd -= OnDrawingEnd;
            drawingCanvas.ResetCanvas();
            drawingCanvas.CanDraw = false;
        }
        
        // 붓글씨 사운드 정지
        if (brushAudioSource != null && brushAudioSource.isPlaying)
        {
            brushAudioSource.Stop();
        }
        
        // 먹물 게이지 리셋
        if (inkGauge != null)
        {
            inkGauge.ResetGauge();
        }
        
        // 채움률 게이지 리셋
        if (fillGauge != null)
        {
            fillGauge.ResetGauge();
        }
        
        // 결과 애니메이션 요소 숨김 및 리셋
        HideResultElements();
        
        // 두루마리 복원
        if (scrollImage != null)
        {
            scrollImage.gameObject.SetActive(true);
            scrollImage.localScale = Vector3.one;
            scrollImage.localRotation = Quaternion.identity;
        }
        
        // 용 이미지 리셋
        if (dragonImage != null)
        {
            dragonImage.anchoredPosition = dragonImageInitialPosition;
            dragonImage.localScale = Vector3.one;
        }
        
        // 두루마리 조각 위치/회전 리셋
        if (scrollLeftPart != null)
        {
            scrollLeftPart.anchoredPosition = scrollLeftPartInitialPosition;
            scrollLeftPart.localRotation = Quaternion.identity;
        }
        if (scrollRightPart != null)
        {
            scrollRightPart.anchoredPosition = scrollRightPartInitialPosition;
            scrollRightPart.localRotation = Quaternion.identity;
        }
    }
    
    /// <summary>
    /// 게임 종료 시 호출
    /// </summary>
    protected override void OnGameEnd()
    {
        base.OnGameEnd();
        
        // 드로잉 비활성화
        if (drawingCanvas != null)
        {
            drawingCanvas.CanDraw = false;
        }
        
        // 커스텀 커서가 설정된 경우에만 기본 커서로 복원
        if (cursorTexture != null)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}