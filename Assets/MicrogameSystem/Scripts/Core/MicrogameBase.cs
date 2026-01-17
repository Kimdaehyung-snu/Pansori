using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pansori.Microgames
{
    /// <summary>
    /// 미니게임 베이스 클래스
    /// 모든 미니게임은 이 클래스를 상속받아 구현하는 것을 권장합니다.
    /// 자식 오브젝트의 활성화 상태를 자동으로 캐시하고 복원하는 기능을 제공합니다.
    /// </summary>
    public abstract class MicrogameBase : MonoBehaviour, IMicrogame
    {
        /// <summary>
        /// 결과 전달 이벤트
        /// </summary>
        public Action<bool> OnResultReported { get; set; }
        
        /// <summary>
        /// 게임이 종료되었는지 여부 (중복 결과 보고 방지)
        /// </summary>
        protected bool isGameEnded = false;
        
        /// <summary>
        /// 현재 난이도 (1~3)
        /// </summary>
        protected int currentDifficulty = 1;
        
        #region 오브젝트 상태 캐시 시스템
        
        /// <summary>
        /// 자식 오브젝트의 초기 활성화 상태 캐시
        /// Key: Transform, Value: activeSelf 상태
        /// </summary>
        private Dictionary<Transform, bool> initialActiveStates = new Dictionary<Transform, bool>();
        
        /// <summary>
        /// 초기 상태가 캐시되었는지 여부
        /// </summary>
        private bool isInitialStatesCached = false;
        
        #endregion
        
        /// <summary>
        /// 현재 게임 이름 (서브 클래스에서 오버라이드 가능)
        /// </summary>
        public virtual string currentGameName => "MicrogameBase";
        
        /// <summary>
        /// 조작법 설명 (서브 클래스에서 오버라이드하여 각 게임의 조작법을 표시)
        /// </summary>
        public virtual string controlDescription => "";
        
        [Header("게임 정보")]
        [SerializeField] protected Sprite thumbnailSprite;
        
        /// <summary>
        /// 썸네일 이미지 (연습 모드 선택 화면에서 표시)
        /// </summary>
        public virtual Sprite ThumbnailSprite => thumbnailSprite;
        
        /// <summary>
        /// 현재 배속 (1.0f 이상)
        /// </summary>
        protected float currentSpeed = 1.0f;

        [Header("결과 애니메이션 설정")]
        [SerializeField] protected MicrogameResultAnimation resultAnimation;
        [SerializeField] protected bool useResultAnimation = true;
        [SerializeField] protected float resultAnimationDelay = 0f;

        /// <summary>
        /// 결과 애니메이션이 재생 중인지 여부
        /// </summary>
        protected bool isPlayingResultAnimation = false;
        
        /// <summary>
        /// Awake 메서드 (오버라이드 가능)
        /// 서브 클래스에서 초기화 로직을 추가할 수 있습니다.
        /// </summary>
        protected virtual void Awake()
        {
            // 결과 애니메이션 컴포넌트 자동 탐색
            if (resultAnimation == null)
            {
                resultAnimation = GetComponentInChildren<MicrogameResultAnimation>();
            }
            
            // 자식 오브젝트 초기 활성화 상태 캐시
            CacheInitialStates();
        }
        
        #region 오브젝트 상태 캐시 메서드
        
        /// <summary>
        /// 모든 자식 오브젝트의 활성화 상태를 캐시합니다.
        /// Awake()에서 한 번만 호출되어 프리팹의 초기 상태를 저장합니다.
        /// </summary>
        protected void CacheInitialStates()
        {
            if (isInitialStatesCached) return;
            
            initialActiveStates.Clear();
            
            // 모든 자식 Transform을 재귀적으로 순회
            CacheChildStatesRecursive(transform);
            
            isInitialStatesCached = true;
            
            Debug.Log($"[MicrogameBase] {gameObject.name} - 자식 오브젝트 {initialActiveStates.Count}개 상태 캐시 완료");
        }
        
        /// <summary>
        /// 자식 오브젝트 상태를 재귀적으로 캐시합니다.
        /// </summary>
        private void CacheChildStatesRecursive(Transform parent)
        {
            foreach (Transform child in parent)
            {
                // 자식의 activeSelf 상태 저장
                initialActiveStates[child] = child.gameObject.activeSelf;
                
                // 손자 오브젝트도 재귀적으로 캐시
                if (child.childCount > 0)
                {
                    CacheChildStatesRecursive(child);
                }
            }
        }
        
        /// <summary>
        /// 캐시된 초기 상태로 모든 자식 오브젝트를 복원합니다.
        /// OnEnable()에서 호출되어 게임 시작 전 초기 상태를 보장합니다.
        /// </summary>
        protected void RestoreInitialStates()
        {
            if (!isInitialStatesCached || initialActiveStates.Count == 0) return;
            
            int restoredCount = 0;
            
            foreach (var kvp in initialActiveStates)
            {
                Transform child = kvp.Key;
                bool initialState = kvp.Value;
                
                // Transform이 유효하고 현재 상태가 초기 상태와 다른 경우에만 복원
                if (child != null && child.gameObject.activeSelf != initialState)
                {
                    child.gameObject.SetActive(initialState);
                    restoredCount++;
                }
            }
            
            if (restoredCount > 0)
            {
                Debug.Log($"[MicrogameBase] {gameObject.name} - 자식 오브젝트 {restoredCount}개 상태 복원");
            }
        }
        
        /// <summary>
        /// 상태 캐시를 수동으로 갱신합니다. (필요한 경우에만 사용)
        /// 기존 캐시를 무시하고 현재 상태를 새로운 초기 상태로 저장합니다.
        /// </summary>
        protected void RefreshStateCache()
        {
            isInitialStatesCached = false;
            CacheInitialStates();
        }
        
        #endregion
        
        /// <summary>
        /// 게임 시작 시 매니저가 호출합니다.
        /// </summary>
        /// <param name="difficulty">난이도 (1~3)</param>
        /// <param name="speed">배속 (1.0f 이상)</param>
        public virtual void OnGameStart(int difficulty, float speed)
        {
            isGameEnded = false;
            isPlayingResultAnimation = false;
            currentDifficulty = difficulty;
            currentSpeed = speed;
            
            Debug.Log($"[MicrogameBase] 게임 시작 - 난이도: {difficulty}, 배속: {speed}");
        }
        
        /// <summary>
        /// 결과를 매니저에게 보고합니다.
        /// </summary>
        /// <param name="success">true: 성공 / false: 실패</param>
        protected void ReportResult(bool success)
        {
            if (isGameEnded)
            {
                Debug.LogWarning($"[MicrogameBase] 이미 종료된 게임에서 결과를 보고하려고 시도했습니다. (성공: {success})");
                return;
            }
            
            isGameEnded = true;
            
            // 결과 판정 직후 즉시 사운드 재생 (판소리 씬 전환 전)
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayMicrogameResultSound(success);
            }
            
            OnGameEnd();
            OnResultReported?.Invoke(success);
            
            Debug.Log($"[MicrogameBase] 결과 보고 - 성공: {success}");
        }

        /// <summary>
        /// 결과 애니메이션을 재생한 후 결과를 보고합니다.
        /// 와리오웨어 스타일의 클리어/실패 연출을 원할 때 사용합니다.
        /// </summary>
        /// <param name="success">true: 성공 / false: 실패</param>
        protected void ReportResultWithAnimation(bool success)
        {
            if (isGameEnded)
            {
                Debug.LogWarning($"[MicrogameBase] 이미 종료된 게임에서 결과를 보고하려고 시도했습니다. (성공: {success})");
                return;
            }

            isGameEnded = true;
            isPlayingResultAnimation = true;
            
            // 결과 판정 직후 즉시 사운드 재생 (판소리 씬 전환 전)
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayMicrogameResultSound(success);
            }
            
            OnGameEnd();

            // 결과 애니메이션 재생
            PlayResultAnimation(success, () =>
            {
                isPlayingResultAnimation = false;
                OnResultReported?.Invoke(success);
                Debug.Log($"[MicrogameBase] 결과 보고 (애니메이션 후) - 성공: {success}");
            });
        }

        /// <summary>
        /// 결과 애니메이션을 재생합니다.
        /// 서브 클래스에서 오버라이드하여 커스텀 애니메이션을 구현할 수 있습니다.
        /// </summary>
        /// <param name="success">성공 여부</param>
        /// <param name="onComplete">완료 콜백</param>
        protected virtual void PlayResultAnimation(bool success, Action onComplete = null)
        {
            if (!useResultAnimation || resultAnimation == null)
            {
                // 애니메이션 없이 바로 완료
                StartCoroutine(DelayedCallback(resultAnimationDelay, onComplete));
                return;
            }

            // 결과 애니메이션 재생
            resultAnimation.PlayResultAnimation(success, onComplete);
            
            Debug.Log($"[MicrogameBase] 결과 애니메이션 재생 - 성공: {success}");
        }

        /// <summary>
        /// 대상 오브젝트에 빠른 이펙트를 적용합니다.
        /// </summary>
        /// <param name="target">대상 Transform</param>
        /// <param name="success">성공 여부</param>
        /// <param name="onComplete">완료 콜백</param>
        protected void PlayQuickEffect(Transform target, bool success, Action onComplete = null)
        {
            if (resultAnimation != null)
            {
                resultAnimation.PlayQuickEffect(target, success, onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// 화면 플래시 효과를 재생합니다.
        /// </summary>
        /// <param name="success">성공 여부 (색상 결정)</param>
        protected void FlashScreen(bool success)
        {
            if (resultAnimation != null)
            {
                resultAnimation.FlashScreen(success);
            }
        }

        /// <summary>
        /// 지연된 콜백 실행
        /// </summary>
        private IEnumerator DelayedCallback(float delay, Action callback)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            callback?.Invoke();
        }
        
        /// <summary>
        /// 게임 종료 시 호출됩니다. (오버라이드 가능)
        /// 서브 클래스에서 게임별 종료 처리를 구현합니다.
        /// </summary>
        protected virtual void OnGameEnd()
        {
            // 서브 클래스에서 필요시 오버라이드
            // 예: 타이머 정지, 입력 비활성화 등
        }
        
        /// <summary>
        /// 게임 상태를 초기값으로 리셋합니다. (추상 메서드 - 반드시 구현 필요)
        /// 주의: 이 메서드는 OnGameStart()에서 호출되거나 서브 클래스에서 필요 시 호출합니다.
        /// OnDisable()에서는 호출하지 않습니다 (부모 비활성화 시 자식 SetActive가 동작하지 않는 문제 방지).
        /// </summary>
        protected abstract void ResetGameState();
        
        /// <summary>
        /// 오브젝트가 활성화될 때 호출됩니다.
        /// 캐시된 초기 상태로 자식 오브젝트를 복원하고, 기본 플래그를 초기화합니다.
        /// </summary>
        protected virtual void OnEnable()
        {
            // 기본 플래그 초기화
            isGameEnded = false;
            isPlayingResultAnimation = false;
            
            // 캐시된 초기 상태로 자식 오브젝트 복원
            RestoreInitialStates();
        }
        
        /// <summary>
        /// 오브젝트가 비활성화될 때 호출됩니다.
        /// 결과 애니메이션만 중지하고, ResetGameState()는 호출하지 않습니다.
        /// (부모가 비활성화된 상태에서 자식 SetActive가 제대로 동작하지 않는 문제 방지)
        /// </summary>
        protected virtual void OnDisable()
        {
            // 결과 애니메이션 중지
            if (resultAnimation != null)
            {
                resultAnimation.StopCurrentAnimation();
            }
            
            isPlayingResultAnimation = false;
            
            // 주의: ResetGameState()는 여기서 호출하지 않습니다.
            // 자식 오브젝트의 SetActive()가 부모 비활성화 상태에서 제대로 동작하지 않기 때문입니다.
            // 대신 OnGameStart()에서 완전한 초기화를 수행합니다.
        }
    }
}