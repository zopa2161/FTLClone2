# FTLClone2

Unity 2D로 제작 중인 **FTL: Faster Than Light** 클론 프로젝트입니다.
우주선 내부 시뮬레이션(산소, 전력, 무기, 승무원, 화재, 전투)을 중심으로 구현되어 있습니다.

---

## 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [개발 환경](#개발-환경)
3. [아키텍처](#아키텍처)
4. [디렉터리 구조](#디렉터리-구조)
5. [핵심 시스템 설명](#핵심-시스템-설명)
   - [Core 계층](#core-계층)
   - [Logic 계층](#logic-계층)
   - [Presentation 계층](#presentation-계층)
6. [데이터 흐름](#데이터-흐름)
7. [주요 수치 / 규칙](#주요-수치--규칙)
8. [현재 미구현 항목](#현재-미구현-항목)
9. [시작하기](#시작하기)

---

## 프로젝트 개요

| 항목 | 내용 |
|---|---|
| 장르 | 로그라이크 우주선 시뮬레이션 |
| 엔진 | Unity 2D |
| 언어 | C# |
| UI | Unity UI Toolkit (UXML / USS) |
| 저장 방식 | JSON (JsonUtility + Application.persistentDataPath) |

### 구현된 주요 기능

- **우주선 내부 시뮬레이션**: 타일 기반 격자, 방/문/승무원/무기 로직
- **산소 시뮬레이션**: 진공 바람(BFS Flow Field), 선체 브리치 시 공기 유출
- **화재 시뮬레이션**: 틱 기반 화재 전파·강화·진화, 승무원 화재 진압(FSM)
- **전력 시스템**: 원자로 전력 배분, 방별 전력 할당/회수
- **실드 시스템**: 충전 게이지, 발사체 도달 전 예약 흡수(pendingAbsorption)
- **무기 & 전투**: 장전 타이머, 자동 발사, 발사체 비행 지연 피해(15틱=1.5초)
- **이벤트 시스템**: 섹터 맵 노드 이동 → 대화/전투/보상 서브이벤트 체인
- **승무원 AI**: A* 경로 탐색 + FSM(대기/이동/근무/화재진압)
- **저장/불러오기**: 전체 게임 상태를 `GameSaveData` JSON으로 직렬화

---

## 개발 환경

- **Unity**: 2D URP
- **C# 어셈블리**: `Core`, `Logic`, `Presentation` (3개의 asmdef 분리)
- **에디터 도구**: 이벤트 에디터 윈도우(`Window/FTL/Event Editor`), 적군 우주선 에디터

---

## 아키텍처

3계층 분리 구조를 채택하여 Unity 의존성 없이 Logic을 독립적으로 유지합니다.

```
┌─────────────────────────────────────────────────────────┐
│  Presentation (Unity MonoBehaviour + UI Toolkit)        │
│  - Views, UI, System (Input, Setup, Session)            │
├─────────────────────────────────────────────────────────┤
│  Logic (순수 C# — MonoBehaviour 없음)                   │
│  - SpaceShip, System, Event, Map                        │
├─────────────────────────────────────────────────────────┤
│  Core (순수 C# 데이터/인터페이스)                       │
│  - Data, Interface, enums                               │
└─────────────────────────────────────────────────────────┘
```

> **원칙**: 상위 계층은 하위 계층에 의존할 수 있지만, 하위 계층은 상위 계층을 참조하지 않습니다.
> `Logic`은 `UnityEngine`을 사용하지 않으며, `Core`는 `UnityEngine` 의존성을 최소화합니다.

---

## 디렉터리 구조

```
Assets/
├── Scripts/
│   ├── Core/                          # 순수 C# 데이터 & 인터페이스
│   │   ├── Data/
│   │   │   ├── SpaceShip/             # TileData, RoomData, DoorData, CrewData, ShipSaveData 등
│   │   │   ├── Weapon/                # WeaponData, WeaponBaseSO
│   │   │   ├── Crews/                 # CrewBaseSO
│   │   │   ├── Combat/                # PendingAttack, HitResult
│   │   │   ├── Map/                   # MapData, NodeData
│   │   │   ├── Event/                 # EventSO, SubEventBaseSO, DialogSubEventSO 등
│   │   │   └── Storage/               # GameSaveData, SaveLoadManager, 매핑 Entry 클래스
│   │   ├── Interface/                 # IShipAPI, ICrewLogic, IRoomLogic 등 전체 인터페이스
│   │   └── enums/                     # MoveDirection, WeaponType, Faction, CrewStateType
│   │
│   ├── Logic/                         # 순수 C# 게임 로직
│   │   ├── SpaceShip/
│   │   │   ├── Rooms/                 # BaseRoomLogic + 방 타입별 구체 클래스
│   │   │   ├── CrewState/             # ICrewState, Idle/Moving/Working/FireFighting
│   │   │   ├── Weapons/               # WeaponLogic
│   │   │   ├── SpaceShipManager.cs    # IShipAPI 구현체
│   │   │   ├── GridBuilder.cs         # SaveData → Logic 객체 조립
│   │   │   ├── CrewLogic.cs           # 승무원 FSM + A* 이동
│   │   │   ├── DoorLogic.cs           # 문 자동 닫힘
│   │   │   └── TileLogic.cs
│   │   ├── System/
│   │   │   ├── SimulationCore.cs      # ITickable 관리, AdvanceTime()
│   │   │   ├── ShipSimulationManager.cs  # 산소·화재 시뮬레이션
│   │   │   ├── CombatResolver.cs      # 발사체 큐 + 지연 피해 처리
│   │   │   ├── ShieldManager.cs       # 실드 충전·흡수
│   │   │   ├── PowerManager.cs        # 원자로 전력 배분
│   │   │   ├── WeaponManager.cs       # 무기 ON/OFF, 승무원 보너스
│   │   │   ├── ResourceManager.cs     # Fuel/Missiles/Drones/Scrap
│   │   │   ├── LogicCommandManager.cs # 승무원 선택/이동 브릿지
│   │   │   └── AStarPathfinder.cs     # 맨해튼 거리 A* 탐색
│   │   ├── Event/
│   │   │   ├── EventLogicManager.cs   # IEventLogic 구현, 서브이벤트 진행
│   │   │   └── EnemyCombatManager.cs  # 적군 Logic 조립 + 전투 흐름
│   │   └── Map/
│   │       ├── MapManager.cs          # 노드 이동·방문 처리
│   │       └── MapGenerator.cs        # 절차적 섹터 맵 생성
│   │
│   └── Presentation/                  # Unity MonoBehaviour
│       ├── System/
│       │   ├── GameSessionManager.cs  # Singleton, JSON 로드·저장
│       │   ├── AssetCatalogManager.cs # SO 카탈로그 (Hull/Crew/Weapon/Event)
│       │   ├── ShipSetupManager.cs    # 게임 시작 시 전체 조립 엔트리포인트
│       │   ├── MouseInputManager.cs   # 클릭/호버 입력 처리
│       │   ├── UnityTimeProvider.cs   # Update() → SimulationCore.AdvanceTime()
│       │   └── Singleton.cs           # DontDestroyOnLoad 기반 제네릭 싱글톤
│       ├── Views/
│       │   ├── SpaceShipView.cs       # Tile/Room/Door View 바인딩 허브
│       │   ├── RoomView.cs            # 산소 위험 오버레이, 무기 조준 하이라이트
│       │   ├── CrewView.cs            # 위치 이동, 체력 바, 사망 처리
│       │   ├── DoorView.cs            # 스프라이트 교체 (열림/닫힘)
│       │   ├── ShieldView.cs          # 실드 활성화 표시
│       │   ├── WeaponView.cs          # (미구현)
│       │   └── Combat/
│       │       └── CombatViewManager.cs  # 발사체 생성·이동·소멸
│       └── UI/
│           ├── PowerSystemUIView.cs   # 원자로 바 + 방별 전력 컬럼
│           ├── WeaponSystemUIView.cs  # 무기 슬롯 + 장전 게이지
│           ├── CrewSystemUIView.cs    # 승무원 초상화 + 체력 바
│           ├── ShieldSystemUIView.cs  # 실드 원형 인디케이터
│           ├── HullHealthUIView.cs    # 선체 체력 바 (1바=1HP)
│           ├── EnemyHullHealthUIView.cs  # 적군 체력 바
│           ├── EnemyShieldUIView.cs   # 적군 실드 UI
│           ├── GameMainUIView.cs      # JUMP/자원 레이블
│           ├── MapView.cs             # 섹터 맵 오버레이
│           └── EventDialogUIManager.cs   # 대화/보상 이벤트 오버레이
│
├── Editor/
│   ├── EventEditorWindow.cs           # Window/FTL/Event Editor — 이벤트 SO 생성/삭제
│   ├── EventSOEditor.cs               # EventSO 커스텀 인스펙터 (체인 트리 시각화)
│   └── EnemyShipEditorWindow.cs       # 적군 우주선 에디터
│
├── Data/
│   ├── EventDatabase.asset            # 전체 이벤트 DB (EventDatabaseSO)
│   ├── EnemyShipDatabase.asset        # 적군 우주선 DB (EnemyShipDatabaseSO)
│   ├── Events/                        # EventSO, DialogSubEventSO, CombatSubEventSO, RewardSubEventSO
│   └── EnemyShips/                    # EnemyShipSO 에셋
│
├── Prefabs/
│   ├── Ship 1.prefab                  # 플레이어 우주선 프리팹 (SpaceShipView)
│   ├── enemy_Ship1.prefab             # 적군 우주선 프리팹
│   ├── Crew_Prefab.prefab             # 승무원 프리팹 (CrewView)
│   ├── DefaultWeaponPrefab.prefab     # 무기 프리팹 (WeaponView)
│   ├── Door.prefab                    # 문 프리팹 (DoorView)
│   ├── RoomPrefab.prefab              # 방 프리팹 (RoomView)
│   ├── Tile.prefab                    # 타일 프리팹 (TileView)
│   ├── laser1.prefab                  # 레이저 발사체 프리팹
│   └── missile.prefab                 # 미사일 발사체 프리팹
│
└── UI Toolkit/
    ├── GameHUD.uxml                   # 통합 HUD 레이아웃
    ├── PowerSystemUI.uxml
    ├── WeaponSystemUI.uxml
    ├── CrewSystemUI.uxml
    └── *.uss                          # 각종 스타일 시트
```

---

## 핵심 시스템 설명

### Core 계층

#### 데이터 구조

| 클래스 | 설명 |
|---|---|
| `GameSaveData` | 최상위 직렬화 루트. `ShipSaveData + MapData + EventSaveData` 통합 |
| `ShipSaveData` | 우주선 전체 상태. HullID, MaxReactorPower, MaxHullHealth(20), Shield, Resources, Tiles/Rooms/Doors/Crews/Weapons |
| `TileData` | 타일 영구 데이터. OxygenLevel(0~100f), BreachLevel, FireLevel, 인접 타일 목록 |
| `RoomData` | 방 영구 데이터. RoomID, RoomType, MaxPower, CurrentAllocatedPower, TileCoords |
| `DoorData` | 문 영구 데이터. TileA/B, IsOpen, IsForcedOpen |
| `CrewData` | 승무원 영구 데이터. CrewID, BaseDataID, CrewName, CurrentX/Y, MaxHealth(100f) |
| `ShieldData` | 실드 상태. ChargeGauge(0~1), CurrentShieldCount(0~4) |
| `ResourceData` | 소모 자원. Fuel(3), Missiles(8), Drones(2), Scrap(0) |
| `WeaponData` | 무기 런타임 상태. IsPowered, CurrentChargeTimer, IsAutoFire, TargetRoomID |
| `WeaponBaseSO` | ScriptableObject 무기 정의. WeaponType, Damage, ProjectileCount, RequiredPower, BaseCooldown |
| `PendingAttack` | 비행 중인 발사체 정보. TargetShipAPI, Hits, TicksRemaining, TargetRoomID, WeaponType |
| `HitResult` | 발사체 1발의 미리 계산된 결과. ReservedShield(bool), Damage(int) |

#### 방 타입 (`RoomTypeString`)

`Pilot` / `Engine` / `Weapon` / `Shield` / `Oxygen` / `MedBay` / `Door` / `Vision` / `Empty`

#### 인터페이스

| 인터페이스 | 역할 |
|---|---|
| `IShipAPI` | `IGridMap` 상속. 승무원/무기/실드/선체 체력 접근, `TakeDamage()`, `TryStartFire()` |
| `IGridMap` | 타일/방/문 조회, 인접 타일/문 검색 |
| `IRoomLogic` | 전력 변경, 산소 평균, 근무 상태, 이벤트 (`OnPowerChanged`, `OnOxygenChanged`) |
| `ICrewLogic` | 이동 명령, 피해, 이벤트 (`OnPositionChanged`, `OnHealthChanged`, `OnDied`) |
| `IDoorLogic` | 문 상태 변경, 자동 닫힘 |
| `IWeaponLogic` | 전력/자동발사/타겟/충전 관리, `TryFire()`, `OnFired` |
| `IShieldLogic` | 충전 게이지, 실드 예약 흡수 패턴 |
| `IPowerSystem` | 원자로 전력 할당/회수 |
| `IResourceManager` | Fuel/Missiles/Drones/Scrap 추적 |
| `IEventLogic` | 이벤트 진행 상태 추적, 서브이벤트 완료 처리 |
| `IMapLogic` | 노드 이동·방문 처리 |
| `ITickable` | `OnTickUpdate()` — 시뮬레이션 틱 인터페이스 |

---

### Logic 계층

#### 우주선 시뮬레이션

**`SimulationCore`**
- 등록된 `ITickable` 목록을 `TickRate = 0.1f` 초 간격으로 일괄 틱 처리
- `TimeScale` 지원 (스페이스바 일시정지)
- pending removal 패턴으로 틱 도중 안전한 등록 해제

**`ShipSimulationManager`**
- **산소 시뮬레이션**: 선체 브리치 발견 시 BFS로 거리맵(Flow Field) 생성 → 각 타일의 공기가 브리치 방향으로 빨려들어감 (WIND_SPEED_MULTIPLIER = 0.15f)
- **화재 시뮬레이션**: 틱마다 불이 붙은 타일에서 인접 타일로 전파(확률 1%), 산소 ≥ 50이면 강화, 산소 < 50이면 진화

**`CrewLogic` + FSM**
승무원은 4가지 상태(State)를 전환합니다.

| 상태 | 동작 |
|---|---|
| `CrewIdleState` | 대기 |
| `CrewMovingState` | A* 경로 따라 3틱/칸 이동. 경로상 문 자동 개방. 목적지 도착 시 WorkingState 또는 IdleState 전환 |
| `CrewWorkingState` | 콘솔 방향으로 회전 후 방에 근무 시작 보고 |
| `CrewFireFightingState` | 현재 방의 모든 타일 FireLevel을 0.5f/틱씩 감소. 진화 완료 시 Working/Idle 전환 |

- 산소 < 50f → 0.5f/틱 피해
- FireLevel > 0f → 2f/틱 피해

**`AStarPathfinder`**
맨해튼 거리 휴리스틱 A*, 반환값: `Queue<TileCoord>`
열린 문만 통과 가능 (이동 명령 시 경로상 문은 자동 개방)

#### 전투 시스템

**`CombatResolver`** (발사체 지연 피해 중계)
1. 무기 `OnFired` 이벤트 수신
2. 발사 즉시: 실드 예약(`TryReserveShield`) + `PendingAttack` 큐 삽입 + `OnAttackQueued` 발행
3. `PROJECTILE_TRAVEL_TICKS = 15` (1.5초) 카운트다운 후 실제 피해 적용
   - Laser: 실드 흡수 우선(`ApplyReservedAbsorption`)
   - Missile: 실드 무시, 직접 HP 감소

**`ShieldManager`**
- 75틱(7.5초)마다 실드 1개 충전
- Manned 보너스: 1.2배 충전 속도
- `TryReserveShield()`: 현재 실드 시각은 유지하면서 `_pendingAbsorption`만 증가 → 발사체 비행 중 실드가 사라지지 않음

**`EnemyCombatManager`**
- `CombatSubEventSO` 수신 시 `GridBuilder`로 적군 Logic 전체 조립
- `SimulationCore`에 적군 Tickable 등록 → 적군 우주선이 실시간 시뮬레이션됨
- 매 틱 적군 체력 확인 → 0 이하 시 `EndCombat()` 자동 호출

#### 전력 시스템

**`PowerManager`**
- 원자로 전력 풀에서 방별로 할당/회수
- 방별 MaxPower 초과 불가

**`WeaponManager`**
- 무기 ON 시 전력 할당. 전력 초과 시 뒤에서부터 강제 OFF
- 무기 방에 승무원 배치 시 장전 속도 1.2배 보너스 적용

#### 이벤트 시스템

```
EventSO (최상위)
  └── StartEvent: SubEventBaseSO
        ├── DialogSubEventSO  → List<DialogChoice> → NextEvent
        ├── CombatSubEventSO  → EnemyShip(ShipSaveData) → NextEvent
        └── RewardSubEventSO  → List<RewardEntry>(타입+수량) → NextEvent
```

- `EventLogicManager`: 서브이벤트 체인 진행. `IsFinished == true` 또는 `NextEvent == null` 시 `OnEventFinished` 발행
- 보상: `IResourceManager`로 즉시 적용 (Scrap/Fuel/Missiles/Drones)
- 이벤트 진행 상황은 `EventSaveData`로 직렬화됨

#### 맵 시스템

**`MapGenerator`**: 컬럼 기반 절차적 섹터 맵 생성 (기본 7컬럼, 최대 4행/컬럼)
**`MapManager`**: 노드 이동·방문 기록. 이동 시 해당 노드의 EventID로 이벤트 자동 시작

---

### Presentation 계층

#### 게임 시작 흐름 (`ShipSetupManager.BeginSetup()`)

```
1. GameSessionManager → JSON 로드 → ShipData + MapData
2. AssetCatalogManager → Hull 프리팹 Instantiate → SpaceShipView
3. GridBuilder.Rebuild(savedData) → IShipAPI (Logic 조립)
4. SpaceShipView.Bind() → Tile/Room/Door/Shield View 이벤트 구독
5. SetupCrews() / SetupWeapons() → CrewView / WeaponView Bind
6. WeaponManager / PowerManager / ResourceManager 초기화
7. SimulationCore에 ITickable 등록 (방/승무원/문/무기/산소/실드)
8. UnityTimeProvider.Initialize(simCore) → Update() 루프 연결
9. 각 UI View 초기화 (Power/Weapon/Crew/Shield/Hull/Map/Event)
10. EnemyCombatManager / EventLogicManager / CombatViewManager 연결
```

#### 입력 (`MouseInputManager`)

| 입력 | 동작 |
|---|---|
| 좌클릭 (승무원) | 선택 / 재클릭 시 해제 |
| 좌클릭 (문) | 문 토글 (열기/닫기) |
| 좌클릭 (적군 방, 무기 선택 중) | 해당 방을 선택 무기의 타겟으로 설정 |
| 우클릭 (방) | 선택된 승무원에게 해당 방으로 이동 명령 |
| 호버 (아군 방, 승무원 선택 중) | 이동 대상 하이라이트 |
| 호버 (적군 방, 무기 선택 중) | 무기 조준 하이라이트 |
| Space | 일시정지 / 재개 |
| Ctrl+S | 수동 저장 |

#### UI 구성 (UI Toolkit)

| UI | 설명 |
|---|---|
| `PowerSystemUIView` | 원자로 바(3개 제한) + 방별 전력 컬럼. 좌클릭=할당, 우클릭=회수 |
| `WeaponSystemUIView` | 무기 슬롯 동적 생성. 장전 게이지 실시간 표시. 좌클릭=ON, 우클릭=OFF |
| `CrewSystemUIView` | 승무원 초상화+이름+체력 바. 30%↓=danger, 사망=dead CSS 클래스 |
| `ShieldSystemUIView` | 실드 원형 인디케이터 4개 + 충전 게이지 바 |
| `HullHealthUIView` | 선체 체력 바. 1바=1HP. 초록(>66%), 노랑(>33%), 빨강(≤33%) |
| `EnemyHullHealthUIView` | 적군 체력 바. 전투 시작/종료 시 표시/숨김 |
| `GameMainUIView` | JUMP 버튼(조건: !전투 && 파일럿 탑승 && Fuel≥1) + 자원 레이블 |
| `MapView` | 섹터 맵 오버레이. 노드 버튼 절대 위치 배치. reachable/current CSS |
| `EventDialogUIManager` | 반투명 오버레이. Dialog: 대화+선택지 버튼. Reward: 보상 목록+Accept |

#### 발사체 애니메이션 (`CombatViewManager`)

1. `CombatResolver.OnAttackQueued` 구독
2. 발사 즉시: 발사체 프리팹 생성, 방향 회전(`Atan2+90°`), `SetActive(false)` (대기)
3. 거리 기반 대기 시간 후 `SetActive(true)` → 목표 지점으로 `PROJECTILE_SPEED = 0.5f` Lerp 이동
4. 전투 종료 시 비행 중인 모든 발사체 일괄 `Destroy`

---

## 데이터 흐름

```
DefaultSheepMaker (ContextMenu)
  └─ DefaultShipData.json 생성 (기본 순양함: 17방, 26문, 3승무원, 1무기)

게임 시작
  └─ GameSessionManager.Awake()
       └─ SaveLoadManager.Load<GameSaveData>()
            └─ ShipData + MapData + EventSaveData

ShipSetupManager.BeginSetup()
  ├─ GridBuilder.Rebuild() → SpaceShipManager (IShipAPI)
  ├─ View.Bind() → Logic 이벤트 구독
  ├─ SimulationCore.RegisterTickables()
  └─ UI.Initialize()

런타임 루프
  └─ UnityTimeProvider.Update()
       └─ SimulationCore.AdvanceTime(deltaTime)
            └─ 0.1초마다 ITickable.OnTickUpdate() 일괄 호출
                 ├─ 방 로직 (산소 생성)
                 ├─ 승무원 로직 (FSM, 산소/화재 피해)
                 ├─ 문 로직 (자동 닫힘)
                 ├─ 무기 로직 (장전 타이머)
                 ├─ ShipSimulationManager (산소·화재 확산)
                 ├─ ShieldManager (충전)
                 └─ CombatResolver (발사체 카운트다운)
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
| 산소 위험 임계값 | 50f (승무원 피해 시작) |
| 산소 피해량 | 0.5f/틱 |
| 화재 피해량 | 2f/틱 |
| 화재 진압 속도 | 0.5f/틱 (`CrewFireFightingState.EXTINGUISH_RATE`) |
| 화재 전파 확률 | 1%/틱 (`FIRE_SPREAD_CHANCE`) |
| 진공 바람 세기 | OxygenLevel × 0.15f/틱 (`WIND_SPEED_MULTIPLIER`) |
| 실드 충전 시간 | 75틱 = 7.5초/1개 |
| 실드 Manned 보너스 | 1.2x |
| 실드 최대 수 | 전력 2당 1개 (최대 4개) |
| 발사체 비행 시간 | 15틱 = 1.5초 (`CombatResolver.PROJECTILE_TRAVEL_TICKS`) |
| 발사체 이동 속도 (View) | 0.5f/틱 (`CombatViewManager.PROJECTILE_SPEED`) |
| 선체 최대 체력 | 20 (`ShipSaveData.MaxHullHealth`) |
| 체력바 색상 임계값 | >66%=초록, >33%=노랑, ≤33%=빨강 |

---

## 현재 미구현 항목

| 항목 | 상태 |
|---|---|
| `WeaponView.Bind()` | 내용 비어있음 |
| 승무원 WorkingState 실제 버프 로직 | 주석 처리됨 |
| 산소 자연 확산 (`SimulateOxygenDiffusion`) | 구현되어 있으나 주석 처리됨 |
| 씬 전환 (`SceneManager.LoadScene`) | 주석 처리됨 |
| 발사체 프리팹 Inspector 할당 | `CombatViewManager`에 LaserPrefab/MissilePrefab/BeamPrefab + LaunchPoint/ShieldAbsorbingPoint Transform 배치 필요 |
| 이벤트 보상: 무기 추가 / MaxReactorPower 증가 | `EventLogicManager.ApplyRewards()`에 TODO 처리됨 |

---

## 시작하기

### 1. 기본 우주선 데이터 생성

Unity 에디터에서 `DefaultSheepMaker` 컴포넌트가 붙은 오브젝트를 선택 후
Inspector의 ContextMenu → **"기본 우주선 데이터 생성"** 실행
→ `DefaultShipData.json`이 `Application.persistentDataPath`에 생성됩니다.

### 2. SO 에셋 설정

- **AssetCatalogManager**: Ship Hull 프리팹, CrewBaseSO, WeaponBaseSO 목록 할당
- **EventDatabaseSO**: `Window/FTL/Event Editor`에서 이벤트 생성 및 관리
- **EnemyShipDatabaseSO**: `Window/FTL/Enemy Ship Editor`에서 적군 우주선 생성

### 3. 씬 실행

`SampleScene`을 열고 Play → `ShipSetupManager.BeginSetup()`이 자동 실행되어 전체 게임 로직이 조립됩니다.

### 4. 에디터 도구

| 메뉴 | 설명 |
|---|---|
| `Window/FTL/Event Editor` | 이벤트/대화/전투/보상 서브이벤트 SO 생성·삭제·관리 |
| `Window/FTL/Enemy Ship Editor` | 적군 우주선 데이터(EnemyShipSO) 생성·관리 |