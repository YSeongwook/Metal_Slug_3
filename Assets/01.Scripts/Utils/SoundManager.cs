using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    [Header("Music")]
    public AudioClip charSelect;        // char selection
    public AudioClip musicClip;         // The background music
    public AudioClip gameOverClip;      // Played once on game over
    public AudioClip bossClip;          // the bgm of the boss fight

    [Header("Player")]
    public AudioClip marcoDeathClip;    // Marco Death Sound

    [Header("Effects")]
    public AudioClip normalShotClip;
    public AudioClip heavyMachineShotClip;
    public AudioClip shotHitClip;
    public AudioClip grenadeHitClip;
    public AudioClip meleeHitClip;
    public AudioClip meleeTakeClip;
    public AudioClip collectibleGrabClip;
    public AudioClip grenadeGrabClip;
    public AudioClip metalSlugDestroy1;
    public AudioClip metalSlugDestroy2;
    public AudioClip metalSlugDestroy3;
    public AudioClip continueSiren;

    [Header("Voice")]
    public AudioClip levelStart;
    public AudioClip levelComplete;
    public AudioClip heavyMachineGunGrab;
    public AudioClip okayClip;

    [Header("Menu")]
    public AudioClip insertCoin;
    public AudioClip marco;             // marco chosen
    public AudioClip menuSound;         // menu sound
    public AudioClip preselect;         // any button
    public AudioClip select;            // press start

    [Header("Mixer Groups")]
    public AudioMixerGroup musicGroup;  // The music mixer group
    public AudioMixerGroup effectGroup; // The effect mixer group
    public AudioMixerGroup enemyGroup;  // The enemy mixer group
    public AudioMixerGroup playerGroup; // The player mixer group
    public AudioMixerGroup voiceGroup;  // The voice mixer group

    AudioSource musicSource;            // Reference to the generated music Audio Source
    AudioSource effectSource;           // Reference to the generated effect Audio Source
    AudioSource enemySource;            // Reference to the generated enemy Audio Source
    AudioSource playerSource;           // Reference to the generated player Audio Source
    AudioSource voiceSource;            // Reference to the generated voice Audio Source

   void Start()
    {
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        effectSource = gameObject.AddComponent<AudioSource>();
        enemySource = gameObject.AddComponent<AudioSource>();
        playerSource = gameObject.AddComponent<AudioSource>();
        voiceSource = gameObject.AddComponent<AudioSource>();

        musicSource.outputAudioMixerGroup = musicGroup;
        effectSource.outputAudioMixerGroup = effectGroup;
        enemySource.outputAudioMixerGroup = enemyGroup;
        playerSource.outputAudioMixerGroup = playerGroup;
        voiceSource.outputAudioMixerGroup = voiceGroup;

        RefreshAudioVolume();
        // StartLevelAudio();
    }

    public void RefreshAudioVolume()
    {
        GameManager.Instance.SetBgmAudio(0.2f);
        GameManager.Instance.SetSfxAudio(0.35f);

        musicSource.volume = GameManager.Instance.GetBgmAudio();
        effectSource.volume = GameManager.Instance.GetSfxAudio();
        enemySource.volume = GameManager.Instance.GetSfxAudio();
        playerSource.volume = GameManager.Instance.GetSfxAudio();
        voiceSource.volume = GameManager.Instance.GetSfxAudio();
    }

   void StartLevelAudio()
    {
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
        PlayLevelStartAudio();
    }

    public void PlayBGM()
    {
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StartBossAudio()
    {
        musicSource.clip = bossClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayLevelStartAudio()
    {
        voiceSource.clip = levelStart;
        voiceSource.Play();
    }

    public void PlayLevelCompleteAudio()
    {
        voiceSource.clip = levelComplete;
        voiceSource.Play();
    }

    public void PlayGameOverAudio()
    {
        musicSource.clip = gameOverClip;
        musicSource.loop = false;
        musicSource.Play();
    }

    public void PlayDeathAudio()
    {
        playerSource.clip = marcoDeathClip;
        playerSource.Play();
    }

    public  bool IsPlayingOtherAudio(AudioClip clip, AudioSource source)
    {
        if (source.clip != clip && source.isPlaying) return true;
        return false;
    }

    public void PlayNormalShotAudio()
    {
        AudioClip clip = normalShotClip;
        AudioSource source = playerSource;

        //Don't overshadow the other sounds
        if (IsPlayingOtherAudio(clip, source)) return;

        //Set the clip for music audio, and then tell it to play
        source.clip = clip;
        source.Play();
    }

    public void PlayHeavyMachineShotAudio()
    {
        AudioClip clip = heavyMachineShotClip;
        AudioSource source = playerSource;

        //Don't overshadow the other sounds
        if (IsPlayingOtherAudio(clip, source)) return;

        //Set the clip for music audio, and then tell it to play
        source.clip = clip;
        source.Play();
    }

    public void PlayEnemyAttackAudio(AudioClip attackClip)
    {
        enemySource.clip = attackClip;
        enemySource.Play();
    }

    public void PlayEnemyDeathAudio(AudioClip deathClip)
    {
        enemySource.clip = deathClip;
        enemySource.Play();
    }

    public void PlayShotHitAudio()
    {
        playerSource.clip = shotHitClip;
        playerSource.Play();
    }

    public void PlayGrenadeHitAudio()
    {
        playerSource.clip = grenadeHitClip;
        playerSource.Play();
    }

    public void PlayMeleeHitAudio()
    {
        playerSource.clip = meleeHitClip;
        playerSource.Play();
    }

    public void PlayMeleeTakeAudio()
    {
        playerSource.clip = meleeTakeClip;
        playerSource.Play();
    }

    public void PlayInsertCoin()
    {
        effectSource.clip = insertCoin;
        effectSource.Play();
    }

    public void PlayCharSelect()
    {
        musicSource.clip = charSelect;
        musicSource.Play();
    }

    public void PlayPreSelect()
    {
        effectSource.clip = preselect;
        effectSource.Play();
    }

    public void PlaySelectMarco()
    {
        effectSource.clip = marco;
        effectSource.Play();
    }

    public void PlayMenuSelect()
    {
        effectSource.clip = select;
        effectSource.Play();
    }

    public void PlayMenuBGM()
    {
        musicSource.clip = menuSound;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayAmmoGrab()
    {
        playerSource.clip = grenadeGrabClip;
        playerSource.Play();
    }

    public void PlayHeavyMachineGunVoice()
    {
        voiceSource.clip = heavyMachineGunGrab;
        voiceSource.Play();
    }

    public void PlayOkayVoice()
    {
        voiceSource.clip = okayClip;
        voiceSource.Play();
    }

    public void PlayMedKitGrab()
    {
        playerSource.clip = collectibleGrabClip;
        playerSource.Play();
    }

    public void PlayMetalSlugDestroy1()
    {
        effectSource.clip = metalSlugDestroy1;
        effectSource.Play();
    }

    public void PlayMetalSlugDestroy2()
    {
        effectSource.clip = metalSlugDestroy2;
        effectSource.Play();
    }

    public void PlayMetalSlugDestroy3()
    {
        effectSource.clip = metalSlugDestroy3;
        effectSource.Play();
    }

    public void PlayContinueSiren()
    {
        effectSource.clip = continueSiren;
        effectSource.Play();
    }

    public void ClearEffectSource()
    {
        effectSource = null;
    }
}
