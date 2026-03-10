using System.Collections.Generic;

namespace Core.Interface
{
    public interface IShipAPI : IGridMap
    {
        IReadOnlyList<ICrewLogic> GetAllCrews();
        List<IWeaponLogic> GetAllWeapons();
    }
}