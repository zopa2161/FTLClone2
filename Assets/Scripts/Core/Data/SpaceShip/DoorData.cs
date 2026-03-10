using System;

namespace Core.Data.SpaceShip
{
    [Serializable]
    public class DoorData
    {
        public int DoorID;
        public bool IsOpen;
        public bool IsForcedOpen;


        // 문이 연결하고 있는 양쪽 타일의 정확한 좌표
        public TileCoord TileA;
        public TileCoord TileB;

        public DoorData(int doorID, TileCoord tileA, TileCoord tileB)
        {
            DoorID = doorID;
            TileA = tileA;
            TileB = tileB;
        }
    }
}