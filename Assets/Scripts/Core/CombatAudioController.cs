using UnityEngine;

public class CombatAudioController : MonoBehaviour
{
    [Header("Refs")]
    public AudioSource audioSource;

    [Header("Attack")]
    public AudioClip[] attackVoiceClips;
    public AudioClip[] attackSwingClips;
    public AudioClip[] attackHitClips;
    public AudioClip[] attackMissClips;

    [Header("Defense / Hurt")]
    public AudioClip[] blockedHitClips;
    public AudioClip[] hurtClips;

    [Header("Hit Effect")]
    public GameObject hitEffectPrefab;
    public float hitEffectLifetime = 0.75f;
    public float hitEffectNormalOffset = 0.02f;

    void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayAttackStart()
    {
        PlayRandomClip(attackVoiceClips);
        PlayRandomClip(attackSwingClips);
    }

    public void PlayAttackHit()
    {
        PlayRandomClip(attackHitClips);
    }

    public void PlayAttackMiss()
    {
        PlayRandomClip(attackMissClips);
    }

    public void PlayBlockedHit()
    {
        PlayRandomClip(blockedHitClips);
    }

    public void PlayHurt()
    {
        PlayRandomClip(hurtClips);
    }

    public void PlayHitEffect(Vector3 position, Vector3 normal)
    {
        if (hitEffectPrefab == null)
            return;

        Vector3 safeNormal = normal.sqrMagnitude > 0.0001f
            ? normal.normalized
            : Vector3.up;

        Vector3 spawnPosition = position + safeNormal * Mathf.Max(0f, hitEffectNormalOffset);
        Quaternion rotation = Quaternion.LookRotation(safeNormal, Vector3.up);
        GameObject instance = Instantiate(hitEffectPrefab, spawnPosition, rotation);
        Destroy(instance, Mathf.Max(0.01f, hitEffectLifetime));
    }

    void PlayRandomClip(AudioClip[] clips)
    {
        if (audioSource == null || clips == null || clips.Length == 0)
            return;

        AudioClip clip = GetRandomClip(clips);
        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    AudioClip GetRandomClip(AudioClip[] clips)
    {
        int startIndex = Random.Range(0, clips.Length);
        for (int i = 0; i < clips.Length; i++)
        {
            AudioClip clip = clips[(startIndex + i) % clips.Length];
            if (clip != null)
                return clip;
        }

        return null;
    }
}
