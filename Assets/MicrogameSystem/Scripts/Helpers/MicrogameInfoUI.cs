using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Pansori.Microgames
{
    /// <summary>
    /// 미니게임 시작 전 정보 표시 UI 컴포넌트
    /// 게임 이름, 목숨, 스테이지 정보를 표시합니다.
    /// </summary>
    public class MicrogameInfoUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private TMP_Text gameNameText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private TMP_Text stageText;
        [SerializeField] private TMP_Text gameOverText; // 게임오버 텍스트
        
        [Header("목숨 스프라이트 설정")]
        [SerializeField] private Sprite lifeSprite; // 목숨 스프라이트
        [SerializeField] private Sprite consumedLifeSprite; // 소모된 목숨 스프라이트 (선택사항)
        [SerializeField] private Transform livesContainer; // 목숨 스프라이트를 배치할 부모 Transform
        [SerializeField] private float lifeSpriteSpacing = 10f; // 스프라이트 간 간격
        [SerializeField] private Vector2 lifeSpriteSize = new Vector2(50, 50); // 스프라이트 크기
        
        [Header("UI 설정")]
        [SerializeField] private int sortOrder = 200; // Canvas 정렬 순서 (다른 UI 위에 표시)
        
        /// <summary>
        /// 생성된 Canvas
        /// </summary>
        private Canvas canvas;
        
        /// <summary>
        /// 생성된 목숨 스프라이트 리스트
        /// </summary>
        private List<GameObject> lifeSpriteObjects = new List<GameObject>();
        
        /// <summary>
        /// 자동 숨김 코루틴 참조
        /// </summary>
        private Coroutine autoHideCoroutine;
        
        private void Awake()
        {
            SetupCanvas();
            SetupLivesContainer();
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
        /// Canvas를 설정합니다.
        /// </summary>
        private void SetupCanvas()
        {
            // Canvas가 없으면 생성
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            
            // Canvas 설정
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            
            // CanvasScaler 추가
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }
            
            // GraphicRaycaster 추가
            GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // RectTransform 설정
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            // 초기에는 숨김 상태
            canvas.enabled = false;
        }
        
        /// <summary>
        /// 게임 정보를 표시합니다.
        /// </summary>
        /// <param name="gameName">게임 이름</param>
        /// <param name="lives">남은 목숨</param>
        /// <param name="stage">현재 스테이지</param>
        public void ShowInfo(string gameName, int lives, int stage)
        {
            if (gameNameText != null)
            {
                gameNameText.text = gameName;
            }
            
            if (livesText != null)
            {
                livesText.text = $"목숨: {lives}";
            }
            
            if (stageText != null)
            {
                stageText.text = $"스테이지: {stage}";
            }
            
            if (canvas != null)
            {
                canvas.enabled = true;
            }
            
            Debug.Log($"[MicrogameInfoUI] 정보 표시 - 게임: {gameName}, 목숨: {lives}, 스테이지: {stage}");
        }
        
        /// <summary>
        /// 게임 정보를 표시합니다. (목숨 스프라이트 포함)
        /// </summary>
        /// <param name="gameName">게임 이름</param>
        /// <param name="totalLives">총 목숨</param>
        /// <param name="consumedLives">소모된 목숨</param>
        /// <param name="stage">현재 스테이지</param>
        /// <param name="autoHideDuration">자동 숨김 시간 (초, 0이면 자동 숨김 안 함)</param>
        public void ShowInfoWithLives(string gameName, int totalLives, int consumedLives, int stage, float autoHideDuration = 0f)
        {
            ShowInfo(gameName, totalLives - consumedLives, stage);
            UpdateLivesDisplay(totalLives, consumedLives);
            
            // 자동 숨김 설정
            if (autoHideDuration > 0f)
            {
                StartAutoHide(autoHideDuration);
            }
        }
        
        /// <summary>
        /// 자동 숨김 코루틴을 시작합니다.
        /// </summary>
        /// <param name="duration">표시 시간 (초)</param>
        private void StartAutoHide(float duration)
        {
            // 기존 코루틴이 있으면 중지
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
            }
            
            autoHideCoroutine = StartCoroutine(AutoHideCoroutine(duration));
        }
        
        /// <summary>
        /// 자동 숨김 코루틴
        /// </summary>
        private IEnumerator AutoHideCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            HideInfo();
            autoHideCoroutine = null;
        }
        
        /// <summary>
        /// 정보 UI를 숨깁니다.
        /// </summary>
        public void HideInfo()
        {
            // 자동 숨김 코루틴 중지
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }
            
            if (canvas != null)
            {
                canvas.enabled = false;
            }
            
            // 게임오버 텍스트도 숨기기
            HideGameOver();
            
            Debug.Log("[MicrogameInfoUI] 정보 숨김");
        }
        
        /// <summary>
        /// 목숨 스프라이트 표시를 업데이트합니다.
        /// </summary>
        /// <param name="totalLives">총 목숨 개수</param>
        /// <param name="consumedLives">소모된 목숨 개수</param>
        public void UpdateLivesDisplay(int totalLives, int consumedLives)
        {
            // 기존 스프라이트 정리
            ClearLifeSprites();
            
            if (lifeSprite == null || livesContainer == null)
            {
                Debug.LogWarning("[MicrogameInfoUI] 목숨 스프라이트 또는 컨테이너가 설정되지 않았습니다.");
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
            
            Debug.Log($"[MicrogameInfoUI] 목숨 표시 업데이트 - 총: {totalLives}, 소모: {consumedLives}");
        }
        
        /// <summary>
        /// 목숨 스프라이트를 생성합니다.
        /// </summary>
        /// <param name="index">인덱스 (0부터 시작)</param>
        /// <param name="totalLives">총 목숨 개수</param>
        /// <returns>생성된 스프라이트 오브젝트</returns>
        private GameObject CreateLifeSprite(int index, int totalLives)
        {
            GameObject spriteObj = new GameObject($"Life_{index}");
            spriteObj.transform.SetParent(livesContainer, false);
            
            Image image = spriteObj.AddComponent<Image>();
            image.sprite = lifeSprite;
            
            RectTransform rectTransform = spriteObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = lifeSpriteSize;
            
            // 좌측부터 나열 (우측 정렬)
            float startX = -(totalLives - 1) * (lifeSpriteSize.x + lifeSpriteSpacing) / 2f;
            float xPos = startX + index * (lifeSpriteSize.x + lifeSpriteSpacing);
            rectTransform.anchoredPosition = new Vector2(xPos, 0);
            
            return spriteObj;
        }
        
        /// <summary>
        /// 목숨 스프라이트의 상태를 설정합니다.
        /// </summary>
        /// <param name="spriteObj">스프라이트 오브젝트</param>
        /// <param name="isConsumed">소모되었는지 여부</param>
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
        /// 게임오버 텍스트를 표시합니다.
        /// </summary>
        public void ShowGameOver()
        {
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(true);
                gameOverText.text = "게임 오버";
            }
            
            // Canvas도 활성화 (숨겨져 있을 수 있음)
            if (canvas != null)
            {
                canvas.enabled = true;
            }
            
            Debug.Log("[MicrogameInfoUI] 게임오버 표시");
        }
        
        /// <summary>
        /// 게임오버 텍스트를 숨깁니다.
        /// </summary>
        public void HideGameOver()
        {
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(false);
            }
            
            Debug.Log("[MicrogameInfoUI] 게임오버 숨김");
        }
    }
}
