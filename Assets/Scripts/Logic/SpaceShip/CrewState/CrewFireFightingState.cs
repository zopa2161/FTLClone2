using Core.enums;
using UnityEngine;

namespace Logic.SpaceShip.CrewState
{
    public class CrewFireFightingState : ICrewState
    {
        public CrewStateType StateType => CrewStateType.FireFighting;

        private const float EXTINGUISH_RATE = 0.5f; // 틱당 타일 FireLevel 감소량

        public void Enter(CrewLogic crew)
        {
            Debug.Log($"[{crew.Data.CrewName}] 화재 진압 시작!");
        }

        public void Execute(CrewLogic crew)
        {
            var room = crew.GridMap.GetRoomAt(crew.CurrentCoord);
            if (room == null)
            {
                crew.ChangeState(new CrewIdleState());
                return;
            }

            // 방 내 모든 타일의 FireLevel 감소
            foreach (var coord in room.GetRoomTiles())
            {
                var tile = crew.GridMap.GetTileAt(coord);
                if (tile.FireLevel > 0f)
                    tile.FireLevel = Mathf.Max(0f, tile.FireLevel - EXTINGUISH_RATE);
            }

            // 불이 완전히 꺼지면 적절한 상태로 전환
            if (!room.IsOnFire)
            {
                Debug.Log($"[{crew.Data.CrewName}] 화재 진압 완료!");
                if (room.IsWorkingTile(crew.CurrentCoord) && room.Data.CurrentAllocatedPower > 0)
                    crew.ChangeState(new CrewWorkingState(room));
                else
                    crew.ChangeState(new CrewIdleState());
            }
        }

        public void Exit(CrewLogic crew)
        {
        }
    }
}
