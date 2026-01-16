using System;
using System.Collections;
using UnityEngine;

namespace Pansori.Microgames
{
    /// <summary>
    /// 게임 상태 열거형
    /// </summary>
    [System.Serializable]
    public enum GameState
    {
        MainMenu,      // 메인 화면
        PansoriScene,  // 판소리 씬 (마이크로게임 사이)
        Ready,         // 판소리 준비 화면 (Ready, Start!)
        Microgame,     // 마이크로게임 진행 중
        Victory,       // 승리 화면 (20회 승리)
        GameOver       // 패배 화면 (4회 패배)
    }

    /// <summary>
    /// 게임 전체 흐름을 관리하는 상태 머신
    /// 메인 화면 → 준비 화면 → 판소리 씬 → 마이크로게임 → 승리/패배 화면
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private MicrogameManager microgameManager;
        [SerializeField] private PansoriSceneUI pansoriSceneUI;
        [SerializeField] private GameScreens gameScreens;

        [Header("게임 설정")]
        [SerializeField] private int winCountForVictory = 20; // 승리 화면으로 가기 위한 승리 횟수
        [SerializeField] private int loseCountForGameOver = 4; // 패배 화면으로 가기 위한 패배 횟수
        [SerializeField] private int winsPerSpeedIncrease = 4; // 속도 증가를 위한 승리 횟수 (4의 배수)

        [Header("속도 설정")]
        [SerializeField] private float baseSpeed = 1.0f; // 기본 속도
        [SerializeField] private float speedIncrement = 0.2f; // 속도 증가량
        [SerializeField] private float maxSpeed = 2.5f; // 최대 속도

        [Header("타이밍 설정")]
        [SerializeField] private float readyDuration = 2f; // 준비 화면 표시 시간
        [SerializeField] private float pansoriCommandDelay = 1f; // "XX해라!" 표시 전 대기 시간
        [SerializeField] private float reactionDisplayDuration = 1.5f; // 환호/야유 반응 표시 시간

        [Header("난이도 설정")]
        [SerializeField] private int baseDifficulty = 1; // 기본 난이도

        /// <summary>
        /// 현재 게임 상태
        /// </summary>
        [SerializeField] private GameState currentState = GameState.MainMenu;

        /// <summary>
        /// 승리 횟수
        /// </summary>
        private int winCount = 0;

        /// <summary>
        /// 패배 횟수
        /// </summary>
        private int loseCount = 0;

        /// <summary>
        /// 현재 속도
        /// </summary>
        private float currentSpeed;

        /// <summary>
        /// 마지막 마이크로게임 결과
        /// </summary>
        private bool lastMicrogameSuccess;

        /// <summary>
        /// 현재 게임 상태 (외부에서 읽기 전용)
        /// </summary>
        public GameState CurrentState => currentState;

        /// <summary>
        /// 현재 승리 횟수
        /// </summary>
        public int WinCount => winCount;

        /// <summary>
        /// 현재 패배 횟수
        /// </summary>
        public int LoseCount => loseCount;

        /// <summary>
        /// 현재 속도
        /// </summary>
        public float CurrentSpeed => currentSpeed;

        /// <summary>
        /// 상태 변경 이벤트
        /// </summary>
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            currentSpeed = baseSpeed;
        }

        private void Start()
        {
            // 마이크로게임 결과 이벤트 구독
            if (microgameManager != null)
            {
                microgameManager.OnMicrogameResult += HandleMicrogameResult;
            }

            // 메인 메뉴로 시작
            ChangeState(GameState.MainMenu);
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (microgameManager != null)
            {
                microgameManager.OnMicrogameResult -= HandleMicrogameResult;
            }
        }

        /// <summary>
        /// 게임 상태를 변경합니다.
        /// </summary>
        /// <param name="newState">새로운 상태</param>
        public void ChangeState(GameState newState)
        {
            GameState previousState = currentState;
            currentState = newState;

            Debug.Log($"[GameFlowManager] 상태 변경: {previousState} → {newState}");

            // 상태 변경 이벤트 호출
            OnStateChanged?.Invoke(newState);

            // 상태별 처리
            switch (newState)
            {
                case GameState.MainMenu:
                    OnEnterMainMenu();
                    break;
                case GameState.Ready:
                    OnEnterReady();
                    break;
                case GameState.PansoriScene:
                    OnEnterPansoriScene(previousState == GameState.Microgame);
                    break;
                case GameState.Microgame:
                    OnEnterMicrogame();
                    break;
                case GameState.Victory:
                    OnEnterVictory();
                    break;
                case GameState.GameOver:
                    OnEnterGameOver();
                    break;
            }
        }

        /// <summary>
        /// 메인 메뉴 진입
        /// </summary>
        private void OnEnterMainMenu()
        {
            // 게임 데이터 초기화
            ResetGameData();

            // 화면 표시
            if (gameScreens != null)
            {
                gameScreens.ShowMainMenu();
            }

            Debug.Log("[GameFlowManager] 메인 메뉴 진입");
        }

        /// <summary>
        /// 준비 화면 진입
        /// </summary>
        private void OnEnterReady()
        {
            if (gameScreens != null)
            {
                gameScreens.ShowReadyScreen(readyDuration, () =>
                {
                    // 준비 화면 종료 후 판소리 씬으로 전환
                    ChangeState(GameState.PansoriScene);
                });
            }
            else
            {
                // 화면 없으면 바로 전환
                StartCoroutine(DelayedStateChange(GameState.PansoriScene, readyDuration));
            }

            Debug.Log("[GameFlowManager] 준비 화면 진입");
        }

        /// <summary>
        /// 판소리 씬 진입
        /// </summary>
        /// <param name="fromMicrogame">마이크로게임에서 돌아온 경우</param>
        private void OnEnterPansoriScene(bool fromMicrogame)
        {
            if (gameScreens != null)
            {
                gameScreens.HideAllScreens();
            }

            if (fromMicrogame)
            {
                // 마이크로게임 결과에 따른 반응 표시
                if (pansoriSceneUI != null)
                {
                    pansoriSceneUI.ShowReaction(lastMicrogameSuccess, reactionDisplayDuration, () =>
                    {
                        // 승리/패배 조건 확인
                        CheckGameEndCondition();
                    });
                }
                else
                {
                    StartCoroutine(DelayedCheckGameEnd(reactionDisplayDuration));
                }
            }
            else
            {
                // 처음 판소리 씬 진입 - 바로 마이크로게임 시작 준비
                PrepareNextMicrogame();
            }

            Debug.Log($"[GameFlowManager] 판소리 씬 진입 (마이크로게임에서: {fromMicrogame})");
        }

        /// <summary>
        /// 다음 마이크로게임 인덱스 (미리 결정)
        /// </summary>
        private int nextMicrogameIndex = -1;

        /// <summary>
        /// 다음 마이크로게임 준비
        /// </summary>
        private void PrepareNextMicrogame()
        {
            // 다음 게임 인덱스 미리 결정
            nextMicrogameIndex = GetNextMicrogameIndex();

            // 다음 게임 이름 가져오기
            string nextGameName = GetNextGameName();

            // 목숨/스테이지 정보 가져오기
            int totalLives = microgameManager != null ? microgameManager.MaxLives : 4;
            int consumedLives = microgameManager != null ? (microgameManager.MaxLives - microgameManager.CurrentLives) : loseCount;
            int currentStage = winCount + 1;

            if (pansoriSceneUI != null)
            {
                // 명령과 함께 게임 정보 표시
                pansoriSceneUI.ShowCommandWithInfo(nextGameName, totalLives, consumedLives, currentStage, pansoriCommandDelay, () =>
                {
                    // 명령 표시 후 마이크로게임으로 전환
                    ChangeState(GameState.Microgame);
                });
            }
            else
            {
                StartCoroutine(DelayedStateChange(GameState.Microgame, pansoriCommandDelay));
            }
        }

        /// <summary>
        /// 다음 마이크로게임 인덱스를 가져옵니다.
        /// </summary>
        private int GetNextMicrogameIndex()
        {
            if (microgameManager != null)
            {
                return microgameManager.GetRandomMicrogameIndex();
            }
            return -1;
        }

        /// <summary>
        /// 다음 마이크로게임 이름을 가져옵니다.
        /// </summary>
        private string GetNextGameName()
        {
            if (microgameManager != null && nextMicrogameIndex >= 0)
            {
                string gameName = microgameManager.GetMicrogameName(nextMicrogameIndex);
                if (!string.IsNullOrEmpty(gameName))
                {
                    return gameName;
                }
            }
            return "게임"; // 기본값
        }

        /// <summary>
        /// 마이크로게임 진입
        /// </summary>
        private void OnEnterMicrogame()
        {
            if (pansoriSceneUI != null)
            {
                pansoriSceneUI.HideAll();
            }

            // 마이크로게임 시작 (미리 결정한 인덱스 사용)
            if (microgameManager != null)
            {
                if (nextMicrogameIndex >= 0)
                {
                    microgameManager.StartMicrogame(nextMicrogameIndex, baseDifficulty, currentSpeed);
                }
                else
                {
                    microgameManager.StartRandomMicrogame(baseDifficulty, currentSpeed);
                }
            }

            Debug.Log($"[GameFlowManager] 마이크로게임 시작 (인덱스: {nextMicrogameIndex}, 속도: {currentSpeed})");
        }

        /// <summary>
        /// 승리 화면 진입
        /// </summary>
        private void OnEnterVictory()
        {
            if (pansoriSceneUI != null)
            {
                pansoriSceneUI.HideAll();
            }

            if (gameScreens != null)
            {
                gameScreens.ShowVictoryScreen(winCount);
            }

            Debug.Log($"[GameFlowManager] 승리! 총 승리 횟수: {winCount}");
        }

        /// <summary>
        /// 게임오버 화면 진입
        /// </summary>
        private void OnEnterGameOver()
        {
            if (pansoriSceneUI != null)
            {
                pansoriSceneUI.HideAll();
            }

            if (gameScreens != null)
            {
                gameScreens.ShowGameOverScreen(winCount, loseCount);
            }

            Debug.Log($"[GameFlowManager] 게임오버! 승리: {winCount}, 패배: {loseCount}");
        }

        /// <summary>
        /// 마이크로게임 결과 처리
        /// </summary>
        /// <param name="success">성공 여부</param>
        private void HandleMicrogameResult(bool success)
        {
            lastMicrogameSuccess = success;

            if (success)
            {
                winCount++;
                Debug.Log($"[GameFlowManager] 승리! 현재 승리 횟수: {winCount}");

                // 속도 증가 확인 (4의 배수마다)
                if (winCount > 0 && winCount % winsPerSpeedIncrease == 0)
                {
                    IncreaseSpeed();
                }
            }
            else
            {
                loseCount++;
                Debug.Log($"[GameFlowManager] 패배! 현재 패배 횟수: {loseCount}");
            }

            // 판소리 씬으로 돌아가기
            ChangeState(GameState.PansoriScene);
        }

        /// <summary>
        /// 게임 종료 조건 확인
        /// </summary>
        private void CheckGameEndCondition()
        {
            if (winCount >= winCountForVictory)
            {
                ChangeState(GameState.Victory);
            }
            else if (loseCount >= loseCountForGameOver)
            {
                ChangeState(GameState.GameOver);
            }
            else
            {
                // 다음 마이크로게임 준비
                PrepareNextMicrogame();
            }
        }

        /// <summary>
        /// 속도를 증가시킵니다.
        /// </summary>
        private void IncreaseSpeed()
        {
            float previousSpeed = currentSpeed;
            currentSpeed = Mathf.Min(currentSpeed + speedIncrement, maxSpeed);

            Debug.Log($"[GameFlowManager] 속도 증가: {previousSpeed:F2} → {currentSpeed:F2}");
        }

        /// <summary>
        /// 게임 데이터를 초기화합니다.
        /// </summary>
        private void ResetGameData()
        {
            winCount = 0;
            loseCount = 0;
            currentSpeed = baseSpeed;
            lastMicrogameSuccess = false;

            // MicrogameManager의 목숨도 초기화
            if (microgameManager != null)
            {
                microgameManager.ResetLives();
            }

            Debug.Log("[GameFlowManager] 게임 데이터 초기화");
        }

        /// <summary>
        /// 게임 시작 (메인 메뉴에서 호출)
        /// </summary>
        public void StartGame()
        {
            if (currentState == GameState.MainMenu)
            {
                ChangeState(GameState.Ready);
            }
        }

        /// <summary>
        /// 게임 재시작 (승리/패배 화면에서 호출)
        /// </summary>
        public void RestartGame()
        {
            if (currentState == GameState.Victory || currentState == GameState.GameOver)
            {
                ChangeState(GameState.MainMenu);
            }
        }

        /// <summary>
        /// 지연된 상태 변경 코루틴
        /// </summary>
        private IEnumerator DelayedStateChange(GameState newState, float delay)
        {
            yield return new WaitForSeconds(delay);
            ChangeState(newState);
        }

        /// <summary>
        /// 지연된 게임 종료 조건 확인 코루틴
        /// </summary>
        private IEnumerator DelayedCheckGameEnd(float delay)
        {
            yield return new WaitForSeconds(delay);
            CheckGameEndCondition();
        }
    }
}
