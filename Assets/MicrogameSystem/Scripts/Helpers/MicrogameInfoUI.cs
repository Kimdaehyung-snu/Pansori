using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Pansori.Microgames
{
    /// <summary>
    /// 미니게임 시작 전 정보 표시 UI 컴포넌트
    /// 게임 이름, 목숨, 스테이지 정보를 표시합니다.
    /// 목숨 스프라이트 표시는 PansoriSceneUI에서 담당합니다.
    /// </summary>
    public class MicrogameInfoUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private TMP_Text gameNameText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private TMP_Text stageText;
        [SerializeField] private TMP_Text gameOverText; // 게임오버 텍스트
        
        [Header("UI 설정")]
        [SerializeField] private int sortOrder = 200; // Canvas 정렬 순서 (다른 UI 위에 표시)
        
        /// <summary>
        /// 생성된 Canvas
        /// </summary>
        private Canvas canvas;
        
        /// <summary>
        /// 자동 숨김 코루틴 참조
        /// </summary>
        private Coroutine autoHideCoroutine;
        
        private void Awake()
        {
            SetupCanvas();
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
        /// 게임 정보를 표시합니다.
        /// </summary>
        /// <param name="gameName">게임 이름</param>
        /// <param name="totalLives">총 목숨</param>
        /// <param name="consumedLives">소모된 목숨</param>
        /// <param name="stage">현재 스테이지</param>
        /// <param name="autoHideDuration">자동 숨김 시간 (초, 0이면 자동 숨김 안 함)</param>
        public void ShowInfoWithLives(string gameName, int totalLives, int consumedLives, int stage, float autoHideDuration = 0f)
        {
            ShowInfo(gameName, totalLives - consumedLives, stage);
            
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
