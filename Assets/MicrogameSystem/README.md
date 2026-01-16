# 미니게임 시스템 개발자 가이드

이 문서는 와리오 라이크 게임의 개별 미니게임을 제작하는 개발자를 위한 표준 규격입니다.

## 목차

1. [시스템 개요](#시스템-개요)
2. [아키텍처](#아키텍처)
3. [게임 플로우 시스템](#게임-플로우-시스템)
4. [인터페이스 및 베이스 클래스](#인터페이스-및-베이스-클래스)
5. [헬퍼 컴포넌트](#헬퍼-컴포넌트)
6. [에디터 도구](#에디터-도구)
7. [제약사항 및 규칙](#제약사항-및-규칙)
8. [체크리스트](#체크리스트)

## 시스템 개요

모든 미니게임은 단일 프리팹(Prefab) 형태로 제출되어야 하며, `IMicrogame` 인터페이스를 구현해야 합니다. `MicrogameBase` 클래스를 상속받아 구현하는 것을 강력히 권장합니다.

### 폴더 구조

```
Assets/MicrogameSystem/
  Scripts/
    Core/              # 핵심 인터페이스 및 매니저
      - IMicrogame.cs
      - MicrogameBase.cs
      - MicrogameManager.cs
      - GameFlowManager.cs
    Helpers/           # 헬퍼 컴포넌트
      - MicrogameTimer.cs
      - MicrogameInputHandler.cs
      - MicrogameUILayer.cs
      - MicrogameInfoUI.cs
      - PansoriSceneUI.cs
      - GameScreens.cs
    Templates/         # 템플릿 스크립트
    Editor/            # 에디터 도구
  Examples/            # 예제 미니게임
  Games/               # 실제 미니게임 (서브 개발자가 추가)
    [GameName]/
      Scripts/
      Prefabs/
      Arts/
      Audios/
```

## 아키텍처

### 계층 구조

```
GameFlowManager (게임 전체 흐름 관리)
  ↓ 상태에 따라 화면 전환 및 게임 시작
MicrogameManager (미니게임 생명주기 관리)
  ↓ 프리팹 풀링 및 OnGameStart 호출
MicrogameBase (각 미니게임의 베이스 클래스)
  ↓ 게임 로직 관리
하위 컴포넌트들 (게임별 로직, UI, 오브젝트 등)
```

### 생명주기

1. **준비**: 프리팹 풀에서 인스턴스 활성화, Awake/Start 호출
2. **정보 표시**: 게임 이름, 목숨, 스테이지 정보 표시
3. **개시**: `OnGameStart(difficulty, speed)` 호출
4. **진행**: 3~5초간 플레이
5. **판정**: `ReportResult()` 호출
6. **종료**: `OnDisable()` → `ResetGameState()` 호출, 인스턴스 비활성화 (풀로 반환)

## 게임 플로우 시스템

"울려라! 판소리" 게임의 전체 흐름을 관리하는 시스템입니다.

### 게임 상태 (GameState)

| 상태 | 설명 |
|------|------|
| `MainMenu` | 메인 화면 (Start 버튼) |
| `Ready` | 준비 화면 ("준비!" → "시작!" 연출) |
| `PansoriScene` | 판소리 씬 (마이크로게임 사이 화면) |
| `Microgame` | 마이크로게임 진행 중 |
| `Victory` | 승리 화면 (20회 승리 시) |
| `GameOver` | 패배 화면 (4회 패배 시) |

### 게임 흐름

```
메인 화면 → [Start] → 준비 화면 → 판소리 씬 → ["XX해라!"] → 마이크로게임
    ↑                                    ↓
    └──────────────────────────────── [결과에 따른 환호/야유]
                                         ↓
                              ┌──────────┴──────────┐
                              ↓                     ↓
                    20회 승리: 승리 화면     4회 패배: 패배 화면
```

### 핵심 컴포넌트

#### GameFlowManager

게임 전체 흐름을 상태 머신으로 관리합니다.

```csharp
[Header("게임 설정")]
[SerializeField] private int winCountForVictory = 20;    // 승리 조건
[SerializeField] private int loseCountForGameOver = 4;   // 패배 조건
[SerializeField] private int winsPerSpeedIncrease = 4;   // 속도 증가 주기

[Header("속도 설정")]
[SerializeField] private float baseSpeed = 1.0f;         // 기본 속도
[SerializeField] private float speedIncrement = 0.2f;    // 속도 증가량
[SerializeField] private float maxSpeed = 2.5f;          // 최대 속도
```

**주요 메서드:**
- `StartGame()`: 메인 메뉴에서 게임 시작
- `RestartGame()`: 승리/패배 화면에서 재시작
- `ChangeState(GameState)`: 상태 변경

**주요 프로퍼티:**
- `CurrentState`: 현재 게임 상태
- `WinCount`: 승리 횟수
- `LoseCount`: 패배 횟수
- `CurrentSpeed`: 현재 속도

#### PansoriSceneUI

판소리 씬의 UI 및 연출을 관리합니다.

**기능:**
- "XX해라!" 명령 텍스트 표시
- 환호/야유 반응 표시 (배경색 변경, 텍스트 애니메이션)

```csharp
// 명령 표시
pansoriSceneUI.ShowCommand("점프", 1f, () => {
    // 1초 후 콜백
});

// 반응 표시
pansoriSceneUI.ShowReaction(success, 1.5f, () => {
    // 반응 표시 후 콜백
});
```

#### GameScreens

메인 메뉴, 준비 화면, 승리/패배 화면을 관리합니다.

**관리 화면:**
- `MainMenuPanel`: 시작 버튼
- `ReadyPanel`: "준비!" → "시작!" 텍스트 연출
- `VictoryPanel`: 승리 메시지 + 재시작 버튼
- `GameOverPanel`: 패배 메시지 + 재시작 버튼

## 인터페이스 및 베이스 클래스

### IMicrogame 인터페이스

```csharp
public interface IMicrogame
{
    void OnGameStart(int difficulty, float speed);
    System.Action<bool> OnResultReported { get; set; }
}
```

- `OnGameStart`: 게임 시작 시 매니저가 호출 (난이도: 1~3, 배속: 1.0f~)
- `OnResultReported`: 결과 전달 이벤트 (true: 성공 / false: 실패)

### MicrogameBase 추상 클래스

`MicrogameBase`는 `IMicrogame`을 구현한 추상 클래스로, 다음 기능을 제공합니다:

- **생명주기 관리**: 게임 시작/종료 자동 처리
- **결과 보고**: `ReportResult(bool success)` 메서드로 결과 전달
- **상태 리셋**: `ResetGameState()` 추상 메서드로 상태 리셋 강제
- **난이도/속도 저장**: `currentDifficulty`, `currentSpeed` 프로퍼티 제공

#### 주요 메서드

- `OnGameStart(int difficulty, float speed)`: 게임 시작 (가상 메서드, 오버라이드 가능)
- `ReportResult(bool success)`: 결과 보고 (protected 메서드)
- `ResetGameState()`: 상태 리셋 (추상 메서드, 반드시 구현 필요)
- `OnGameEnd()`: 게임 종료 시 호출 (가상 메서드, 오버라이드 가능)

## 헬퍼 컴포넌트

### MicrogameTimer

난이도와 속도를 반영한 타이머 컴포넌트입니다.

```csharp
[SerializeField] private MicrogameTimer timer;

void Start()
{
    timer.OnTimerEnd += HandleTimerEnd;
}

public override void OnGameStart(int difficulty, float speed)
{
    base.OnGameStart(difficulty, speed);
    timer.StartTimer(5f, speed); // 속도 자동 반영
}
```

### MicrogameInputHandler

표준화된 입력 처리 컴포넌트입니다.

```csharp
[SerializeField] private MicrogameInputHandler inputHandler;

void Start()
{
    inputHandler.OnKeyPressed += HandleKeyPress;
    inputHandler.OnMouseClick += HandleMouseClick;
}

private void HandleKeyPress(KeyCode key)
{
    if (key == KeyCode.Space)
    {
        // 스페이스바 처리
    }
}
```

### MicrogameUILayer

미니게임 전용 UI 레이어 관리 컴포넌트입니다.

```csharp
[SerializeField] private MicrogameUILayer uiLayer;

void Start()
{
    // UI 요소 추가
    GameObject uiElement = new GameObject("UIElement");
    uiLayer.AddUIElement(uiElement);
}
```

### MicrogameInfoUI

게임 시작 전 정보(게임 이름, 목숨, 스테이지)를 표시하는 컴포넌트입니다.

```csharp
[SerializeField] private MicrogameInfoUI infoUI;

// 기본 정보 표시
infoUI.ShowInfo("게임 이름", 3, 1);

// 스프라이트 기반 목숨 표시 + 자동 숨김
infoUI.ShowInfoWithLives("게임 이름", totalLives: 4, consumedLives: 1, stage: 1, autoHideDuration: 2f);

// 게임오버 표시
infoUI.ShowGameOver();
```

## 에디터 도구

미니게임 개발을 돕기 위한 Unity 에디터 확장 도구들이 제공됩니다.

### MicrogameTemplateCreator

새 미니게임을 빠르게 생성하는 마법사 도구입니다.

**사용 방법:**
1. `Tools > Microgames > Create New Microgame` 선택
2. 미니게임 이름 입력
3. 생성할 항목 선택 (스크립트, 프리팹, 폴더 구조)
4. "미니게임 생성" 버튼 클릭

**생성되는 항목:**
- 폴더 구조 (Scripts, Prefabs, Arts, Audios)
- 매니저 스크립트 템플릿
- 기본 프리팹 (헬퍼 컴포넌트 포함)

### MicrogameValidator

프리팹이 미니게임 규격을 준수하는지 검증하는 도구입니다.

**사용 방법:**
1. `Tools > Microgames > Validate Prefab` 선택
2. 검증할 프리팹을 할당
3. "검증 시작" 버튼 클릭

**검증 항목:**
- Transform 초기값 확인 (위치, 회전, 스케일)
- IMicrogame 인터페이스 구현 확인
- ResetGameState() 메서드 구현 확인
- 필수 컴포넌트 확인 (선택사항)

### MicrogameManagerTester

MicrogameManager를 쉽게 테스트할 수 있는 독립적인 에디터 윈도우입니다.

**사용 방법:**
1. `Tools > Microgames > Test Microgame Manager` 선택
2. 씬에서 MicrogameManager 자동 찾기 또는 수동 할당
3. 프리팹 목록에 테스트할 미니게임 프리팹 추가
4. 난이도와 배속 설정
5. "인덱스로 시작" 또는 "랜덤 시작" 버튼으로 게임 시작

**주요 기능:**
- **Manager 선택**: 씬에서 자동 검색 또는 수동 할당
- **프리팹 목록 관리**: 드래그 앤 드롭으로 프리팹 할당
- **게임 설정**: 난이도 슬라이더 (1-3), 배속 슬라이더 (1.0-5.0)
- **제어 버튼**: 인덱스로 시작, 랜덤 시작, 강제 종료
- **상태 표시**: 실시간 실행 상태, 현재 실행 중인 미니게임 이름
- **결과 로그**: 최근 20개 결과 저장, 타임스탬프 포함

### GameFlowSetupWizard

GameFlow 시스템의 모든 UI 요소를 자동으로 생성하는 마법사 도구입니다.

**사용 방법:**
1. `Tools > Microgame System > GameFlow 설정 마법사` 선택
2. MicrogameManager 참조 설정 (선택사항, 없으면 자동 생성)
3. 배경색과 강조색 설정
4. "GameFlow 시스템 생성" 버튼 클릭

**자동 생성되는 항목:**
- `GameFlowManager` 오브젝트
- `GameScreensCanvas` (메인/준비/승리/패배 화면)
  - MainMenuPanel (제목, Start 버튼)
  - ReadyPanel (준비 텍스트)
  - VictoryPanel (승리 메시지, 점수, 재시작 버튼)
  - GameOverPanel (게임오버 메시지, 점수, 재시작 버튼)
- `PansoriSceneCanvas` (판소리 씬)
  - PansoriPanel (배경)
  - CommandText ("XX해라!")
  - ReactionText ("얼쑤!" / "에잇...")

**자동 연결:**
- 모든 컴포넌트 간 참조 자동 연결
- 버튼 이벤트 자동 연결

## 제약사항 및 규칙

### 필수 규칙

1. **씬 로드 금지**: `SceneManager.LoadScene`을 절대 사용하지 마세요. 모든 로직은 프리팹 내에서 완결되어야 합니다.
2. **UI 사용**: 개별 게임의 UI는 `MicrogameUILayer`를 사용하세요.
3. **카메라**: 메인 카메라를 직접 수정하지 마세요. 필요시 프리팹 내의 서브 카메라를 활용하세요.
4. **Transform 초기값**: 프리팹의 루트 Transform은 (0, 0, 0) 위치, (0, 0, 0) 회전, (1, 1, 1) 스케일로 설정되어야 합니다.

### 권장사항

- `MicrogameBase`를 상속받아 구현하세요.
- 헬퍼 컴포넌트(`MicrogameTimer`, `MicrogameInputHandler`, `MicrogameUILayer`)를 활용하세요.
- 난이도와 속도를 게임 로직에 반영하세요.

## 체크리스트

제출 전 다음 항목을 확인하세요:

- [ ] 프리팹의 Transform 값이 초기화되어 있는가? (위치: 0,0,0 / 회전: 0,0,0 / 스케일: 1,1,1)
- [ ] 모든 에셋이 지정된 폴더 안에 포함되어 있는가?
- [ ] 게임이 끝났을 때 `ReportResult`가 반드시 한 번은 호출되는가?
- [ ] `OnDisable` 시점에 오브젝트들이 초기 위치로 돌아가는가? (재사용 가능 확인)
- [ ] `Update` 문에서 매니저로부터 전달받은 `speed` 값이 반영되어 로직이 흐르는가?
- [ ] `ResetGameState()` 메서드가 올바르게 구현되어 있는가?
- [ ] 씬 로드를 사용하지 않았는가?

### 에디터 도구 사용

Unity 에디터에서 제공하는 도구들을 활용하세요:

- **프리팹 검증**: `Tools > Microgames > Validate Prefab`으로 프리팹 규격 확인
- **게임 테스트**: `Tools > Microgames > Test Microgame Manager`로 빠른 테스트
- **템플릿 생성**: `Tools > Microgames > Create New Microgame`으로 새 게임 생성
- **GameFlow 설정**: `Tools > Microgame System > GameFlow 설정 마법사`로 전체 게임 플로우 구성

## 빠른 시작

### 미니게임 개발

새 미니게임을 만들려면:

1. Unity 에디터에서 `Tools > Microgames > Create New Microgame` 선택
2. 미니게임 이름 입력 (예: "MG_Jump_01")
3. 자동으로 폴더 구조 생성 및 템플릿 스크립트 생성
4. 게임 로직 구현
5. 검증 도구로 체크리스트 확인

### GameFlow 설정

전체 게임 플로우를 설정하려면:

1. Unity 에디터에서 `Tools > Microgame System > GameFlow 설정 마법사` 선택
2. MicrogameManager 참조 설정 (없으면 자동 생성)
3. "GameFlow 시스템 생성" 버튼 클릭
4. 생성된 GameFlowManager의 MicrogameManager에 미니게임 프리팹 추가
5. 플레이 모드에서 테스트

자세한 내용은 [QUICK_START.md](QUICK_START.md)를 참조하세요.

## 예제

`Assets/MicrogameSystem/Games/Jaewon_Example_1/` 폴더에 완전히 작동하는 예제 미니게임이 있습니다. 참고하여 개발하세요.

## 문의

문제가 발생하거나 질문이 있으면 프로젝트 매니저에게 문의하세요.
