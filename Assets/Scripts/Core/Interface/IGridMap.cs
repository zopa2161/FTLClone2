using System.Collections.Generic;
using Core.Data.SpaceShip;

namespace Core.Interface
{
    public interface IGridMap
    {
        IReadOnlyList<ITileLogic> GetAllTiles();
        IReadOnlyList<IRoomLogic> GetAllRooms();
        IReadOnlyList<IDoorLogic> GetAllDoors();
        List<TileCoord> GetConnectedNeighbors(TileCoord current);
        IDoorLogic GetDoorBetween(TileCoord a, TileCoord b);
        IRoomLogic GetRoomAt(TileCoord coord);
        ITileLogic GetTileAt(TileCoord coord);
    }
}