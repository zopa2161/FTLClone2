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

        bool IsOnFire { get; }

        //===전력량===
        int CurrentPower { get; }
        int MaxPowerCapacity { get; }

        IReadOnlyList<TileCoord> GetRoomTiles();
        bool IsWorkingTile(TileCoord coord);
        void ChangePower(int amount);

        //=== 승무원 근무 관련
        // 💡 현재 이 방에서 근무 중인 승무원이 있는가?
        bool IsManned { get; }

        // 📢 근무 상태가 변했을 때 발송할 무전기
        event System.Action<bool> OnMannedStatusChanged;

        // 💡 승무원이 콘솔에 앉거나 일어날 때 호출할 스위치
        void ChangeWorkingCrewCount(bool isManned);

        event Action<int, int> OnPowerChanged;
        event Action<float> OnOxygenChanged;
    }
}