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
        public List<TileCoord> ConnectedNeighborCoords = new();


        public TileData(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}