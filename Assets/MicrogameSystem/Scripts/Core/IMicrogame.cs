using System;
using UnityEngine;

namespace CautionPotion.Microgames
{
    /// <summary>
    /// 미니게임 인터페이스
    /// 모든 미니게임은 이 인터페이스를 구현해야 합니다.
    /// </summary>
    public interface IMicrogame
    {
        /// <summary>
        /// 게임 시작 시 매니저가 호출합니다.
        /// </summary>
        /// <param name="difficulty">난이도 (1~3)</param>
        /// <param name="speed">배속 (1.0f 이상)</param>
        void OnGameStart(int difficulty, float speed);
        
        /// <summary>
        /// 결과 전달 이벤트 (true: 성공 / false: 실패)
        /// 매니저가 이 이벤트를 구독하여 결과를 받습니다.
        /// </summary>
        System.Action<bool> OnResultReported { get; set; }
    }
}
