using Logic.System;
using UnityEngine;

namespace Presentation.System
{
    public class UnityTimeProvider : MonoBehaviour
    {
        // 💡 로직 어셈블리에 있는 순수 C# 매니저를 들고 있습니다.
        private SimulationCore _simulationCore;

        private void Update()
        {
            // 시뮬레이션 코어가 세팅되지 않았다면 무시
            if (_simulationCore == null) return;

            // 💡 엔진의 렌더링 시간(deltaTime)을 순수 로직에게 먹여줍니다!
            _simulationCore.AdvanceTime(Time.deltaTime);
        }

        public void Initialize(SimulationCore core)
        {
            _simulationCore = core;
        }
    }
}