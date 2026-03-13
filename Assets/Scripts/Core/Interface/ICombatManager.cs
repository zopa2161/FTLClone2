using System;

namespace Core.Interface
{
    public interface ICombatManager
    {
        bool IsInCombat { get; }
        event Action<bool> OnCombatStateChanged;
        void SetCombatState(bool isInCombat);
    }
}
