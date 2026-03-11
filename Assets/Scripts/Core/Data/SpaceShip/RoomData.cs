using System;
using System.Collections.Generic;
using Core.enums;

namespace Core.Data.SpaceShip
{
    [Serializable]
    public class RoomData
    {
        public int RoomID; // 고유 번호 (예: 0, 1, 2...)
        public string RoomType; // 역할 명찰 (예: "Pilot", "Oxygen", "Empty")

        public int MaxPower; // 최대 할당 가능 전력
        public int CurrentAllocatedPower; // 현재 할당된 전력
        public int CurrentAllocateToWeapon = 0; //무기 방을 위한 전용 
        public bool ISManned;

        //고장난 정도
        //0,30,60을 임계점으로 정해 놓고 ㅇㅇ
        public float DestructionLevel;

        public bool IsMannable;

        // 💡 0번 타일에서 작업을 시작할 때 바라봐야 할 방향 (콘솔의 위치)
        public MoveDirection ConsoleDirection;

        // 이 방이 소유하고 있는 타일들의 좌표 목록
        public List<TileCoord> TileCoords = new();

        public RoomData(int roomID, string roomType, int maxPower, bool isMannable)
        {
            RoomID = roomID;
            RoomType = roomType;
            MaxPower = maxPower;
            CurrentAllocatedPower = 0;
            IsMannable = isMannable;
        }
    }
}