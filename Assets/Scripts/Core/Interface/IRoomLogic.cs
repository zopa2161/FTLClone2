using System;
using System.Collections.Generic;
using Core.Data.SpaceShip;

namespace Core.Interface
{
    public interface IRoomLogic
    {
        int RoomID { get; }

        RoomData Data { get; }

        float AverageOxygen { get; }

        //===전력량===
        int CurrentPower { get; }
        int MaxPowerCapacity { get; }

        IReadOnlyList<TileCoord> GetRoomTiles();
        bool IsWorkingTile(TileCoord coord);
        void ChangePower(int amount);


        event Action<int, int> OnPowerChanged;
        event Action<float> OnOxygenChanged;
    }
}