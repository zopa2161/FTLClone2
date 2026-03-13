using System;

namespace Core.Interface
{
    public interface IResourceManager
    {
        int Fuel { get; }
        int Missiles { get; }
        int Drones { get; }
        int Scrap { get; }

        event Action<int> OnFuelChanged;
        event Action<int> OnMissilesChanged;
        event Action<int> OnDronesChanged;
        event Action<int> OnScrapChanged;

        bool TryConsumeFuel(int amount);
        bool TryConsumeMissiles(int amount);
        bool TryConsumeDrones(int amount);
        bool TryConsumeScrap(int amount);

        void AddFuel(int amount);
        void AddMissiles(int amount);
        void AddDrones(int amount);
        void AddScrap(int amount);
    }
}
