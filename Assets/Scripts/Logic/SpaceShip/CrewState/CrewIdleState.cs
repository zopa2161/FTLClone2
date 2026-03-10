namespace Logic.SpaceShip.CrewState
{
    public class CrewIdleState : ICrewState
    {
        public void Enter(CrewLogic crew)
        {
            // 대기 애니메이션 무전 발송
            // crew.TriggerStateEvent("Idle");
        }

        public void Execute(CrewLogic crew)
        {
            // 💡 대기 중에는 할 일이 없습니다. (나중에 방의 산소가 없으면 질식 상태로 전환하는 등 환경 감지를 여기서 합니다)
        }

        public void Exit(CrewLogic crew)
        {
        }
    }
}