using System.Collections.Generic;
using Core.Data.Crews;
using Core.Data.Storage;
using Core.Data.Weapon;
using UnityEngine;

namespace Presentation.System
{
    public class AssetCatalogManager : Singleton<AssetCatalogManager>
    {
        [Header("우주선 외형 카탈로그")] public List<ShipHullEntry> ShipHullList;

        [Header("승무원 기획 데이터 카탈로그")] public List<CrewSOEntry> CrewSOList;

        [Header("무기 SO 데이터 카탈로그")] public List<WeaponBaseSO> WeaponSOList;

        private Dictionary<string, CrewBaseSO> _crewSODict;

        private Dictionary<string, GameObject> _shipHullDict;

        private Dictionary<string, WeaponBaseSO> _weaponSO;

        protected override void Awake()
        {
            base.Awake(); // 싱글톤 유지 로직 실행

            // 3. 게임 시작 시, 장부(List)를 보고 캐비닛(Dictionary)을 꽉 채워둡니다.
            InitializeCatalogs();
        }

        private void InitializeCatalogs()
        {
            _shipHullDict = new Dictionary<string, GameObject>();
            foreach (var entry in ShipHullList)
                if (!_shipHullDict.ContainsKey(entry.HullID))
                    _shipHullDict.Add(entry.HullID, entry.HullPrefab);
                else
                    Debug.LogWarning($"[AssetCatalog] 중복된 우주선 ID 발견: {entry.HullID}");

            _crewSODict = new Dictionary<string, CrewBaseSO>();
            foreach (var entry in CrewSOList)
                if (!_crewSODict.ContainsKey(entry.CrewDataID))
                    _crewSODict.Add(entry.CrewDataID, entry.BaseDataSO);
                else
                    Debug.LogWarning($"[AssetCatalog] 중복된 승무원 ID 발견: {entry.CrewDataID}");
            
            _weaponSO = new Dictionary<string, WeaponBaseSO>();
            foreach (var entry in WeaponSOList)
                if (!_weaponSO.ContainsKey(entry.WeaponID))
                    _weaponSO.Add(entry.WeaponID, entry);
                else
                    Debug.LogWarning($"[AssetCatalog] 중복된 무기 ID 발견: {entry.WeaponID}");
        
        }

        public GameObject GetShipHullPrefab(string hullID)
        {
            if (_shipHullDict.TryGetValue(hullID, out var prefab)) return prefab;

            Debug.LogError($"[AssetCatalog] 창고에 '{hullID}' 우주선이 없습니다!");
            return null; // (또는 보라색 에러용 기본 큐브 프리팹 반환)
        }

        public CrewBaseSO GetCrewBaseData(string crewDataID)
        {
            if (_crewSODict.TryGetValue(crewDataID, out var so)) return so;

            Debug.LogError($"[AssetCatalog] 창고에 '{crewDataID}' 승무원 데이터가 없습니다!");
            return null;
        }

        public WeaponBaseSO GetWeaponBaseData(string weaponDataID)
        {
            if (_weaponSO.TryGetValue(weaponDataID, out var so)) return so;
            
            Debug.LogError($"[AssetCatalog] 창고에 '{weaponDataID}' 무기 데이터가 없습니다!");
            return null;
        }
    }
}