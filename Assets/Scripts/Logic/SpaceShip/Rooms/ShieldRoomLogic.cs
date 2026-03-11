using Core.Data.SpaceShip;

namespace Logic.SpaceShip.Rooms
{
    /// <summary>실드 방 로직. 전력/산소/승무원 근무는 BaseRoomLogic에 위임.</summary>
    public class ShieldRoomLogic : BaseRoomLogic
    {
        public ShieldRoomLogic(RoomData roomData) : base(roomData)
        {
        }
    }
}
