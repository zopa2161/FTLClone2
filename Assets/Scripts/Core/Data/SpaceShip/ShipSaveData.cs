using System;
using System.Collections.Generic;
using Core.Data.Weapon;
using Core.Data;

namespace Core.Data.SpaceShip
{
    [Serializable]
    public class ShipSaveData
    {
        public string Version = "1.0.0"; // 규격 버전 관리용

        //프리팹 ID
        public string ShipHullID;

        //===전력===
        public int MaxReactorPower = 10;


        //===실드===
        public ShieldData Shield = new();

        //===자원===
        public ResourceData Resources = new ResourceData();

        //===무기===
        public int MaxWeaponSlots = 4;

        public List<WeaponData> EquippedWeapons = new();

        // 모든 객체는 계층 없이 1차원 배열(List)로 납작하게 저장됩니다.
        public List<TileData> Tiles = new();
        public List<RoomData> Rooms = new();
        public List<DoorData> Doors = new();
        public List<CrewData> Crews = new();
    }
}