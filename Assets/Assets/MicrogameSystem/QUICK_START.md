# 🚀 마이크로게임 시스템 빠른 시작 가이드

## 1단계: 씬 자동 세팅 (30초)

1. Unity 메뉴에서 **Tools > Microgames > Scene Setup Wizard** 클릭
2. 녹색 **"원클릭 씬 세팅 실행"** 버튼 클릭
3. 완료! 자동으로 생성됩니다:
   - `GameFlowManager` - 게임 흐름 관리
   - `MicrogameManager` - 마이크로게임 실행 관리
   - `GameScreensCanvas` - 메인/준비/승리/패배 화면
   - `PansoriSceneCanvas` - 판소리 씬 UI
   - `DefaultSettings.asset` - 게임 설정 파일

---

## 2단계: 새 마이크로게임 만들기 (2분)

1. **Tools > Microgames > Create New Microgame** 클릭
2. 정보 입력:
   - **게임 이름**: `MG_Jump_01` (영문만)
   - **게임 설명**: `점프 게임`
   - **게임 명령어**: `점프해라!`
3. **"마이크로게임 생성"** 버튼 클릭
4. 생성된 스크립트 편집 (`Assets/MicrogameSystem/Games/MG_Jump_01/Scripts/MG_Jump_01Manager.cs`)

```csharp
// 핵심 부분만 수정하세요:

private void Update()
{
    if (isGameEnded) return;
    
    timer -= Time.deltaTime;
    
    if (timer <= 0)
    {
        ReportResultWithAnimation(false);  // 시간 초과 = 실패
        return;
    }
    
    // 여기에 승리 조건 추가
    if (Input.GetKeyDown(KeyCode.Space))  // 예: 스페이스바 누르면 성공
    {
        ReportResultWithAnimation(true);
    }
}
```

---

## 3단계: 마이크로게임 등록 (10초)

1. **Tools > Microgames > Scan Prefabs** 클릭
2. **"폴더 스캔"** 버튼 클릭
3. 원하는 프리팹 선택 (체크박스)
4. **"선택한 프리팹을 MicrogameManager에 등록"** 버튼 클릭

---

## 4단계: 테스트 (플레이!)

1. **Play** 버튼 클릭
2. "시작" 버튼으로 게임 시작
3. 디버그가 필요하면: **Tools > Microgames > Debug Inspector**
   - 강제 성공/실패
   - 속도 조절
   - 특정 게임 직접 시작

---

## 💡 핵심 팁

### 필수 구현 사항
```csharp
public class MyGameManager : MicrogameBase
{
    // 1. 게임 이름 (판소리 씬에 표시)
    public override string currentGameName => "행동해라!";
    
    // 2. 게임 시작 처리
    public override void OnGameStart(int difficulty, float speed)
    {
        base.OnGameStart(difficulty, speed);
        // 초기화 코드
    }
    
    // 3. 상태 초기화 (풀링을 위해 필수!)
    protected override void ResetGameState()
    {
        // 모든 변수와 오브젝트를 초기 상태로
    }
}
```

### 결과 보고
```csharp
// 즉시 결과 보고
ReportResult(true);   // 성공
ReportResult(false);  // 실패

// 애니메이션과 함께 보고 (권장)
ReportResultWithAnimation(true);
ReportResultWithAnimation(false);
```

### 속도 활용
```csharp
// speed 값은 1.0 ~ 2.5 범위
// 높을수록 어려워야 함!

float adjustedDuration = baseDuration / speed;  // 시간 단축
float adjustedSpeed = moveSpeed * speed;        // 움직임 빠르게
```

---

## 🔧 에디터 도구 메뉴

| 메뉴 | 기능 |
|------|------|
| Tools > Microgames > Scene Setup Wizard | 씬 자동 구성 |
| Tools > Microgames > Scan Prefabs | 프리팹 스캔/등록 |
| Tools > Microgames > Debug Inspector | 실시간 디버그 |
| Tools > Microgames > Create New Microgame | 새 게임 생성 |
| Tools > Microgames > Validate Prefab | 프리팹 검증 |
| Tools > Microgames > Test Microgame Manager | 매니저 테스트 |

---

## ⚠️ 주의사항

1. **`ResetGameState()` 꼭 구현하세요!** - 프리팹이 재사용되므로 초기화 필수
2. **결과는 한 번만 보고하세요** - `isGameEnded` 체크 필수
3. **프리팹 이름 = BGM 클립 이름** - 자동 재생을 위해

---

## 📞 문제 해결

### Q: 마이크로게임이 시작되지 않아요
- MicrogameManager의 프리팹 목록 확인
- Scan Prefabs로 등록했는지 확인

### Q: 게임이 끝나지 않아요
- `ReportResult()` 또는 `ReportResultWithAnimation()` 호출 확인

### Q: 두 번째 게임부터 이상해요
- `ResetGameState()` 구현 확인
- 모든 변수와 오브젝트 초기화 확인

자세한 내용은 [README.md](README.md) 참조!
