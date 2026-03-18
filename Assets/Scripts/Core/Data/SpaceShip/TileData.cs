using System;
using System.Collections.Generic;

namespace Core.Data.SpaceShip
{
    [Serializable]
    public class TileData
    {
        public int X;
        public int Y;
        public float OxygenLevel = 100f; // 기본값 100

        public float BreachLevel;
        public float FireLevel = 0f; // 0 = 불 없음, 0~100 = 화재 강도
        public List<TileCoord> ConnectedNeighborCoords = new();


        public TileData(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}