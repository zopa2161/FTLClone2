using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Data.Event
{
    public enum RewardType
    {
        Scrap,
        Fuel,
        Missiles,
        Drones,
        Weapon,
        MaxReactorPower
    }

    [Serializable]
    public class RewardEntry
    {
        public RewardType Type;
        [Tooltip("음수 = 자원 소모")]
        public int Amount;
        [Tooltip("Type == Weapon일 때만 사용하는 무기 ID")]
        public string WeaponID;
    }

    [CreateAssetMenu(fileName = "NewRewardSubEvent", menuName = "FTL/Event/RewardSubEvent")]
    public class RewardSubEventSO : SubEventBaseSO
    {
        public List<RewardEntry> Rewards = new();

        [Tooltip("보상 지급 후 진행할 세부 이벤트 (null이면 종료)")]
        public SubEventBaseSO NextEvent;
    }
}
