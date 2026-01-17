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
    /// ScriptableObject 설정 연동 및 강화된 이벤트 시스템을 지원합니다.
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        [Header("시스템 설정")]
        [SerializeField] private MicrogameSystemSettings settings;

        [Header("참조")]
        [SerializeField] private MicrogameManager microgameManager;
        [SerializeField] private PansoriSceneUI pansoriSceneUI;
        [SerializeField] private GameScreens gameScreens;

        [Header("게임 설정 (설정 없을 시 사용)")]
        [SerializeField] private int winCountForVictory = 20;
        [SerializeField] private int loseCountForGameOver = 4;
        [SerializeField] private int winsPerSpeedIncrease = 4;

        [Header("속도 설정 (설정 없을 시 사용)")]
        [SerializeField] private float baseSpeed = 1.0f;
        [SerializeField] private float speedIncrement = 0.2f;
        [SerializeField] private float maxSpeed = 2.5f;

        [Header("타이밍 설정 (설정 없을 시 사용)")]
        [SerializeField] private float readyDuration = 2f;
        [SerializeField] private float pansoriCommandDelay = 1f;
        [SerializeField] private float reactionDisplayDuration = 1.5f;

        [Header("난이도 설정 (설정 없을 시 사용)")]
        [SerializeField] private int baseDifficulty = 1;

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
        /// 현재 난이도
        /// </summary>
        private int currentDifficulty;

        /// <summary>
        /// 마지막 마이크로게임 결과
        /// </summary>
        private bool lastMicrogameSuccess;

        /// <summary>
        /// 다음 마이크로게임 인덱스 (미리 결정)
        /// </summary>
        private int nextMicrogameIndex = -1;

        #region Properties

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
        /// 현재 난이도
        /// </summary>
        public int CurrentDifficulty => currentDifficulty;

        /// <summary>
        /// 현재 스테이지 (승리 횟수 + 1)
        /// </summary>
        public int CurrentStage => winCount + 1;

        /// <summary>
        /// 시스템 설정
        /// </summary>
        public MicrogameSystemSettings Settings => settings;

        /// <summary>
        /// MicrogameManager 참조
        /// </summary>
        public MicrogameManager MicrogameManager => microgameManager;

        #endregion

        #region Events

        /// <summary>
        /// 상태 변경 이벤트
        /// </summary>
        public event Action<GameState> OnStateChanged;

        /// <summary>
        /// 속도 변경 이벤트
        /// </summary>
        public event Action<float> OnSpeedChanged;

        /// <summary>
        /// 난이도 변경 이벤트
        /// </summary>
        public event Action<int> OnDifficultyChanged;

        /// <summary>
        /// 스테이지 변경 이벤트 (승리 횟수 + 1)
        /// </summary>
        public event Action<int> OnStageChanged;

        /// <summary>
        /// 승리 이벤트 (총 승리 횟수)
        /// </summary>
        public event Action<int> OnWin;

        /// <summary>
        /// 패배 이벤트 (총 패배 횟수)
        /// </summary>
        public event Action<int> OnLose;

        /// <summary>
        /// 게임 완료 이벤트 (승리/패배 여부, 승리 횟수, 패배 횟수)
        /// </summary>
        public event Action<bool, int, int> OnGameComplete;

        #endregion

        #region Settings Accessors

        private int WinCountForVictory => settings != null ? settings.WinCountForVictory : winCountForVictory;
        private int LoseCountForGameOver => settings != null ? settings.LoseCountForGameOver : loseCountForGameOver;
        private int WinsPerSpeedIncrease => settings != null ? settings.WinsPerSpeedIncrease : winsPerSpeedIncrease;
        private float BaseSpeed => settings != null ? settings.BaseSpeed : baseSpeed;
        private float SpeedIncrement => settings != null ? settings.SpeedIncrement : speedIncrement;
        private float MaxSpeed => settings != null ? settings.MaxSpeed : maxSpeed;
        private float ReadyDuration => settings != null ? settings.ReadyDuration : readyDuration;
        private float CommandDelay => settings != null ? settings.CommandDelay : pansoriCommandDelay;
        private float ReactionDuration => settings != null ? settings.ReactionDisplayDuration : reactionDisplayDuration;
        private int BaseDifficulty => settings != null ? settings.BaseDifficulty : baseDifficulty;

        #endregion

        private void Awake()
        {
            currentSpeed = BaseSpeed;
            currentDifficulty = BaseDifficulty;
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
        /// 설정을 변경합니다. (런타임에서 프로필 변경 시 사용)
        /// </summary>
        public void SetSettings(MicrogameSystemSettings newSettings)
        {
            settings = newSettings;
            
            // 현재 속도와 난이도 재계산
            UpdateSpeedAndDifficulty();
        }

        /// <summary>
        /// 게임 상태를 변경합니다.
        /// </summary>
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
            
            // 메인 BGM 재생 (자진모리)
            PlayMainBGMWithCurrentSpeed();

            Debug.Log("[GameFlowManager] 메인 메뉴 진입");
        }

        /// <summary>
        /// 준비 화면 진입
        /// </summary>
        private void OnEnterReady()
        {
            if (gameScreens != null)
            {
                gameScreens.ShowReadyScreen(ReadyDuration, () =>
                {
                    // 준비 화면 종료 후 판소리 씬으로 전환
                    ChangeState(GameState.PansoriScene);
                });
            }
            else
            {
                // 화면 없으면 바로 전환
                StartCoroutine(DelayedStateChange(GameState.PansoriScene, ReadyDuration));
            }

            Debug.Log("[GameFlowManager] 준비 화면 진입");
        }

        /// <summary>
        /// 판소리 씬 진입
        /// </summary>
        private void OnEnterPansoriScene(bool fromMicrogame)
        {
            if (gameScreens != null)
            {
                gameScreens.HideAllScreens();
            }
            
            // 메인 BGM 재생 (자진모리) - 마이크로게임에서 돌아왔으면 재생 시작
            if (fromMicrogame)
            {
                PlayMainBGMWithCurrentSpeed();
            }

            if (fromMicrogame)
            {
                // 판소리 씬 반응 사운드 재생
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayPansoriReactionSound(lastMicrogameSuccess);
                }
                
                // 마이크로게임 결과에 따른 반응 표시 (목숨 정보 포함)
                if (pansoriSceneUI != null)
                {
                    int totalLives = microgameManager != null ? microgameManager.MaxLives : LoseCountForGameOver;
                    int consumedLives = microgameManager != null ? (microgameManager.MaxLives - microgameManager.CurrentLives) : loseCount;
                    
                    pansoriSceneUI.ShowReactionWithInfo(lastMicrogameSuccess, totalLives, consumedLives, CurrentStage, ReactionDuration, () =>
                    {
                        // 승리/패배 조건 확인
                        CheckGameEndCondition();
                    });
                }
                else
                {
                    StartCoroutine(DelayedCheckGameEnd(ReactionDuration));
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
        /// 다음 마이크로게임 준비
        /// </summary>
        private void PrepareNextMicrogame()
        {
            // 다음 게임 인덱스 미리 결정
            nextMicrogameIndex = GetNextMicrogameIndex();

            // 다음 게임 이름 가져오기
            string nextGameName = GetNextGameName();
            
            // 다음 게임 조작법 가져오기
            string controlDescription = GetNextGameControlDescription();

            // 목숨/스테이지 정보 가져오기
            int totalLives = microgameManager != null ? microgameManager.MaxLives : LoseCountForGameOver;
            int consumedLives = microgameManager != null ? (microgameManager.MaxLives - microgameManager.CurrentLives) : loseCount;

            if (pansoriSceneUI != null)
            {
                // 명령과 함께 게임 정보 및 조작법 표시
                pansoriSceneUI.ShowCommandWithInfo(nextGameName, totalLives, consumedLives, CurrentStage, CommandDelay, () =>
                {
                    // 명령 표시 후 마이크로게임으로 전환
                    ChangeState(GameState.Microgame);
                }, controlDescription);
            }
            else
            {
                StartCoroutine(DelayedStateChange(GameState.Microgame, CommandDelay));
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
        /// 다음 마이크로게임 조작법 설명을 가져옵니다.
        /// </summary>
        private string GetNextGameControlDescription()
        {
            if (microgameManager != null && nextMicrogameIndex >= 0)
            {
                return microgameManager.GetMicrogameControlDescription(nextMicrogameIndex);
            }
            return string.Empty;
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
            return "게임";
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
            
            // 메인 BGM 정지 (미니게임 BGM이 대신 재생됨)
            StopMainBGM();

            // 마이크로게임 시작 (미리 결정한 인덱스 사용)
            if (microgameManager != null)
            {
                if (nextMicrogameIndex >= 0)
                {
                    microgameManager.StartMicrogame(nextMicrogameIndex, currentDifficulty, currentSpeed);
                }
                else
                {
                    microgameManager.StartRandomMicrogame(currentDifficulty, currentSpeed);
                }
            }

            Debug.Log($"[GameFlowManager] 마이크로게임 시작 (인덱스: {nextMicrogameIndex}, 난이도: {currentDifficulty}, 속도: {currentSpeed})");
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
            
            // 메인 BGM 재생
            PlayMainBGMWithCurrentSpeed();
            
            // 게임 완료 이벤트 발생
            OnGameComplete?.Invoke(true, winCount, loseCount);

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
            
            // 메인 BGM 재생
            PlayMainBGMWithCurrentSpeed();
            
            // 게임 완료 이벤트 발생
            OnGameComplete?.Invoke(false, winCount, loseCount);

            Debug.Log($"[GameFlowManager] 게임오버! 승리: {winCount}, 패배: {loseCount}");
        }

        /// <summary>
        /// 마이크로게임 결과 처리
        /// </summary>
        private void HandleMicrogameResult(bool success)
        {
            lastMicrogameSuccess = success;

            if (success)
            {
                winCount++;
                OnWin?.Invoke(winCount);
                OnStageChanged?.Invoke(CurrentStage);
                
                Debug.Log($"[GameFlowManager] 승리! 현재 승리 횟수: {winCount}");

                // 속도/난이도 업데이트
                UpdateSpeedAndDifficulty();
            }
            else
            {
                loseCount++;
                OnLose?.Invoke(loseCount);
                
                Debug.Log($"[GameFlowManager] 패배! 현재 패배 횟수: {loseCount}");
            }

            // 판소리 씬으로 돌아가기
            ChangeState(GameState.PansoriScene);
        }

        /// <summary>
        /// 속도와 난이도를 업데이트합니다.
        /// </summary>
        private void UpdateSpeedAndDifficulty()
        {
            float previousSpeed = currentSpeed;
            int previousDifficulty = currentDifficulty;

            if (settings != null)
            {
                currentSpeed = settings.CalculateSpeed(winCount);
                currentDifficulty = settings.CalculateDifficulty(winCount);
            }
            else
            {
                // 수동 계산 (설정이 없을 때)
                if (WinsPerSpeedIncrease > 0 && winCount > 0 && winCount % WinsPerSpeedIncrease == 0)
                {
                    currentSpeed = Mathf.Min(currentSpeed + SpeedIncrement, MaxSpeed);
                }
            }

            // 이벤트 발생
            if (Math.Abs(previousSpeed - currentSpeed) > 0.001f)
            {
                OnSpeedChanged?.Invoke(currentSpeed);
                
                // 메인 BGM 속도도 업데이트
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.SetMainBGMSpeed(currentSpeed);
                }
                
                Debug.Log($"[GameFlowManager] 속도 변경: {previousSpeed:F2} → {currentSpeed:F2}");
            }

            if (previousDifficulty != currentDifficulty)
            {
                OnDifficultyChanged?.Invoke(currentDifficulty);
                Debug.Log($"[GameFlowManager] 난이도 변경: {previousDifficulty} → {currentDifficulty}");
            }
        }

        /// <summary>
        /// 게임 종료 조건 확인
        /// </summary>
        private void CheckGameEndCondition()
        {
            bool isVictory = settings != null ? settings.IsVictory(winCount) : winCount >= WinCountForVictory;
            bool isGameOver = settings != null ? settings.IsGameOver(loseCount) : loseCount >= LoseCountForGameOver;

            if (isVictory)
            {
                ChangeState(GameState.Victory);
            }
            else if (isGameOver)
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
        /// 게임 데이터를 초기화합니다.
        /// </summary>
        private void ResetGameData()
        {
            winCount = 0;
            loseCount = 0;
            currentSpeed = BaseSpeed;
            currentDifficulty = BaseDifficulty;
            lastMicrogameSuccess = false;
            nextMicrogameIndex = -1;

            // MicrogameManager의 목숨도 초기화
            if (microgameManager != null)
            {
                microgameManager.ResetLives();
                microgameManager.ResetStatistics();
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
        
        #region Main BGM Control
        
        /// <summary>
        /// 현재 속도로 메인 BGM 재생
        /// </summary>
        private void PlayMainBGMWithCurrentSpeed()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayMainBGM(currentSpeed);
            }
        }
        
        /// <summary>
        /// 메인 BGM 정지
        /// </summary>
        private void StopMainBGM()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopMainBGM();
            }
        }
        
        #endregion

        #region Debug Methods

        /// <summary>
        /// 디버그용: 특정 상태로 강제 변경
        /// </summary>
        public void DebugSetState(GameState state)
        {
            ChangeState(state);
        }

        /// <summary>
        /// 디버그용: 승리 횟수 설정
        /// </summary>
        public void DebugSetWinCount(int count)
        {
            winCount = Mathf.Max(0, count);
            UpdateSpeedAndDifficulty();
            OnStageChanged?.Invoke(CurrentStage);
        }

        /// <summary>
        /// 디버그용: 패배 횟수 설정
        /// </summary>
        public void DebugSetLoseCount(int count)
        {
            loseCount = Mathf.Max(0, count);
        }

        /// <summary>
        /// 디버그용: 현재 속도 설정
        /// </summary>
        public void DebugSetSpeed(float speed)
        {
            currentSpeed = Mathf.Clamp(speed, BaseSpeed, MaxSpeed);
            OnSpeedChanged?.Invoke(currentSpeed);
        }

        #endregion
    }
}