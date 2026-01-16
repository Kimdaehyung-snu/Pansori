# 마이크로게임 시스템 (Microgame System)

와리오웨어 스타일의 마이크로게임 시스템입니다.

## 📁 폴더 구조

```
MicrogameSystem/
├── Scripts/
│   ├── Core/
│   │   ├── IMicrogame.cs              # 마이크로게임 인터페이스
│   │   ├── MicrogameBase.cs           # 마이크로게임 추상 베이스 클래스
│   │   ├── MicrogameManager.cs        # 마이크로게임 풀링 및 실행 관리
│   │   ├── GameFlowManager.cs         # 게임 흐름 상태 머신
│   │   └── MicrogameSystemSettings.cs # 게임 설정 ScriptableObject
│   ├── Helpers/
│   │   ├── MicrogameTimer.cs          # 타이머 헬퍼
│   │   ├── MicrogameInputHandler.cs   # 입력 헬퍼
│   │   ├── PansoriSceneUI.cs          # 판소리 씬 UI
│   │   └── GameScreens.cs             # 메인/승리/패배 화면
│   └── Editor/
│       ├── MicrogameSceneSetupWizard.cs  # 원클릭 씬 세팅
│       ├── MicrogamePrefabScanner.cs     # 프리팹 자동 스캔/등록
│       ├── MicrogameDebugInspector.cs    # 실시간 디버그 창
│       ├── MicrogameTemplateCreator.cs   # 새 게임 템플릿 생성
│       ├── MicrogameValidator.cs         # 프리팹 검증
│       └── MicrogameManagerTester.cs     # 매니저 테스트
├── Games/                              # 마이크로게임 프리팹 폴더
└── Settings/                           # 설정 파일 폴더
```

## 🚀 빠른 시작

### 1. 씬 자동 세팅

1. **Tools > Microgames > Scene Setup Wizard** 실행
2. "원클릭 씬 세팅 실행" 버튼 클릭
3. 자동으로 생성됨:
   - GameFlowManager
   - MicrogameManager  
   - 메인 메뉴 / 준비 / 승리 / 패배 화면
   - 판소리 씬 UI
   - 시스템 설정 파일

### 2. 마이크로게임 생성

1. **Tools > Microgames > Create New Microgame** 실행
2. 게임 이름, 설명, 명령어 입력
3. "마이크로게임 생성" 버튼 클릭
4. 생성된 `{게임이름}Manager.cs` 스크립트 편집

### 3. 마이크로게임 등록

1. **Tools > Microgames > Scan Prefabs** 실행
2. "폴더 스캔" 클릭
3. 등록할 프리팹 선택
4. "선택한 프리팹을 MicrogameManager에 등록" 클릭

### 4. 테스트

1. 플레이 모드 진입
2. **Tools > Microgames > Debug Inspector** 열기
3. 게임 상태 모니터링 및 디버그 기능 사용

## 📋 게임 흐름

```
MainMenu → Ready → PansoriScene ↔ Microgame → Victory/GameOver
                       ↑                 ↓
                       └─────────────────┘
```

1. **MainMenu**: 게임 시작 대기
2. **Ready**: "준비!" 화면 표시 (2초)
3. **PansoriScene**: 명령어 표시 & 결과 반응
4. **Microgame**: 마이크로게임 진행
5. **Victory**: 20회 승리 시
6. **GameOver**: 4회 패배 시

## ⚙️ 설정 (MicrogameSystemSettings)

`Assets/MicrogameSystem/Settings/` 폴더에 ScriptableObject로 저장됩니다.

| 설정 | 설명 | 기본값 |
|------|------|--------|
| winCountForVictory | 승리에 필요한 승리 횟수 | 20 |
| loseCountForGameOver | 게임오버까지 허용되는 패배 횟수 | 4 |
| maxLives | 최대 목숨 수 | 4 |
| baseSpeed | 기본 게임 속도 | 1.0 |
| winsPerSpeedIncrease | 속도 증가 간격 (N승마다) | 4 |
| speedIncrement | 속도 증가량 | 0.2 |
| maxSpeed | 최대 속도 | 2.5 |
| enableShuffle | 게임 셔플 활성화 | true |
| shuffleHistorySize | 연속 중복 방지 개수 | 3 |

## 🎮 마이크로게임 작성 가이드

### 기본 템플릿

```csharp
using UnityEngine;
using Pansori.Microgames;

public class MyGameManager : MicrogameBase
{
    [SerializeField] private float gameDuration = 5f;
    private float timer;
    
    // 게임 이름 (판소리 씬에 표시됨)
    public override string currentGameName => "점프해라!";
    
    // 게임 시작
    public override void OnGameStart(int difficulty, float speed)
    {
        base.OnGameStart(difficulty, speed);
        timer = gameDuration / speed;  // 속도에 따라 시간 조정
    }
    
    private void Update()
    {
        if (isGameEnded) return;
        
        timer -= Time.deltaTime;
        
        if (timer <= 0)
        {
            // 시간 초과 = 실패
            ReportResultWithAnimation(false);
            return;
        }
        
        // 승리 조건 확인
        if (/* 승리 조건 */)
        {
            ReportResultWithAnimation(true);
        }
    }
    
    // 필수: 게임 상태 초기화 (풀링을 위해)
    protected override void ResetGameState()
    {
        timer = gameDuration;
        // 모든 게임 요소 초기 상태로 복원
    }
}
```

### 중요 규칙

1. **`ResetGameState()` 필수 구현**: 프리팹 풀링을 위해 모든 상태를 초기화해야 함
2. **`currentGameName` 오버라이드**: 판소리 씬에 표시될 명령어
3. **`speed` 매개변수 활용**: 난이도에 따라 게임 속도 조정
4. **결과 보고**: `ReportResult(bool)` 또는 `ReportResultWithAnimation(bool)` 사용

### 헬퍼 컴포넌트

#### MicrogameTimer
```csharp
[SerializeField] private MicrogameTimer timer;

void Start()
{
    timer.OnTimerEnd += OnTimeOut;
    timer.StartTimer(5f, currentSpeed);
}
```

#### MicrogameInputHandler
```csharp
[SerializeField] private MicrogameInputHandler inputHandler;

void Start()
{
    inputHandler.OnKeyPressed += OnKeyPress;
    inputHandler.OnMouseClick += OnClick;
}
```

## 🔧 에디터 도구

### Scene Setup Wizard
- **위치**: Tools > Microgames > Scene Setup Wizard
- **기능**: 원클릭으로 전체 씬 구성 자동 생성

### Prefab Scanner
- **위치**: Tools > Microgames > Scan Prefabs
- **기능**: Games 폴더의 프리팹 자동 스캔 및 등록

### Debug Inspector
- **위치**: Tools > Microgames > Debug Inspector
- **기능**: 
  - 실시간 게임 상태 모니터링
  - 강제 성공/실패
  - 값 조정 (승리 횟수, 속도 등)
  - 특정 게임 직접 시작
  - 통계 확인

### Template Creator
- **위치**: Tools > Microgames > Create New Microgame
- **기능**: 새 마이크로게임 템플릿 자동 생성

### Validator
- **위치**: Tools > Microgames > Validate Prefab
- **기능**: 프리팹이 규격에 맞는지 검증

## 📊 이벤트 시스템

### GameFlowManager 이벤트
```csharp
// 상태 변경
flowManager.OnStateChanged += (GameState state) => { };

// 속도 변경
flowManager.OnSpeedChanged += (float speed) => { };

// 스테이지 변경
flowManager.OnStageChanged += (int stage) => { };

// 승리/패배
flowManager.OnWin += (int winCount) => { };
flowManager.OnLose += (int loseCount) => { };

// 게임 완료
flowManager.OnGameComplete += (bool isVictory, int wins, int losses) => { };
```

### MicrogameManager 이벤트
```csharp
// 마이크로게임 결과
microgameManager.OnMicrogameResult += (bool success) => { };

// 마이크로게임 시작
microgameManager.OnMicrogameStarted += (int index, int difficulty, float speed) => { };

// 목숨 변경
microgameManager.OnLivesChanged += (int current, int max) => { };
```

## 🎵 사운드 연동

SoundManager와 자동 연동됩니다:
- 메인 BGM (자진모리): 게임 속도에 따라 피치 조절
- 마이크로게임 BGM: 프리팹 이름과 일치하는 클립 자동 재생
- 결과 효과음: 성공/실패 시 자동 재생

## ✅ 체크리스트

### 새 마이크로게임 추가 시
- [ ] `MicrogameBase` 상속
- [ ] `currentGameName` 오버라이드
- [ ] `OnGameStart()` 구현
- [ ] `ResetGameState()` 구현
- [ ] 결과 보고 (`ReportResult` 또는 `ReportResultWithAnimation`)
- [ ] 프리팹 생성 및 스캔/등록
- [ ] (선택) BGM 클립 추가 (프리팹 이름과 동일)

### 씬 구성 시
- [ ] Scene Setup Wizard 실행
- [ ] 설정 파일 조정
- [ ] 마이크로게임 프리팹 등록
- [ ] 플레이 테스트
