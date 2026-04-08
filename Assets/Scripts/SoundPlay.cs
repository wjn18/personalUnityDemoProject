using UnityEngine;

public class PlaySfxOnQ : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sfx;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PlaySound();
        }
    }

    void PlaySound()
    {
        if (audioSource == null)
        {
            Debug.LogWarning($"{name}: 没有找到 AudioSource");
            return;
        }

        if (sfx == null)
        {
            Debug.LogWarning($"{name}: 没有指定音效 AudioClip");
            return;
        }

        audioSource.PlayOneShot(sfx);
        Debug.Log("Listener pause = " + AudioListener.pause);
        Debug.Log("播放音效");
    }
}