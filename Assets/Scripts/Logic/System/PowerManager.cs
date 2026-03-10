using System;
using System.Collections.Generic;
using Core.Interface;

namespace Logic.System
{
    public class PowerManager : IPowerSystem
    {
        private readonly Dictionary<int, IRoomLogic> _rooms = new();

        public int MaxReactorPower { get; private set; }

        public int AvailableReactorPower { get; private set; }

        public event Action<int, int> OnReactorPowerChanged;

        public bool TryAddPowerToRoom(int roomID)
        {
            if (!_rooms.TryGetValue(roomID, out var room)) return false;

            // 검사: 발전기에 남은 전력이 있고 && 방의 한도를 초과하지 않았는가?
            if (AvailableReactorPower > 0 && room.CurrentPower < room.MaxPowerCapacity)
            {
                AvailableReactorPower--; // 중앙 잔고 감소
                room.ChangePower(1); // 🌟 방에게 전력 +1 지시

                // 총 전력량이 변했음을 허공에 방송 (하단 중앙 UI 갱신용)
                OnReactorPowerChanged?.Invoke(AvailableReactorPower, MaxReactorPower);
                return true; // 성공!
            }

            return false;
        }

        public bool TryRemovePowerFromRoom(int roomID)
        {
            if (!_rooms.TryGetValue(roomID, out var room)) return false;

            // 검사: 이 방에 빼낼 전력이 있는가?
            if (room.CurrentPower > 0)
            {
                AvailableReactorPower++; // 중앙 잔고 환불
                room.ChangePower(-1); // 🌟 방에게 전력 -1 지시

                OnReactorPowerChanged?.Invoke(AvailableReactorPower, MaxReactorPower);
                return true; // 성공!
            }

            return false; // 뺄 전력이 없어서 실패
        }

        public void Initialize(int maxPower, IEnumerable<IRoomLogic> rooms)
        {
            MaxReactorPower = maxPower;
            AvailableReactorPower = maxPower; // 일단 최대치로 채웁니다.

            foreach (var room in rooms)
            {
                _rooms[room.Data.RoomID] = room;

                // 세이브 파일에서 불러온 데이터에 이미 할당된 전력이 있다면 그만큼 빼줍니다.
                AvailableReactorPower -= room.CurrentPower;
            }
        }
    }
}