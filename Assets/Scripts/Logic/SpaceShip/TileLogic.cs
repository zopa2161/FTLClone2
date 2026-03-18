using System.Collections.Generic;
using Core.Data.SpaceShip;
using Core.Interface;
using System;
namespace Logic.SpaceShip
{
    public class TileLogic : ITileLogic
    {
        public List<TileLogic> Neighbors = new();
        public TileData Data { get; private set; }

        public TileCoord TileCoord => new(Data.X, Data.Y);

        public float OxygenLevel
        {
            get => Data.OxygenLevel;
            set => Data.OxygenLevel = value;
        }

        public float BreachLevel
        {
            get => Data.BreachLevel;
            set => Data.BreachLevel = value;
        }

        public event Action<float> OnFireChanged;

        public float FireLevel
        {
            get => Data.FireLevel;
            set
            {
                Data.FireLevel = value;
                OnFireChanged?.Invoke(value);
            }
        }

        public void Initialize(TileData data)
        {
            Data = data;
        }
    }
}