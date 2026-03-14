using System.Collections.Generic;
using Core.Interface;
using Core.enums;

namespace Core.Data.Combat
{
    /// <summary>발사체 한 발의 미리 계산된 결과.</summary>
    public struct HitResult
    {
        /// <summary>실드가 예약됨. 도달 시 ApplyReservedAbsorption 호출 필요.</summary>
        public bool ReservedShield;
        /// <summary>실드를 뚫었을 때 적용할 피해량. ReservedShield=true면 0.</summary>
        public int Damage;
    }

    /// <summary>
    /// 발사 즉시 계산되어 큐에 쌓이는 공격 정보.
    /// TicksRemaining이 0에 도달하면 HP 피해를 실제로 적용합니다.
    /// </summary>
    public class PendingAttack
    {
        public IShipAPI TargetShipAPI;
        public List<HitResult> Hits;
        public int TicksRemaining;
        public IWeaponLogic SourceWeapon; // 향후 애니메이션 View가 OnAttackQueued로 구독
        public int TargetRoomID;          // 발사체가 날아갈 목표 방 ID (애니메이션 목적지 계산용)
        public WeaponType WeaponType;     // 투사체 종류 선별용 (Laser / Missile / Beam)
    }
}
