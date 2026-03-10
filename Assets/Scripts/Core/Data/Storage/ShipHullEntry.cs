using System;
using UnityEngine;

namespace Core.Data.Storage
{
    [Serializable]
    public struct ShipHullEntry
    {
        public string HullID; // 예: "Ship_Kestrel"
        public GameObject HullPrefab; // 실제 십자가 모양 우주선 프리팹
    }
}