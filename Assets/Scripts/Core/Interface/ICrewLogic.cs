using System;
using Core.Data.SpaceShip;
using MoveDirection = Core.enums.MoveDirection;

namespace Core.Interface
{
    public interface ICrewLogic
    {
        int CrewID { get; }
        CrewData Data { get; }

        //===체력관련===
        float CurrentHealth { get; }
        float MaxHealth { get; }

        event Action<int, int, MoveDirection> OnPositionChanged;

        void CommandMoveTo(TileCoord targetCoord);

        // 📢 체력이 변했을 때 발송할 무전기 (현재 체력, 최대 체력)
        event Action<float, float> OnHealthChanged;

        // 💡 외부(산소 부족, 적 공격 등)에서 체력을 깎을 때 쓸 스위치
        void TakeDamage(float amount);

        event Action<ICrewLogic> OnDied;
    }
}