using Core.Data.SpaceShip;

namespace Logic.SpaceShip.Rooms
{
    public class OxygenRoomLogic : BaseRoomLogic
    {
        private const float OxygenFactor = 0.5f;

        public OxygenRoomLogic(RoomData roomData) : base(roomData)
        {
        }

        public float GetOxygenGeneration()
        {
            if ((Data.DestructionLevel > 0 && Data.DestructionLevel < 60) || Data.CurrentAllocatedPower == 0) return -1;
            switch (Data.CurrentAllocatedPower)
            {
                case 1:
                    return 1;
                case 2:
                    return 3;
                case 3:
                    return 6;
                default:
                    return 1;
            }
        }
    }
}