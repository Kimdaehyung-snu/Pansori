using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pansori.Microgames
{
    /// <summary>
    /// 판소리 씬 UI 및 연출 관리
    /// 마이크로게임 사이에 표시되는 판소리 무대를 관리합니다.
    /// 게임 정보(목숨, 스테이지)도 함께 표시합니다.
    /// </summary>
    public class PansoriSceneUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private GameObject pansoriPanel; // 판소리 씬 패널
        [SerializeField] private Image backgroundImage; // 배경 이미지
        [SerializeField] private TMP_Text commandText; // "XX해라!" 명령 텍스트
        [SerializeField] private TMP_Text reactionText; // 환호/야유 반응 텍스트
        [SerializeField] private TMP_Text controlDescriptionText; // 조작법 설명 텍스트
        
        [Header("게임 정보 UI")]
        [SerializeField] private TMP_Text livesText; // 목숨 텍스트
        [SerializeField] private TMP_Text stageText; // 스테이지 텍스트
        [SerializeField] private Transform livesContainer; // 목숨 스프라이트 컨테이너
        
        [Header("목숨 스프라이트 설정")]
        [SerializeField] private Sprite lifeSprite; // 목숨 스프라이트
        [SerializeField] private Sprite consumedLifeSprite; // 소모된 목숨 스프라이트 (선택사항)
        [SerializeField] private float lifeSpriteSpacing = 10f; // 스프라이트 간 간격
        [SerializeField] private Vector2 lifeSpriteSize = new Vector2(50, 50); // 스프라이트 크기
        
        [Header("관객 스프라이트 (목숨 표시)")]
        [Tooltip("4개의 관객 스프라이트를 좌->우 순서(1,2,3,4)로 할당. 목숨 소모 시 퇴장 순서: 4>1>3>2")]
        [SerializeField] private Image[] audienceSprites; // 4개의 관객 스프라이트 (좌->우: 1,2,3,4)
        [SerializeField] private float audienceExitDuration = 0.5f; // 퇴장 애니메이션 시간
        [SerializeField] private float audienceExitDistance = 500f; // 퇴장 거리 (화면 밖)
        
        [Header("관객 스프라이트 애니메이션")]
        [Tooltip("각 관객의 첫 번째 스프라이트 (4개)")]
        [SerializeField] private Sprite[] audienceSprite1Array; // 각 관객의 첫 번째 스프라이트
        [Tooltip("각 관객의 두 번째 스프라이트 (4개)")]
        [SerializeField] private Sprite[] audienceSprite2Array; // 각 관객의 두 번째 스프라이트
        [SerializeField] private float audienceSpriteAnimationInterval = 0.3f; // 애니메이션 간격 (초)
        
        [Header("캐릭터 (플레이스홀더)")]
        [SerializeField] private GameObject performerObject; // 소리꾼 오브젝트
        [SerializeField] private GameObject audienceObject; // 관객 오브젝트
        [SerializeField] private Animator performerAnimator; // 소리꾼 애니메이터
        [SerializeField] private Animator audienceAnimator; // 관객 애니메이터
        
        [Header("색상 설정")]
        [SerializeField] private Color normalBackgroundColor = new Color(0.9f, 0.85f, 0.75f); // 기본 배경색
        [SerializeField] private Color successBackgroundColor = new Color(0.7f, 1f, 0.7f); // 성공 시 배경색
        [SerializeField] private Color failureBackgroundColor = new Color(1f, 0.7f, 0.7f); // 실패 시 배경색
        
        [Header("텍스트 설정")]
        [SerializeField] private string successReactionText = "얼쑤!"; // 성공 반응 텍스트
        [SerializeField] private string failureReactionText = "에잇..."; // 실패 반응 텍스트
        [SerializeField] private Color successTextColor = new Color(0.2f, 0.6f, 0.2f); // 성공 텍스트 색상
        [SerializeField] private Color failureTextColor = new Color(0.8f, 0.2f, 0.2f); // 실패 텍스트 색상
        
        [Header("애니메이션 설정")]
        [SerializeField] private float commandFadeInDuration = 0.3f; // 명령 텍스트 페이드인 시간
        [SerializeField] private float reactionScalePunchAmount = 1.2f; // 반응 텍스트 스케일 펀치 크기
        
        [Header("화면 효과 설정")]
        [SerializeField] private Image screenFlashOverlay; // 화면 플래시 오버레이
        [SerializeField] private bool useScreenFlash = true; // 화면 플래시 사용 여부
        [SerializeField] private float screenFlashDuration = 0.2f; // 화면 플래시 시간
        [SerializeField] private float screenFlashAlpha = 0.6f; // 화면 플래시 투명도
        
        [Header("화면 흔들림 설정")]
        [SerializeField] private bool useScreenShake = true; // 화면 흔들림 사용 여부
        [SerializeField] private float screenShakeIntensity = 10f; // 화면 흔들림 강도
        [SerializeField] private float screenShakeDuration = 0.3f; // 화면 흔들림 시간
        [SerializeField] private int screenShakeCount = 5; // 화면 흔들림 횟수
        
        [Header("캐릭터 애니메이션 트리거")]
        [SerializeField] private string successAnimTrigger = "Success"; // 성공 애니메이션 트리거
        [SerializeField] private string failureAnimTrigger = "Failure"; // 실패 애니메이션 트리거
        [SerializeField] private string idleAnimTrigger = "Idle"; // 대기 애니메이션 트리거
        
        [Header("추가 연출 설정")]
        [SerializeField] private bool useCharacterAnimation = true; // 캐릭터 애니메이션 사용 여부
        [SerializeField] private bool useBackgroundColorChange = true; // 배경색 변경 사용 여부
        [SerializeField] private float backgroundColorTransitionDuration = 0.3f; // 배경색 전환 시간
        
        [Header("판소리 스프라이트 애니메이션")]
        [SerializeField] private Animator pansoriSpriteAnimator; // 판소리 스프라이트 애니메이터
        [SerializeField] private GameObject pansoriSpriteAnimatorObject; // 애니메이터 오브젝트 (활성화/비활성화용)
        
        [Header("속도 증가 알림 설정 (지화자!)")]
        [SerializeField] private string speedUpNotificationText = "지화자!"; // 속도 증가 알림 텍스트
        [SerializeField] private Color speedUpTextColor = new Color(1f, 0.8f, 0f); // 속도 증가 텍스트 색상 (금색)
        [SerializeField] private Color speedUpBackgroundColor = new Color(1f, 0.95f, 0.7f); // 속도 증가 배경색
        [SerializeField] private float speedUpScalePunchAmount = 1.5f; // 속도 증가 스케일 펀치 크기
        
        [Header("문 트랜지션 설정")]
        [SerializeField] private Image leftDoorImage; // 좌측 문 이미지
        [SerializeField] private Image rightDoorImage; // 우측 문 이미지
        [SerializeField] private Sprite doorSprite; // 문 스프라이트 (없으면 단색 사용)
        [SerializeField] private Color doorColor = Color.black; // 문 색상 (스프라이트 없을 시)
        [SerializeField] private float doorTransitionDuration = 0.5f; // 문 트랜지션 시간
        [SerializeField] private bool useDoorTransition = true; // 문 트랜지션 사용 여부
        
        private Coroutine currentCoroutine;
        private Canvas canvas;
        private RectTransform canvasRectTransform;
        private Vector3 originalCanvasPosition;
        
        /// <summary>
        /// 생성된 목숨 스프라이트 리스트
        /// </summary>
        private List<GameObject> lifeSpriteObjects = new List<GameObject>();
        
        /// <summary>
        /// 관객 스프라이트 원래 위치 배열
        /// </summary>
        private Vector2[] audienceOriginalPositions;
        
        /// <summary>
        /// 관객 퇴장 순서 (4>1>3>2 -> 인덱스: 3,0,2,1)
        /// </summary>
        private readonly int[] audienceExitOrder = { 3, 0, 2, 1 };
        
        /// <summary>
        /// 이전에 소모된 목숨 수 (변경 감지용)
        /// </summary>
        private int previousConsumedLives = 0;
        
        /// <summary>
        /// 관객 스프라이트 애니메이션 코루틴
        /// </summary>
        private Coroutine audienceSpriteAnimationCoroutine;
        
        /// <summary>
        /// 각 관객의 현재 프레임 인덱스 (true=sprite1, false=sprite2)
        /// </summary>
        private bool[] audienceSpriteFrameIndex;
        
        private void Awake()
        {
            SetupCanvas();
            SetupLivesContainer();
            SetupScreenFlashOverlay();
            SetupDoorTransitionUI();
            InitializeAudienceSprites();
            HideAll();
        }
        
        /// <summary>
        /// Canvas 설정
        /// </summary>
        private void SetupCanvas()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            
            canvasRectTransform = GetComponent<RectTransform>();
            if (canvasRectTransform != null)
            {
                originalCanvasPosition = canvasRectTransform.localPosition;
            }
            
            // CanvasScaler 설정
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            // GraphicRaycaster 설정
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }
        
        /// <summary>
        /// 목숨 컨테이너를 설정합니다.
        /// </summary>
        private void SetupLivesContainer()
        {
            // 컨테이너가 없으면 자동 생성
            if (livesContainer == null)
            {
                GameObject containerObj = new GameObject("LivesContainer");
                containerObj.transform.SetParent(transform, false);
                livesContainer = containerObj.transform;
                
                RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                rectTransform.anchoredPosition = new Vector2(0, -50);
                rectTransform.sizeDelta = new Vector2(500, 100);
            }
        }
        
        /// <summary>
        /// 화면 플래시 오버레이를 설정합니다.
        /// </summary>
        private void SetupScreenFlashOverlay()
        {
            if (screenFlashOverlay == null && useScreenFlash)
            {
                GameObject overlayObj = new GameObject("ScreenFlashOverlay");
                overlayObj.transform.SetParent(transform, false);
                
                RectTransform rectTransform = overlayObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                
                screenFlashOverlay = overlayObj.AddComponent<Image>();
                screenFlashOverlay.color = Color.clear;
                screenFlashOverlay.raycastTarget = false;
                
                overlayObj.SetActive(false);
            }
        }
        
        /// <summary>
        /// 문 트랜지션 UI를 설정합니다.
        /// </summary>
        private void SetupDoorTransitionUI()
        {
            if (!useDoorTransition) return;
            
            // 좌측 문 이미지가 없으면 동적 생성
            if (leftDoorImage == null)
            {
                GameObject leftDoorObj = new GameObject("LeftDoor");
                leftDoorObj.transform.SetParent(transform, false);
                
                RectTransform leftRect = leftDoorObj.AddComponent<RectTransform>();
                leftRect.anchorMin = new Vector2(0f, 0f);
                leftRect.anchorMax = new Vector2(0.5f, 1f);
                leftRect.pivot = new Vector2(1f, 0.5f); // 오른쪽 중앙 피벗 (열릴 때 왼쪽으로)
                leftRect.sizeDelta = Vector2.zero;
                leftRect.anchoredPosition = Vector2.zero;
                
                leftDoorImage = leftDoorObj.AddComponent<Image>();
                if (doorSprite != null)
                {
                    leftDoorImage.sprite = doorSprite;
                }
                leftDoorImage.color = doorColor;
                leftDoorImage.raycastTarget = false;
                
                leftDoorObj.SetActive(false);
            }
            
            // 우측 문 이미지가 없으면 동적 생성
            if (rightDoorImage == null)
            {
                GameObject rightDoorObj = new GameObject("RightDoor");
                rightDoorObj.transform.SetParent(transform, false);
                
                RectTransform rightRect = rightDoorObj.AddComponent<RectTransform>();
                rightRect.anchorMin = new Vector2(0.5f, 0f);
                rightRect.anchorMax = new Vector2(1f, 1f);
                rightRect.pivot = new Vector2(0f, 0.5f); // 왼쪽 중앙 피벗 (열릴 때 오른쪽으로)
                rightRect.sizeDelta = Vector2.zero;
                rightRect.anchoredPosition = Vector2.zero;
                
                rightDoorImage = rightDoorObj.AddComponent<Image>();
                if (doorSprite != null)
                {
                    rightDoorImage.sprite = doorSprite;
                }
                rightDoorImage.color = doorColor;
                rightDoorImage.raycastTarget = false;
                
                rightDoorObj.SetActive(false);
            }
            
            Debug.Log("[PansoriSceneUI] 문 트랜지션 UI 설정 완료");
        }
        
        /// <summary>
        /// 관객 스프라이트 초기화 - 원래 위치 저장
        /// </summary>
        private void InitializeAudienceSprites()
        {
            if (audienceSprites == null || audienceSprites.Length == 0)
            {
                return;
            }
            
            audienceOriginalPositions = new Vector2[audienceSprites.Length];
            
            for (int i = 0; i < audienceSprites.Length; i++)
            {
                if (audienceSprites[i] != null)
                {
                    RectTransform rectTransform = audienceSprites[i].rectTransform;
                    audienceOriginalPositions[i] = rectTransform.anchoredPosition;
                }
            }
            
            Debug.Log($"[PansoriSceneUI] 관객 스프라이트 초기화 완료 - {audienceSprites.Length}개");
        }
        
        /// <summary>
        /// 모든 UI 숨기기
        /// </summary>
        public void HideAll()
        {
            // 모든 코루틴 정지 (단일 코루틴 참조만으로는 부족)
            StopAllCoroutines();
            currentCoroutine = null;
            
            if (pansoriPanel != null)
            {
                pansoriPanel.SetActive(false);
            }
            
            if (commandText != null)
            {
                commandText.gameObject.SetActive(false);
            }
            
            if (reactionText != null)
            {
                reactionText.gameObject.SetActive(false);
            }
            
            // 화면 플래시 오버레이 숨기기
            if (screenFlashOverlay != null)
            {
                screenFlashOverlay.gameObject.SetActive(false);
            }
            
            // 판소리 스프라이트 애니메이션 중지
            StopPansoriSpriteAnimation();
            
            // 관객 스프라이트 애니메이션 중지
            StopAudienceSpriteAnimation();
            
            // 캔버스 위치 복원
            if (canvasRectTransform != null)
            {
                canvasRectTransform.localPosition = originalCanvasPosition;
            }
            
            // 게임 정보 UI 숨기기
            HideGameInfo();
            
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }
        
        /// <summary>
        /// 게임 정보 UI 숨기기
        /// </summary>
        private void HideGameInfo()
        {
            if (livesText != null)
            {
                livesText.gameObject.SetActive(false);
            }
            
            if (stageText != null)
            {
                stageText.gameObject.SetActive(false);
            }
            
            if (controlDescriptionText != null)
            {
                controlDescriptionText.gameObject.SetActive(false);
            }
            
            ClearLifeSprites();
            
            // 관객 스프라이트는 게임 완전 리셋 시에만 위치 초기화
            // ResetAudiencePositions()는 외부에서 명시적으로 호출해야 함
        }
        
        /// <summary>
        /// 판소리 씬 표시
        /// </summary>
        public void Show()
        {
            if (canvas != null)
            {
                canvas.enabled = true;
            }
            
            if (pansoriPanel != null)
            {
                pansoriPanel.SetActive(true);
            }
            
            // 기본 배경색으로 설정
            if (backgroundImage != null)
            {
                backgroundImage.color = normalBackgroundColor;
            }
            
            // 판소리 스프라이트 애니메이션 시작
            StartPansoriSpriteAnimation();
            
            // 관객 스프라이트 애니메이션 시작
            StartAudienceSpriteAnimation();
            
            // 캐릭터 Idle 애니메이션
            PlayCharacterAnimation(idleAnimTrigger);
        }
        
        /// <summary>
        /// "XX해라!" 명령 표시 (기존 메서드 - 호환성 유지)
        /// </summary>
        /// <param name="gameName">게임 이름</param>
        /// <param name="delay">표시 전 대기 시간</param>
        /// <param name="onComplete">완료 콜백</param>
        public void ShowCommand(string gameName, float delay, Action onComplete)
        {
            Show();
            currentCoroutine = StartCoroutine(ShowCommandCoroutine(gameName, delay, onComplete));
        }
        
        /// <summary>
        /// "XX해라!" 명령과 게임 정보를 함께 표시
        /// </summary>
        /// <param name="gameName">게임 이름</param>
        /// <param name="totalLives">총 목숨</param>
        /// <param name="consumedLives">소모된 목숨</param>
        /// <param name="stage">현재 스테이지</param>
        /// <param name="delay">표시 전 대기 시간</param>
        /// <param name="onComplete">완료 콜백</param>
        /// <param name="controlDescription">조작법 설명 (선택)</param>
        public void ShowCommandWithInfo(string gameName, int totalLives, int consumedLives, int stage, float delay, Action onComplete, string controlDescription = "")
        {
            Show();
            
            // 게임 정보 표시
            UpdateLivesDisplay(totalLives, consumedLives);
            UpdateStageDisplay(stage);
            
            // 조작법 설명 표시
            UpdateControlDescription(controlDescription);
            
            currentCoroutine = StartCoroutine(ShowCommandCoroutine(gameName, delay, onComplete));
        }
        
        /// <summary>
        /// 조작법 설명 업데이트
        /// </summary>
        /// <param name="controlDescription">조작법 설명</param>
        public void UpdateControlDescription(string controlDescription)
        {
            if (controlDescriptionText != null)
            {
                if (!string.IsNullOrEmpty(controlDescription))
                {
                    controlDescriptionText.text = controlDescription;
                    controlDescriptionText.gameObject.SetActive(true);
                }
                else
                {
                    controlDescriptionText.gameObject.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// 명령 표시 코루틴
        /// </summary>
        private IEnumerator ShowCommandCoroutine(string gameName, float delay, Action onComplete)
        {
            // 명령 텍스트 숨기기
            if (commandText != null)
            {
                commandText.gameObject.SetActive(false);
            }
            
            // 대기
            yield return new WaitForSeconds(delay);
            
            // 명령 텍스트 표시
            if (commandText != null)
            {
                commandText.text = $"{gameName}";
                commandText.gameObject.SetActive(true);
                
                // 간단한 페이드인 효과
                yield return StartCoroutine(FadeInText(commandText, commandFadeInDuration));
            }
            
            // 약간의 추가 대기 후 콜백
            yield return new WaitForSeconds(1.5f);
            
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 마이크로게임 결과에 따른 반응 표시
        /// </summary>
        /// <param name="success">성공 여부</param>
        /// <param name="duration">표시 시간</param>
        /// <param name="onComplete">완료 콜백</param>
        public void ShowReaction(bool success, float duration, Action onComplete)
        {
            Show();
            currentCoroutine = StartCoroutine(ShowReactionCoroutine(success, duration, onComplete));
        }
        
        /// <summary>
        /// 마이크로게임 결과에 따른 반응과 게임 정보를 함께 표시
        /// </summary>
        /// <param name="success">성공 여부</param>
        /// <param name="totalLives">총 목숨</param>
        /// <param name="consumedLives">소모된 목숨</param>
        /// <param name="stage">현재 스테이지</param>
        /// <param name="duration">표시 시간</param>
        /// <param name="onComplete">완료 콜백</param>
        public void ShowReactionWithInfo(bool success, int totalLives, int consumedLives, int stage, float duration, Action onComplete)
        {
            Show();
            UpdateLivesDisplay(totalLives, consumedLives);
            UpdateStageDisplay(stage);
            currentCoroutine = StartCoroutine(ShowReactionWithInfoCoroutine(success, duration, onComplete));
        }
        
        /// <summary>
        /// 속도 증가 알림 표시 ("지화자!" 연출)
        /// 4단계마다 속도/난이도가 증가할 때 호출됩니다.
        /// </summary>
        /// <param name="duration">표시 시간</param>
        /// <param name="onComplete">완료 콜백</param>
        public void ShowSpeedUpNotification(float duration, Action onComplete)
        {
            Show();
            currentCoroutine = StartCoroutine(ShowSpeedUpNotificationCoroutine(duration, onComplete));
        }
        
        /// <summary>
        /// 속도 증가 알림 표시 코루틴
        /// </summary>
        private IEnumerator ShowSpeedUpNotificationCoroutine(float duration, Action onComplete)
        {
            // 명령 텍스트 숨기기
            if (commandText != null)
            {
                commandText.gameObject.SetActive(false);
            }
            
            // 게임 정보 숨기기
            HideGameInfo();
            
            // 화면 플래시 효과 (특별한 색상으로)
            if (useScreenFlash && screenFlashOverlay != null)
            {
                StartCoroutine(PlaySpeedUpScreenFlash());
            }
            
            // 배경색 변경 (특별한 색상으로)
            if (useBackgroundColorChange && backgroundImage != null)
            {
                StartCoroutine(TransitionBackgroundColor(speedUpBackgroundColor, backgroundColorTransitionDuration));
            }
            
            // 캐릭터 성공 애니메이션
            if (useCharacterAnimation)
            {
                PlayCharacterAnimation(successAnimTrigger);
            }
            
            // 속도 증가 사운드 재생
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySpeedUpSound();
            }
            
            // 반응 텍스트에 "지화자!" 표시
            if (reactionText != null)
            {
                reactionText.text = speedUpNotificationText;
                reactionText.color = speedUpTextColor;
                reactionText.gameObject.SetActive(true);
                
                // 더 강한 스케일 펀치 효과
                yield return StartCoroutine(ScalePunchEffect(reactionText.rectTransform, speedUpScalePunchAmount, 0.4f));
            }
            
            // 대기
            yield return new WaitForSeconds(duration);
            
            // 반응 텍스트 숨기기
            if (reactionText != null)
            {
                reactionText.gameObject.SetActive(false);
            }
            
            // 배경색 복원
            if (useBackgroundColorChange && backgroundImage != null)
            {
                yield return StartCoroutine(TransitionBackgroundColor(normalBackgroundColor, backgroundColorTransitionDuration));
            }
            
            // 캐릭터 Idle로 복귀
            PlayCharacterAnimation(idleAnimTrigger);
            
            Debug.Log("[PansoriSceneUI] 속도 증가 알림 완료 (지화자!)");
            
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 속도 증가 전용 화면 플래시 효과
        /// </summary>
        private IEnumerator PlaySpeedUpScreenFlash()
        {
            if (screenFlashOverlay == null) yield break;
            
            Color flashColor = speedUpBackgroundColor;
            flashColor.a = screenFlashAlpha;
            
            screenFlashOverlay.gameObject.SetActive(true);
            screenFlashOverlay.color = flashColor;
            
            // 페이드 아웃
            float elapsed = 0f;
            while (elapsed < screenFlashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / screenFlashDuration;
                Color color = flashColor;
                color.a = Mathf.Lerp(screenFlashAlpha, 0f, t);
                screenFlashOverlay.color = color;
                yield return null;
            }
            
            screenFlashOverlay.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 반응 표시 코루틴 (강화된 버전)
        /// </summary>
        private IEnumerator ShowReactionCoroutine(bool success, float duration, Action onComplete)
        {
            // 명령 텍스트 숨기기
            if (commandText != null)
            {
                commandText.gameObject.SetActive(false);
            }
            
            // 게임 정보 숨기기
            HideGameInfo();
            
            // 화면 플래시 효과 (비동기)
            if (useScreenFlash)
            {
                StartCoroutine(PlayScreenFlash(success));
            }
            
            // 화면 흔들림 효과 (실패 시에만, 비동기)
            if (!success && useScreenShake)
            {
                StartCoroutine(PlayScreenShake());
            }
            
            // 배경색 변경 (애니메이션)
            if (useBackgroundColorChange && backgroundImage != null)
            {
                Color targetColor = success ? successBackgroundColor : failureBackgroundColor;
                StartCoroutine(TransitionBackgroundColor(targetColor, backgroundColorTransitionDuration));
            }
            
            // 캐릭터 애니메이션
            if (useCharacterAnimation)
            {
                string trigger = success ? successAnimTrigger : failureAnimTrigger;
                PlayCharacterAnimation(trigger);
            }
            
            // 반응 텍스트 표시
            if (reactionText != null)
            {
                reactionText.text = success ? successReactionText : failureReactionText;
                reactionText.color = success ? successTextColor : failureTextColor;
                reactionText.gameObject.SetActive(true);
                
                // 스케일 펀치 효과
                yield return StartCoroutine(ScalePunchEffect(reactionText.rectTransform, reactionScalePunchAmount, 0.3f));
            }
            
            // 대기
            yield return new WaitForSeconds(duration);
            
            // 반응 텍스트 숨기기
            if (reactionText != null)
            {
                reactionText.gameObject.SetActive(false);
            }
            
            // 배경색 복원 (애니메이션)
            if (useBackgroundColorChange && backgroundImage != null)
            {
                yield return StartCoroutine(TransitionBackgroundColor(normalBackgroundColor, backgroundColorTransitionDuration));
            }
            
            // 캐릭터 Idle로 복귀
            PlayCharacterAnimation(idleAnimTrigger);
            
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 반응 표시 코루틴 (게임 정보 유지 버전)
        /// </summary>
        private IEnumerator ShowReactionWithInfoCoroutine(bool success, float duration, Action onComplete)
        {
            // 명령 텍스트 숨기기
            if (commandText != null)
            {
                commandText.gameObject.SetActive(false);
            }
            
            // 게임 정보는 숨기지 않음 (목숨, 스테이지 유지)
            
            // 화면 플래시 효과 (비동기)
            if (useScreenFlash)
            {
                StartCoroutine(PlayScreenFlash(success));
            }
            
            // 화면 흔들림 효과 (실패 시에만, 비동기)
            if (!success && useScreenShake)
            {
                StartCoroutine(PlayScreenShake());
            }
            
            // 배경색 변경 (애니메이션)
            if (useBackgroundColorChange && backgroundImage != null)
            {
                Color targetColor = success ? successBackgroundColor : failureBackgroundColor;
                StartCoroutine(TransitionBackgroundColor(targetColor, backgroundColorTransitionDuration));
            }
            
            // 캐릭터 애니메이션
            if (useCharacterAnimation)
            {
                string trigger = success ? successAnimTrigger : failureAnimTrigger;
                PlayCharacterAnimation(trigger);
            }
            
            // 반응 텍스트 표시
            if (reactionText != null)
            {
                reactionText.text = success ? successReactionText : failureReactionText;
                reactionText.color = success ? successTextColor : failureTextColor;
                reactionText.gameObject.SetActive(true);
                
                // 스케일 펀치 효과
                yield return StartCoroutine(ScalePunchEffect(reactionText.rectTransform, reactionScalePunchAmount, 0.3f));
            }
            
            // 대기
            yield return new WaitForSeconds(duration);
            
            // 반응 텍스트 숨기기
            if (reactionText != null)
            {
                reactionText.gameObject.SetActive(false);
            }
            
            // 배경색 복원 (애니메이션)
            if (useBackgroundColorChange && backgroundImage != null)
            {
                yield return StartCoroutine(TransitionBackgroundColor(normalBackgroundColor, backgroundColorTransitionDuration));
            }
            
            // 캐릭터 Idle로 복귀
            PlayCharacterAnimation(idleAnimTrigger);
            
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 캐릭터 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="trigger">애니메이션 트리거 이름</param>
        private void PlayCharacterAnimation(string trigger)
        {
            if (string.IsNullOrEmpty(trigger)) return;
            
            if (performerAnimator != null)
            {
                performerAnimator.SetTrigger(trigger);
            }
            
            if (audienceAnimator != null)
            {
                audienceAnimator.SetTrigger(trigger);
            }
        }
        
        /// <summary>
        /// 화면 플래시 효과를 재생합니다.
        /// </summary>
        private IEnumerator PlayScreenFlash(bool success)
        {
            if (screenFlashOverlay == null) yield break;
            
            Color flashColor = success ? successBackgroundColor : failureBackgroundColor;
            flashColor.a = screenFlashAlpha;
            
            screenFlashOverlay.gameObject.SetActive(true);
            screenFlashOverlay.color = flashColor;
            
            // 페이드 아웃
            float elapsed = 0f;
            while (elapsed < screenFlashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / screenFlashDuration;
                Color color = flashColor;
                color.a = Mathf.Lerp(screenFlashAlpha, 0f, t);
                screenFlashOverlay.color = color;
                yield return null;
            }
            
            screenFlashOverlay.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 화면 흔들림 효과를 재생합니다.
        /// </summary>
        private IEnumerator PlayScreenShake()
        {
            if (canvasRectTransform == null) yield break;
            
            float shakeDuration = screenShakeDuration / screenShakeCount;
            
            for (int i = 0; i < screenShakeCount; i++)
            {
                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range(-screenShakeIntensity, screenShakeIntensity),
                    UnityEngine.Random.Range(-screenShakeIntensity, screenShakeIntensity),
                    0f
                );
                
                float elapsed = 0f;
                while (elapsed < shakeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / shakeDuration;
                    // 감쇠 효과
                    float damping = 1f - ((float)i / screenShakeCount);
                    canvasRectTransform.localPosition = originalCanvasPosition + randomOffset * damping * (1f - t);
                    yield return null;
                }
            }
            
            canvasRectTransform.localPosition = originalCanvasPosition;
        }
        
        /// <summary>
        /// 배경색 전환 애니메이션
        /// </summary>
        private IEnumerator TransitionBackgroundColor(Color targetColor, float duration)
        {
            if (backgroundImage == null) yield break;
            
            Color startColor = backgroundImage.color;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                backgroundImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }
            
            backgroundImage.color = targetColor;
        }
        
        /// <summary>
        /// 목숨 표시 업데이트
        /// </summary>
        /// <param name="totalLives">총 목숨</param>
        /// <param name="consumedLives">소모된 목숨</param>
        public void UpdateLivesDisplay(int totalLives, int consumedLives)
        {
            // 텍스트 업데이트
            if (livesText != null)
            {
                int remainingLives = totalLives - consumedLives;
                livesText.text = $"목숨: {remainingLives}";
                livesText.gameObject.SetActive(true);
            }
            
            // 관객 스프라이트 시스템 사용 (인스펙터에서 할당된 경우)
            if (audienceSprites != null && audienceSprites.Length > 0)
            {
                UpdateAudienceLivesDisplay(consumedLives);
                return;
            }
            
            // 기존 동적 스프라이트 생성 방식 (관객 스프라이트가 없는 경우)
            ClearLifeSprites();
            
            if (lifeSprite == null || livesContainer == null)
            {
                return;
            }
            
            // 총 목숨만큼 스프라이트 생성
            for (int i = 0; i < totalLives; i++)
            {
                GameObject spriteObj = CreateLifeSprite(i, totalLives);
                
                // 우측에서부터 consumedLives개는 소모된 상태로 표시
                bool isConsumed = i >= (totalLives - consumedLives);
                SetLifeSpriteState(spriteObj, isConsumed);
                
                lifeSpriteObjects.Add(spriteObj);
            }
            
            Debug.Log($"[PansoriSceneUI] 목숨 표시 업데이트 - 총: {totalLives}, 소모: {consumedLives}");
        }
        
        /// <summary>
        /// 관객 스프라이트 기반 목숨 표시 업데이트
        /// 소모된 목숨이 증가하면 해당 관객이 퇴장합니다.
        /// 퇴장 순서: 4 > 1 > 3 > 2 (인덱스: 3, 0, 2, 1)
        /// </summary>
        /// <param name="consumedLives">소모된 목숨 수</param>
        private void UpdateAudienceLivesDisplay(int consumedLives)
        {
            // 새로 소모된 목숨이 있는지 확인
            if (consumedLives > previousConsumedLives)
            {
                // 새로 소모된 목숨만큼 퇴장 애니메이션 실행
                for (int i = previousConsumedLives; i < consumedLives; i++)
                {
                    if (i < audienceExitOrder.Length)
                    {
                        int spriteIndex = audienceExitOrder[i];
                        PlayAudienceExitAnimation(spriteIndex);
                        Debug.Log($"[PansoriSceneUI] 관객 퇴장 - 순서: {i + 1}, 스프라이트 인덱스: {spriteIndex + 1}");
                    }
                }
            }
            
            previousConsumedLives = consumedLives;
        }
        
        /// <summary>
        /// 스테이지 표시 업데이트
        /// </summary>
        /// <param name="stage">현재 스테이지</param>
        public void UpdateStageDisplay(int stage)
        {
            if (stageText != null)
            {
                stageText.text = $"스테이지: {stage}";
                stageText.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// 목숨 스프라이트를 생성합니다.
        /// </summary>
        private GameObject CreateLifeSprite(int index, int totalLives)
        {
            GameObject spriteObj = new GameObject($"Life_{index}");
            spriteObj.transform.SetParent(livesContainer, false);
            
            Image image = spriteObj.AddComponent<Image>();
            image.sprite = lifeSprite;
            
            RectTransform rectTransform = spriteObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = lifeSpriteSize;
            
            // 좌측부터 나열 (중앙 정렬)
            float startX = -(totalLives - 1) * (lifeSpriteSize.x + lifeSpriteSpacing) / 2f;
            float xPos = startX + index * (lifeSpriteSize.x + lifeSpriteSpacing);
            rectTransform.anchoredPosition = new Vector2(xPos, 0);
            
            return spriteObj;
        }
        
        /// <summary>
        /// 목숨 스프라이트의 상태를 설정합니다.
        /// </summary>
        private void SetLifeSpriteState(GameObject spriteObj, bool isConsumed)
        {
            Image image = spriteObj.GetComponent<Image>();
            if (image == null)
                return;
            
            if (isConsumed)
            {
                // 소모된 목숨 처리
                if (consumedLifeSprite != null)
                {
                    image.sprite = consumedLifeSprite;
                }
                else
                {
                    // 스프라이트가 없으면 반투명 처리
                    Color color = image.color;
                    color.a = 0.3f;
                    image.color = color;
                }
            }
            else
            {
                // 정상 목숨
                image.sprite = lifeSprite;
                Color color = image.color;
                color.a = 1.0f;
                image.color = color;
            }
        }
        
        /// <summary>
        /// 기존 목숨 스프라이트를 모두 제거합니다.
        /// </summary>
        private void ClearLifeSprites()
        {
            foreach (GameObject spriteObj in lifeSpriteObjects)
            {
                if (spriteObj != null)
                {
                    Destroy(spriteObj);
                }
            }
            lifeSpriteObjects.Clear();
        }
        
        /// <summary>
        /// 관객 스프라이트 퇴장 애니메이션 재생
        /// </summary>
        /// <param name="spriteIndex">퇴장할 스프라이트 인덱스 (0~3)</param>
        private void PlayAudienceExitAnimation(int spriteIndex)
        {
            if (audienceSprites == null || spriteIndex < 0 || spriteIndex >= audienceSprites.Length)
            {
                return;
            }
            
            Image sprite = audienceSprites[spriteIndex];
            if (sprite == null)
            {
                return;
            }
            
            // 스프라이트 인덱스 0,1은 왼쪽으로, 2,3은 오른쪽으로 퇴장
            bool exitLeft = (spriteIndex == 0 || spriteIndex == 1);
            float exitDirection = exitLeft ? -1f : 1f;
            
            StartCoroutine(AudienceExitCoroutine(sprite, exitDirection));
        }
        
        /// <summary>
        /// 관객 퇴장 애니메이션 코루틴
        /// </summary>
        /// <param name="sprite">퇴장할 스프라이트</param>
        /// <param name="direction">퇴장 방향 (-1: 왼쪽, 1: 오른쪽)</param>
        private IEnumerator AudienceExitCoroutine(Image sprite, float direction)
        {
            if (sprite == null) yield break;
            
            RectTransform rectTransform = sprite.rectTransform;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(audienceExitDistance * direction, 0f);
            
            float elapsed = 0f;
            
            while (elapsed < audienceExitDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / audienceExitDuration);
                
                // EaseOutQuad 이징 적용 (자연스러운 감속)
                float easedT = 1f - (1f - t) * (1f - t);
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, easedT);
                yield return null;
            }
            
            rectTransform.anchoredPosition = endPos;
            
            Debug.Log($"[PansoriSceneUI] 관객 스프라이트 퇴장 완료 - 방향: {(direction < 0 ? "왼쪽" : "오른쪽")}");
        }
        
        /// <summary>
        /// 모든 관객 스프라이트를 원래 위치로 복원
        /// </summary>
        public void ResetAudiencePositions()
        {
            if (audienceSprites == null || audienceOriginalPositions == null)
            {
                return;
            }
            
            for (int i = 0; i < audienceSprites.Length; i++)
            {
                if (audienceSprites[i] != null && i < audienceOriginalPositions.Length)
                {
                    audienceSprites[i].rectTransform.anchoredPosition = audienceOriginalPositions[i];
                }
            }
            
            previousConsumedLives = 0;
            Debug.Log("[PansoriSceneUI] 관객 스프라이트 위치 초기화");
        }
        
        #region 관객 스프라이트 애니메이션
        
        /// <summary>
        /// 관객 스프라이트 애니메이션을 시작합니다.
        /// </summary>
        public void StartAudienceSpriteAnimation()
        {
            // 스프라이트 배열이 없거나 비어있으면 무시
            if (audienceSprites == null || audienceSprites.Length == 0 ||
                audienceSprite1Array == null || audienceSprite1Array.Length == 0 ||
                audienceSprite2Array == null || audienceSprite2Array.Length == 0)
            {
                return;
            }
            
            // 이미 실행 중이면 중지
            StopAudienceSpriteAnimation();
            
            // 프레임 인덱스 초기화
            audienceSpriteFrameIndex = new bool[audienceSprites.Length];
            for (int i = 0; i < audienceSpriteFrameIndex.Length; i++)
            {
                audienceSpriteFrameIndex[i] = true; // 첫 번째 스프라이트로 시작
            }
            
            // 초기 스프라이트 적용
            ApplyAudienceSprites();
            
            // 코루틴 시작
            audienceSpriteAnimationCoroutine = StartCoroutine(AudienceSpriteAnimationCoroutine());
            
            Debug.Log("[PansoriSceneUI] 관객 스프라이트 애니메이션 시작");
        }
        
        /// <summary>
        /// 관객 스프라이트 애니메이션을 중지합니다.
        /// </summary>
        public void StopAudienceSpriteAnimation()
        {
            if (audienceSpriteAnimationCoroutine != null)
            {
                StopCoroutine(audienceSpriteAnimationCoroutine);
                audienceSpriteAnimationCoroutine = null;
            }
        }
        
        /// <summary>
        /// 관객 스프라이트 애니메이션 코루틴
        /// </summary>
        private IEnumerator AudienceSpriteAnimationCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(audienceSpriteAnimationInterval);
                ToggleAudienceSprites();
            }
        }
        
        /// <summary>
        /// 모든 관객 스프라이트를 토글합니다.
        /// </summary>
        private void ToggleAudienceSprites()
        {
            if (audienceSprites == null || audienceSpriteFrameIndex == null)
            {
                return;
            }
            
            for (int i = 0; i < audienceSprites.Length; i++)
            {
                if (i < audienceSpriteFrameIndex.Length)
                {
                    audienceSpriteFrameIndex[i] = !audienceSpriteFrameIndex[i];
                }
            }
            
            ApplyAudienceSprites();
        }
        
        /// <summary>
        /// 현재 프레임 인덱스에 따라 관객 스프라이트를 적용합니다.
        /// </summary>
        private void ApplyAudienceSprites()
        {
            if (audienceSprites == null || audienceSpriteFrameIndex == null)
            {
                return;
            }
            
            for (int i = 0; i < audienceSprites.Length; i++)
            {
                if (audienceSprites[i] == null)
                {
                    continue;
                }
                
                if (i < audienceSpriteFrameIndex.Length &&
                    i < audienceSprite1Array.Length &&
                    i < audienceSprite2Array.Length)
                {
                    Sprite targetSprite = audienceSpriteFrameIndex[i] 
                        ? audienceSprite1Array[i] 
                        : audienceSprite2Array[i];
                    
                    if (targetSprite != null)
                    {
                        audienceSprites[i].sprite = targetSprite;
                    }
                }
            }
        }
        
        #endregion
        
        /// <summary>
        /// 텍스트 페이드인 효과
        /// </summary>
        private IEnumerator FadeInText(TMP_Text text, float duration)
        {
            if (text == null) yield break;
            
            Color originalColor = text.color;
            Color startColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                text.color = Color.Lerp(startColor, originalColor, t);
                yield return null;
            }
            
            text.color = originalColor;
        }
        
        /// <summary>
        /// 스케일 펀치 효과
        /// </summary>
        private IEnumerator ScalePunchEffect(RectTransform target, float punchAmount, float duration)
        {
            if (target == null) yield break;
            
            Vector3 originalScale = Vector3.one;
            Vector3 punchScale = originalScale * punchAmount;
            
            float halfDuration = duration * 0.5f;
            
            // 커지는 단계
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                target.localScale = Vector3.Lerp(originalScale, punchScale, t);
                yield return null;
            }
            
            // 원래대로 돌아오는 단계
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                target.localScale = Vector3.Lerp(punchScale, originalScale, t);
                yield return null;
            }
            
            target.localScale = originalScale;
        }
        
        /// <summary>
        /// 현재 표시 상태
        /// </summary>
        public bool IsVisible => canvas != null && canvas.enabled;
        
        /// <summary>
        /// 외부에서 화면 플래시를 직접 호출할 수 있습니다.
        /// </summary>
        public void TriggerScreenFlash(bool success)
        {
            if (useScreenFlash)
            {
                StartCoroutine(PlayScreenFlash(success));
            }
        }
        
        /// <summary>
        /// 외부에서 화면 흔들림을 직접 호출할 수 있습니다.
        /// </summary>
        public void TriggerScreenShake()
        {
            if (useScreenShake)
            {
                StartCoroutine(PlayScreenShake());
            }
        }
        
        /// <summary>
        /// 판소리 스프라이트 애니메이션을 시작합니다.
        /// </summary>
        private void StartPansoriSpriteAnimation()
        {
            // 오브젝트 활성화
            if (pansoriSpriteAnimatorObject != null)
            {
                pansoriSpriteAnimatorObject.SetActive(true);
            }
            
            // 애니메이터 활성화 및 재생
            if (pansoriSpriteAnimator != null)
            {
                pansoriSpriteAnimator.enabled = true;
                pansoriSpriteAnimator.Play("PansoriLoop", 0, 0f);
            }
        }
        
        /// <summary>
        /// 판소리 스프라이트 애니메이션을 중지합니다.
        /// </summary>
        private void StopPansoriSpriteAnimation()
        {
            // 애니메이터 비활성화
            if (pansoriSpriteAnimator != null)
            {
                pansoriSpriteAnimator.enabled = false;
            }
            
            // 오브젝트 비활성화
            if (pansoriSpriteAnimatorObject != null)
            {
                pansoriSpriteAnimatorObject.SetActive(false);
            }
        }
        
        #region Door Transition
        
        /// <summary>
        /// 문이 열리는 트랜지션을 재생합니다.
        /// 좌측 문은 왼쪽으로, 우측 문은 오른쪽으로 열립니다.
        /// </summary>
        /// <param name="onComplete">완료 콜백</param>
        public void PlayDoorOpenTransition(Action onComplete)
        {
            if (!useDoorTransition || (leftDoorImage == null && rightDoorImage == null))
            {
                onComplete?.Invoke();
                return;
            }
            
            StartCoroutine(DoorOpenCoroutine(onComplete));
        }
        
        /// <summary>
        /// 문이 닫히는 트랜지션을 재생합니다.
        /// 좌측 문은 오른쪽으로, 우측 문은 왼쪽으로 닫힙니다.
        /// </summary>
        /// <param name="onComplete">완료 콜백</param>
        public void PlayDoorCloseTransition(Action onComplete)
        {
            if (!useDoorTransition || (leftDoorImage == null && rightDoorImage == null))
            {
                onComplete?.Invoke();
                return;
            }
            
            StartCoroutine(DoorCloseCoroutine(onComplete));
        }
        
        /// <summary>
        /// 문 열림 애니메이션 코루틴
        /// </summary>
        private IEnumerator DoorOpenCoroutine(Action onComplete)
        {
            // 문 활성화 및 초기 위치 설정 (닫힌 상태)
            SetDoorsActive(true);
            SetDoorPositions(0f); // 0 = 닫힌 상태
            
            float elapsed = 0f;
            
            while (elapsed < doorTransitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / doorTransitionDuration);
                
                // EaseOutQuad 이징 적용 (자연스러운 감속)
                float easedT = 1f - (1f - t) * (1f - t);
                
                SetDoorPositions(easedT); // 1 = 완전히 열린 상태
                yield return null;
            }
            
            SetDoorPositions(1f);
            SetDoorsActive(false); // 열린 후 비활성화
            
            Debug.Log("[PansoriSceneUI] 문 열림 트랜지션 완료");
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 문 닫힘 애니메이션 코루틴
        /// </summary>
        private IEnumerator DoorCloseCoroutine(Action onComplete)
        {
            // 문 활성화 및 초기 위치 설정 (열린 상태)
            SetDoorsActive(true);
            SetDoorPositions(1f); // 1 = 열린 상태
            
            float elapsed = 0f;
            
            while (elapsed < doorTransitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / doorTransitionDuration);
                
                // EaseInQuad 이징 적용 (자연스러운 가속)
                float easedT = t * t;
                
                SetDoorPositions(1f - easedT); // 0 = 완전히 닫힌 상태
                yield return null;
            }
            
            SetDoorPositions(0f);
            // 닫힌 후에도 활성 상태 유지 (화면 가림 유지)
            
            Debug.Log("[PansoriSceneUI] 문 닫힘 트랜지션 완료");
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 문 활성화/비활성화
        /// </summary>
        private void SetDoorsActive(bool active)
        {
            if (leftDoorImage != null)
            {
                leftDoorImage.gameObject.SetActive(active);
            }
            
            if (rightDoorImage != null)
            {
                rightDoorImage.gameObject.SetActive(active);
            }
        }
        
        /// <summary>
        /// 문 위치 설정 (0 = 닫힘, 1 = 열림)
        /// </summary>
        /// <param name="openAmount">열린 정도 (0~1)</param>
        private void SetDoorPositions(float openAmount)
        {
            // 화면 절반 너비 계산 (Canvas 기준)
            float screenHalfWidth = 960f; // 기준 해상도 1920의 절반
            if (canvasRectTransform != null)
            {
                screenHalfWidth = canvasRectTransform.rect.width * 0.5f;
            }
            
            // 좌측 문: 열릴 때 왼쪽으로 이동
            if (leftDoorImage != null)
            {
                RectTransform leftRect = leftDoorImage.rectTransform;
                float leftTargetX = -screenHalfWidth * openAmount;
                leftRect.anchoredPosition = new Vector2(leftTargetX, 0f);
            }
            
            // 우측 문: 열릴 때 오른쪽으로 이동
            if (rightDoorImage != null)
            {
                RectTransform rightRect = rightDoorImage.rectTransform;
                float rightTargetX = screenHalfWidth * openAmount;
                rightRect.anchoredPosition = new Vector2(rightTargetX, 0f);
            }
        }
        
        /// <summary>
        /// 문을 즉시 닫힌 상태로 설정합니다.
        /// </summary>
        public void SetDoorsClosed()
        {
            if (!useDoorTransition) return;
            
            SetDoorsActive(true);
            SetDoorPositions(0f);
        }
        
        /// <summary>
        /// 문을 즉시 숨깁니다.
        /// </summary>
        public void HideDoors()
        {
            SetDoorsActive(false);
        }
        
        #endregion
    }
}