using System;
using UnityEngine;
using Pansori.Microgames;

/// <summary>
/// 의좋은 형재
/// 명령어: 던져라!
/// </summary>
public class Jaewon_GAME_2Manager : MicrogameBase
{
    [Header("의좋은 형재 설정")]
    [SerializeField] private float gameDuration = 5f;
    
    // 게임 고유 변수들
    private float timer;
    private bool hasSucceeded = false;
    
    /// <summary>
    /// 이 게임의 표시 이름
    /// </summary>
    public override string currentGameName => "던져라!";
    
    /// <summary>
    /// 게임 시작 시 호출
    /// </summary>
    public override void OnGameStart(int difficulty, float speed)
    {
        base.OnGameStart(difficulty, speed);
        
        // 게임 상태 초기화
        timer = gameDuration / speed;
        hasSucceeded = false;
        
        // TODO: 게임 초기화 로직 추가
        Debug.Log($"[Jaewon_GAME_2] 게임 시작 - 난이도: {difficulty}, 속도: {speed}");
    }
    
    private void Update()
    {
        if (isGameEnded) return;
        
        // 타이머 업데이트
        timer -= Time.deltaTime;
        
        // 시간 초과 시 실패
        if (timer <= 0)
        {
            OnTimeOut();
            return;
        }
        
        // TODO: 게임 로직 추가
        // 예시: 승리 조건 확인
        // if (승리조건)
        // {
        //     OnSuccess();
        // }
    }
    
    /// <summary>
    /// 성공 처리
    /// </summary>
    private void OnSuccess()
    {
        if (isGameEnded) return;
        
        hasSucceeded = true;
        Debug.Log("[Jaewon_GAME_2] 성공!");
        
        // 결과 애니메이션과 함께 보고
        ReportResultWithAnimation(true);
    }
    
    /// <summary>
    /// 실패 처리 (시간 초과)
    /// </summary>
    private void OnTimeOut()
    {
        if (isGameEnded) return;
        
        Debug.Log("[Jaewon_GAME_2] 시간 초과!");
        
        // 결과 애니메이션과 함께 보고
        ReportResultWithAnimation(false);
    }
    
    /// <summary>
    /// 게임 상태 초기화 (재사용을 위해 필수 구현)
    /// </summary>
    protected override void ResetGameState()
    {
        timer = gameDuration;
        hasSucceeded = false;
        
        // TODO: 게임 요소들 초기 상태로 복원
    }
    
    /// <summary>
    /// 게임 종료 시 호출
    /// </summary>
    protected override void OnGameEnd()
    {
        base.OnGameEnd();
        
        // TODO: 게임 종료 시 정리 작업
    }
}
