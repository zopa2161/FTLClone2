using System;

namespace Core.Interface
{
    public interface IDoorLogic
    {
        int DoorID { get; }
        bool IsOpen { get; }

        event Action<bool> OnDoorStateChanged;

        void SetDoorState(bool open);
        void ToggleDoorManual();
    }
}