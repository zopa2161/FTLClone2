using System.Collections.Generic;
using Core.Interface;
using UnityEngine;

namespace Logic.System
{
    public class SimulationCore
    {
        private readonly List<ITickable> _tickables = new();
        private readonly List<ITickable> _pendingRemovals = new();

        private float _tickTimer;
        public float TickRate { get; set; } = 0.1f;
        public float TimeScale { get; set; } = 1.0f;

        public void RegisterTickables(IEnumerable<ITickable> tickables)
        {
            if(tickables == null)Debug.Log("null");
            _tickables.AddRange(tickables);
        }

        public void RegisterTickables(ITickable tickable)
        {
            _tickables.Add(tickable);
        }

        public void UnregisterTickable(ITickable tickable)
            => _pendingRemovals.Add(tickable);

        public void UnregisterTickables(IEnumerable<ITickable> tickables)
            => _pendingRemovals.AddRange(tickables);

        // 💡 외부(View)에서 프레임워크의 시간(deltaTime)을 주입해 줍니다.
        public void AdvanceTime(float realDeltaTime)
        {
            if (TimeScale <= 0f) return;

            _tickTimer += realDeltaTime * TimeScale;

            while (_tickTimer >= TickRate)
            {
                _tickTimer -= TickRate;
                ProcessTick();
            }
        }

        private void ProcessTick()
        {
            if (_pendingRemovals.Count > 0)
            {
                foreach (var t in _pendingRemovals) _tickables.Remove(t);
                _pendingRemovals.Clear();
            }
            foreach (var tickable in _tickables) tickable.OnTickUpdate();
        }
    }
}