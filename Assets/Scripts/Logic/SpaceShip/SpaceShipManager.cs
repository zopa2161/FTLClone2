using System;
using System.Collections.Generic;
using Core.Data.SpaceShip;
using Core.Interface;
using Logic.SpaceShip.Rooms;
using Logic.System;
using UnityEngine;
using Random = System.Random;

namespace Logic.SpaceShip
{
    public class SpaceShipManager : IShipAPI
    {
        private const float FIRE_IGNITION_CHANCE = 0.30f;
        private const float FIRE_INITIAL_LEVEL   = 10f;
        private readonly Random _random   = new Random();

        private List<CrewLogic> _crews = new();
        private List<DoorLogic> _doors = new();
        private Dictionary<TileCoord, TileLogic> _logicMap;
        private List<BaseRoomLogic> _rooms = new();
        private List<ITileLogic> _tiles = new();
        
        private List<IWeaponLogic> _weapons = new List<IWeaponLogic>();
        private IShieldLogic _shieldLogic;

        public int MaxHullHealth { get; private set; }
        public int CurrentHullHealth { get; private set; }
        public event Action<int, int> OnHullHealthChanged;

        //===인터페이스 함수===

        public IReadOnlyList<ITileLogic> GetAllTiles()
        {
            return _tiles;
        }

        public IReadOnlyList<IRoomLogic> GetAllRooms()
        {
            return _rooms;
        }

        public IReadOnlyList<IDoorLogic> GetAllDoors()
        {
            return _doors;
        }


        public IReadOnlyList<ICrewLogic> GetAllCrews()
        {
            return _crews;
        }

        public List<IWeaponLogic> GetAllWeapons()
        {
            return _weapons;
        }

        public IShieldLogic GetShieldLogic()
        {
            return _shieldLogic;
        }

        //=== IGridMap 구현 함수===//
        public List<TileCoord> GetConnectedNeighbors(TileCoord current)
        {
            var result = new List<TileCoord>();
            var currentTile = _logicMap[current];
            foreach (var neighbor in currentTile.Neighbors)
                result.Add(new TileCoord(neighbor.TileCoord.X, neighbor.TileCoord.Y));

            foreach (var door in _doors)
                if (door.TileA.TileCoord == current || door.TileB.TileCoord == current)
                    result.Add(new TileCoord(door.GetConnectedTile(currentTile).TileCoord.X,
                        door.GetConnectedTile(currentTile).TileCoord.Y));

            return result;
        }

        public IDoorLogic GetDoorBetween(TileCoord a, TileCoord b)
        {
            foreach (var door in _doors)
                if ((door.Data.TileA == a && door.Data.TileB == b) || (door.Data.TileA == b && door.Data.TileB == a))
                    return door;

            return null;
        }

        public IRoomLogic GetRoomAt(TileCoord coord)
        {
            foreach (var room in _rooms)
            foreach (var tileCoord in room.GetRoomTiles())
                if (tileCoord == coord)
                    return room;

            return null;
        }

        public ITileLogic GetTileAt(TileCoord coord)
        {
            return _logicMap[coord];
        }


        private void HandleCrewDied(ICrewLogic deadCrew)
        {
            // 우주선 승무원 명부에서 삭제
            _crews.Remove(deadCrew as CrewLogic);

            // 구독 해제 (메모리 누수 방지)
            deadCrew.OnDied -= HandleCrewDied;

            Debug.Log("[SpaceShipManager] 승무원 사망 확인. 명부에서 삭제됨.");

            // (참고) 만약 SimulationCore가 별도로 ITickable 리스트를 관리하고 있다면,
            // 여기서 SimulationCore에게도 "얘 죽었으니 Tick 그만 줘"라고 알려야 합니다.
        }

        public OxygenRoomLogic GetOxygenRoom()
        {
            foreach (var room in _rooms)
                if (room.Data.RoomType == RoomTypeString.Oxygen)
                    return room as OxygenRoomLogic;

            return null;
        }

        //=== Setup을 위한 함수 ===//
        public void SetTiles(Dictionary<TileCoord, TileLogic> logicMap, List<ITileLogic> tiles)
        {
            _logicMap = logicMap;
            _tiles = tiles;
        }

        public void SetRooms(List<BaseRoomLogic> rooms)
        {
            _rooms = rooms;
        }

        public void SetDoors(List<DoorLogic> doors)
        {
            _doors = doors;
        }

        public void SetCrews(List<CrewLogic> crews)
        {
            _crews = crews;
            foreach (var crew in _crews) crew.OnDied += HandleCrewDied;
        }

        public void SetWeapons(List<IWeaponLogic> weapons)
        {
            _weapons = weapons;
        }

        public void SetHullHealth(int maxHealth, int currentHealth)
        {
            MaxHullHealth = maxHealth;
            CurrentHullHealth = currentHealth;
            OnHullHealthChanged?.Invoke(CurrentHullHealth, MaxHullHealth);
        }

        public void TakeDamage(int damage)
        {
            int newHealth = Math.Max(0, CurrentHullHealth - damage);
            SetHullHealth(MaxHullHealth, newHealth);
        }

        public void TryStartFire(int roomID)
        {
            if (_random.NextDouble() > FIRE_IGNITION_CHANCE) return;

            var room = _rooms.Find(r => r.Data.RoomID == roomID);
            if (room == null) return;

            var tiles = room.GetRoomTiles();
            if (tiles.Count == 0) return;

            var coord = tiles[_random.Next(tiles.Count)];
            var tile  = _logicMap[coord];
            if (tile.FireLevel <= 0f)
            {
                Debug.Log("불지르기");
                tile.FireLevel = FIRE_INITIAL_LEVEL;
            }
                
        }

        public void SetShieldLogic(ShieldData shieldData)
        {
            var shield = new ShieldManager();
            IRoomLogic shieldRoom =null;
            foreach(var room in _rooms)
            {
                if (room.Data.RoomType == RoomTypeString.Shield)
                {
                    shieldRoom = room;
                    break;
                }
            }
            shield.Initialize(shieldRoom,shieldData);
            _shieldLogic = shield;
        }
    }
}