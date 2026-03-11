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
- `ShipSaveData` — JSON 직렬화 루트. `TileData`, `RoomData`, `DoorData`, `CrewData`, `WeaponData` 목록 보유
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
| `IShipAPI` | `IGridMap` 상속 + `GetAllCrews()`, `GetAllWeapons()` |
| `IGridMap` | `GetAllTiles/Rooms/Doors()`, `GetConnectedNeighbors()`, `GetDoorBetween()`, `GetRoomAt()`, `GetTileAt()` |
| `IRoomLogic` | RoomID, Data, AverageOxygen, CurrentPower, MaxPowerCapacity, IsManned, `ChangePower()`, `ChangeWorkingCrewCount()`, `OnPowerChanged`, `OnOxygenChanged`, `OnMannedStatusChanged` |
| `IWeaponLogic` | Data, BaseData, IsPowered, IsReadyToFire, ChargeProgress, `SetPower()`, `SetAutoFire()`, `SetTarget()`, `TryFire()`, `SetBaseData()`, `SetChargeMultiplier()`, `OnChargeUpdated`, `OnPowerStateChanged`, `OnFired` |
| `ICrewLogic` | CrewID, Data, CurrentHealth, MaxHealth, `CommandMoveTo()`, `TakeDamage()`, `OnPositionChanged`, `OnHealthChanged`, `OnDied` |
| `IDoorLogic` | DoorID, IsOpen, `SetDoorState()`, `ToggleDoorManual()`, `OnDoorStateChanged` |
| `ITileLogic` | TileCoord, OxygenLevel, BreachLevel |
| `IPowerSystem` | MaxReactorPower, AvailableReactorPower, `TryAddPowerToRoom()`, `TryRemovePowerFromRoom()`, `OnReactorPowerChanged` |
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
| `SpaceShipManager` | `IShipAPI` 구현체. 타일/방/문/승무원/무기 목록 보유. 승무원 사망 시 명부 삭제 |
| `GridBuilder` | `ShipSaveData` → Logic 객체 일괄 조립 (`Rebuild()` 반환값: `IShipAPI`) |
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
| `LogicCommandManager` | 승무원 선택/이동 명령 브릿지. `SelectCrew(crew, clickedByUI)`, `OrderMoveCommand()`, `DeselectCrew()`. `OnSelectionChanged`, `OnCrewUIClicked` 이벤트 제공 |
| `AStarPathfinder` | static. 맨해튼 거리 휴리스틱 A*. 반환값: `Queue<TileCoord>` |

---

## Presentation 계층 (`Assets/Scripts/Presentation/`)

### System

| 클래스 | 역할 |
|---|---|
| `Singleton<T>` | DontDestroyOnLoad 싱글톤 기반 클래스 |
| `GameSessionManager` | Singleton. `Awake()`에서 JSON 로드 → `ShipSaveData` 보유, `HandOverData()` 제공 |
| `AssetCatalogManager` | Singleton. SO 카탈로그 (ShipHull / Crew / Weapon) List→Dictionary 변환 |
| `ShipSetupManager` | 게임 시작 시 전체 조립. GridBuilder → SpaceShipView.Bind() → PowerManager/WeaponManager 초기화 → SimulationCore 등록 → UI 초기화 |
| `MouseInputManager` | 좌클릭(승무원 선택 / 문 토글), 우클릭(이동 명령), Hover(방 하이라이트). `OnCrewUIClicked` 이벤트로 UI 클릭과 인게임 클릭 충돌 방지. `RegisterCrewViews()`로 CrewView 등록. `CommandManager` 프로퍼티로 외부 공유 |
| `UnityTimeProvider` | `MonoBehaviour.Update()` → `SimulationCore.AdvanceTime(Time.deltaTime)` |

### Views

| 클래스 | 역할 |
|---|---|
| `SpaceShipView` | Tile/Room/DoorView 바인딩 허브. `GetWorldPosition(x, y)` 제공 |
| `RoomView` | `OnOxygenChanged` 구독 → 산소 < 50f 시 빨간 오버레이 (Alpha 최대 0.5) |
| `TileView` | `ITileLogic` 바인딩. 에디터 Gizmos로 좌표 표시 |
| `DoorView` | `OnDoorStateChanged` 구독 → 스프라이트 교체 (OpenedDoor / ClosedDoor) |
| `CrewView` | `OnPositionChanged` → 방향 회전 + MoveTowards 이동. `OnHealthChanged` → 체력바. `OnDied` → Destroy |
| `WeaponView` | `Bind()` 내용 **미구현** |

### UI (Unity UI Toolkit)

| 클래스/파일 | 역할 |
|---|---|
| `PowerSystemUIView` | 좌클릭=전력 할당, 우클릭=전력 회수. 방 바 동적 생성. `OnReactorPowerChanged`, `OnPowerChanged` 구독 |
| `WeaponSystemUIView` | 무기 슬롯 동적 생성. 장전 게이지(%) 실시간 업데이트. 좌클릭=ON, 우클릭=OFF |
| `CrewSystemUIView` | 승무원 슬롯 동적 생성. 초상화(`CrewBaseSO.DefaultSprite`), 이름, 체력바 표시. 슬롯 클릭=선택/재클릭=해제. `OnSelectionChanged` 구독 → 슬롯 시각 동기화. `OnHealthChanged`, `OnDied` 구독. 30% 이하 체력 시 danger 스타일. 사망 시 dead 스타일 |
| `PowerSystemUI.uxml` | `ReactorBarContainer` + `RoomControlsGroup` |
| `WeaponSystemUI.uxml` | `overlay` (클릭 무시) + `WeaponPanel` |
| `CrewSystemUI.uxml` | `overlay` (클릭 무시) + `CrewPanel` (하단 고정) |
| `PowerSystemUI.uss` | reactor-bar, room-bar, room-button 스타일 |
| `WeaponSystemUI.uss` | weapon-slot, weapon-label, weapon-charge-bg/fill 스타일 |
| `CrewSystemUI.uss` | crew-panel, crew-slot, crew-slot.selected(파란 테두리), crew-slot.dead(반투명 빨강), crew-portrait, crew-name, crew-health-bg/fill, crew-health-fill.danger 스타일 |

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

---

## 현재 미구현 / TODO (git status 기준)
- `WeaponView.Bind()` — 내용 비어있음
- 승무원 WorkingState — 실제 버프 로직 없음 (주석 처리)
- `SimulateOxygenDiffusion()` — 구현되어 있으나 주석 처리됨
- 씬 전환 (`SceneManager.LoadScene`) — 주석 처리됨
- 발사체 생성 로직 — `WeaponLogic.TryFire()` 내 주석으로 TODO

## 최근 추가 내역
### 승무원 시스템 UI (CrewSystemUIView)
- `CrewSystemUIView` 신규 추가: 하단 고정 승무원 패널. 슬롯 클릭으로 승무원 선택/해제
- `CrewData`에 `CrewName` 필드 추가 (LogicCommandManager 로그에서 확인)
- `LogicCommandManager.SelectCrew()` 에 `clickedByUI` 파라미터 추가 → `OnCrewUIClicked` 이벤트로 UI·게임월드 클릭 충돌 방지
- `MouseInputManager`: `OnCrewUIClicked` 구독 → `_clickCrewUI` 플래그로 UI 클릭 시 인게임 클릭 무시
- `ShipSetupManager`: CrewSystemUIView 초기화 추가, 방 UI 순서를 `SortRoomsForUI()`로 재정렬 (Shield→Engine→Medical→Oxygen→Weapon), `MaxPowerCapacity=0`인 방 제외
