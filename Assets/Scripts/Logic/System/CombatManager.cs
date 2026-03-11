using System;
using Core.Interface;

namespace Logic.System
{
    public class CombatManager : ICombatManager
    {
        public bool IsInCombat { get; private set; }
        public event Action<bool> OnCombatStateChanged;

        public void SetCombatState(bool isInCombat)
        {
            if (IsInCombat == isInCombat) return;
            IsInCombat = isInCombat;
            OnCombatStateChanged?.Invoke(IsInCombat);
        }
    }
}
