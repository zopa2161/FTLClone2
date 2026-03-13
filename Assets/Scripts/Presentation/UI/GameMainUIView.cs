using Core.Interface;
using Presentation.System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Presentation.Views.UI
{
    public class GameMainUIView : MonoBehaviour
    {
        public UIDocument Document;

        private IResourceManager _resourceManager;
        private ICombatManager _combatManager;
        private IRoomLogic _pilotRoom;
        private Presentation.UI.MapView _mapView;

        private Button _jumpButton;
        private Button _upgradeButton;
        private Button _settingsButton;

        private Label _fuelLabel;
        private Label _missilesLabel;
        private Label _dronesLabel;
        private Label _scrapLabel;

        public void Initialize(IResourceManager resourceManager, ICombatManager combatManager,
                               IRoomLogic pilotRoom, Presentation.UI.MapView mapView)
        {
            _resourceManager = resourceManager;
            _combatManager   = combatManager;
            _pilotRoom       = pilotRoom;
            _mapView         = mapView;

            var root = Document.rootVisualElement;
            _jumpButton     = root.Q<Button>("JumpButton");
            _upgradeButton  = root.Q<Button>("UpgradeButton");
            _settingsButton = root.Q<Button>("SettingsButton");
            _fuelLabel      = root.Q<Label>("FuelLabel");
            _missilesLabel  = root.Q<Label>("MissilesLabel");
            _dronesLabel    = root.Q<Label>("DronesLabel");
            _scrapLabel     = root.Q<Label>("ScrapLabel");

            _pilotRoom.OnMannedStatusChanged    += OnMannedStatusChanged;
            _resourceManager.OnFuelChanged      += OnFuelChanged;
            _resourceManager.OnMissilesChanged  += OnMissilesChanged;
            _resourceManager.OnDronesChanged    += OnDronesChanged;
            _resourceManager.OnScrapChanged     += OnScrapChanged;
            _combatManager.OnCombatStateChanged += OnCombatStateChanged;

            if (_mapView != null)
                _mapView.OnNodeJumped += OnNodeJumped;

            _jumpButton.RegisterCallback<ClickEvent>(_ => HandleJump());

            RefreshJumpButton();
            RefreshResourceLabels();
        }

        private void OnMannedStatusChanged(bool isManned) => RefreshJumpButton();
        private void OnCombatStateChanged(bool inCombat)  => RefreshJumpButton();

        private void OnFuelChanged(int fuel)
        {
            _fuelLabel.text = fuel.ToString();
            RefreshJumpButton();
        }

        private void OnMissilesChanged(int missiles) => _missilesLabel.text = missiles.ToString();
        private void OnDronesChanged(int drones)     => _dronesLabel.text = drones.ToString();
        private void OnScrapChanged(int scrap)       => _scrapLabel.text = scrap.ToString();

        private void RefreshResourceLabels()
        {
            _fuelLabel.text     = _resourceManager.Fuel.ToString();
            _missilesLabel.text = _resourceManager.Missiles.ToString();
            _dronesLabel.text   = _resourceManager.Drones.ToString();
            _scrapLabel.text    = _resourceManager.Scrap.ToString();
        }

        private void RefreshJumpButton()
        {
            bool canJump = !_combatManager.IsInCombat
                        && _pilotRoom.IsManned
                        && _resourceManager.Fuel >= 1;

            _jumpButton.SetEnabled(canJump);
        }

        private void HandleJump()
        {
            if (_mapView != null) _mapView.Show();
        }

        private void OnNodeJumped(string nodeID)
        {
            _resourceManager.TryConsumeFuel(1);
            GameSessionManager.Instance.SaveGame();
        }

        private void OnDestroy()
        {
            if (_pilotRoom != null)
                _pilotRoom.OnMannedStatusChanged -= OnMannedStatusChanged;
            if (_resourceManager != null)
            {
                _resourceManager.OnFuelChanged     -= OnFuelChanged;
                _resourceManager.OnMissilesChanged -= OnMissilesChanged;
                _resourceManager.OnDronesChanged   -= OnDronesChanged;
                _resourceManager.OnScrapChanged    -= OnScrapChanged;
            }
            if (_combatManager != null)
                _combatManager.OnCombatStateChanged -= OnCombatStateChanged;
            if (_mapView != null)
                _mapView.OnNodeJumped -= OnNodeJumped;
        }
    }
}
