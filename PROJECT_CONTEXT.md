# FTLClone2 프로젝트 구조 분석

## 아키텍처 (3계층 분리)

```
Core (순수 C# 데이터/인터페이스)
  └─ Logic (순수 C# 로직, MonoBehaviour 없음)
       └─ Presentation (Unity MonoBehaviour + UI Toolkit)
```

---

## Core 계층 (`Assets/Scripts/Core/`)

### Data
- `ShipSaveData` — JSON 직렬화 루트. `TileData`, `RoomData`, `DoorData`, `CrewData`, `WeaponData` 목록 + `ShieldData` 보유
- `ShieldData` — `ChargeGauge(0~1)`, `CurrentShieldCount(0~4)` 보유
- `TileCoord` — (X, Y) 구조체, 커스텀 `==` / `GetHashCode` 구현
- `TileData` — OxygenLevel, BreachLevel, ConnectedNeighborCoords
- `RoomData` — RoomID, RoomType, MaxPower, CurrentAllocatedPower, TileCoords, ConsoleDirection
- `DoorData` — DoorID, TileA, TileB, IsOpen, IsForcedOpen
- `CrewData` — CrewID, BaseDataID, CurrentX/Y, MaxHealth, CurrentHealth
- `WeaponData` — WeaponID, IsPowered, CurrentChargeTimer, IsAutoFire, TargetRoomID
- `WeaponBaseSO` — ScriptableObject. WeaponID, WeaponName, Damage, RequiredPower, BaseCooldown, WeaponType
- `CrewBaseSO` — ScriptableObject. DefaultSprite
- `SaveLoadManager` — JSON 직렬화/역직렬화 (`JsonUtility`)

### Interface
| 인터페이스 | 주요 멤버 |
|---|---|
| `IShipAPI` | `IGridMap` 상속 + `GetAllCrews()`, `GetAllWeapons()`, `GetShieldLogic()` |
| `IGridMap` | `GetAllTiles/Rooms/Doors()`, `GetConnectedNeighbors()`, `GetDoorBetween()`, `GetRoomAt()`, `GetTileAt()` |
| `IRoomLogic` | RoomID, Data, AverageOxygen, CurrentPower, MaxPowerCapacity, IsManned, `ChangePower()`, `ChangeWorkingCrewCount()`, `OnPowerChanged`, `OnOxygenChanged`, `OnMannedStatusChanged` |
| `IWeaponLogic` | Data, BaseData, IsPowered, IsReadyToFire, ChargeProgress, `SetPower()`, `SetAutoFire()`, `SetTarget()`, `TryFire()`, `SetBaseData()`, `SetChargeMultiplier()`, `OnChargeUpdated`, `OnPowerStateChanged`, `OnFired` |
| `ICrewLogic` | CrewID, Data, CurrentHealth, MaxHealth, `CommandMoveTo()`, `TakeDamage()`, `OnPositionChanged`, `OnHealthChanged`, `OnDied` |
| `IDoorLogic` | DoorID, IsOpen, `SetDoorState()`, `ToggleDoorManual()`, `OnDoorStateChanged` |
| `ITileLogic` | TileCoord, OxygenLevel, BreachLevel |
| `IPowerSystem` | MaxReactorPower, AvailableReactorPower, `TryAddPowerToRoom()`, `TryRemovePowerFromRoom()`, `OnReactorPowerChanged` |
| `IShieldLogic` | MaxShields, CurrentShields, ChargeGauge, `TryAbsorbDamage()`, `OnShieldChanged(current, max, chargeGauge)` |
| `ITickable` | `OnTickUpdate()` |

### enums
- `MoveDirection` — None, Up, Down, Left, Right
- `WeaponType` — Laser, Missile, Beam

### RoomTypeString (상수)
Pilot, Oxygen, Empty, Engine, Weapon, Shield, Door, Vision, MedBay

---

## Logic 계층 (`Assets/Scripts/Logic/`)

### SpaceShip

| 클래스 | 역할 |
|---|---|
| `SpaceShipManager` | `IShipAPI` 구현체. 타일/방/문/승무원/무기/실드 목록 보유. 승무원 사망 시 명부 삭제. `SetShieldLogic()` / `GetShieldLogic()` 제공 |
| `GridBuilder` | `ShipSaveData` → Logic 객체 일괄 조립 (`Rebuild()` 반환값: `IShipAPI`). `RebuildShield()` 추가 — Shield 방을 찾아 `ShieldManager` 생성 후 연결 |
| `RoomLogicFactory` | RoomType 문자열 → 구체 Room 클래스 생성 (switch 패턴) |
| `TileLogic` | `ITileLogic` 구현. `Neighbors` 리스트(TileLogic 참조) 보유 |
| `DoorLogic` | `IDoorLogic` + `ITickable`. `AUTO_CLOSE_DELAY_TICKS=5` 틱 후 자동 닫힘. `IsForcedOpen` 시 타이머 멈춤 |
| `CrewLogic` | `ICrewLogic` + `ITickable`. FSM(`ICrewState`) + A* 경로. 산소 < 50f 시 0.5f/틱 피해 |

#### 방 로직 (모두 BaseRoomLogic 상속)
- `BaseRoomLogic` (abstract) — 전력 변경, 산소 평균, 승무원 근무 상태 공통 처리
- `OxygenRoomLogic` — `GetOxygenGeneration()` (전력 1→1, 2→3, 3→6, 고장 시 -1)
- `PilotRoomLogic`, `EngineRoomLogic`, `WeaponRoomLogic`, `ShieldRoomLogic`, `EmptyRoomLogic` — 현재는 생성자만 존재

#### 승무원 FSM (CrewState)
| 상태 | Enter | Execute | Exit |
|---|---|---|---|
| `CrewIdleState` | (없음) | (없음) | (없음) |
| `CrewMovingState` | movingCount=3 | 3틱마다 한 칸 이동, 문 자동 개방, 목적지 도착 시 WorkingState or IdleState 전환 | (없음) |
| `CrewWorkingState` | `LookAt(ConsoleDirection)`, 방에 근무 시작 보고 | (없음) | 방에 근무 종료 보고 |

### System

| 클래스 | 역할 |
|---|---|
| `SimulationCore` | `ITickable` 목록 관리. `AdvanceTime(deltaTime)` → `TickRate=0.1f` 간격 틱 발행. `TimeScale` 지원 |
| `ShipSimulationManager` | `ITickable`. 브리치 타일 BFS 거리맵 → 진공 바람 시뮬레이션. 산소 생성은 `OxygenRoomLogic` 위임 |
| `PowerManager` | `IPowerSystem` 구현. 원자로 전력 할당(`TryAddPowerToRoom`)/회수(`TryRemovePowerFromRoom`) |
| `WeaponManager` | 무기 ON/OFF, 전력 초과 시 뒤에서부터 강제 종료. 승무원 배치 보너스 1.2x (`SetChargeMultiplier`) |
| `ShieldManager` | `IShieldLogic` + `ITickable` 구현. Shield 방 전력·Manned 상태 참조 → 충전 진행. 전력 없으면 게이지/실드 초기화. `TryAbsorbDamage()` 로 피해 1회 흡수 |
| `LogicCommandManager` | 승무원 선택/이동 명령 브릿지. `SelectCrew(crew, clickedByUI)`, `OrderMoveCommand()`, `OrderMoveToRoom(room)`, `DeselectCrew()`. `OnSelectionChanged`, `OnCrewUIClicked` 이벤트 제공. `SetAllCrews()`로 전체 크루 목록 주입 → 방 이동 시 빈 타일 순서대로 배정, 가득 차면 이동 불가 |
| `AStarPathfinder` | static. 맨해튼 거리 휴리스틱 A*. 반환값: `Queue<TileCoord>` |

---

## Presentation 계층 (`Assets/Scripts/Presentation/`)

### System

| 클래스 | 역할 |
|---|---|
| `Singleton<T>` | DontDestroyOnLoad 싱글톤 기반 클래스 |
| `GameSessionManager` | Singleton. `Awake()`에서 JSON 로드 → `GameSaveData` 보유. `ShipData`, `MapData` 편의 접근자 제공. `SetMapData()`, `SaveGame()` 제공 |
| `AssetCatalogManager` | Singleton. SO 카탈로그 (ShipHull / Crew / Weapon) List→Dictionary 변환 |
| `ShipSetupManager` | 게임 시작 시 전체 조립. `GameSessionManager.ShipData`로 데이터 수신. GridBuilder → SpaceShipView.Bind() → PowerManager/WeaponManager 초기화 → SimulationCore 등록 → UI 초기화 |
| `MouseInputManager` | 좌클릭(승무원 선택 / 문 토글), 우클릭(이동 명령), Hover(방 하이라이트). `OnCrewUIClicked` 이벤트로 UI 클릭과 인게임 클릭 충돌 방지. `RegisterCrewViews()`로 CrewView 등록. `CommandManager` 프로퍼티로 외부 공유 |
| `UnityTimeProvider` | `MonoBehaviour.Update()` → `SimulationCore.AdvanceTime(Time.deltaTime)` |

### Views

| 클래스 | 역할 |
|---|---|
| `SpaceShipView` | Tile/Room/DoorView 바인딩 허브. `GetWorldPosition(x, y)` 제공. `BindShield(IShieldLogic)` 메서드 추가. `SimulationCore` 프로퍼티 보유 |
| `RoomView` | `OnOxygenChanged` 구독 → 산소 < 50f 시 빨간 오버레이 (Alpha 최대 0.5) |
| `TileView` | `ITileLogic` 바인딩. 에디터 Gizmos로 좌표 표시 |
| `DoorView` | `OnDoorStateChanged` 구독 → 스프라이트 교체 (OpenedDoor / ClosedDoor) |
| `CrewView` | `OnPositionChanged` → 방향 회전 + MoveTowards 이동. `OnHealthChanged` → 체력바. `OnDied` → Destroy |
| `WeaponView` | `Bind()` 내용 **미구현** |
| `ShieldView` | `IShieldLogic.OnShieldChanged` 구독 → 실드 0↔1 경계에서 GameObject on/off |

### UI (Unity UI Toolkit)

| 클래스/파일 | 역할 |
|---|---|
| `PowerSystemUIView` | 좌클릭=전력 할당, 우클릭=전력 회수. 방 바 동적 생성. `OnReactorPowerChanged`, `OnPowerChanged` 구독 |
| `WeaponSystemUIView` | 무기 슬롯 동적 생성. 장전 게이지(%) 실시간 업데이트. 좌클릭=ON, 우클릭=OFF |
| `CrewSystemUIView` | 승무원 슬롯 동적 생성. 초상화(`CrewBaseSO.DefaultSprite`), 이름, 체력바 표시. 슬롯 클릭=선택/재클릭=해제. `OnSelectionChanged` 구독 → 슬롯 시각 동기화. `OnHealthChanged`, `OnDied` 구독. 30% 이하 체력 시 danger 스타일. 사망 시 dead 스타일 |
| `GameHUD.uxml` | **통합 HUD UXML** (신규). `overlay` 하나에 승무원(상단 좌), 전력(하단 좌), 무기(하단 중앙) 패널을 모두 포함. 각 서브 USS를 `@import` 방식으로 참조 |
| `GameHUD.uss` | HUD 레이아웃 스타일. `hud-overlay`, `hud-top-row`, `hud-spacer`, `hud-bottom-row`, `hud-bottom-center`, `hud-bottom-right`(180px 우측 균형 여백) |
| `PowerSystemUI.uxml` | `ReactorBarContainer` + `RoomControlsGroup` |
| `WeaponSystemUI.uxml` | `overlay` (클릭 무시) + `WeaponPanel` |
| `CrewSystemUI.uxml` | `overlay` (클릭 무시) + `CrewPanel` |
| `PowerSystemUI.uss` | reactor-bar, room-bar, room-button 스타일 |
| `WeaponSystemUI.uss` | weapon-slot, weapon-label, weapon-charge-bg/fill 스타일 |
| `CrewSystemUI.uss` | crew-panel(좌상단 세로), crew-slot, crew-slot.selected(파란 테두리), crew-slot.dead(반투명 빨강), crew-portrait(56×56), crew-info-column(이름+체력바 세로 컨테이너), crew-name, crew-health-bg/fill, crew-health-fill.danger 스타일 |

---

## 데이터 흐름 (시작 ~ 실행)

```
1. DefaultSheepMaker (ContextMenu) → DefaultShipData.json 생성
2. GameSessionManager.Awake() → SaveLoadManager.Load() → ShipSaveData
3. ShipSetupManager.BeginSetup()
   ├─ AssetCatalogManager로 Hull 프리팹 취득 → Instantiate
   ├─ GridBuilder.Rebuild(savedData) → IShipAPI (SpaceShipManager)
   ├─ SpaceShipView.Bind() → TileView/RoomView/DoorView 이벤트 구독
   ├─ SetupCrews() → CrewView.Bind()
   ├─ SetupWeapons() → weaponLogic.SetBaseData(SO), WeaponView.Bind()
   ├─ WeaponManager.Initialize()
   ├─ PowerManager.Initialize()
   ├─ SimulationCore.RegisterTickables(rooms, crews, doors, weapons, shipSim)
   ├─ SimulationCore.RegisterTickables(shieldManager) ← ShieldRoom 있을 때만
   ├─ SpaceShipView.BindShield(shieldManager)
   ├─ UnityTimeProvider.Initialize(simCore)
   ├─ PowerSystemUIView.Initialize()
   ├─ WeaponSystemUIView.Initialize()
   └─ CrewSystemUIView.Initialize(crewLogics, commandManager)
4. UnityTimeProvider.Update() → SimulationCore.AdvanceTime(deltaTime)
   └─ 0.1초마다 ITickable.OnTickUpdate() 일괄 호출
```

---

## 주요 수치 / 규칙

| 항목 | 값 |
|---|---|
| 틱 간격 | 0.1초 (`SimulationCore.TickRate`) |
| 승무원 이동 속도 | 3틱/칸 (`CrewMovingState.TickForMove`) |
| 문 자동 닫힘 | 5틱 후 (`DoorLogic.AUTO_CLOSE_DELAY_TICKS`) |
| 무기 장전 단위 | 0.1f/틱 (`WeaponLogic.TICK_TIME_STEP`) |
| 승무원 배치 장전 보너스 | 1.2x (`WeaponManager.MANNING_BONUS_MULTIPLIER`) |
| 산소 위험 임계값 | 50f (크루 피해 시작) |
| 산소 피해량 | 0.5f/틱 |
| 진공 바람 세기 | OxygenLevel × 0.15f/틱 |
| 실드 충전 시간 | 75틱 = 7.5초/1개 (`ShieldManager.BASE_CHARGE_RATE = 1/75`) |
| 실드 Manned 보너스 | 1.2x (`ShieldManager.MANNED_BONUS`) |
| 실드 최대 수 | 전력 2당 1개 (최대 4개) |

---

### GameSaveData 도입 — 최상위 세이브 데이터 통합
- **신규 파일**:
  - `Core/Data/Storage/GameSaveData.cs` — `ShipSaveData Ship` + `MapData Map` 보유. 최상위 직렬화 루트
- **수정 파일**:
  - `SaveLoadManager` — `Save<T>()` / `Load<T>()` 제네릭으로 교체 (ShipSaveData 전용 메서드 제거)
  - `GameSessionManager` — `CurrentGameData`를 `GameSaveData`로 교체. `ShipData`, `MapData` 접근자 추가. `SetMapData()`, `SaveGame()` 추가. 세이브 없을 때 오류 대신 새 `GameSaveData()` 생성
  - `ShipSetupManager` — `HandOverData()` → `GameSessionManager.Instance.ShipData` 로 변경

### 맵 시스템 스켈레톤 추가
- **신규 파일**:
  - `Core/Data/Map/NodeData.cs` — `NodeType` enum, 노드 위치(X,Y 0~1 정규화), 연결 ID 목록, IsVisited, EventID
  - `Core/Data/Map/MapData.cs` — 전체 노드 목록, CurrentNodeID, 맵 크기, 컬럼 수
  - `Core/Data/Map/MapEventBaseSO.cs` — `EventType` enum, `EventChoiceData` 중첩 클래스, ScriptableObject
  - `Core/Interface/IMapLogic.cs` — CurrentNode, GetReachableNodes(), MoveToNode(), OnNodeChanged 이벤트
  - `Logic/Map/MapManager.cs` — IMapLogic 구현체. Initialize(MapData), 이동 유효성 검사, 이벤트 발행
  - `Logic/Map/MapGenerator.cs` — 컬럼 기반 절차적 맵 생성. PlaceNodes(컬럼별 랜덤 행), ConnectColumns(고립 방지)
  - `Presentation/UI/MapView.cs` — UI Toolkit 기반. 노드 버튼 동적 생성, reachable/current CSS 클래스 토글
- **향후 연결 포인트**:
  - `MapSetupManager` (Presentation/System) 추가 예정 — ShipSetupManager 패턴으로 조립
  - `MapScreen.uxml` / `MapScreen.uss` 추가 예정
  - `MapView.GetAllNodes()` 구현 시 `Initialize(IMapLogic, MapData)` 시그니처 변경 또는 `IMapData` 인터페이스 고려

---

### GameMainUI + 자원 시스템 + 맵 오버레이 추가
- **신규 파일**:
  - `Core/Data/SpaceShip/ResourceData.cs` — Fuel, Missiles, Drones int 필드 (Serializable)
  - `Core/Interface/IResourceManager.cs` — 자원 조회/소비/추가 + 이벤트 인터페이스
  - `Core/Interface/ICombatManager.cs` — IsInCombat, OnCombatStateChanged 인터페이스
  - `Logic/System/ResourceManager.cs` — IResourceManager 순수 C# 구현체. Initialize(ResourceData)
  - `Logic/System/CombatManager.cs` — ICombatManager 순수 C# 구현체. SetCombatState(bool)
  - `Presentation/UI/GameMainUIView.cs` — UI Toolkit HUD. JUMP/업그레이드/설정 버튼 + FUEL/MSL/DRN 자원 표시. JUMP 조건: !IsInCombat && PilotRoom.IsManned && Fuel >= 1
  - `Presentation/UI/Styles/GameMainUI.uss` — GameMain 패널 스타일 (자원 행 + 버튼 행)
  - `Presentation/UI/Styles/MapUI.uss` — 맵 오버레이/컨테이너/노드/취소버튼 스타일
- **수정 파일**:
  - `ShipSaveData` — `ResourceData Resources` 필드 추가
  - `DefaultSheepMaker` — 기본 자원값 설정 (Fuel=3, Missiles=8, Drones=2)
  - `GameHUD.uxml` — GameMainPanel(상단 중앙) + MapOverlay(전체화면 모달) 추가. MapUI.uss 참조 추가
  - `MapView.cs` — Initialize(IMapLogic, MapData) 시그니처 변경. Show()/Hide() 추가. GetAllNodes() 구현. reachable 외 SetEnabled(false). OnNodeJumped 이벤트 추가
  - `ShipSetupManager` — ResourceManager/CombatManager 생성, 맵 생성(MapGenerator)/초기화(MapManager), MapView 초기화, GameMainUIView.Initialize() 호출

**JUMP 흐름**: JUMP 클릭 → MapView.Show() → 노드 클릭 → MoveToNode() + OnNodeJumped 이벤트 → 연료 -1 → MapView.Hide()

---

### 저장 기능 구현
- **데이터 동기화 현황**: 대부분의 Logic 클래스(CrewLogic, WeaponLogic, BaseRoomLogic, ShieldManager, DoorLogic, TileLogic, MapManager)는 자신의 Data 객체를 직접 mutate → 별도 동기화 불필요. MapManager._mapData는 GameSessionManager.CurrentGameData.Map과 동일 객체 참조이므로 MoveToNode() 결과도 자동 반영.
- **수정 파일**:
  - `Logic/System/ResourceManager` — `Initialize()`에서 `_data` 참조 보관. TryConsume*/Add* 호출 시 `_data` 필드도 동기화 (기존에는 내부 int만 변경하고 원본 ResourceData는 건드리지 않아 저장 누락)
  - `Presentation/UI/GameMainUIView` — `OnNodeJumped()`에서 연료 소비 후 `GameSessionManager.Instance.SaveGame()` 호출 (노드 점프 시 자동저장)
  - `Presentation/System/UnityTimeProvider` — `Ctrl+S` 단축키로 수동저장 추가

---

### 스페이스바 일시정지 기능 추가
- **수정 파일**:
  - `UnityTimeProvider` — `Input.GetKeyDown(KeyCode.Space)` 감지 → `_isPaused` 토글. 일시정지 중 `AdvanceTime()` 호출 중단. `UIDocument`에서 `PauseOverlay` 캐시 → `DisplayStyle.Flex/None` 전환
  - `GameHUD.uxml` — `PauseOverlay` VisualElement 추가 (MapOverlay 형제, "PAUSED" + 재개 힌트 라벨)
  - `MapUI.uss` — `.pause-overlay`, `.pause-label`, `.pause-hint` 스타일 추가 (display:none 초기값)

---

## 현재 미구현 / TODO (git status 기준)
- `WeaponView.Bind()` — 내용 비어있음
- 승무원 WorkingState — 실제 버프 로직 없음 (주석 처리)
- `SimulateOxygenDiffusion()` — 구현되어 있으나 주석 처리됨
- 씬 전환 (`SceneManager.LoadScene`) — 주석 처리됨
- 발사체 생성 로직 — `WeaponLogic.TryFire()` 내 주석으로 TODO
- `ShieldView` — 실드 수·충전 게이지 시각화 미완 (0↔1 on/off만 구현)

---

## 변경 이력

### 방 이동 타일 배정 로직 개선
- **수정 파일**:
  - `LogicCommandManager` — `SetAllCrews()`, `OrderMoveToRoom(IRoomLogic)` 추가. 방의 타일을 순서대로 검사해 비어 있는 첫 번째 타일로 배정. 방이 가득 차면 이동 불가
  - `MouseInputManager` — 우클릭 시 `OrderMoveToRoom()` 호출로 변경 (기존 `tileCoords[0]` 하드코딩 제거)
  - `ShipSetupManager` — `commandManager.SetAllCrews(shipAPI.GetAllCrews())` 호출 추가

### 실드 시스템 프레임워크 추가 (commit: c8cd749)
- **신규 파일**:
  - `Core/Data/SpaceShip/ShieldData.cs` — `ChargeGauge`, `CurrentShieldCount` 직렬화 데이터
  - `Core/Interface/IShieldLogic.cs` — 실드 로직 인터페이스
  - `Logic/System/ShieldManager.cs` — `IShieldLogic` + `ITickable`. 75틱/1실드 충전, Manned 1.2× 보너스, `TryAbsorbDamage()`
  - `Presentation/Views/ShieldView.cs` — 실드 0↔1 경계에서 GameObject on/off
  - `Assets/Sprite/ShieldImage.png` — 실드 스프라이트 이미지
- **수정 파일**:
  - `ShipSaveData` — `Shield = new ShieldData()` 필드 추가
  - `IShipAPI` — `GetShieldLogic()` 메서드 추가
  - `SpaceShipManager` — `_shieldLogic` 보유, `SetShieldLogic()` / `GetShieldLogic()` 구현
  - `GridBuilder` — `RebuildShield()` 추가 (Shield 방 탐색 → `ShieldManager` 생성·초기화)
  - `ShipSetupManager` — `shieldManager` 취득, `simCore`에 `ITickable`로 등록, `spaceShipView.BindShield()` 호출
  - `SpaceShipView` — `SimulationCore` 프로퍼티, `BindShield(IShieldLogic)` 메서드 추가

### 승무원 시스템 UI 추가 및 통합 GameHUD (commit: af006e3)
- **신규 파일**:
  - `Presentation/UI/UIDocuments/GameHUD.uxml` — 통합 HUD UXML. 승무원(상단 좌) + 전력(하단 좌) + 무기(하단 중앙)
  - `Presentation/UI/Styles/GameHUD.uss` — HUD 레이아웃 전담 USS
- **수정 파일**:
  - `CrewSystemUIView` — 초상화+이름+체력바 세로 레이아웃(`crew-info-column`), 좌상단 배치
  - `CrewSystemUI.uss` — `crew-info-column` 추가, hover/selected/dead 스타일 정비
  - `CrewData` — `CrewName` 필드 추가
  - `LogicCommandManager.SelectCrew()` — `clickedByUI` 파라미터 추가 → `OnCrewUIClicked` 이벤트로 UI·게임월드 클릭 충돌 방지
  - `MouseInputManager` — `OnCrewUIClicked` 구독 → UI 클릭 시 인게임 클릭 무시
  - `ShipSetupManager` — `SortRoomsForUI()` 추가 (Shield→Engine→Medical→Oxygen→Weapon 순, `MaxPowerCapacity=0` 방 제외), CrewSystemUIView 초기화 추가
