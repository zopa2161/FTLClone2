# FTLClone2 프로젝트 구조 분석

## 아키텍처 (3계층 분리)

```
Core (순수 C# 데이터/인터페이스)
  └─ Logic (순수 C# 로직, MonoBehaviour 없음)
       └─ Presentation (Unity MonoBehaviour + UI Toolkit)
```

---

## Core 계층 (`Assets/Scripts/Core/`)

### Data/SpaceShip

| 파일 | 설명 |
|---|---|
| `ShipSaveData` | JSON 직렬화 루트. Version, HullID, MaxReactorPower, MaxHullHealth(20), CurrentHullHealth(20), Shield, Resources, MaxWeaponSlots, EquippedWeapons + Tiles/Rooms/Doors/Crews 플랫 리스트 |
| `TileCoord` | (X, Y) 구조체. 커스텀 `==` / `GetHashCode` 구현. 딕셔너리 키로 사용 |
| `TileData` | 타일 영구 데이터. OxygenLevel(100f), BreachLevel, ConnectedNeighborCoords 목록 |
| `RoomData` | 방 영구 데이터. RoomID, RoomType, MaxPower, CurrentAllocatedPower, DestructionLevel, IsManned, IsMannable, ConsoleDirection, TileCoords |
| `DoorData` | 문 영구 데이터. DoorID, TileA, TileB, IsOpen, IsForcedOpen |
| `CrewData` | 승무원 영구 데이터. CrewID, BaseDataID, CrewName, CurrentX/Y, MaxHealth(100f), CurrentHealth(100f) |
| `ShieldData` | 실드 상태. ChargeGauge(0~1), CurrentShieldCount(0~4) |
| `ResourceData` | 소모 자원. Fuel(3), Missiles(8), Drones(2), Scrap(0) |
| `RoomTypeString` | 방 타입 문자열 상수. Pilot, Oxygen, Empty, Engine, Weapon, Shield, Door, Vision, MedBay |

### Data/Weapon

| 파일 | 설명 |
|---|---|
| `WeaponData` | 무기 런타임 상태. WeaponID, IsPowered, CurrentChargeTimer, IsAutoFire, TargetRoomID(-1=미지정) |
| `WeaponBaseSO` | ScriptableObject 무기 정의. WeaponID, Name, Description, WeaponType, Damage, ProjectileCount, RequiredPower, BaseCooldown |

### Data/Crews

| 파일 | 설명 |
|---|---|
| `CrewBaseSO` | ScriptableObject 승무원 정의. CrewDataID, DefaultSprite |

### Data/Map

| 파일 | 설명 |
|---|---|
| `MapData` | 섹터 맵 데이터. 노드 목록, CurrentNodeID, 맵 크기, 컬럼 수 |
| `NodeData` | 노드 데이터. NodeID, NodeType(Start/Normal/Store/Elite/Exit), 정규화 좌표(0~1), 연결 노드 ID 목록, IsVisited, EventID |
| `MapEventBaseSO` | [DEPRECATED] EventSO로 대체됨 |

### Data/Event

| 파일 | 설명 |
|---|---|
| `SubEventBaseSO` | 모든 세부 이벤트의 추상 기반 SO. `SubEventID(string)` — DB 조회 + 세이브/로드용. `IsFinished(bool)` — true이면 이벤트 전체 종료, 점프 가능 상태로 복귀 |
| `DialogSubEventSO` | 대화 세부 이벤트. SpeakerName, DialogText, List\<DialogChoice\>(ChoiceText + NextEvent 링크) |
| `CombatSubEventSO` | 전투 세부 이벤트. EnemyShipData(ShipSaveData), NextEvent 링크 |
| `RewardSubEventSO` | 보상 세부 이벤트. List\<RewardEntry\>(RewardType + Amount + WeaponID), NextEvent 링크 |
| `EventSO` | 최상위 이벤트 SO. EventID, Title, StartEvent(SubEventBaseSO) 진입 링크. NodeData.EventID로 조회 |
| `EventDatabaseSO` | **에디터 전용** 컨테이너 SO. Events/DialogEvents/CombatEvents/RewardEvents 목록 보유. Add/Remove + `GetSubEvent(string)` 메서드. `Assets/Data/EventDatabase.asset` 1개 운용 |
| `EventSaveData` | 이벤트 진행 상황 직렬화. IsEventActive, ActiveEventID, ActiveSubEventID. GameSaveData.Event 필드로 보유 |
| `RewardType` (enum) | Scrap, Fuel, Missiles, Drones, Weapon, MaxReactorPower |

### Data/Storage

| 파일 | 설명 |
|---|---|
| `GameSaveData` | 최상위 직렬화 루트. ShipSaveData + MapData + EventSaveData 통합 보유 |
| `SaveLoadManager` | 정적 유틸. `Save<T>()` / `Load<T>()` — JsonUtility + Application.persistentDataPath |
| `ShipHullEntry` | HullID → 프리팹 GameObject 매핑 구조체 |
| `CrewSOEntry` | CrewDataID → CrewBaseSO 매핑 구조체 |
| `WeaponSOEntry` | WeaponID → WeaponBaseSO 매핑 클래스 |

### Interface

| 인터페이스 | 주요 멤버 |
|---|---|
| `IEventLogic` | `IsEventActive`, `CurrentEvent`, `CurrentSubEvent` + `OnSubEventChanged`, `OnEventFinished` 이벤트 + `StartEvent()`, `CompleteDialogSubEvent(int)`, `CompleteCombatSubEvent()`, `CompleteRewardSubEvent()`, `GetSaveData()` |
| `IShipAPI` | `IGridMap` 상속 + `GetAllCrews()`, `GetAllWeapons()`, `GetShieldLogic()`, `MaxHullHealth`, `CurrentHullHealth` |
| `IGridMap` | `GetAllTiles/Rooms/Doors()`, `GetConnectedNeighbors()`, `GetDoorBetween()`, `GetRoomAt()`, `GetTileAt()` |
| `IRoomLogic` | RoomID, Data, AverageOxygen, CurrentPower, MaxPowerCapacity, IsManned + `ChangePower()`, `OnPowerChanged`, `OnOxygenChanged`, `OnMannedStatusChanged` |
| `ICrewLogic` | CrewID, Data, CurrentHealth, MaxHealth + `CommandMoveTo()`, `TakeDamage()`, `OnPositionChanged`, `OnHealthChanged`, `OnDied` |
| `IDoorLogic` | DoorID, IsOpen + `SetDoorState()`, `ToggleDoorManual()`, `OnDoorStateChanged` |
| `ITileLogic` | TileCoord, OxygenLevel, BreachLevel |
| `IPowerSystem` | MaxReactorPower, AvailableReactorPower + `TryAddPowerToRoom()`, `TryRemovePowerFromRoom()`, `OnReactorPowerChanged` |
| `IWeaponLogic` | Data, BaseData, IsPowered, IsReadyToFire, ChargeProgress(0~1) + `SetPower()`, `SetAutoFire()`, `SetTarget()`, `TryFire()`, `SetBaseData()`, `SetChargeMultiplier()`, `OnChargeUpdated`, `OnPowerStateChanged`, `OnFired` |
| `IShieldLogic` | MaxShields, CurrentShields, ChargeGauge + `TryAbsorbDamage()`, `OnShieldChanged(current, max, chargeGauge)` |
| `IResourceManager` | Fuel, Missiles, Drones, Scrap + 각 `OnXxxChanged` 이벤트, `TryConsume/Add` 메서드 |
| `ICombatManager` | IsInCombat + `OnCombatStateChanged` |
| `IMapLogic` | CurrentNode, `GetReachableNodes()`, `MoveToNode()`, `OnNodeChanged` |
| `ITickable` | `OnTickUpdate()` |

### enums

| 파일 | 값 |
|---|---|
| `MoveDirection` | None, Up, Down, Left, Right |
| `WeaponType` | Laser, Missile, Beam |

---

## Logic 계층 (`Assets/Scripts/Logic/`)

### SpaceShip

| 클래스 | 역할 |
|---|---|
| `SpaceShipManager` | `IShipAPI` 구현체. 타일/방/문/승무원/무기/실드 목록 보유. MaxHullHealth·CurrentHullHealth 프로퍼티. 승무원 사망 시 명부 삭제 |
| `GridBuilder` | `ShipSaveData` → Logic 객체 일괄 조립. `Rebuild()` 반환: `IShipAPI`. 실드·선체체력 초기화 포함 |
| `RoomLogicFactory` | RoomType 문자열 → 구체 Room 클래스 생성 (switch 패턴) |
| `TileLogic` | `ITileLogic` 구현. `Neighbors` 리스트(TileLogic 참조) 보유 |
| `DoorLogic` | `IDoorLogic` + `ITickable`. AUTO_CLOSE_DELAY_TICKS=5 후 자동 닫힘. IsForcedOpen 시 타이머 정지 |
| `CrewLogic` | `ICrewLogic` + `ITickable`. FSM(ICrewState) + A* 경로. 산소 < 50f 시 0.5f/틱 피해 |

#### 방 로직 (`Logic/SpaceShip/Rooms/`)

| 클래스 | 역할 |
|---|---|
| `BaseRoomLogic` | abstract. 전력 변경, 산소 평균, 승무원 근무 공통 처리. `OnRoomTick()` 가상 훅 |
| `OxygenRoomLogic` | 산소 생성량 계산. 전력 1→1, 2→3, 3→6. 고장 시 -1 |
| `PilotRoomLogic` | 현재 생성자만 존재 |
| `EngineRoomLogic` | 현재 생성자만 존재 |
| `WeaponRoomLogic` | 현재 생성자만 존재 |
| `ShieldRoomLogic` | 현재 생성자만 존재 (실드 로직은 ShieldManager 위임) |
| `EmptyRoomLogic` | 특수 기능 없음 |

#### 승무원 FSM (`Logic/SpaceShip/CrewState/`)

| 상태 | 동작 |
|---|---|
| `CrewIdleState` | 대기. 행동 없음 |
| `CrewMovingState` | 3틱/칸 이동. 경로상 문 자동 개방. 도착 시 WorkingState 또는 IdleState 전환 |
| `CrewWorkingState` | 콘솔 방향으로 회전 후 방에 근무 시작 보고. Exit 시 근무 종료 보고 |

### System (`Logic/System/`)

| 클래스 | 역할 |
|---|---|
| `SimulationCore` | ITickable 목록 관리. `AdvanceTime(deltaTime)` → TickRate=0.1f 간격 틱. TimeScale 지원. `UnregisterTickable(s)()` — pending removal 패턴으로 틱 도중 안전 제거 |
| `ShipSimulationManager` | ITickable. BFS 거리맵으로 진공 바람 시뮬레이션. 산소 생성은 OxygenRoomLogic 위임 |
| `PowerManager` | `IPowerSystem` 구현. 원자로 전력 할당/회수. 방별 최대치 초과 방지 |
| `WeaponManager` | 무기 ON/OFF. 전력 초과 시 뒤에서부터 강제 종료. Manned 보너스 1.2x |
| `ShieldManager` | `IShieldLogic` + `ITickable`. 75틱/1실드 충전. Manned 1.2× 보너스. `TryAbsorbDamage()` |
| `ResourceManager` | `IResourceManager` 구현. Fuel/Missiles/Drones/Scrap 추적. 소비·증가 + 이벤트 발행 |
| `CombatManager` | `ICombatManager` 구현. SetCombatState(bool) |
| `LogicCommandManager` | 승무원 선택/이동 명령 브릿지. `SelectCrew(clickedByUI)`, `OrderMoveToRoom()`. `OnSelectionChanged`, `OnCrewUIClicked` 이벤트 |
| `AStarPathfinder` | static. 맨해튼 거리 휴리스틱 A*. 반환: `Queue<TileCoord>` |
| `WeaponLogic` | `IWeaponLogic` + `ITickable`. 0.1f/틱 충전(승수 적용). 자동발사·타겟·전력 관리. 충전/발사 이벤트 |

### Event (`Logic/Event/`)

| 클래스 | 역할 |
|---|---|
| `EventLogicManager` | `IEventLogic` 구현. 이벤트 진행 상태 추적. Dialog/Combat/Reward 서브이벤트 완료 처리. `IsFinished==true` 또는 `NextEvent==null` 시 `OnEventFinished` 발행. Reward 완료 시 `IResourceManager`로 보상 즉시 적용. `Initialize()` → 세이브에서 복원 |
| `EnemyCombatManager` | `IEnemyCombatManager` + `ITickable`. `StartCombat(CombatSubEventSO)` → `GridBuilder`로 적군 Logic 조립 + SimulationCore 등록. 매 틱 적군 체력 체크 → 0 이하 시 `EndCombat()` + `CompleteCombatSubEvent()`. `EndCombat()` → `UnregisterTickables()` |

### Map (`Logic/Map/`)

| 클래스 | 역할 |
|---|---|
| `MapManager` | `IMapLogic` 구현. 노드 이동·방문 처리·이벤트 발행 |
| `MapGenerator` | 컬럼 기반 절차적 맵 생성. 1~3 노드/컬럼, 컬럼 간 연결 보장, Start/Exit 지정 |

---

## Presentation 계층 (`Assets/Scripts/Presentation/`)

### System

| 클래스 | 역할 |
|---|---|
| `Singleton<T>` | DontDestroyOnLoad 싱글톤 기반 클래스 |
| `GameSessionManager` | Singleton. Awake()에서 JSON 로드 → GameSaveData 보유. ShipData/MapData 접근자, SetMapData(), SaveGame() |
| `AssetCatalogManager` | Singleton. SO 카탈로그(ShipHull/Crew/Weapon) List→Dictionary 변환 |
| `ShipSetupManager` | 게임 시작 시 전체 조립. GridBuilder → View 바인딩 → 매니저 초기화 → SimulationCore 등록 → UI 초기화 |
| `MouseInputManager` | 좌클릭(승무원 선택/문 토글/적군 방 타겟 설정), 우클릭(이동 명령), Hover(아군=이동 대상, 적군=무기 조준 하이라이트). 무기 선택 중 적군 방 클릭 → `WeaponManager.SetTargetForSelectedWeapon()`. `OnCrewUIClicked`으로 UI·인게임 충돌 방지 |
| `UnityTimeProvider` | Update() → SimulationCore.AdvanceTime(). Space=일시정지, Ctrl+S=수동저장 |

### Views

| 클래스 | 역할 |
|---|---|
| `SpaceShipView` | Tile/Room/DoorView 바인딩 허브. `GetWorldPosition(x,y)`, `BindShield()`, `SimulationCore` 프로퍼티 |
| `TileView` | ITileLogic 바인딩. 에디터 Gizmos로 좌표 표시 |
| `RoomView` | `OnOxygenChanged` 구독 → 산소 < 50f 시 빨간 오버레이 (Alpha 최대 0.5). `Faction`(Player/Enemy) 프로퍼티, `SetHighlight()`, `SetTargeted()` — 무기 조준·호버 하이라이트 |
| `DoorView` | `OnDoorStateChanged` 구독 → 스프라이트 교체 (OpenedDoor/ClosedDoor) |
| `CrewView` | `OnPositionChanged` → 방향 회전 + MoveTowards 이동. `OnHealthChanged` → 체력바. `OnDied` → Destroy |
| `WeaponView` | Bind() **미구현** |
| `ShieldView` | `OnShieldChanged` 구독 → 실드 0↔1+ 경계에서 GameObject on/off |

### UI (Unity UI Toolkit)

| 클래스 | 역할 |
|---|---|
| `PowerSystemUIView` | 원자로 바(MaxPower-3개) + 방별 전력 컬럼 동적 생성. 좌클릭=할당, 우클릭=회수 |
| `WeaponSystemUIView` | 무기 슬롯 동적 생성. 장전 게이지(%) 실시간. 좌클릭=ON, 우클릭=OFF |
| `CrewSystemUIView` | 승무원 슬롯 동적 생성. 초상화+이름+체력바. 클릭=선택/재클릭=해제. 30% 이하=danger, 사망=dead |
| `ShieldSystemUIView` | 실드 원형 인디케이터 4개 + 충전 게이지 바. `OnShieldChanged` 구독 |
| `GameMainUIView` | JUMP/업그레이드/설정 버튼 + FUEL/MSL/DRN/SCR 자원 레이블. JUMP 조건: !전투 && 파일럿 탑승 && Fuel≥1 |
| `EventDialogUIManager` | `IEventLogic.OnSubEventChanged` 구독. Dialog 시: 반투명 파란 오버레이 + 텍스트 + 선택지 버튼. Reward 시: 보상 목록 + Accept 버튼. `OnEventFinished` 시 오버레이 숨김 |
| `HullHealthUIView` | 선체 체력 바 동적 생성(MaxHullHealth개). 1바=1HP. 초록(>66%), 노랑(>33%), 빨강(≤33%) |
| `MapView` | 섹터 맵 오버레이. 노드 버튼 동적 생성(절대 위치). reachable/current CSS 클래스 토글. `OnNodeJumped` 이벤트 |

### Test/Utility

| 클래스 | 역할 |
|---|---|
| `DefaultSheepMaker` | ContextMenu로 DefaultShipData.json 생성. 기본 순양함(17방, 26문, 3승무원, 1무기) |

---

## UI 파일 (`Assets/Scripts/Presentation/UI/`)

### UXML (레이아웃)

| 파일 | 설명 |
|---|---|
| `GameHUD.uxml` | **통합 HUD** (메인). 좌측컬럼(체력바+SCR / 실드+리소스 / 승무원) + 버튼(JUMP/▲/⚙) + 하단(전력/무기) + 일시정지 오버레이 + 맵 오버레이 + **이벤트 오버레이(EventOverlay)** |
| `PowerSystemUI.uxml` | ReactorBarContainer + RoomControlsGroup |
| `WeaponSystemUI.uxml` | WeaponPanel 컨테이너 |
| `CrewSystemUI.uxml` | CrewPanel 컨테이너 |

### USS (스타일)

| 파일 | 주요 클래스 |
|---|---|
| `GameHUD.uss` | `.hud-overlay`, `.hud-top-row`, `.hud-left-column`, `.hud-health-scrap-row`, `.hud-shield-resource-row`, `.hud-spacer`, `.hud-bottom-row`, `.hud-bottom-center`, `.hud-bottom-right`(180px) |
| `HullHealthUI.uss` | `.hull-health-panel`, `.hull-bar`, `.hull-bar-green/yellow/red`, `.scrap-panel`, `.scrap-icon` |
| `PowerSystemUI.uss` | `.reactor-bar(.filled)`, `.room-bar`, `.room-button`, `.room-bar-container`(column-reverse) |
| `WeaponSystemUI.uss` | `.weapon-slot`(140×60px), `.weapon-charge-fill(.ready/.unpowered)`, `.weapon-slot.off` |
| `CrewSystemUI.uss` | `.crew-panel`, `.crew-slot(.selected/.dead)`, `.crew-portrait`(56×56), `.crew-info-column`, `.crew-health-fill(.danger)` |
| `ShieldSystemUI.uss` | `.shield-panel`, `.shield-circles-row`, `.shield-circle(.active)`, `.shield-charge-fill` |
| `GameMainUI.uss` | `.game-main-panel`, `.game-main-buttons-row`, `.jump-button`, `.game-main-button` |
| `MapUI.uss` | `.map-overlay`, `.map-container`, `.node-button(.reachable/.current/:disabled)`, `.map-cancel-button`, `.pause-overlay`, `.pause-label`, `.pause-hint` |
| `EventUI.uss` | `.event-overlay`, `.event-panel`, `.event-title`, `.event-dialog-text`, `.event-choices-container`, `.event-choice-button`, `.event-reward-container`, `.event-reward-entry`, `.event-accept-button` |

---

## 데이터 흐름 (시작 ~ 실행)

```
1. DefaultSheepMaker (ContextMenu) → DefaultShipData.json 생성
2. GameSessionManager.Awake() → SaveLoadManager.Load<GameSaveData>() → ShipData + MapData
3. ShipSetupManager.BeginSetup()
   ├─ AssetCatalogManager → Hull 프리팹 Instantiate → SpaceShipView
   ├─ GridBuilder.Rebuild(savedData) → IShipAPI (SpaceShipManager)
   │   └─ SetHullHealth(Max, Current) 포함
   ├─ SpaceShipView.Bind() → TileView/RoomView/DoorView 이벤트 구독
   ├─ SpaceShipView.BindShield(shieldManager)
   ├─ SetupCrews() → CrewView.Bind()
   ├─ SetupWeapons() → WeaponView.Bind()
   ├─ WeaponManager.Initialize()
   ├─ PowerManager.Initialize()
   ├─ ResourceManager.Initialize(Resources)
   ├─ CombatManager 생성
   ├─ SimulationCore.RegisterTickables(rooms, crews, doors, weapons, shipSim, shieldManager)
   ├─ UnityTimeProvider.Initialize(simCore)
   ├─ PowerSystemUIView.Initialize()
   ├─ WeaponSystemUIView.Initialize()
   ├─ CrewSystemUIView.Initialize()
   ├─ ShieldSystemUIView.Initialize()
   ├─ GameMainUIView.Initialize(resourceManager, combatManager, pilotRoom, mapView)
   ├─ HullHealthUIView.Initialize(shipAPI)
   └─ MapView.Initialize(mapManager, mapData)
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
| 선체 최대 체력 | 20 (`ShipSaveData.MaxHullHealth`) |
| 체력바 색상 임계값 | >66%=초록, >33%=노랑, ≤33%=빨강 |

---

## 현재 미구현 / TODO

- `WeaponView.Bind()` — 내용 비어있음
- 승무원 WorkingState — 실제 버프 로직 없음 (주석 처리)
- `SimulateOxygenDiffusion()` — 구현되어 있으나 주석 처리됨
- 씬 전환 (`SceneManager.LoadScene`) — 주석 처리됨
- 발사체 시각 이펙트 — 피해 계산 자체는 `CombatResolver.ApplyDamage()`로 완전 구현됨. 발사체 오브젝트 생성/애니메이션만 미구현


---

## 변경 이력

### 전투 로직 구현 (CombatResolver) + 무기 타겟 선택 UI
- `IShipAPI` — `void TakeDamage(int damage)` 추가
- `SpaceShipManager` — `TakeDamage` 구현 (`System.Math.Max(0, HP-damage)` → `SetHullHealth` 경유)
- `WeaponLogic` — 오토파이어 조건에서 `IsAutoFire` 제거 → 타겟(`TargetRoomID != -1`)만 있으면 장전 완료 시 자동 발사
- `CombatResolver.cs` — **신규** (`Logic/System/`). 아군 `OnFired`→적 피해(`ApplyDamage`), `TickEnemyWeapons()`→적군 랜덤 공격. 타입별 실드: Laser=발사체마다 `TryAbsorbDamage()`, Missile=실드 무시. 피해는 `ProjectileCount`회 반복 적용
- `EnemyCombatManager` — `StartCombat`에서 적군 무기 전력 전부 자동 ON. `SetCombatResolver()` 추가. `OnTickUpdate`에서 `TickEnemyWeapons()` 호출
- `ShipSetupManager` — `_playerShipAPI`, `_weaponManager` 필드화. 전투 시작 시 적군 무기 BaseData SO 주입 후 `CombatResolver` 생성/바인딩
- `RoomView` — `Faction`(Player/Enemy) 프로퍼티, `SetHighlight()`, `SetTargeted()` 추가
- `MouseInputManager` — 무기 선택 중 좌클릭 시 적군 방(`Faction.Enemy`) 타겟 설정. `SetWeaponTarget()` → `WeaponManager.SetTargetForSelectedWeapon()`. 호버: 승무원 선택=아군 방, 무기 선택=적군 방 하이라이트

### 적 우주선 HP 바 (우측 상단)
- `IShipAPI` — `event Action<int,int> OnHullHealthChanged` 추가 (current, max)
- `SpaceShipManager` — `OnHullHealthChanged` 이벤트 구현, `SetHullHealth()` 내에서 발화
- `EnemyHullHealthUIView.cs` — **신규** (`Presentation/UI/`). `ShowEnemy(IShipAPI)` / `HideEnemy()`. 전투 시작 시 표시, 종료 시 숨김. 이벤트 구독 기반 실시간 갱신
- `GameHUD.uxml` — `GameMainPanel`을 `hud-right-column`으로 묶고 `EnemyHullHealthPanel` 하위 추가 (기본 `display:none`)
- `HullHealthUI.uss` — `.enemy-hull-health-panel` 스타일 추가 (빨간 테두리)
- `GameHUD.uss` — `.hud-right-column` 스타일 추가
- `ShipSetupManager` — `SetupEnemyShipView`에서 `EnemyHullHealthUIView.ShowEnemy()`, `TeardownEnemyShipView`에서 `HideEnemy()` 호출

### 이벤트 UI & 적군 전투 시뮬레이션
- `SimulationCore` — `UnregisterTickable(s)()` 추가. pending removal 패턴으로 틱 도중 안전 제거
- `IEnemyCombatManager.cs` — **신규**. `EnemyShipAPI`, `StartCombat()`, `EndCombat()`
- `EnemyCombatManager.cs` — **신규** (`Logic/Event/`). GridBuilder로 적군 Logic 조립, simCore 등록, 체력 체크 → `CompleteCombatSubEvent()` 자동 호출
- `EventUI.uss` — **신규**. 반투명 파란 오버레이 스타일
- `EventDialogUIManager.cs` — **신규** (`Presentation/UI/`). Dialog: 선택지 버튼 동적 생성. Reward: 보상 목록 + Accept 버튼. `OnEventFinished` 시 숨김
- `GameHUD.uxml` — `EventOverlay` 추가 + `EventUI.uss` import
- `ShipSetupManager` — `_simCore` 필드화, `EnemyCombatManager` 생성/구독, `EventDialogUIManager.Initialize()` 추가
- `Plans/event-ui-and-combat-simulation.md` — 구현 계획 문서 저장

### 이벤트 진행 상황 저장 & EventLogicManager
- `SubEventBaseSO` — `SubEventID(string)` 필드 추가 (세이브/로드 및 DB 조회용 고유 ID)
- `EventDatabaseSO` — `GetSubEvent(string subEventID)` 메서드 추가 (Dialog/Combat/Reward 목록 순차 탐색)
- `EventSaveData.cs` — **신규**. `IsEventActive`, `ActiveEventID`, `ActiveSubEventID` 직렬화
- `GameSaveData` — `EventSaveData Event` 필드 추가
- `IEventLogic.cs` — **신규**. `StartEvent()`, `CompleteDialogSubEvent(int)`, `CompleteCombatSubEvent()`, `CompleteRewardSubEvent()`, `GetSaveData()`, `OnSubEventChanged`, `OnEventFinished`
- `EventLogicManager.cs` — **신규** (`Logic/Event/`). `IEventLogic` 구현. `Initialize()` → 세이브 복원. `ApplyRewards()` → `IResourceManager` 위임 (MaxReactorPower/Weapon은 TODO)
- `AssetCatalogManager` — `EventDatabaseSO EventDatabase` 필드 + `GetSubEvent(string)` 메서드 추가
- `ShipSetupManager` — `EventLogicManager` 생성/초기화, `CombatManager` 연동 (OnSubEventChanged → SetCombatState)

### 이벤트 에디터 윈도우 + 데이터베이스 SO
- `Core/Data/Event/EventDatabaseSO.cs` — 에디터 전용 컨테이너 SO. 4종 목록 + Add/Remove
- `Assets/Editor/EventEditorWindow.cs` — `Window/FTL/Event Editor` 메뉴. 탭(Event/Dialog/Combat/Reward)+리스트+CachedEditor 레이아웃
  - Add: `Assets/Data/Events/{Type}/` 에 Asset 생성 후 DB 등록
  - Delete: DB에서 제거 + Asset 파일 삭제
  - DB 없을 때 "생성하기" 버튼으로 자동 생성

### 이벤트 시스템 데이터 구조 신규
- `Core/Data/Event/SubEventBaseSO.cs` — abstract SO 기반. `IsFinished` 공통 플래그
- `Core/Data/Event/DialogSubEventSO.cs` — 대화+선택지. `List<DialogChoice>(ChoiceText, NextEvent)`
- `Core/Data/Event/CombatSubEventSO.cs` — 적군 `ShipSaveData` + `NextEvent` 링크
- `Core/Data/Event/RewardSubEventSO.cs` — `List<RewardEntry>(RewardType, Amount, WeaponID)` + `NextEvent`
- `Core/Data/Event/EventSO.cs` — 최상위 이벤트. `EventID`, `Title`, `StartEvent`
- `RewardType` enum — Scrap, Fuel, Missiles, Drones, Weapon, MaxReactorPower
- `AssetCatalogManager` — `EventSOList` + `GetEvent(string)` 추가
- `Assets/Editor/EventSOEditor.cs` — `[CustomEditor(EventSO)]` 체인 트리 시각화
- `MapEventBaseSO.cs` — DEPRECATED (EventSO로 대체)

### UI 개편 — 체력 바, 스크랩, 실드/리소스 레이아웃 재배치
- `ShipSaveData` — `MaxHullHealth=20`, `CurrentHullHealth=20` 추가
- `ResourceData` — `Scrap=0` 추가
- `IResourceManager` — Scrap 프로퍼티, `OnScrapChanged`, `TryConsumeScrap()`, `AddScrap()` 추가
- `IShipAPI` — `MaxHullHealth`, `CurrentHullHealth` 프로퍼티 추가
- `SpaceShipManager` — hull health 프로퍼티 + `SetHullHealth()` 추가
- `GridBuilder` — `Rebuild()` 내 `SetHullHealth()` 호출 추가
- `HullHealthUIView.cs` — 신규. 바 개수=MaxHullHealth, 1바=1HP, 색상 자동 결정
- `HullHealthUI.uss` — 신규
- `GameHUD.uxml` — 좌측 컬럼 구조 재편(체력바+SCR / 실드+리소스 / 승무원). 버튼만 남긴 GameMainPanel
- `GameHUD.uss` — `.hud-left-column`, `.hud-health-scrap-row`, `.hud-shield-resource-row` 추가
- `GameMainUIView` — ScrapLabel 연동 추가

### 스페이스바 일시정지 + Ctrl+S 수동저장
- `UnityTimeProvider` — Space=일시정지 토글, Ctrl+S=저장. PauseOverlay DisplayStyle 전환
- `GameHUD.uxml` — PauseOverlay 추가
- `MapUI.uss` — `.pause-overlay`, `.pause-label`, `.pause-hint` 추가

### GameMainUI + 자원 시스템 + 맵 오버레이
- `ResourceData`, `IResourceManager`, `ResourceManager`, `CombatManager` 신규
- `GameMainUIView`, `GameMainUI.uss`, `MapUI.uss` 신규
- `MapView` — Show()/Hide(), OnNodeJumped 이벤트, 노드 이동 구현
- `ShipSetupManager` — ResourceManager/CombatManager/MapManager/MapView/GameMainUIView 초기화 추가

### 맵 시스템 스켈레톤
- `NodeData`, `MapData`, `MapEventBaseSO` 신규
- `IMapLogic`, `MapManager`, `MapGenerator` 신규
- `GameSaveData` — ShipSaveData + MapData 통합 루트

### 실드 시스템
- `ShieldData`, `IShieldLogic`, `ShieldManager`, `ShieldView` 신규
- `ShipSaveData` — Shield 필드 추가
- `IShipAPI` — `GetShieldLogic()` 추가
- `SpaceShipManager` — ShieldManager 보유 + SetShieldLogic()

### 승무원 시스템 UI + 통합 GameHUD
- `GameHUD.uxml`, `GameHUD.uss` 신규
- `CrewSystemUIView` — crew-info-column 레이아웃
- `CrewData` — CrewName 필드 추가
- `LogicCommandManager.SelectCrew()` — clickedByUI 파라미터 추가

### 방 이동 타일 배정 로직 개선
- `LogicCommandManager` — `SetAllCrews()`, `OrderMoveToRoom()` 추가. 빈 타일 순서 배정
- `MouseInputManager` — 우클릭 시 `OrderMoveToRoom()` 호출

### GameSaveData 도입
- `GameSaveData` 신규. `SaveLoadManager` 제네릭화. `GameSessionManager` GameSaveData로 교체
