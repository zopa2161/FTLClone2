using Core.Data.SpaceShip;
using Logic.SpaceShip.Rooms;

namespace Logic.SpaceShip
{
    public static class RoomLogicFactory
    {
        public static BaseRoomLogic CreateRoomLogic(RoomData roomData)
        {
            switch (roomData.RoomType)
            {
                case "Oxygen":
                    return new OxygenRoomLogic(roomData);
                case "Engine":
                    return new EngineRoomLogic(roomData);
                case "Pilot":
                    return new PilotRoomLogic(roomData);
                case "Weapon":
                    return new WeaponRoomLogic(roomData);
                case "Shield":
                    return new ShieldRoomLogic(roomData);
                case "Empty":
                default:
                    return new EmptyRoomLogic(roomData);
            }
        }
    }
}