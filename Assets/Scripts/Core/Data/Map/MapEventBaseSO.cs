using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Data.Map
{
    public enum EventType
    {
        Combat,
        Choice,
        Shop,
        Empty,
        Distress
    }

    [Serializable]
    public class EventChoiceData
    {
        public string ChoiceText;
        public string OutcomeDescription;
    }

    [CreateAssetMenu(fileName = "NewMapEvent", menuName = "Map/Map Event")]
    public class MapEventBaseSO : ScriptableObject
    {
        [Header("식별")]
        public string EventID;
        public string Title;
        [TextArea(2, 5)]
        public string Description;

        [Header("이벤트 타입")]
        public EventType Type;

        [Header("선택지 (Choice 타입일 때 사용)")]
        public List<EventChoiceData> Choices = new List<EventChoiceData>();
    }
}
