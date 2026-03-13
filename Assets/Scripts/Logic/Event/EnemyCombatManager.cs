using System;
using System.Collections.Generic;
using Core.Data.Event;
using Core.Interface;
using Logic.SpaceShip;
using Logic.System;
using UnityEngine;

namespace Logic.Event
{
    /// <summary>
    /// CombatSubEventSO 시작 시 적군 우주선 Logic을 조립하고 SimulationCore에 등록합니다.
    /// 매 틱마다 적군 체력을 확인하여 0 이하가 되면 전투를 종료하고 EventLogicManager에 통보합니다.
    /// ITickable 구현 — 승리 조건 체크용으로 자신도 simCore에 등록됩니다.
    /// MonoBehaviour 없음 — 순수 C#.
    /// </summary>
    public class EnemyCombatManager : IEnemyCombatManager, ITickable
    {
        public IShipAPI EnemyShipAPI => _enemyShipAPI;
        public event Action OnCombatEnded;

        private IShipAPI _enemyShipAPI;
        private bool _ended;

        private readonly IEventLogic _eventLogic;
        private readonly SimulationCore _simCore;
        private readonly List<ITickable> _enemyTickables = new();

        public EnemyCombatManager(IEventLogic eventLogic, SimulationCore simCore)
        {
            _eventLogic = eventLogic;
            _simCore = simCore;
        }

        /// <summary>
        /// 적군 우주선을 EnemyShipSO의 ShipData로부터 조립하고 SimulationCore에 등록합니다.
        /// </summary>
        public void StartCombat(CombatSubEventSO combatEvent)
        {
            if (combatEvent.EnemyShip == null)
            {
                Debug.LogWarning("[EnemyCombatManager] CombatSubEventSO에 EnemyShip이 없습니다.");
                return;
            }

            _ended = false;
            _enemyTickables.Clear();

            // 적군 Logic 조립 (View 없음 — 순수 Logic만)
            var builder = new GridBuilder();
            _enemyShipAPI = builder.Rebuild(combatEvent.EnemyShip.ShipData);

            // 적군 tickable 수집
            foreach (var room in _enemyShipAPI.GetAllRooms())
                if (room is ITickable t) _enemyTickables.Add(t);

            foreach (var door in _enemyShipAPI.GetAllDoors())
                if (door is ITickable t) _enemyTickables.Add(t);

            var weapons = _enemyShipAPI.GetAllWeapons();
            if (weapons != null)
                foreach (var weapon in weapons)
                    if (weapon is ITickable t) _enemyTickables.Add(t);

            // SimulationCore에 등록
            _simCore.RegisterTickables(_enemyTickables);
            _simCore.RegisterTickables(this); // 승리 조건 체크용

            Debug.Log($"[EnemyCombatManager] 전투 시작 — 적군 체력: {_enemyShipAPI.CurrentHullHealth}/{_enemyShipAPI.MaxHullHealth}");
        }

        /// <summary>매 틱 적군 체력을 확인합니다. 0 이하가 되면 전투를 종료합니다.</summary>
        public void OnTickUpdate()
        {
            if (_ended || _enemyShipAPI == null) return;

            if (_enemyShipAPI.CurrentHullHealth <= 0)
            {
                Debug.Log("[EnemyCombatManager] 적군 격파! 전투 종료.");
                EndCombat();
                _eventLogic.CompleteCombatSubEvent();
            }
        }

        /// <summary>적군 tickable을 SimulationCore에서 제거합니다.</summary>
        public void EndCombat()
        {
            if (_ended) return;
            _ended = true;

            _simCore.UnregisterTickables(_enemyTickables);
            _simCore.UnregisterTickable(this);
            _enemyTickables.Clear();

            Debug.Log("[EnemyCombatManager] 전투 종료 — 틱 해제 완료.");
            OnCombatEnded?.Invoke();
        }
    }
}
