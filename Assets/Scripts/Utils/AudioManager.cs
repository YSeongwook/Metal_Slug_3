using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] audioClips;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void InitAudioClips(AudioClip[] audioClips)
    {
        this.audioClips = audioClips;
    }

    public AudioSource PlaySound(int soundIndex)
    {
        audioSource.clip = audioClips[soundIndex];
        audioSource.Play();
        return audioSource;
    }

    public void PlaySoundByClip(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PlaySoundSameSource(int soundIndex)
    {
        audioSource.clip = audioClips[soundIndex];
        audioSource.Play();
    }
}