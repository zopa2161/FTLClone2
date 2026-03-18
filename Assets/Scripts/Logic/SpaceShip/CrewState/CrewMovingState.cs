using Core.enums;

namespace Logic.SpaceShip.CrewState
{
    public class CrewMovingState : ICrewState
    {
        public CrewStateType StateType => CrewStateType.Moving;

        private const int TickForMove = 3;
        private int movingCount;

        public void Enter(CrewLogic crew)
        {
            movingCount = TickForMove;
            // 이동 시작 애니메이션을 틀라고 뷰에게 무전을 칠 수 있습니다.
            // crew.TriggerStateEvent("Move");
        }

        public void Execute(CrewLogic crew)
        {
            // 💡 이전에 CrewLogic.OnTickUpdate() 에 있던 길찾기 이동 코드가 그대로 들어옵니다!
            if (crew.HasPath())
            {
                var nextStep = crew.PeekNextPath();

                // 문 검사 로직
                var door = crew.GridMap.GetDoorBetween(crew.CurrentCoord, nextStep);
                if (door != null && !door.IsOpen) door.SetDoorState(true);
                //return; // 이번 틱은 문 여는 데 사용
                movingCount--;

                if (movingCount <= 0)
                {
                    crew.MoveToNextPath();
                    movingCount = TickForMove;
                }
                // 실제 이동
            }
            else
            {
                // 🏁 목적지 타일에 완전히 도착했습니다!

                // 1. 내가 서 있는 좌표의 방을 가져옵니다.
                var currentRoom = crew.GridMap.GetRoomAt(crew.CurrentCoord);

                // 2. 불이 나고 있으면 진압 상태 우선
                if (currentRoom != null && currentRoom.IsOnFire)
                {
                    crew.ChangeState(new CrewFireFightingState());
                }
                // 3. 💡 이 방이 존재하고, 여기가 '작업 타일(0번 타일)'이라면?
                else if (currentRoom != null && currentRoom.IsWorkingTile(crew.CurrentCoord))
                {
                    // 작업 상태로 돌입!
                    if (currentRoom.Data.CurrentAllocatedPower > 0) crew.ChangeState(new CrewWorkingState(currentRoom));
                    else
                        crew.ChangeState(new CrewIdleState());
                }
                else
                {
                    // 빈 방이거나, 1번 타일이거나, 복도라면 그냥 대기 상태로 돌입!
                    crew.ChangeState(new CrewIdleState());
                }
            }
        }

        public void Exit(CrewLogic crew)
        {
            // 이동 종료 처리 (예: 발소리 끄기 등)
        }
    }
}