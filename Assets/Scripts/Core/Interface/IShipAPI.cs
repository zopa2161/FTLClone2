using System;
using System.Collections.Generic;

namespace Core.Interface
{
    public interface IShipAPI : IGridMap
    {
        IReadOnlyList<ICrewLogic> GetAllCrews();
        List<IWeaponLogic> GetAllWeapons();
        IShieldLogic GetShieldLogic();
        int MaxHullHealth { get; }
        int CurrentHullHealth { get; }
        event Action<int, int> OnHullHealthChanged; // (current, max)
        void TakeDamage(int damage);
    }
}