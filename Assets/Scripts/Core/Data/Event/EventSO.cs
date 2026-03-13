using UnityEngine;

namespace Core.Data.Event
{
    [CreateAssetMenu(fileName = "NewEvent", menuName = "FTL/Event/Event")]
    public class EventSO : ScriptableObject
    {
        [Tooltip("NodeData.EventID와 매칭되는 고유 식별자")]
        public string EventID;
        public string Title;

        [Tooltip("이벤트 진입 시 첫 번째로 실행되는 세부 이벤트")]
        public SubEventBaseSO StartEvent;
    }
}
