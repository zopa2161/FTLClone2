using System;
using Core.Data.Weapon;
using Core.Interface;
using UnityEngine;

namespace Logic.SpaceShip.Weapons
{
    public class WeaponLogic : IWeaponLogic, ITickable
    {
        // 💡 1틱당 더해질 시간 (예: 시뮬레이션이 초당 10틱이라면 0.1f)
        private const float TICK_TIME_STEP = 0.1f;

        public WeaponData Data { get; private set; }

        public bool IsPowered => Data.IsPowered;

        // 장전 타이머가 쿨타임 이상 도달했는가?
        public bool IsReadyToFire => Data.CurrentChargeTimer >= Data.BaseData.BaseCooldown;

        // 0.0 ~ 1.0 비율 반환
        public float ChargeProgress =>
            Data.BaseData.BaseCooldown > 0f ? Mathf.Clamp01(Data.CurrentChargeTimer / Data.BaseData.BaseCooldown) : 1f;

        public event Action<float> OnChargeUpdated;
        public event Action<bool> OnPowerStateChanged;
        public event Action OnFired;

        public void Initialize(WeaponData data)
        {
            Data = data;
        }

        public void OnTickUpdate()
        {
            // 전력이 꺼져있거나, 이미 장전 완료면 타이머를 올리지 않음
            if (!IsPowered || IsReadyToFire) return;

            // 타이머 증가
            Data.CurrentChargeTimer += TICK_TIME_STEP;

            // 한도 초과 방지
            if (Data.CurrentChargeTimer > Data.BaseData.BaseCooldown)
                Data.CurrentChargeTimer = Data.BaseData.BaseCooldown;

            // UI 갱신을 위해 무전 발송
            OnChargeUpdated?.Invoke(ChargeProgress);

            // 🌟 오토 파이어(자동 발사) 모드이고 타겟이 있다면 즉시 발사 시도
            if (IsReadyToFire && Data.IsAutoFire && Data.TargetRoomID != -1) TryFire();
        }

        public void SetPower(bool isOn)
        {
            if (Data.IsPowered == isOn) return;

            Data.IsPowered = isOn;

            // 만약 전력이 꺼지면 장전 게이지를 초기화하는 룰 (FTL 방식)
            if (!isOn)
            {
                Data.CurrentChargeTimer = 0f;
                OnChargeUpdated?.Invoke(ChargeProgress);
            }

            OnPowerStateChanged?.Invoke(isOn);
        }

        public void SetAutoFire(bool isOn)
        {
            Data.IsAutoFire = isOn;
        }

        public void SetTarget(int roomID)
        {
            Data.TargetRoomID = roomID;
        }

        public bool TryFire()
        {
            if (!IsPowered || !IsReadyToFire) return false;

            // 발사 로직 처리 (추후 발사체 생성 매니저로 명령 전달)
            // Debug.Log($"{Data.BaseData.WeaponName} 발사! 타겟: {Data.TargetRoomID}");

            // 타이머 초기화 (반동)
            Data.CurrentChargeTimer = 0f;
            OnChargeUpdated?.Invoke(ChargeProgress);

            // 발사 이펙트를 위해 뷰(View)에 무전
            OnFired?.Invoke();

            return true;
        }
    }
}