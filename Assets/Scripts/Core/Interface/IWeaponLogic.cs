using System;
using Core.Data.Weapon;

namespace Core.Interface
{
    public interface IWeaponLogic
    {
        // 원본 및 상태 데이터
        WeaponData Data { get; }

        // 상태 확인용 프로퍼티
        bool IsPowered { get; }
        bool IsReadyToFire { get; }

        // UI에서 게이지 바를 그리기 위한 장전 비율 (0.0f ~ 1.0f)
        float ChargeProgress { get; }

        // 📢 UI나 시스템에 상태 변화를 알리는 무전기
        event Action<float> OnChargeUpdated; // 장전 진행도 변경 시
        event Action<bool> OnPowerStateChanged; // 전력 ON/OFF 시
        event Action OnFired; // 무기가 발사되었을 때 (이펙트/사운드 재생용)

        // 💡 외부(WeaponManager 등)에서 제어하는 스위치
        void SetPower(bool isOn);
        void SetAutoFire(bool isOn);
        void SetTarget(int roomID);

        // 발사 명령 (조건이 맞으면 발사 후 타이머 초기화)
        bool TryFire();
    }
}