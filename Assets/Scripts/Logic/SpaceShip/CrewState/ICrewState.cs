namespace Logic.SpaceShip.CrewState
{
    public interface ICrewState
    {
        // 상태에 진입할 때 딱 한 번 실행
        void Enter(CrewLogic crew);

        // 🌟 매 틱(Tick)마다 실행될 핵심 행동
        void Execute(CrewLogic crew);

        // 다른 상태로 넘어갈 때 딱 한 번 실행 (정리 작업)
        void Exit(CrewLogic crew);
    }
}