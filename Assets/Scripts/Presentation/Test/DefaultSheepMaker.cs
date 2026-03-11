using System.Collections.Generic;
using Core.Data.SpaceShip;
using Core.Data.Storage;
using Core.Data.Weapon;
using Core.enums;
using UnityEngine;

namespace Presentation.Test
{
    public class DefaultSheepMaker : MonoBehaviour
    {
        [Header("파일 설정")] public string SaveFileName = "GameSave";

        [Header("우주선 기본 설정")] public string ShipHullID = "Cruiser_TypeA";

        [ContextMenu("데이터 굽기 (JSON으로 저장)")]
        public void BuildDefaultShip()
        {
            var gameSaveData = new GameSaveData();
            var data = gameSaveData.Ship;
            data.ShipHullID = ShipHullID;

            data.MaxReactorPower = 10;

            var tileMap = new TileData[16, 8];

            var TileDatas = new List<TileData>
            {
                new(-1, 2), new(-1, 3),
                new(0, 2), new(0, 3),
                new(1, 1), new(1, 2), new(1, 3), new(1, 4),
                new(2, 1), new(2, 2), new(2, 3), new(2, 4),
                new(3, 1), new(3, 4),
                new(4, 1), new(4, 2), new(4, 3), new(4, 4),
                new(5, 2), new(5, 3),
                new(6, -1), new(6, 0), new(6, 1), new(6, 2), new(6, 3), new(6, 4), new(6, 5), new(6, 6),
                new(7, -1), new(7, 0), new(7, 1), new(7, 2), new(7, 3), new(7, 4), new(7, 5), new(7, 6),
                new(8, 1), new(8, 2), new(8, 3), new(8, 4),
                new(9, 1), new(9, 2), new(9, 3), new(9, 4),
                new(10, 2), new(10, 3),
                new(11, 2), new(11, 3),
                new(12, 2), new(12, 3),
                new(13, 2), new(13, 3),
                new(14, 2), new(14, 3)
            };

            foreach (var tileData in TileDatas)
                if (tileData.X >= 0 && tileData.Y >= 0)
                    tileMap[tileData.X, tileData.Y] = tileData;

            //일부 타일 구멍내기
            TileDatas[0].BreachLevel = 100f;
            TileDatas[1].BreachLevel = 100f;

            TileDatas[20].BreachLevel = 100f;
            TileDatas[27].BreachLevel = 100f;
            TileDatas[28].BreachLevel = 100f;
            TileDatas[35].BreachLevel = 100f;


            ConnectOpenTile(tileMap[0, 2], tileMap[0, 3]);

            ConnectOpenTile(tileMap[1, 1], tileMap[2, 1]);

            ConnectOpenTile(tileMap[1, 2], tileMap[2, 2]);
            ConnectOpenTile(tileMap[1, 2], tileMap[1, 3]);
            ConnectOpenTile(tileMap[1, 3], tileMap[2, 3]);
            ConnectOpenTile(tileMap[2, 2], tileMap[2, 3]);

            ConnectOpenTile(tileMap[1, 4], tileMap[2, 4]);

            ConnectOpenTile(tileMap[3, 1], tileMap[4, 1]);

            ConnectOpenTile(tileMap[3, 4], tileMap[4, 4]);

            ConnectOpenTile(tileMap[4, 2], tileMap[4, 3]);
            ConnectOpenTile(tileMap[4, 2], tileMap[5, 2]);
            ConnectOpenTile(tileMap[5, 2], tileMap[5, 3]);
            ConnectOpenTile(tileMap[4, 3], tileMap[5, 3]);

            ConnectOpenTile(tileMap[6, 0], tileMap[7, 0]);

            ConnectOpenTile(tileMap[6, 1], tileMap[7, 1]);
            ConnectOpenTile(tileMap[6, 1], tileMap[6, 2]);
            ConnectOpenTile(tileMap[6, 2], tileMap[7, 2]);
            ConnectOpenTile(tileMap[7, 1], tileMap[7, 2]);

            ConnectOpenTile(tileMap[6, 3], tileMap[7, 3]);
            ConnectOpenTile(tileMap[6, 3], tileMap[6, 4]);
            ConnectOpenTile(tileMap[6, 4], tileMap[7, 4]);
            ConnectOpenTile(tileMap[7, 3], tileMap[7, 4]);

            ConnectOpenTile(tileMap[6, 5], tileMap[7, 5]);

            ConnectOpenTile(tileMap[8, 1], tileMap[9, 1]);
            ConnectOpenTile(tileMap[8, 1], tileMap[8, 2]);
            ConnectOpenTile(tileMap[8, 2], tileMap[9, 2]);
            ConnectOpenTile(tileMap[9, 1], tileMap[9, 2]);

            ConnectOpenTile(tileMap[8, 3], tileMap[9, 3]);
            ConnectOpenTile(tileMap[8, 3], tileMap[8, 4]);
            ConnectOpenTile(tileMap[8, 4], tileMap[9, 4]);
            ConnectOpenTile(tileMap[9, 3], tileMap[9, 4]);


            ConnectOpenTile(tileMap[10, 2], tileMap[11, 2]);
            ConnectOpenTile(tileMap[10, 3], tileMap[11, 3]);

            ConnectOpenTile(tileMap[12, 2], tileMap[13, 2]);
            ConnectOpenTile(tileMap[12, 2], tileMap[12, 3]);
            ConnectOpenTile(tileMap[12, 3], tileMap[13, 3]);
            ConnectOpenTile(tileMap[13, 2], tileMap[13, 3]);

            ConnectOpenTile(tileMap[14, 2], tileMap[14, 3]);


            var DoorDatas = new List<DoorData>();
            DoorDatas.Add(CreateDoorData(0, tileMap[14, 2], tileMap[13, 2]));
            DoorDatas.Add(CreateDoorData(1, tileMap[12, 3], tileMap[11, 3]));
            DoorDatas.Add(CreateDoorData(2, tileMap[11, 2], tileMap[12, 2]));
            DoorDatas.Add(CreateDoorData(3, tileMap[10, 3], tileMap[9, 3]));
            DoorDatas.Add(CreateDoorData(4, tileMap[9, 2], tileMap[10, 2]));
            DoorDatas.Add(CreateDoorData(5, tileMap[8, 2], tileMap[8, 3]));
            DoorDatas.Add(CreateDoorData(6, tileMap[7, 4], tileMap[8, 4]));
            DoorDatas.Add(CreateDoorData(7, tileMap[7, 1], tileMap[8, 1]));
            DoorDatas.Add(CreateDoorData(8, TileDatas[35], tileMap[7, 5]));
            DoorDatas.Add(CreateDoorData(9, tileMap[7, 5], tileMap[7, 4]));
            DoorDatas.Add(CreateDoorData(10, tileMap[7, 1], tileMap[7, 0]));
            DoorDatas.Add(CreateDoorData(11, tileMap[7, 0], TileDatas[28]));
            DoorDatas.Add(CreateDoorData(12, tileMap[6, 5], TileDatas[27]));
            DoorDatas.Add(CreateDoorData(13, tileMap[6, 0], TileDatas[20]));
            DoorDatas.Add(CreateDoorData(14, tileMap[5, 3], tileMap[6, 3]));
            DoorDatas.Add(CreateDoorData(15, tileMap[5, 2], tileMap[6, 2]));
            DoorDatas.Add(CreateDoorData(16, tileMap[4, 4], tileMap[4, 3]));
            DoorDatas.Add(CreateDoorData(17, tileMap[4, 1], tileMap[4, 2]));
            DoorDatas.Add(CreateDoorData(18, tileMap[2, 4], tileMap[3, 4]));
            DoorDatas.Add(CreateDoorData(19, tileMap[2, 1], tileMap[3, 1]));
            DoorDatas.Add(CreateDoorData(20, tileMap[2, 4], tileMap[2, 3]));
            DoorDatas.Add(CreateDoorData(21, tileMap[2, 2], tileMap[2, 1]));
            DoorDatas.Add(CreateDoorData(22, tileMap[0, 3], tileMap[1, 3]));
            DoorDatas.Add(CreateDoorData(23, tileMap[0, 2], tileMap[1, 2]));
            DoorDatas.Add(CreateDoorData(24, tileMap[0, 3], TileDatas[1]));
            DoorDatas.Add(CreateDoorData(25, tileMap[0, 2], TileDatas[0]));


            var RoomDatas = new List<RoomData>
            {
                new(0, RoomTypeString.Pilot, 1, true),
                new(1, RoomTypeString.Empty, 0, false),
                new(2, RoomTypeString.Door, 1, false),
                new(3, RoomTypeString.Vision, 1, false),
                new(4, RoomTypeString.MedBay, 1, false),
                new(5, RoomTypeString.Shield, 2, true),
                new(6, RoomTypeString.Empty, 0, false),
                new(7, RoomTypeString.Empty, 0, false),
                new(8, RoomTypeString.Empty, 0, false),
                new(9, RoomTypeString.Empty, 0, false),
                new(10, RoomTypeString.Weapon, 3, true),
                new(11, RoomTypeString.Empty, 0, false),
                new(12, RoomTypeString.Empty, 0, false),
                new(13, RoomTypeString.Oxygen, 1, false),
                new(14, RoomTypeString.Engine, 0, true),
                new(15, RoomTypeString.Empty, 0, false),
                new(16, RoomTypeString.Empty, 0, false)
            };

            RoomDatas[0].ConsoleDirection = MoveDirection.Right;
            RoomDatas[5].ConsoleDirection = MoveDirection.Left;
            RoomDatas[10].ConsoleDirection = MoveDirection.Up;
            RoomDatas[14].ConsoleDirection = MoveDirection.Down;

            RoomDatas[0].TileCoords.Add(new TileCoord(14, 3));
            RoomDatas[0].TileCoords.Add(new TileCoord(14, 2));
            RoomDatas[0].CurrentAllocatedPower =1;

            RoomDatas[1].TileCoords.Add(new TileCoord(13, 3));
            RoomDatas[1].TileCoords.Add(new TileCoord(12, 3));
            RoomDatas[1].TileCoords.Add(new TileCoord(12, 2));
            RoomDatas[1].TileCoords.Add(new TileCoord(13, 2));

            RoomDatas[2].TileCoords.Add(new TileCoord(10, 3));
            RoomDatas[2].TileCoords.Add(new TileCoord(11, 3));
            RoomDatas[2].CurrentAllocatedPower = 1;   

            RoomDatas[3].TileCoords.Add(new TileCoord(11, 2));
            RoomDatas[3].TileCoords.Add(new TileCoord(10, 2));
            RoomDatas[3].CurrentAllocatedPower =1;
            RoomDatas[4].TileCoords.Add(new TileCoord(9, 4));
            RoomDatas[4].TileCoords.Add(new TileCoord(9, 3));
            RoomDatas[4].TileCoords.Add(new TileCoord(8, 3));
            RoomDatas[4].TileCoords.Add(new TileCoord(8, 4));

            RoomDatas[5].TileCoords.Add(new TileCoord(8, 2));
            RoomDatas[5].TileCoords.Add(new TileCoord(9, 2));
            RoomDatas[5].TileCoords.Add(new TileCoord(9, 1));
            RoomDatas[5].TileCoords.Add(new TileCoord(8, 1));

            RoomDatas[6].TileCoords.Add(new TileCoord(7, 5));
            RoomDatas[6].TileCoords.Add(new TileCoord(6, 5));

            RoomDatas[7].TileCoords.Add(new TileCoord(6, 4));
            RoomDatas[7].TileCoords.Add(new TileCoord(6, 3));
            RoomDatas[7].TileCoords.Add(new TileCoord(7, 3));
            RoomDatas[7].TileCoords.Add(new TileCoord(7, 4));

            RoomDatas[8].TileCoords.Add(new TileCoord(6, 2));
            RoomDatas[8].TileCoords.Add(new TileCoord(6, 1));
            RoomDatas[8].TileCoords.Add(new TileCoord(7, 1));
            RoomDatas[8].TileCoords.Add(new TileCoord(7, 2));

            RoomDatas[9].TileCoords.Add(new TileCoord(6, 0));
            RoomDatas[9].TileCoords.Add(new TileCoord(7, 0));

            RoomDatas[10].TileCoords.Add(new TileCoord(4, 3));
            RoomDatas[10].TileCoords.Add(new TileCoord(5, 3));
            RoomDatas[10].TileCoords.Add(new TileCoord(4, 2));
            RoomDatas[10].TileCoords.Add(new TileCoord(5, 2));

            RoomDatas[11].TileCoords.Add(new TileCoord(3, 4));
            RoomDatas[11].TileCoords.Add(new TileCoord(4, 4));

            RoomDatas[12].TileCoords.Add(new TileCoord(3, 1));
            RoomDatas[12].TileCoords.Add(new TileCoord(4, 1));

            RoomDatas[13].TileCoords.Add(new TileCoord(2, 4));
            RoomDatas[13].TileCoords.Add(new TileCoord(1, 4));

            RoomDatas[14].TileCoords.Add(new TileCoord(1, 3));
            RoomDatas[14].TileCoords.Add(new TileCoord(2, 3));
            RoomDatas[14].TileCoords.Add(new TileCoord(1, 2));
            RoomDatas[14].TileCoords.Add(new TileCoord(2, 2));

            RoomDatas[15].TileCoords.Add(new TileCoord(1, 1));
            RoomDatas[15].TileCoords.Add(new TileCoord(2, 1));

            RoomDatas[16].TileCoords.Add(new TileCoord(0, 3));
            RoomDatas[16].TileCoords.Add(new TileCoord(0, 2));

            var CrewDatas = new List<CrewData>();

            var crew1 = new CrewData(0, "Crew_Human", "john");
            crew1.CurrentX = 14;
            crew1.CurrentY = 3;
            CrewDatas.Add(crew1);

            var crew2 = new CrewData(1, "Crew_Human", "mane");
            crew2.CurrentX = 4;
            crew2.CurrentY = 3;
            CrewDatas.Add(crew2);

            var crew3 = new CrewData(2, "Crew_Human", "Tyler");
            crew3.CurrentX = 1;
            crew3.CurrentY = 2;
            CrewDatas.Add(crew3);

            var WeaponData = new List<WeaponData>();
            var weapon1 = new WeaponData("Burst_Laser");
            WeaponData.Add(weapon1);

            var ShieldData = new ShieldData();
            ShieldData.ChargeGauge= 0;
            ShieldData.CurrentShieldCount = 1;

            data.Resources = new ResourceData { Fuel = 3, Missiles = 8, Drones = 2 };

            data.Tiles = TileDatas;
            data.Rooms = RoomDatas;
            data.Doors = DoorDatas;
            data.Crews = CrewDatas;
            data.EquippedWeapons = WeaponData;

            SaveLoadManager.Save(gameSaveData, SaveFileName);
        }

        private void ConnectOpenTile(TileData tileA, TileData tileB)
        {
            tileA.ConnectedNeighborCoords.Add(new TileCoord(tileB.X, tileB.Y));
            tileB.ConnectedNeighborCoords.Add(new TileCoord(tileA.X, tileA.Y));
        }

        private DoorData CreateDoorData(int doorID, TileData tileA, TileData tileB)
        {
            return new DoorData(doorID, new TileCoord(tileA.X, tileA.Y), new TileCoord(tileB.X, tileB.Y));
        }
    }
}