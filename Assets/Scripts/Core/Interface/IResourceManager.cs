using System;

namespace Core.Interface
{
    public interface IResourceManager
    {
        int Fuel { get; }
        int Missiles { get; }
        int Drones { get; }

        event Action<int> OnFuelChanged;
        event Action<int> OnMissilesChanged;
        event Action<int> OnDronesChanged;

        bool TryConsumeFuel(int amount);
        bool TryConsumeMissiles(int amount);
        bool TryConsumeDrones(int amount);

        void AddFuel(int amount);
        void AddMissiles(int amount);
        void AddDrones(int amount);
    }
}
