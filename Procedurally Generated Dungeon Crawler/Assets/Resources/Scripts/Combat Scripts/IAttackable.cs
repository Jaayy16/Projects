using UnityEngine;

namespace ProceduralDungeon.Combat
{
    public interface IAttackable
    {
        void TakeDamage(float damage);
        float GetHealth();
        Transform GetTransform();
        bool IsAlive();
    }
}
