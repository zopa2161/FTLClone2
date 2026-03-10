using System;

namespace Core.Data.SpaceShip
{
    [Serializable]
    public struct TileCoord
    {
        public int X;
        public int Y;

        public TileCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(TileCoord a, TileCoord b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(TileCoord a, TileCoord b)
        {
            // 이미 만들어둔 == 연산자를 반전시켜서 재사용합니다.
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TileCoord)) return false;
            var other = (TileCoord)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            // .NET Core / 최신 유니티에서 해시 충돌을 막아주는 가장 깔끔한 방법
            return HashCode.Combine(X, Y);
        }
    }
}