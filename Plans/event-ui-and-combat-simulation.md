# 이벤트 UI & 적군 전투 시뮬레이션 구현 계획

## Context
이벤트 Logic 계층(EventLogicManager)은 완성되어 있으나 Presentation과의 연결이 없음.
두 가지를 구현한다:
1. **EventDialogUIManager** — Dialog/Reward 서브이벤트를 화면 중앙 오버레이 UI로 표시
2. **EnemyCombatManager** — Combat 서브이벤트 시작 시 적군 우주선을 Logic만으로 조립, 동일 틱 루프에 등록, 적 체력 0 → 전투 종료 자동 호출

---

## Part 1 — EventDialogUIManager (Dialog + Reward UI)

### 신규 파일

#### `Assets/Scripts/Presentation/UI/EventDialogUIManager.cs`

```
MonoBehaviour. UIDocument 공유(GameHUD).
Initialize(IEventLogic) 호출로 이벤트 구독.

- OnSubEventChanged(DialogSubEventSO):
    EventOverlay 표시 / RewardContainer 숨김 / AcceptButton 숨김
    제목·대화 텍스트 설정
    선택지 버튼 동적 생성 (각각 CompleteDialogSubEvent(i) 호출)

- OnSubEventChanged(RewardSubEventSO):
    EventOverlay 표시 / ChoicesContainer 숨김
    보상 항목 라벨 동적 생성 (Type + Amount 표시)
    Accept 버튼 표시 → CompleteRewardSubEvent() 호출

- OnEventFinished():
    EventOverlay 숨김
    동적 생성 버튼/라벨 정리
```

#### `Assets/Scripts/Presentation/UI/Styles/EventUI.uss`

```css
.event-overlay          /* position:absolute, 100%×100%, 반투명 어두운 파란 배경 rgba(0,20,50,0.6) */
.event-panel            /* 중앙 패널 560px, rgba(10,40,80,0.85), 테두리 rgba(100,180,255,0.4) */
.event-title            /* 상단 제목 라벨, 굵게, 흰색 */
.event-dialog-text      /* 본문 텍스트, 여백, 줄바꿈 허용 */
.event-choices-container /* 선택지 버튼 세로 배치 */
.event-choice-button    /* 각 선택지 버튼: 좌측 정렬, 테두리, hover 효과 */
.event-reward-container  /* 보상 항목 세로 배치 */
.event-reward-entry     /* 보상 라벨 행 */
.event-accept-button    /* 하단 Accept 버튼 */
```

### 수정 파일

#### `Assets/Scripts/Presentation/UI/UIDocuments/GameHUD.uxml`
PauseOverlay·MapOverlay와 같은 형제 레벨로 추가 (position:absolute 전체 화면 덮음):

```xml
<!-- 이벤트 오버레이 (dialog / reward) -->
<ui:VisualElement name="EventOverlay" class="event-overlay" style="display: none;">
    <ui:VisualElement class="event-panel">
        <ui:Label name="EventTitle"      class="event-title" />
        <ui:Label name="EventDialogText" class="event-dialog-text" />
        <ui:VisualElement name="RewardContainer"  class="event-reward-container" />
        <ui:VisualElement name="ChoicesContainer" class="event-choices-container" />
        <ui:Button        name="AcceptButton"     class="event-accept-button" text="Accept" />
    </ui:VisualElement>
</ui:VisualElement>
```

`GameHUD.uxml` 상단에 EventUI.uss 임포트 추가.

#### `Assets/Scripts/Presentation/System/ShipSetupManager.cs`
`BeginSetup()` 마지막에 추가:
```csharp
var eventDialogUI = FindObjectOfType<EventDialogUIManager>();
if (eventDialogUI != null)
    eventDialogUI.Initialize(eventLogicManager);
```

---

## Part 2 — EnemyCombatManager (Logic 계층)

### 수정 파일 1: `Assets/Scripts/Logic/System/SimulationCore.cs`

현재 `ProcessTick()` 도중 리스트 수정 불가 → pending removal 패턴 추가:

```csharp
private readonly List<ITickable> _pendingRemovals = new();

public void UnregisterTickable(ITickable tickable)
    => _pendingRemovals.Add(tickable);

public void UnregisterTickables(IEnumerable<ITickable> tickables)
    => _pendingRemovals.AddRange(tickables);

private void ProcessTick()
{
    if (_pendingRemovals.Count > 0)
    {
        foreach (var t in _pendingRemovals) _tickables.Remove(t);
        _pendingRemovals.Clear();
    }
    foreach (var tickable in _tickables) tickable.OnTickUpdate();
}
```

### 신규 파일 1: `Assets/Scripts/Core/Interface/IEnemyCombatManager.cs`

```csharp
public interface IEnemyCombatManager
{
    IShipAPI EnemyShipAPI { get; }
    void StartCombat(CombatSubEventSO combatEvent);
    void EndCombat();
}
```

### 신규 파일 2: `Assets/Scripts/Logic/Event/EnemyCombatManager.cs`

```
IEnemyCombatManager + ITickable 구현. 순수 C#.

생성자: EnemyCombatManager(IEventLogic eventLogic, SimulationCore simCore)

StartCombat(CombatSubEventSO):
  1. GridBuilder.Rebuild(combatEvent.EnemyShip.ShipData) → _enemyShipAPI
  2. 적군 ITickable 목록 수집 (rooms, doors, weapons)
  3. simCore.RegisterTickables(_enemyTickables)
  4. simCore.RegisterTickables(this)

OnTickUpdate():
  if (_enemyShipAPI.CurrentHullHealth <= 0 && !_ended)
      EndCombat(); _eventLogic.CompleteCombatSubEvent()

EndCombat():
  simCore.UnregisterTickables(_enemyTickables)
  simCore.UnregisterTickable(this)
  _ended = true
```

### 수정 파일 2: `Assets/Scripts/Presentation/System/ShipSetupManager.cs`

- `simCore` 지역변수 → `_simCore` 필드로 승격
- `EnemyCombatManager` 생성 및 `OnSubEventChanged` 구독 (CombatSubEventSO일 때 StartCombat 호출)

---

## 수정 대상 파일 요약

| 파일 | 변경 유형 |
|------|-----------|
| `Presentation/UI/EventDialogUIManager.cs` | **신규** |
| `Presentation/UI/Styles/EventUI.uss` | **신규** |
| `Core/Interface/IEnemyCombatManager.cs` | **신규** |
| `Logic/Event/EnemyCombatManager.cs` | **신규** |
| `UI/UIDocuments/GameHUD.uxml` | EventOverlay 추가 + Style import |
| `Logic/System/SimulationCore.cs` | UnregisterTickable(s) + pending removal |
| `Presentation/System/ShipSetupManager.cs` | simCore 필드화, EnemyCombatManager/EventDialogUIManager 초기화 |

---

## 이벤트 완성 흐름

```
StartEvent(DialogSO) → EventDialogUIManager: 오버레이 + 선택지 표시
선택지 클릭 → CompleteDialog → CombatSO → EnemyCombatManager.StartCombat()
적 체력 0 → CompleteCombat → RewardSO → EventDialogUIManager: 보상 UI
Accept 클릭 → CompleteReward → OnEventFinished → 오버레이 숨김
```
