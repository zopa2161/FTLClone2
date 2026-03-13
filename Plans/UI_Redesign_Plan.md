# UI 개편 계획

## Context
현재 UI는 상단 우측에 실드, 중앙에 리소스(FUEL/MSL/DRN)+버튼이 있는 구조.
선박 체력 바, 스크랩(화폐) UI를 새로 추가하고, 실드/리소스 UI를 좌측 컬럼으로 이동해 Crew UI와 그루핑하는 레이아웃 개편.

---

## 변경 레이아웃 목표

```
[좌측 컬럼]                          [중앙/우측]
┌──────────────────────────────┐
│ [체력 바 × 20]    [SCR: 0]   │
├──────────────────────────────┤     [JUMP / ▲ / ⚙]
│ [실드 ●●●●]  [FUEL MSL DRN] │
├──────────────────────────────┤
│ [Crew UI (기존)]              │
└──────────────────────────────┘

[하단 좌]              [하단 중앙]
[전력 패널]            [무기 패널]
```

---

## 수정 파일 목록

### A. Core 데이터 계층

**1. `Assets/Scripts/Core/Data/SpaceShip/ShipSaveData.cs`**
- `public int MaxHullHealth = 30;` 추가
- `public int CurrentHullHealth = 30;` 추가

**2. `Assets/Scripts/Core/Data/SpaceShip/ResourceData.cs`**
- `public int Scrap = 0;` 추가

**3. `Assets/Scripts/Core/Interface/IResourceManager.cs`**
- `int Scrap { get; }` 프로퍼티 추가
- `event Action<int> OnScrapChanged;` 추가
- `bool TryConsumeScrap(int amount);` 추가
- `void AddScrap(int amount);` 추가

**4. `Assets/Scripts/Core/Interface/IShipAPI.cs`**
- `int MaxHullHealth { get; }` 추가
- `int CurrentHullHealth { get; }` 추가

### B. Logic 계층

**5. `Assets/Scripts/Logic/System/ResourceManager.cs`**
- `public int Scrap { get; private set; }` 추가
- `public event Action<int> OnScrapChanged;` 추가
- `Initialize()`에서 `Scrap = data.Scrap;` 초기화
- `TryConsumeScrap()`, `AddScrap()` 구현 (기존 패턴 그대로)

**6. `Assets/Scripts/Logic/SpaceShip/SpaceShipManager.cs`**
- `public int MaxHullHealth { get; private set; }` 추가
- `public int CurrentHullHealth { get; private set; }` 추가
- `public void SetHullHealth(int maxHealth, int currentHealth)` 메서드 추가

**7. `Assets/Scripts/Logic/SpaceShip/GridBuilder.cs`**
- `Rebuild()` 내부 `_spaceShipManager.SetShieldLogic()` 다음에
  `_spaceShipManager.SetHullHealth(saveData.MaxHullHealth, saveData.CurrentHullHealth);` 추가

### C. Presentation 계층 - UI 뷰

**8. `Assets/Scripts/Presentation/UI/HullHealthUIView.cs` (신규)**
- MonoBehaviour
- `public UIDocument Document;`
- `Initialize(IShipAPI shipAPI)`:
  - `HullHealthPanel`에 20개 bar `VisualElement` 동적 생성
  - `shipAPI.CurrentHullHealth`, `shipAPI.MaxHullHealth`로 초기 상태 렌더링
  - 채워진 바 개수 = `(int)Math.Round((float)current / max * 20)`
  - 색상 결정: >66% → `hull-bar-green`, >33% → `hull-bar-yellow`, ≤33% → `hull-bar-red`
- 주의: 이 시점엔 이벤트 구독 없음 (차후 연결 예정)

**9. `Assets/Scripts/Presentation/UI/GameMainUIView.cs` (수정)**
- `private Label _scrapLabel;` 추가
- `root.Q<Label>("ScrapLabel")` 쿼리 추가 (이름 기반 검색이므로 UXML 위치 이동 영향 없음)
- `_resourceManager.OnScrapChanged += OnScrapChanged;` 구독 추가
- `OnScrapChanged(int scrap)` 핸들러 추가
- `RefreshResourceLabels()`에 scrap 갱신 추가
- `OnDestroy()`에 이벤트 해제 추가
- **resource-row 관련 코드는 변경 없음** (Q\<Label\> 검색은 UXML 트리 전체를 대상으로 하므로 UXML 위치 이동해도 동작)

**10. `Assets/Scripts/Presentation/System/ShipSetupManager.cs` (수정)**
- `HullHealthUIView` 초기화 코드 추가:
  ```csharp
  var hullHealthUI = FindObjectOfType<HullHealthUIView>();
  if (hullHealthUI != null)
      hullHealthUI.Initialize(shipAPI);
  ```

### D. UI 마크업 / 스타일

**11. `Assets/Scripts/Presentation/UI/UIDocuments/GameHUD.uxml` (전체 구조 재편)**

현재 구조:
```
hud-top-row: [CrewPanel] [spacer] [GameMainPanel: resources+buttons] [spacer] [ShieldPanel]
```

변경 후 구조:
```
hud-top-row:
  hud-left-column (column):
    hud-health-scrap-row (row): [HullHealthPanel] [scrap-panel]
    hud-shield-resource-row (row): [ShieldPanel] [resource-row]
    CrewPanel
  hud-top-spacer
  GameMainPanel (버튼만): [JUMP] [▲] [⚙]
```

**12. `Assets/Scripts/Presentation/UI/Styles/GameHUD.uss` (수정)**

신규 클래스 추가:
```css
.hud-left-column         { flex-direction: column; }
.hud-health-scrap-row    { flex-direction: row; align-items: center; }
.hud-shield-resource-row { flex-direction: row; align-items: center; }
```

**13. `Assets/Scripts/Presentation/UI/Styles/HullHealthUI.uss` (신규)**

```css
.hull-health-panel { flex-direction: row; padding: 4px; background-color: rgba(0,0,0,0.7); }
.hull-bar          { width: 8px; height: 14px; margin: 1px; background-color: rgba(60,60,60,1); border-radius: 2px; }
.hull-bar-green    { background-color: rgb(80, 200, 80); }
.hull-bar-yellow   { background-color: rgb(220, 180, 0); }
.hull-bar-red      { background-color: rgb(200, 60, 60); }
.scrap-panel       { flex-direction: column; align-items: center; padding: 4px 6px; background-color: rgba(0,0,0,0.7); margin-left: 4px; }
```

---

## 참조 기존 패턴 (재사용)
- 이벤트 구독 패턴: `ResourceManager.cs` (L13-15)
- UI 동적 생성 패턴: `CrewSystemUIView.cs`, `WeaponSystemUIView.cs`
- Q\<Label\> 이름 쿼리: `GameMainUIView.cs` (L38-39)

---

## 작업 순서
1. Core 데이터 (ShipSaveData, ResourceData)
2. Core 인터페이스 (IResourceManager, IShipAPI)
3. Logic (ResourceManager, SpaceShipManager, GridBuilder)
4. UXML + USS (레이아웃 재편)
5. HullHealthUIView.cs 신규 생성
6. GameMainUIView.cs 스크랩 추가
7. ShipSetupManager.cs HullHealthUIView 초기화 추가

---

## 검증 방법
1. 유니티 에디터에서 플레이 진입
2. 좌상단: 체력 바 20개 초록색으로 표시 확인
3. 체력 바 우측: SCR 0 표시 확인
4. 체력 바 아래: 실드 circles + FUEL/MSL/DRN 나란히 표시 확인
5. 실드 아래: 승무원 슬롯 표시 확인
6. 상단 중앙/우측: JUMP/▲/⚙ 버튼만 표시 확인
7. 기존 실드 이벤트(OnShieldChanged), 리소스 이벤트 정상 동작 확인
8. DefaultSheepMaker Context Menu로 JSON 재생성 시 MaxHullHealth/CurrentHullHealth/Scrap 포함 확인
