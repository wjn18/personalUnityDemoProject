using UnityEngine;

public class GuardAttackHit : MonoBehaviour
{
    public GuardAI guardAI;

    public void DealDamage()
    {
        if (guardAI != null)
        {
            guardAI.ApplyAttackDamageNow();
        }
    }
}