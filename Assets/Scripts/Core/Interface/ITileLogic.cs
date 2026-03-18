using Core.Data.SpaceShip;

namespace Core.Interface
{
    public interface ITileLogic
    {
        TileCoord TileCoord { get; }

        float OxygenLevel { get; set; }

        float BreachLevel { get; set; }

        float FireLevel { get; set; }

        event System.Action<float> OnFireChanged;
    }
}