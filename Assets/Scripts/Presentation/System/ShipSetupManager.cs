using System.Collections.Generic;
using System.Linq;
using Core.Data.SpaceShip;
using Core.Interface;
using Logic.SpaceShip;
using Logic.System;
using Presentation.Views;
using Presentation.Views.UI;
using UnityEngine;

namespace Presentation.System
{
    public class ShipSetupManager : MonoBehaviour
    {
        [Header("임시 프리팹 (나중엔 창고에서 관리)")] public GameObject defaultCrewPrefab; // 승무원 기본 프리팹
        [Header(("무기 프리팹"))] public GameObject defaultWeaponPrefab;
        private void Start()
        {
            // 씬이 켜지자마자 조립 시작!
            BeginSetup();
        }

        private void BeginSetup()
        {
            // 1. 배달부(GameSessionManager)에게서 데이터 가방 넘겨받기
            var savedData = GameSessionManager.Instance.HandOverData();
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


            spaceShipView.Bind(shipAPI, shipAPI.GetAllTiles(), shipAPI.GetAllRooms(), shipAPI.GetAllDoors());
            SetupCrews(shipAPI.GetAllCrews(), spaceShipView);

            var weaponManager = new WeaponManager();
            SetupWeapons(shipAPI.GetAllWeapons(), spaceShipView);
            
            weaponManager.Initialize(shipAPI.GetAllWeapons(), shipAPI.GetAllRooms().First(x => x.Data.RoomType == RoomTypeString.Weapon));


            //===전력 매니저 셋업
            var powerManager = new PowerManager();
            powerManager.Initialize(savedData.MaxReactorPower, shipAPI.GetAllRooms());

            // 5. 시뮬레이션 운영팀에게 권한 양도 및 Ticker(시계) 시작
            // SimulationManager.Instance.StartSimulation(_activeCrewLogics, gridAPI);

            var simCore = new SimulationCore();
            simCore.RegisterTickables(shipAPI.GetAllRooms() as IEnumerable<ITickable>);
            simCore.RegisterTickables(shipAPI.GetAllCrews() as IEnumerable<ITickable>);
            simCore.RegisterTickables(shipAPI.GetAllDoors() as IEnumerable<ITickable>);

            var shipSimulationManager = new ShipSimulationManager(shipAPI as SpaceShipManager);

            simCore.RegisterTickables(shipSimulationManager);

            var timeProvider = FindObjectOfType<UnityTimeProvider>();
            timeProvider.Initialize(simCore);

            spaceShipView.SimulationCore = simCore;

            // 6. 입력 매니저 초기화
            var inputManager = FindObjectOfType<MouseInputManager>();
            inputManager.Initialize();


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

            Debug.Log("🚀 우주선 셋업 및 바인딩 완벽하게 종료!");
        }

        private void SetupCrews(IEnumerable<ICrewLogic> crewLogics, SpaceShipView spaceShipView)
        {
            foreach (var crewLogic in crewLogics)
            {
                var crewSO =
                    AssetCatalogManager.Instance.CrewSOList.First(x => x.CrewDataID == crewLogic.Data.BaseDataID);
                var crewObj = Instantiate(defaultCrewPrefab);

                var crewView = crewObj.GetComponent<CrewView>();
                crewView.Bind(crewSO.BaseDataSO, crewLogic, spaceShipView);
            }
        }

        private void SetupWeapons(IEnumerable<IWeaponLogic> weaponLogics, SpaceShipView spaceShipView)
        {
            foreach (var weaponLogic in weaponLogics)
            {
                var weaponSO =
                    AssetCatalogManager.Instance.GetWeaponBaseData(weaponLogic.Data.WeaponID);
                var weaponObj = Instantiate(defaultWeaponPrefab);
                
                var weaponView = weaponObj.GetComponent<WeaponView>();
                weaponView.Bind(weaponSO, weaponLogic, spaceShipView);
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