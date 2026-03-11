using Logic.System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.System
{
    public class UnityTimeProvider : MonoBehaviour
    {
        private SimulationCore _simulationCore;
        private bool _isPaused;
        private VisualElement _pauseOverlay;

        private void Update()
        {
            if (_simulationCore == null) return;

            if (Input.GetKeyDown(KeyCode.Space))
                TogglePause();

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
                GameSessionManager.Instance.SaveGame();

            if (!_isPaused)
                _simulationCore.AdvanceTime(Time.deltaTime);
        }

        public void Initialize(SimulationCore core)
        {
            _simulationCore = core;

            var doc = FindObjectOfType<UIDocument>();
            if (doc != null)
                _pauseOverlay = doc.rootVisualElement.Q<VisualElement>("PauseOverlay");
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;
            if (_pauseOverlay != null)
                _pauseOverlay.style.display = _isPaused ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}