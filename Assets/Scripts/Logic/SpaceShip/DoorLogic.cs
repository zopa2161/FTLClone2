using System;
using Core.Data.SpaceShip;
using Core.Interface;

namespace Logic.SpaceShip
{
    public class DoorLogic : IDoorLogic, ITickable
    {
        private const int AUTO_CLOSE_DELAY_TICKS = 5; // 3틱(예: 0.3초) 뒤에 닫힘

        private int _autoCloseTimer;


        public TileLogic TileA;
        public TileLogic TileB;

        public DoorData Data { get; private set; }

        public bool IsOpen => Data.IsOpen;
        public event Action<bool> OnDoorStateChanged;
        public int DoorID => Data.DoorID;

        public void ToggleDoorManual()
        {
            // 토글 상태를 뒤집습니다. (강제 개방 <-> 강제 개방 해제)
            Data.IsForcedOpen = !Data.IsForcedOpen;

            if (Data.IsForcedOpen)
            {
                // 강제로 열었으니 즉시 문을 열고 무전을 칩니다.
                Data.IsOpen = true;
                OnDoorStateChanged?.Invoke(true);
            }
            else
            {
                // 강제 개방을 풀었으니 즉시 문을 닫습니다.
                Data.IsOpen = false;
                OnDoorStateChanged?.Invoke(false);
            }
        }

        public void SetDoorState(bool open)
        {
            // 이미 유저가 강제로 열어둔 문이라면, 승무원이 건드릴 필요가 없습니다!
            if (Data.IsForcedOpen) return;

            if (Data.IsOpen == open)
            {
                if (open) _autoCloseTimer = AUTO_CLOSE_DELAY_TICKS; // 꼬리물기 방지 리셋
                return;
            }

            Data.IsOpen = open;
            if (open) _autoCloseTimer = AUTO_CLOSE_DELAY_TICKS;

            OnDoorStateChanged?.Invoke(open);
        }


        public void OnTickUpdate()
        {
            if (!Data.IsOpen) return;

            // 🌟 작성자님의 핵심 아이디어: 강제로 열어둔 상태면 타이머가 흐르지 않습니다!
            if (Data.IsForcedOpen) return;

            if (_autoCloseTimer > 0)
                _autoCloseTimer--;
            else if (_autoCloseTimer <= 0) SetDoorState(false);
        }


        public void Initialize(TileLogic tileA, TileLogic tileB, DoorData doorData)
        {
            TileA = tileA;
            TileB = tileB;
            Data = doorData;
        }

        public TileLogic GetConnectedTile(TileLogic tile)
        {
            return tile == TileA ? TileB : TileA;
        }
    }
}