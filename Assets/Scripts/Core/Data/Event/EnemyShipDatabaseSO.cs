using System.Collections.Generic;
using UnityEngine;

namespace Core.Data.Event
{
    [CreateAssetMenu(fileName = "EnemyShipDatabase", menuName = "FTL/Event/EnemyShipDatabase")]
    public class EnemyShipDatabaseSO : ScriptableObject
    {
        public List<EnemyShipSO> Ships = new();

        public void Add(EnemyShipSO e)    { if (!Ships.Contains(e)) Ships.Add(e); }
        public void Remove(EnemyShipSO e) { Ships.Remove(e); }
    }
}
