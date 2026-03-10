using System;
using System.Collections.Generic;
using Core.Data.SpaceShip;
using Core.Interface;

namespace Logic.SpaceShip.Rooms
{
    public abstract class BaseRoomLogic : IRoomLogic, ITickable
    {
        private float _lastReportedOxygen = -1f;

        private List<TileLogic> _tileLogics;


        public BaseRoomLogic(RoomData roomData)
        {
            Data = roomData;
        }

        public event Action<int, int> OnPowerChanged;
        public event Action<float> OnOxygenChanged;

        public RoomData Data { get; }


        public int CurrentPower => Data.CurrentAllocatedPower;
        public int MaxPowerCapacity => Data.MaxPower;


        public int RoomID => Data.RoomID;

        public IReadOnlyList<TileCoord> GetRoomTiles()
        {
            return Data.TileCoords;
        }

        public float AverageOxygen
        {
            get
            {
                if (_tileLogics.Count == 0) return 0f;
                var result = 0f;
                foreach (var tile in _tileLogics) result += tile.Data.OxygenLevel;
                return result / _tileLogics.Count;
            }
        }


        public bool IsWorkingTile(TileCoord coord)
        {
            return Data.TileCoords[0].X == coord.X && Data.TileCoords[0].Y == coord.Y;
        }

        //===전력===
        public void ChangePower(int amount)
        {
            // 데이터 갱신
            Data.CurrentAllocatedPower += amount;

            // 📢 뷰(View)와 시스템에 "내 전력 바뀌었어!" 라고 무전 발송
            OnPowerChanged?.Invoke(CurrentPower, MaxPowerCapacity);

            // (참고) 나중에 여기서 전력이 0이 되었을 때 
            // 방의 기능을 끄는 로직(예: 산소 생산 중단)을 트리거할 수 있습니다.
        }


        public void OnTickUpdate()
        {
            if (_tileLogics.Count == 0) return;
            OnOxygenChanged?.Invoke(AverageOxygen);
        }

        public void Initialize(List<TileLogic> tileLogics)
        {
            _tileLogics = tileLogics;
        }
    }
}