using System;
using Core.Data.SpaceShip;
using Core.Interface;

namespace Logic.System
{
    public class ResourceManager : IResourceManager
    {
        public int Fuel { get; private set; }
        public int Missiles { get; private set; }
        public int Drones { get; private set; }
        public int Scrap { get; private set; }

        public event Action<int> OnFuelChanged;
        public event Action<int> OnMissilesChanged;
        public event Action<int> OnDronesChanged;
        public event Action<int> OnScrapChanged;

        private ResourceData _data;

        public void Initialize(ResourceData data)
        {
            _data = data;
            Fuel = data.Fuel;
            Missiles = data.Missiles;
            Drones = data.Drones;
            Scrap = data.Scrap;
        }

        public bool TryConsumeFuel(int amount)
        {
            if (Fuel < amount) return false;
            Fuel -= amount;
            _data.Fuel = Fuel;
            OnFuelChanged?.Invoke(Fuel);
            return true;
        }

        public bool TryConsumeMissiles(int amount)
        {
            if (Missiles < amount) return false;
            Missiles -= amount;
            _data.Missiles = Missiles;
            OnMissilesChanged?.Invoke(Missiles);
            return true;
        }

        public bool TryConsumeDrones(int amount)
        {
            if (Drones < amount) return false;
            Drones -= amount;
            _data.Drones = Drones;
            OnDronesChanged?.Invoke(Drones);
            return true;
        }

        public void AddFuel(int amount)     { Fuel += amount;     _data.Fuel = Fuel;         OnFuelChanged?.Invoke(Fuel); }
        public void AddMissiles(int amount) { Missiles += amount; _data.Missiles = Missiles; OnMissilesChanged?.Invoke(Missiles); }
        public void AddDrones(int amount)   { Drones += amount;   _data.Drones = Drones;     OnDronesChanged?.Invoke(Drones); }

        public bool TryConsumeScrap(int amount)
        {
            if (Scrap < amount) return false;
            Scrap -= amount;
            _data.Scrap = Scrap;
            OnScrapChanged?.Invoke(Scrap);
            return true;
        }

        public void AddScrap(int amount) { Scrap += amount; _data.Scrap = Scrap; OnScrapChanged?.Invoke(Scrap); }
    }
}
