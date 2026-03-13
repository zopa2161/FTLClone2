using System.Collections.Generic;
using UnityEngine;

namespace Core.Data.Event
{
    [CreateAssetMenu(fileName = "EventDatabase", menuName = "FTL/Event/EventDatabase")]
    public class EventDatabaseSO : ScriptableObject
    {
        public List<EventSO>          Events       = new();
        public List<DialogSubEventSO> DialogEvents = new();
        public List<CombatSubEventSO> CombatEvents = new();
        public List<RewardSubEventSO> RewardEvents = new();

        public void Add(EventSO e)             { if (!Events.Contains(e))       Events.Add(e); }
        public void Remove(EventSO e)          { Events.Remove(e); }

        public void Add(DialogSubEventSO e)    { if (!DialogEvents.Contains(e)) DialogEvents.Add(e); }
        public void Remove(DialogSubEventSO e) { DialogEvents.Remove(e); }

        public void Add(CombatSubEventSO e)    { if (!CombatEvents.Contains(e)) CombatEvents.Add(e); }
        public void Remove(CombatSubEventSO e) { CombatEvents.Remove(e); }

        public void Add(RewardSubEventSO e)    { if (!RewardEvents.Contains(e)) RewardEvents.Add(e); }
        public void Remove(RewardSubEventSO e) { RewardEvents.Remove(e); }

        public SubEventBaseSO GetSubEvent(string subEventID)
        {
            foreach (var e in DialogEvents) if (e != null && e.SubEventID == subEventID) return e;
            foreach (var e in CombatEvents) if (e != null && e.SubEventID == subEventID) return e;
            foreach (var e in RewardEvents) if (e != null && e.SubEventID == subEventID) return e;
            return null;
        }
    }
}
