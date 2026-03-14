using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Core.Data.SpaceShip;
using Core.enums;
using Core.Interface;
using Logic.Event;
using Logic.SpaceShip;
using Logic.Map;
using Logic.System;
using Presentation.UI;
using Presentation.Views;
using Presentation.Views.UI;
using UnityEngine;

namespace Presentation.System
{
    public class ShipSetupManager : MonoBehaviour
    {
        [Header("임시 프리팹 (나중엔 창고에서 관리)")] public GameObject defaultCrewPrefab; // 승무원 기본 프리팹
        [Header(("무기 프리팹"))] public GameObject defaultWeaponPrefab;

        private SimulationCore _simCore;
        private GameObject _enemyShipObj;
        private IShipAPI _playerShipAPI;
        private WeaponManager _weaponManager;

        private void Start()
        {
            // 씬이 켜지자마자 조립 시작!
            BeginSetup();
        }

        private void BeginSetup()
        {
            // 1. 배달부(GameSessionManager)에게서 데이터 가방 넘겨받기
            var savedData = GameSessionManager.Instance.ShipData;
            if (savedData == null)
            {
                Debug.LogError("[Setup] 전달받은 데이터가 없습니다!");
                return;
            }

            // 2. 껍데기(우주선 외형)부터 바닥에 깔기
            var hullPrefab = AssetCatalogManager.Instance.GetShipHullPrefab(savedData.ShipHullID);
            GameObject hullObj;
            if (hullPrefab == null) return;

            hullObj = Instantiate(hullPrefab, Vector3.zero, Quaternion.identity);
            var spaceShipView = hullObj.GetComponent<SpaceShipView>();

            // 3. 논리적 타일/방 조립 (이전 대화의 GridBuilder 로직)
            var builder = new GridBuilder();
            var shipAPI = builder.Rebuild(savedData);
            var shieldManager = shipAPI.GetShieldLogic();
            _playerShipAPI = shipAPI;

            spaceShipView.Bind(shipAPI, shipAPI.GetAllTiles(), shipAPI.GetAllRooms(), shipAPI.GetAllDoors());
            var crewViews = SetupCrews(shipAPI.GetAllCrews(), spaceShipView);

            _weaponManager = new WeaponManager();
            var weaponManager = _weaponManager;
            SetupWeapons(shipAPI.GetAllWeapons(), spaceShipView);

            weaponManager.Initialize(shipAPI.GetAllWeapons(), shipAPI.GetAllRooms().First(x => x.Data.RoomType == RoomTypeString.Weapon));


            //===전력 매니저 셋업
            var powerManager = new PowerManager();
            powerManager.Initialize(savedData.MaxReactorPower, shipAPI.GetAllRooms());

            // 5. 시뮬레이션 운영팀에게 권한 양도 및 Ticker(시계) 시작
            // SimulationManager.Instance.StartSimulation(_activeCrewLogics, gridAPI);

            _simCore = new SimulationCore();
            _simCore.RegisterTickables(shipAPI.GetAllRooms() as IEnumerable<ITickable>);
            _simCore.RegisterTickables(shipAPI.GetAllCrews() as IEnumerable<ITickable>);
            _simCore.RegisterTickables(shipAPI.GetAllDoors() as IEnumerable<ITickable>);
            //무기는 장착 상태가 아닐 수도 있으니까 일단 이렇게 함
            if (shipAPI.GetAllWeapons() != null)
            {
                var iTickList = new List<ITickable>();
                foreach (var weapon in shipAPI.GetAllWeapons())
                {
                    iTickList.Add(weapon as ITickable);
                }

                _simCore.RegisterTickables(iTickList);
            }
            var shipSimulationManager = new ShipSimulationManager(shipAPI as SpaceShipManager);
            _simCore.RegisterTickables(shipSimulationManager);

            // 실드 매니저 등록 (ShieldRoom이 없는 경우 null일 수 있음)
            if (shieldManager is ITickable shieldTick)
                _simCore.RegisterTickables(shieldTick);

            var timeProvider = FindObjectOfType<UnityTimeProvider>();
            timeProvider.Initialize(_simCore);

            spaceShipView.SimulationCore = _simCore;
            spaceShipView.BindShield(shieldManager);

            // 6. 입력 매니저 초기화
            var inputManager = FindObjectOfType<MouseInputManager>();
            inputManager.Initialize();
            inputManager.RegisterCrewViews(crewViews);
            inputManager.InitializeWeaponManager(weaponManager);
            var commandManager = inputManager.CommandManager;
            commandManager.SetAllCrews(shipAPI.GetAllCrews());


            // 7. UI셋업
            var powerSystemUI = FindObjectOfType<PowerSystemUIView>();
            if (powerSystemUI != null)
            {
                // 💡 요구사항: "실드, 엔진, Medical, 산소, 무기 순"
                // 방의 RoomType 문자열을 기준으로 원하는 순서대로 리스트를 재배열합니다.
                var orderedRooms = SortRoomsForUI(shipAPI.GetAllRooms());

                // 정렬된 방 목록과 전력 관리자를 UI에게 넘겨주어 화면을 그리게 합니다!
                powerSystemUI.Initialize(powerManager, orderedRooms, shipAPI as LogicCommandManager); //눈을 감아줘
            }

            var weaponSystemUI = FindObjectOfType<WeaponSystemUIView>();
            if (weaponSystemUI != null)
            {
                weaponSystemUI.Initialize(weaponManager);
            }

            var crewSystemUI = FindObjectOfType<CrewSystemUIView>();
            if (crewSystemUI != null)
            {
                crewSystemUI.Initialize(shipAPI.GetAllCrews(), commandManager);
            }

            var shieldSystemUI = FindObjectOfType<ShieldSystemUIView>();
            if (shieldSystemUI != null)
            {
                shieldSystemUI.Initialize(shieldManager);
            }

            var resourceManager = new ResourceManager();
            resourceManager.Initialize(savedData.Resources);

            var combatManager = new CombatManager();

            // 이벤트 로직 매니저 초기화 (CombatManager를 직접 주입 — 전투 상태 전환은 Logic 내부에서 처리)
            var eventLogicManager = new EventLogicManager(resourceManager, combatManager);
            eventLogicManager.Initialize(
                GameSessionManager.Instance.CurrentGameData.Event,
                AssetCatalogManager.Instance.GetEvent,
                AssetCatalogManager.Instance.GetSubEvent
            );

            // 적군 전투 매니저 — CombatSubEvent 시작 시 적군 Logic 조립 + View 생성
            var enemyCombatManager = new EnemyCombatManager(eventLogicManager, _simCore);
            eventLogicManager.OnSubEventChanged += subEvent =>
            {
                Debug.Log("여기선 반응?");
                if (subEvent is Core.Data.Event.CombatSubEventSO combat)
                {
                    enemyCombatManager.StartCombat(combat);
                    SetupEnemyShipView(enemyCombatManager.EnemyShipAPI, combat.EnemyShip.ShipData);

                    // 적군 무기 BaseData 설정 (Presentation 계층에서만 AssetCatalogManager 접근 가능)
                    var enemyWeapons = enemyCombatManager.EnemyShipAPI.GetAllWeapons();
                    if (enemyWeapons != null)
                        foreach (var weapon in enemyWeapons)
                        {
                            var so = AssetCatalogManager.Instance.GetWeaponBaseData(weapon.Data.WeaponID);
                            if (so != null) weapon.SetBaseData(so);
                        }

                    // CombatResolver 생성 및 바인딩 (적군 무기 BaseData 설정 이후)
                    var resolver = new CombatResolver(_playerShipAPI, enemyCombatManager.EnemyShipAPI, _weaponManager);
                    resolver.BindWeaponEvents();
                    enemyCombatManager.SetCombatResolver(resolver);
                }
            };
            enemyCombatManager.OnCombatEnded += TeardownEnemyShipView;

            // 맵 생성/로드
            // JsonUtility는 null 클래스 필드를 빈 객체로 직렬화/역직렬화하므로
            // null 체크 대신 Nodes.Count로 유효성을 판별합니다.
            var mapData = GameSessionManager.Instance.MapData;
            if (mapData == null || mapData.Nodes.Count == 0)
            {
                var mapGen = new MapGenerator();
                var eventIDs = AssetCatalogManager.Instance.EventSOList.Select(e => e.EventID).ToList();
                mapData = mapGen.GenerateMap(columns: 7, maxRowsPerColumn: 4, mapWidth: 1f, mapHeight: 1f, eventIDs: eventIDs);
                GameSessionManager.Instance.SetMapData(mapData);
            }
            var mapManager = new MapManager();
            mapManager.Initialize(mapData);
            mapManager.OnNodeChanged += node =>
            {
   
                if (!string.IsNullOrEmpty(node.EventID))
                {
                    Debug.Log("노드이동 반응");
                    eventLogicManager.StartEvent(AssetCatalogManager.Instance.GetEvent(node.EventID));
                }
                    
            };

            var mapView = FindFirstObjectByType<MapView>();
            if (mapView != null)
                mapView.Initialize(mapManager, mapData);

            var pilotRoom = shipAPI.GetAllRooms().FirstOrDefault(r => r.Data.RoomType == RoomTypeString.Pilot);

            var gameMainUI = FindObjectOfType<GameMainUIView>();
            if (gameMainUI != null && pilotRoom != null)
                gameMainUI.Initialize(resourceManager, combatManager, pilotRoom, mapView);

            var hullHealthUI = FindObjectOfType<HullHealthUIView>();
            if (hullHealthUI != null)
                hullHealthUI.Initialize(shipAPI);

            var eventDialogUI = FindObjectOfType<EventDialogUIManager>();
            if (eventDialogUI != null)
                eventDialogUI.Initialize(eventLogicManager);

            Debug.Log("🚀 우주선 셋업 및 바인딩 완벽하게 종료!");
        }

        private List<CrewView> SetupCrews(IEnumerable<ICrewLogic> crewLogics, SpaceShipView spaceShipView)
        {
            var result = new List<CrewView>();
            foreach (var crewLogic in crewLogics)
            {
                var crewSO =
                    AssetCatalogManager.Instance.CrewSOList.First(x => x.CrewDataID == crewLogic.Data.BaseDataID);
                var crewObj = Instantiate(defaultCrewPrefab);

                var crewView = crewObj.GetComponent<CrewView>();
                crewView.Bind(crewSO.BaseDataSO, crewLogic, spaceShipView);
                result.Add(crewView);
            }
            return result;
        }

        private void SetupWeapons(IEnumerable<IWeaponLogic> weaponLogics, SpaceShipView spaceShipView)
        {
            foreach (var weaponLogic in weaponLogics)
            {
                var weaponSO =
                    AssetCatalogManager.Instance.GetWeaponBaseData(weaponLogic.Data.WeaponID);
                weaponLogic.SetBaseData(weaponSO);
                var weaponObj = Instantiate(defaultWeaponPrefab);
                
                var weaponView = weaponObj.GetComponent<WeaponView>();
                weaponView.Bind(weaponSO, weaponLogic, spaceShipView);
            }
        }

        private void SetupEnemyShipView(IShipAPI enemyAPI, ShipSaveData saveData)
        {
            if (enemyAPI == null || saveData == null) return;

            var hullPrefab = AssetCatalogManager.Instance.GetShipHullPrefab(saveData.ShipHullID);
            if (hullPrefab == null)
            {
                Debug.LogWarning($"[ShipSetupManager] 적군 Hull 프리팹 없음: {saveData.ShipHullID}");
                return;
            }

            // 적군 우주선은 화면 우측에 배치 (추후 위치 조정 가능)
            _enemyShipObj = Instantiate(hullPrefab, new Vector3(10f, 0f, 0f), Quaternion.identity);
            var enemyShipView = _enemyShipObj.GetComponent<SpaceShipView>();
            if (enemyShipView == null) return;

            enemyShipView.Bind(enemyAPI, enemyAPI.GetAllTiles(), enemyAPI.GetAllRooms(), enemyAPI.GetAllDoors());
            enemyShipView.BindShield(enemyAPI.GetShieldLogic());

            // 모든 RoomView에 적군 Faction 지정
            foreach (var roomView in enemyShipView.RoomViews)
                roomView.SetFaction(Faction.Enemy);

            // 적 HP 바 UI 초기화
            var enemyHullUI = FindObjectOfType<EnemyHullHealthUIView>();
            if (enemyHullUI != null)
                enemyHullUI.ShowEnemy(enemyAPI);

            // 적 실드 UI 초기화
            var enemyShieldUI = FindObjectOfType<EnemyShieldUIView>();
            if (enemyShieldUI != null)
                enemyShieldUI.ShowEnemy(enemyAPI.GetShieldLogic());

            Debug.Log("[ShipSetupManager] 적군 SpaceShipView 생성 완료.");
        }

        private void TeardownEnemyShipView()
        {
            var enemyHullUI = FindObjectOfType<EnemyHullHealthUIView>();
            if (enemyHullUI != null)
                enemyHullUI.HideEnemy();

            var enemyShieldUI = FindObjectOfType<EnemyShieldUIView>();
            if (enemyShieldUI != null)
                enemyShieldUI.HideEnemy();

            if (_enemyShipObj != null)
            {
                Destroy(_enemyShipObj);
                _enemyShipObj = null;
                Debug.Log("[ShipSetupManager] 적군 SpaceShipView 제거 완료.");
            }
        }

        private IEnumerable<IRoomLogic> SortRoomsForUI(IEnumerable<IRoomLogic> allRooms)
        {
            // 정렬 기준이 되는 순서 (기획 데이터에 맞게 문자열을 맞춰주세요)
            var targetOrder = new List<string>
            {
                "Shield",
                "Engine",
                "Medical",
                "Oxygen",
                "Weapon"
            };

            // LINQ를 사용하여 targetOrder의 인덱스 순서대로 방을 정렬합니다.
            // (만약 목록에 없는 방이 있다면 제일 뒤로 밀어냅니다)
            return allRooms
                .Where(room => room.MaxPowerCapacity > 0) // 전력이 필요 없는 방은 아예 제외
                .OrderBy(room =>
                {
                    var index = targetOrder.IndexOf(room.Data.RoomType);
                    return index != -1 ? index : 999;
                })
                .ToList();
        }
    }
}