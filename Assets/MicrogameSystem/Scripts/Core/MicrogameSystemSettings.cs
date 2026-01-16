using UnityEngine;

namespace Pansori.Microgames
{
    /// <summary>
    /// 마이크로게임 시스템 설정 ScriptableObject
    /// 게임 설정을 Inspector에서 쉽게 조정하고 여러 프로필을 지원합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "MicrogameSystemSettings", menuName = "Pansori/Microgame System Settings")]
    public class MicrogameSystemSettings : ScriptableObject
    {
        [Header("게임 규칙")]
        [Tooltip("승리 화면으로 가기 위한 승리 횟수")]
        [SerializeField] private int winCountForVictory = 20;
        
        [Tooltip("패배 화면으로 가기 위한 패배 횟수")]
        [SerializeField] private int loseCountForGameOver = 4;
        
        [Tooltip("최대 목숨 (게임오버 조건과 동일하게 설정 권장)")]
        [SerializeField] private int maxLives = 4;

        [Header("속도 설정")]
        [Tooltip("기본 게임 속도")]
        [SerializeField] private float baseSpeed = 1.0f;
        
        [Tooltip("속도 증가를 위한 승리 횟수 (N의 배수마다 속도 증가)")]
        [SerializeField] private int winsPerSpeedIncrease = 4;
        
        [Tooltip("속도 증가량")]
        [SerializeField] private float speedIncrement = 0.2f;
        
        [Tooltip("최대 속도")]
        [SerializeField] private float maxSpeed = 2.5f;

        [Header("난이도 설정")]
        [Tooltip("기본 난이도 (1-3)")]
        [Range(1, 3)]
        [SerializeField] private int baseDifficulty = 1;
        
        [Tooltip("난이도 증가를 위한 승리 횟수 (N의 배수마다 난이도 증가, 0이면 고정)")]
        [SerializeField] private int winsPerDifficultyIncrease = 0;

        [Header("타이밍 설정")]
        [Tooltip("준비 화면 표시 시간 (초)")]
        [SerializeField] private float readyDuration = 2f;
        
        [Tooltip("명령 표시 전 대기 시간 (초)")]
        [SerializeField] private float commandDelay = 1f;
        
        [Tooltip("결과 반응 표시 시간 (초)")]
        [SerializeField] private float reactionDisplayDuration = 1.5f;
        
        [Tooltip("게임 정보 표시 시간 (초)")]
        [SerializeField] private float infoDisplayDuration = 2f;

        [Header("셔플 설정")]
        [Tooltip("마이크로게임 셔플 활성화 (중복 방지)")]
        [SerializeField] private bool enableShuffle = true;
        
        [Tooltip("연속 중복 방지 개수 (최근 N개 게임 중복 방지)")]
        [SerializeField] private int shuffleHistorySize = 3;

        [Header("프리팹 스캔 설정")]
        [Tooltip("마이크로게임 프리팹이 있는 폴더 경로 (Assets 기준)")]
        [SerializeField] private string microgameFolderPath = "MicrogameSystem/Games";

        #region Properties

        // 게임 규칙
        public int WinCountForVictory => winCountForVictory;
        public int LoseCountForGameOver => loseCountForGameOver;
        public int MaxLives => maxLives;

        // 속도 설정
        public float BaseSpeed => baseSpeed;
        public int WinsPerSpeedIncrease => winsPerSpeedIncrease;
        public float SpeedIncrement => speedIncrement;
        public float MaxSpeed => maxSpeed;

        // 난이도 설정
        public int BaseDifficulty => baseDifficulty;
        public int WinsPerDifficultyIncrease => winsPerDifficultyIncrease;

        // 타이밍 설정
        public float ReadyDuration => readyDuration;
        public float CommandDelay => commandDelay;
        public float ReactionDisplayDuration => reactionDisplayDuration;
        public float InfoDisplayDuration => infoDisplayDuration;

        // 셔플 설정
        public bool EnableShuffle => enableShuffle;
        public int ShuffleHistorySize => shuffleHistorySize;

        // 프리팹 스캔 설정
        public string MicrogameFolderPath => microgameFolderPath;

        #endregion

        #region Utility Methods

        /// <summary>
        /// 주어진 승리 횟수에 따른 현재 속도를 계산합니다.
        /// </summary>
        /// <param name="winCount">현재 승리 횟수</param>
        /// <returns>계산된 속도</returns>
        public float CalculateSpeed(int winCount)
        {
            if (winsPerSpeedIncrease <= 0) return baseSpeed;
            
            int speedLevel = winCount / winsPerSpeedIncrease;
            float calculatedSpeed = baseSpeed + (speedLevel * speedIncrement);
            return Mathf.Min(calculatedSpeed, maxSpeed);
        }

        /// <summary>
        /// 주어진 승리 횟수에 따른 현재 난이도를 계산합니다.
        /// </summary>
        /// <param name="winCount">현재 승리 횟수</param>
        /// <returns>계산된 난이도 (1-3)</returns>
        public int CalculateDifficulty(int winCount)
        {
            if (winsPerDifficultyIncrease <= 0) return baseDifficulty;
            
            int difficultyLevel = winCount / winsPerDifficultyIncrease;
            int calculatedDifficulty = baseDifficulty + difficultyLevel;
            return Mathf.Clamp(calculatedDifficulty, 1, 3);
        }

        /// <summary>
        /// 승리 조건 달성 여부를 확인합니다.
        /// </summary>
        /// <param name="winCount">현재 승리 횟수</param>
        /// <returns>승리 조건 달성 여부</returns>
        public bool IsVictory(int winCount)
        {
            return winCount >= winCountForVictory;
        }

        /// <summary>
        /// 게임오버 조건 달성 여부를 확인합니다.
        /// </summary>
        /// <param name="loseCount">현재 패배 횟수</param>
        /// <returns>게임오버 조건 달성 여부</returns>
        public bool IsGameOver(int loseCount)
        {
            return loseCount >= loseCountForGameOver;
        }

        #endregion

        #region Editor Validation

        private void OnValidate()
        {
            // 값 범위 검증
            winCountForVictory = Mathf.Max(1, winCountForVictory);
            loseCountForGameOver = Mathf.Max(1, loseCountForGameOver);
            maxLives = Mathf.Max(1, maxLives);
            
            baseSpeed = Mathf.Max(0.1f, baseSpeed);
            speedIncrement = Mathf.Max(0f, speedIncrement);
            maxSpeed = Mathf.Max(baseSpeed, maxSpeed);
            winsPerSpeedIncrease = Mathf.Max(0, winsPerSpeedIncrease);
            
            baseDifficulty = Mathf.Clamp(baseDifficulty, 1, 3);
            winsPerDifficultyIncrease = Mathf.Max(0, winsPerDifficultyIncrease);
            
            readyDuration = Mathf.Max(0f, readyDuration);
            commandDelay = Mathf.Max(0f, commandDelay);
            reactionDisplayDuration = Mathf.Max(0f, reactionDisplayDuration);
            infoDisplayDuration = Mathf.Max(0f, infoDisplayDuration);
            
            shuffleHistorySize = Mathf.Max(0, shuffleHistorySize);
        }

        #endregion
    }
}