# 미니게임 빠른 시작 가이드

이 가이드는 5분 안에 첫 미니게임을 만드는 방법을 설명합니다.

## 1단계: 미니게임 생성 (1분)

1. Unity 에디터를 엽니다.
2. 상단 메뉴에서 `Tools > Microgames > Create New Microgame`을 선택합니다.
3. 미니게임 이름을 입력합니다 (예: "MG_Jump_01").
4. "미니게임 생성" 버튼을 클릭합니다.

자동으로 다음이 생성됩니다:
- `Assets/MicrogameSystem/Games/[GameName]/` 폴더 구조
- 매니저 스크립트 (`[GameName]_Manager.cs`)
- 기본 프리팹

## 2단계: 프리팹 설정 (1분)

1. 생성된 프리팹을 엽니다 (`Assets/MicrogameSystem/Games/[GameName]/Prefabs/[GameName].prefab`).
2. 프리팹의 루트 오브젝트가 다음을 확인합니다:
   - Transform 위치: (0, 0, 0)
   - Transform 회전: (0, 0, 0)
   - Transform 스케일: (1, 1, 1)
3. 매니저 스크립트가 연결되어 있는지 확인합니다.

## 3단계: 게임 로직 구현 (2분)

생성된 매니저 스크립트를 열고 다음을 구현합니다:

### 기본 구조

```csharp
public class MG_YourGame_Manager : MicrogameBase
{
    [Header("게임 오브젝트")]
    [SerializeField] private GameObject player;
    
    [Header("게임 설정")]
    [SerializeField] private float gameDuration = 5f;
    
    [Header("헬퍼 컴포넌트")]
    [SerializeField] private MicrogameTimer timer;
    
    private Vector3 playerStartPos;
    
    protected override void Awake()
    {
        base.Awake();
        playerStartPos = player.transform.position;
    }
    
    public override void OnGameStart(int difficulty, float speed)
    {
        base.OnGameStart(difficulty, speed);
        
        // 타이머 시작
        if (timer != null)
        {
            timer.StartTimer(gameDuration, speed);
            timer.OnTimerEnd += OnTimeUp;
        }
        
        // TODO: 게임 시작 로직 추가
    }
    
    private void OnTimeUp()
    {
        ReportResult(true); // 또는 false
    }
    
    protected override void ResetGameState()
    {
        // 오브젝트 초기 위치로 복원
        if (player != null)
        {
            player.transform.position = playerStartPos;
        }
        
        // 타이머 중지
        if (timer != null)
        {
            timer.Stop();
            timer.OnTimerEnd -= OnTimeUp;
        }
    }
}
```

### 주요 포인트

1. **초기 위치 저장**: `Awake()`에서 초기 위치를 저장합니다.
2. **게임 시작**: `OnGameStart()`에서 게임 로직을 시작합니다.
3. **결과 보고**: `ReportResult(true/false)`로 결과를 보고합니다.
4. **상태 리셋**: `ResetGameState()`에서 모든 것을 초기 상태로 복원합니다.

## 4단계: 테스트 (1분)

### 방법 1: 테스트 에디터 사용 (권장)

1. Unity 에디터에서 `Tools > Microgames > Test Microgame Manager`를 선택합니다.
2. 씬에 `MicrogameManager` 오브젝트가 있으면 자동으로 찾습니다. 없으면 수동으로 할당합니다.
3. "프리팹 목록" 섹션에서 `MicrogameManager`의 프리팹 배열에 생성한 프리팹을 추가합니다.
4. 난이도와 배속을 설정합니다.
5. "인덱스로 시작" 또는 "랜덤 시작" 버튼을 클릭하여 게임을 시작합니다.
6. 결과는 자동으로 로그에 기록됩니다.

**테스트 에디터 주요 기능:**
- 프리팹 목록 관리 (드래그 앤 드롭 지원)
- 난이도/배속 슬라이더로 쉽게 설정
- 각 프리팹 옆 "시작" 버튼으로 바로 테스트
- 실시간 상태 표시 (실행 중/대기 중)
- 결과 로그 (성공/실패 기록)

### 방법 2: 코드로 직접 테스트

1. 씬에 `MicrogameManager` 오브젝트를 생성합니다.
2. `MicrogameManager` 컴포넌트의 "Microgame Prefabs" 배열에 생성한 프리팹을 추가합니다.
3. 플레이 모드에서 `MicrogameManager.StartMicrogame(0, 1, 1.0f)`를 호출하여 테스트합니다.

## GameFlow 시스템 설정

전체 게임("울려라! 판소리")의 플로우를 설정하는 방법입니다.

### GameFlow 빠른 설정

1. Unity 에디터에서 `Tools > Microgame System > GameFlow 설정 마법사`를 선택합니다.
2. (선택) MicrogameManager 참조를 할당합니다. 없으면 자동 생성됩니다.
3. "GameFlow 시스템 생성" 버튼을 클릭합니다.

**자동 생성되는 항목:**
- `GameFlowManager` - 게임 흐름 관리
- `GameScreensCanvas` - 메인/준비/승리/패배 화면
- `PansoriSceneCanvas` - 판소리 씬 UI

### 게임 흐름

```
메인 화면 → [Start] → 준비 화면 → 판소리 씬 → [XX해라!] → 마이크로게임
                                        ↓
                              [환호/야유 반응]
                                        ↓
                    ┌───────────────────┴───────────────────┐
                    ↓                                       ↓
          20회 승리: 승리 화면                    4회 패배: 패배 화면
```

### GameFlowManager 설정

| 설정 | 기본값 | 설명 |
|------|--------|------|
| Win Count For Victory | 20 | 승리 화면으로 가기 위한 승리 횟수 |
| Lose Count For GameOver | 4 | 패배 화면으로 가기 위한 패배 횟수 |
| Wins Per Speed Increase | 4 | 속도가 증가하는 승리 횟수 간격 |
| Base Speed | 1.0 | 기본 게임 속도 |
| Speed Increment | 0.2 | 속도 증가량 |
| Max Speed | 2.5 | 최대 속도 |

## 예제 미니게임 분석

`Assets/MicrogameSystem/Games/Jaewon_Example_1/` 폴더의 예제를 참고하세요.

### 주요 패턴

- **난이도 반영**: `OnGameStart()`에서 난이도에 따라 설정 조정
- **속도 반영**: 타이머와 이동 속도에 속도 배율 적용
- **이벤트 구독**: 헬퍼 컴포넌트의 이벤트 구독 및 해제

## 자주 묻는 질문 (FAQ)

### Q: 프리팹을 어떻게 검증하나요?

A: Unity 에디터에서 `Tools > Microgames > Validate Prefab`을 선택하고 프리팹을 검증하세요.

### Q: 미니게임을 어떻게 테스트하나요?

A: `Tools > Microgames > Test Microgame Manager`를 사용하여 쉽게 테스트할 수 있습니다. 씬에서 MicrogameManager를 자동으로 찾고, 프리팹을 할당한 후 난이도와 배속을 설정하여 바로 시작할 수 있습니다.

### Q: 전체 게임 플로우는 어떻게 테스트하나요?

A: `Tools > Microgame System > GameFlow 설정 마법사`로 GameFlow 시스템을 생성하고, 플레이 모드에서 Start 버튼을 클릭하세요.

### Q: 타이머는 어떻게 사용하나요?

A: `MicrogameTimer` 컴포넌트를 추가하고 `StartTimer(duration, speed)`를 호출하세요. `OnTimerEnd` 이벤트를 구독하여 시간 초과를 처리하세요.

### Q: 입력은 어떻게 처리하나요?

A: `MicrogameInputHandler` 컴포넌트를 추가하고 `OnKeyPressed`, `OnMouseClick` 등의 이벤트를 구독하세요.

### Q: UI는 어떻게 추가하나요?

A: `MicrogameUILayer` 컴포넌트를 사용하세요. Canvas가 자동으로 생성되고 관리됩니다.

### Q: 난이도와 속도는 어떻게 반영하나요?

A: `OnGameStart(int difficulty, float speed)`에서 받은 값을 게임 로직에 반영하세요. 예: `obstacleSpeed *= speed;`

### Q: 게임이 끝나지 않아요.

A: `ReportResult(true/false)`가 호출되는지 확인하세요. 모든 경로에서 결과가 보고되어야 합니다.

### Q: 게임을 다시 시작하면 상태가 리셋되지 않아요.

A: `ResetGameState()` 메서드가 모든 오브젝트를 초기 상태로 복원하는지 확인하세요. 프리팹 풀링 시스템에서 인스턴스가 재사용되므로 상태 리셋이 중요합니다.

### Q: 속도가 점점 빨라지지 않아요.

A: `GameFlowManager`가 설정되어 있는지 확인하세요. 기본적으로 4회 승리마다 속도가 0.2씩 증가합니다.

## 에디터 도구 요약

| 도구 | 메뉴 경로 | 용도 |
|------|-----------|------|
| 미니게임 생성 | `Tools > Microgames > Create New Microgame` | 새 미니게임 템플릿 생성 |
| 프리팹 검증 | `Tools > Microgames > Validate Prefab` | 프리팹 규격 검증 |
| 매니저 테스터 | `Tools > Microgames > Test Microgame Manager` | 미니게임 빠른 테스트 |
| GameFlow 설정 | `Tools > Microgame System > GameFlow 설정 마법사` | 전체 게임 플로우 구성 |

## 다음 단계

- [README.md](README.md)에서 전체 가이드를 읽어보세요.
- 예제 미니게임을 분석하여 패턴을 학습하세요.
- 검증 도구를 사용하여 프리팹을 검증하세요.
- 테스트 에디터를 사용하여 게임을 빠르게 테스트하세요.
- GameFlow 설정 마법사로 전체 게임을 구성해보세요.

행운을 빕니다!
