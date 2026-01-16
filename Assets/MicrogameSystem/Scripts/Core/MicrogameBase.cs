using System;
using System.Collections;
using UnityEngine;

namespace Pansori.Microgames
{
    /// <summary>
    /// 미니게임 베이스 클래스
    /// 모든 미니게임은 이 클래스를 상속받아 구현하는 것을 권장합니다.
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
        
        /// <summary>
        /// 현재 게임 이름 (서브 클래스에서 오버라이드 가능)
        /// </summary>
        public virtual string currentGameName => "MicrogameBase";
        
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
        }
        
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
        /// </summary>
        protected abstract void ResetGameState();
        
        /// <summary>
        /// 오브젝트가 비활성화될 때 자동으로 상태를 리셋합니다.
        /// </summary>
        private void OnDisable()
        {
            // 결과 애니메이션 중지
            if (resultAnimation != null)
            {
                resultAnimation.StopCurrentAnimation();
            }
            
            isPlayingResultAnimation = false;
            ResetGameState();
        }
    }
}