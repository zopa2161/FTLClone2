using System;

namespace Core.Interface
{
    public interface IPowerSystem
    {
        int MaxReactorPower { get; }
        int AvailableReactorPower { get; }

        event Action<int, int> OnReactorPowerChanged;


        bool TryAddPowerToRoom(int roomID);
        bool TryRemovePowerFromRoom(int roomID);
    }
}