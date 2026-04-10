using UnityEngine;

public class ScreenShakeController : MonoBehaviour
{
    [System.Serializable]
    public struct ShakeSettings
    {
        public float intensity;
        public float duration;
        public float damping;
    }

    public static ScreenShakeController Instance { get; private set; }

    [Header("Player Heavy Hit")]
    public ShakeSettings playerHeavyHitShake = new ShakeSettings
    {
        intensity = 0.2f,
        duration = 0.12f,
        damping = 10f
    };

    [Header("Player Attack 4 Hit")]
    public ShakeSettings playerAttack4HitShake = new ShakeSettings
    {
        intensity = 0.16f,
        duration = 0.1f,
        damping = 11f
    };

    [Header("Boss Ranged Hit")]
    public ShakeSettings bossRangedHitShake = new ShakeSettings
    {
        intensity = 0.22f,
        duration = 0.14f,
        damping = 9f
    };

    private Vector3 currentOffset;
    private float remainingDuration;
    private float currentIntensity;
    private float currentDamping;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        RemoveCurrentOffset();
    }

    void LateUpdate()
    {
        RemoveCurrentOffset();

        if (remainingDuration <= 0f || currentIntensity <= 0f)
            return;

        remainingDuration -= Time.unscaledDeltaTime;
        currentIntensity = Mathf.MoveTowards(
            currentIntensity,
            0f,
            Mathf.Max(0f, currentDamping) * Time.unscaledDeltaTime
        );

        if (remainingDuration <= 0f || currentIntensity <= 0f)
        {
            currentOffset = Vector3.zero;
            return;
        }

        currentOffset = Random.insideUnitSphere * currentIntensity;
        transform.localPosition += currentOffset;
    }

    public void PlayPlayerHeavyHitShake()
    {
        PlayShake(playerHeavyHitShake);
    }

    public void PlayPlayerAttack4HitShake()
    {
        PlayShake(playerAttack4HitShake);
    }

    public void PlayBossRangedHitShake()
    {
        PlayShake(bossRangedHitShake);
    }

    public void PlayShake(ShakeSettings settings)
    {
        if (settings.intensity <= 0f || settings.duration <= 0f)
            return;

        remainingDuration = Mathf.Max(remainingDuration, settings.duration);
        currentIntensity = Mathf.Max(currentIntensity, settings.intensity);
        currentDamping = Mathf.Max(0f, settings.damping);
    }

    void RemoveCurrentOffset()
    {
        if (currentOffset == Vector3.zero)
            return;

        transform.localPosition -= currentOffset;
        currentOffset = Vector3.zero;
    }
}
