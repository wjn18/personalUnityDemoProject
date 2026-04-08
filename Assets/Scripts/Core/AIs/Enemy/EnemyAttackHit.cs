using UnityEngine;

public class EnemyAttackHit : MonoBehaviour
{
    public EnemyAI enemyAI;

    public void DealDamage()
    {
        if (enemyAI == null) return;

        if (!enemyAI.enabled) return;

        enemyAI.ApplyAttackDamageNow();
    }
}