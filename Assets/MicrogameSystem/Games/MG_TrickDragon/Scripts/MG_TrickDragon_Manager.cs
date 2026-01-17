using Pansori.Microgames;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using URandom = UnityEngine.Random;

/// <summary>
/// 별주부전에서 토끼가 자신의 간 위치를 속이는 마이크로 게임
/// 
/// - A/W/D로 구성된 랜덤 시퀀스를 표시
/// - 플레이어는 표시된 순서대로 키를 입력해야 함
/// - 틀리면 즉시 실패 / 제한시간 초과 시 실패 / 전부 맞추면 성공
/// 
/// 명령어: 속여라!
/// </summary>
public class MG_TrickDragon_Manager : MicrogameBase
{
    [Header("별주부전에서 토끼가 자신의 간 위치를 속이는 마이크로 게임 설정")]
    [SerializeField] private float gameDuration = 5f;   // 기본 제한시간(속도 적용 전)
    [SerializeField] int seqLength;                     // 시퀀스 길이(인스펙터에서 설정)
    [SerializeField] TMP_Text[] seqTexts;               // 시퀀스 UI 텍스트들 (길이는 seqLength와 동일해야 함)
    private KeyCode[] seqKeys;                          // 실제 정답 시퀀스 (A/W/D)

    [Header("시퀀스 표시 색상")]
    [SerializeField] Color defaultSeqColor = Color.white;   // 초기/리셋 색
    [SerializeField] Color successSeqColor = Color.green;   // 맞춘 칸 색
    [SerializeField] Color failedSeqColor = Color.gray;     // 실패 시 색

    private int expectedSeqIdx; // 현재 기대하는 입력 인덱스(0부터 시작)

    // 게임 고유 변수들
    private float timer;
    private bool hasSucceeded = false;

    /// <summary>
    /// 시퀀스에 사용될 키 집합(A/W/D)
    /// </summary>
    private readonly KeyCode[] AllowedKeys =
    {
        KeyCode.A,
        KeyCode.W,
        KeyCode.D
    };

    [Header("헬퍼 컴포넌트")]
    [SerializeField] private MicrogameInputHandler inputHandler;
    [SerializeField] private MicrogameUILayer uiLayer;

    /// <summary>
    /// 이 게임의 표시 이름
    /// </summary>
    public override string currentGameName => "속여라!";

    public override string controlDescription => "순서에 맞게 키를 입력하세요!";

    /// <summary>
    /// 게임 시작 시 호출
    /// </summary>
    public override void OnGameStart(int difficulty, float speed)
    {
        base.OnGameStart(difficulty, speed);
        
        // 게임 상태 초기화
        timer = gameDuration / speed;
        hasSucceeded = false;
        expectedSeqIdx = 0;

        // 정답 시퀀스 생성 + UI 표시
        seqKeys = GenerateSequence(seqLength);

        Debug.Log($"[MG_TrickDragon] 게임 시작 - 난이도: {difficulty}, 속도: {speed}");

        // 입력 이벤트 구독
        if (inputHandler != null)
        {
            inputHandler.OnKeyPressed += HandleKeyPress;
        }
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
    }

    private void HandleKeyPress(KeyCode key)
    {
        if (isGameEnded || seqKeys == null)
        {
            return;
        }

        if (key != KeyCode.A && key != KeyCode.W && key != KeyCode.D)
        {
            return;
        }

        // 방어: 인덱스 범위가 비정상이라면 더 진행하지 않음
        if (expectedSeqIdx < 0 || expectedSeqIdx >= seqKeys.Length)
        {
            Debug.LogWarning($"[MG_TrickDragon] expectedSeqIdx out of range: {expectedSeqIdx}");
            return;
        }

        // --- 판정 ---

        if (key != seqKeys[expectedSeqIdx])
        {
            foreach (var seq in seqTexts)
            {
                seq.color = failedSeqColor;
            }

            ReportResultWithAnimation(false);
        }
        else
        {
            seqTexts[expectedSeqIdx].color = successSeqColor;
            expectedSeqIdx++;

            if (expectedSeqIdx == seqLength)
            {
                OnSuccess();
            }
        }
    }
  
    /// <summary>
    /// 성공 처리
    /// </summary>
    private void OnSuccess()
    {
        if (isGameEnded) return;

        hasSucceeded = true;
        Debug.Log("[MG_TrickDragon] 성공!");
        
        // 결과 애니메이션과 함께 보고
        ReportResultWithAnimation(true);
    }
    
    /// <summary>
    /// 실패 처리 (시간 초과)
    /// </summary>
    private void OnTimeOut()
    {
        if (isGameEnded) return;
        
        Debug.Log("[MG_TrickDragon] 시간 초과!");
        
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
        expectedSeqIdx = 0;
        seqKeys = null;

        foreach (var seq in seqTexts)
        {
            seq.color = defaultSeqColor;
        }

        // 입력 핸들러 이벤트 구독 해제
        if (inputHandler != null)
        {
            inputHandler.OnKeyPressed -= HandleKeyPress;
        }
    }
    
    /// <summary>
    /// 게임 종료 시 호출
    /// </summary>
    protected override void OnGameEnd()
    {
        base.OnGameEnd();
        
        // TODO: 게임 종료 시 정리 작업
    }

    protected override void PlayResultAnimation(bool success, Action onComplete = null)
    {
        StartCoroutine(ResultAnimationCoroutine(success, onComplete));
    }

    private IEnumerator ResultAnimationCoroutine(bool success, Action onComplete)
    {
        yield return new WaitForSeconds(0.2f);

        if (success)
        {
            // TODO... 토끼의 망했다 애니메이션 + 용왕의 저놈 잡아 같은 느낌
        }
        else
        {
            // TODO... 토끼의 그렇다니까요~ 라는 표정 + 용왕의 한숨(실망)
        }

        yield return new WaitForSeconds(0.8f);

        onComplete?.Invoke();
    }

    /// <summary>
    /// A/W/D로 구성된 QTE 입력 시퀀스를 생성하는 메서드 (연속 중복 허용)
    /// </summary>
    private KeyCode[] GenerateSequence(int length)
    {
        if (length <= 0)
        {
            Debug.LogWarning("[MG_TrickDragon_Manager] length must be > 0. Fallback to 1.");
            length = 1;
        }

        if (seqTexts.Length != seqLength)
        {
            return null;
        }

        // 시퀀스 생성
        var seq = new KeyCode[length];

        for (int i = 0; i < length; i++)
        {
            seq[i] = AllowedKeys[URandom.Range(0, AllowedKeys.Length)];
        }

        // UI 표시
        for (int i = 0; i < length; i++)
        {
            seqTexts[i].text = $"{seq[i]}";
        }

        return seq;
    }
}
