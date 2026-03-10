using System.Collections.Generic;
using Core.Data.SpaceShip;
using Core.Data.Weapon;
using Core.Interface;
using Logic.SpaceShip.Rooms;
using Logic.SpaceShip.Weapons;

namespace Logic.SpaceShip
{
    public class GridBuilder
    {
        private readonly Dictionary<TileCoord, TileLogic> logicMap = new();
        private SpaceShipManager _spaceShipManager;

        public IShipAPI Rebuild(ShipSaveData saveData)
        {
            _spaceShipManager = new SpaceShipManager();

            RebuildTiles(saveData.Tiles);
            RebuildDoors(saveData.Doors);
            RebuildRooms(saveData.Rooms);
            RebuildCrews(saveData.Crews);

            return _spaceShipManager;
        }

        private void RebuildTiles(List<TileData> tiles)
        {
            var tileLogics = new List<ITileLogic>();
            foreach (var data in tiles)
            {
                var newLogic = new TileLogic();
                newLogic.Initialize(data);
                tileLogics.Add(newLogic);
                logicMap[new TileCoord(data.X, data.Y)] = newLogic;
            }

            foreach (var data in tiles)
            {
                // 현재 내 로직 객체를 찾습니다.
                var currentLogic = logicMap[new TileCoord(data.X, data.Y)];

                // JSON에 적혀있던 '이웃들의 좌표 명단(ConnectedNeighborCoords)'을 하나씩 봅니다.
                foreach (var neighborCoord in data.ConnectedNeighborCoords)
                    // 그 좌표에 해당하는 '진짜 로직 객체'를 지도에서 찾습니다.
                    if (logicMap.TryGetValue(neighborCoord, out var neighborLogic))
                        // 💡 진짜 메모리 참조를 연결합니다!
                        currentLogic.Neighbors.Add(neighborLogic);
            }

            _spaceShipManager.SetTiles(logicMap, tileLogics);
        }

        private void RebuildDoors(List<DoorData> doors)
        {
            var doorLogics = new List<DoorLogic>();
            foreach (var door in doors)
            {
                var tileA = logicMap[door.TileA];
                var tileB = logicMap[door.TileB];

                var builded = new DoorLogic();
                builded.Initialize(tileA, tileB, door);
                doorLogics.Add(builded);
            }

            _spaceShipManager.SetDoors(doorLogics);
        }

        private void RebuildRooms(List<RoomData> rooms)
        {
            var roomLogics = new List<BaseRoomLogic>();
            foreach (var room in rooms)
            {
                var builded = RoomLogicFactory.CreateRoomLogic(room);

                var roomTiles = new List<TileLogic>();
                foreach (var coord in room.TileCoords)
                    if (logicMap.TryGetValue(coord, out var tileLogic))
                        roomTiles.Add(tileLogic);

                builded.Initialize(roomTiles);
                roomLogics.Add(builded);
            }

            _spaceShipManager.SetRooms(roomLogics);
        }

        private void RebuildCrews(List<CrewData> crews)
        {
            var crewLogics = new List<CrewLogic>();
            foreach (var crew in crews)
            {
                var builded = new CrewLogic();
                builded.Initialize(crew, _spaceShipManager);
                crewLogics.Add(builded);
            }

            _spaceShipManager.SetCrews(crewLogics);
        }

        private void RebuildWeapons(List<WeaponData> weapons)
        {
            var weaponLogics = new List<IWeaponLogic>();

            foreach (var weapon in weapons)
            {
                var logic = new WeaponLogic();
                logic.Initialize(weapon);
                weaponLogics.Add(logic);
                
            }
        }
    }
    
}