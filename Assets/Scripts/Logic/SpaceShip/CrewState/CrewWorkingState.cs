using Core.Interface;
using UnityEngine;

namespace Logic.SpaceShip.CrewState
{
    public class CrewWorkingState : ICrewState
    {
        private IRoomLogic _workingRoom;

        // 상태를 생성할 때, 어느 방에서 일하는지 알려줍니다.
        public CrewWorkingState(IRoomLogic room)
        {
            _workingRoom = room;
        }

        public void Enter(CrewLogic crew)
        {
            // 🌟 1. 방에 설정된 콘솔 방향으로 강제 회전 지시!
            crew.LookAt(_workingRoom.Data.ConsoleDirection);

            _workingRoom = crew.GridMap.GetRoomAt(crew.CurrentCoord);
            
            if (_workingRoom != null)
            {
                // 2. 방에게 "나 일 시작했음!" (+1) 보고합니다.
                _workingRoom.ChangeWorkingCrewCount(true);
            }
            
            
            // 2. 방 로직에게 "나 여기서 일 시작했소" 알림 (회피율 증가, 무기 충전 속도 증가 등)
            // _workingRoom.SetManned(true); 

            Debug.Log($"[{crew.Data.CrewName}] {_workingRoom.Data.RoomType} 방에서 작업 시작!");
        }

        public void Execute(CrewLogic crew)
        {
            // 💡 매 틱마다 작업 숙련도가 오르거나, 수리 게이지가 차오르는 로직이 여기 들어갑니다.
            // crew.Data.PilotingSkill += 0.01f;
        }

        public void Exit(CrewLogic crew)
        {
            // 방에게 "나 일 그만두고 간다" 알림 (부스트 효과 제거)
            _workingRoom.ChangeWorkingCrewCount(false);
            
            Debug.Log($"[{crew.Data.CrewName}] 작업 중단.");
        }
    }
}