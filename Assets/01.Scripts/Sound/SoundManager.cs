using _01.Scripts.Utils;
using UnityEngine;
using UnityEngine.Audio;

namespace _01.Scripts.Sound
{
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

        private AudioSource _musicSource;            // Reference to the generated music Audio Source
        private AudioSource _effectSource;           // Reference to the generated effect Audio Source
        private AudioSource _enemySource;            // Reference to the generated enemy Audio Source
        private AudioSource _playerSource;           // Reference to the generated player Audio Source
        private AudioSource _voiceSource;            // Reference to the generated voice Audio Source

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            _musicSource = gameObject.AddComponent<AudioSource>();
            _effectSource = gameObject.AddComponent<AudioSource>();
            _enemySource = gameObject.AddComponent<AudioSource>();
            _playerSource = gameObject.AddComponent<AudioSource>();
            _voiceSource = gameObject.AddComponent<AudioSource>();

            _musicSource.outputAudioMixerGroup = musicGroup;
            _effectSource.outputAudioMixerGroup = effectGroup;
            _enemySource.outputAudioMixerGroup = enemyGroup;
            _playerSource.outputAudioMixerGroup = playerGroup;
            _voiceSource.outputAudioMixerGroup = voiceGroup;

            RefreshAudioVolume();
        }

        private void RefreshAudioVolume()
        {
            GameManager.Instance.SetBgmAudio(0.2f);
            GameManager.Instance.SetSfxAudio(0.35f);

            _musicSource.volume = GameManager.Instance.GetBgmAudio();
            _effectSource.volume = GameManager.Instance.GetSfxAudio();
            _enemySource.volume = GameManager.Instance.GetSfxAudio();
            _playerSource.volume = GameManager.Instance.GetSfxAudio();
            _voiceSource.volume = GameManager.Instance.GetSfxAudio();
        }

        private void StartLevelAudio()
        {
            _musicSource.clip = musicClip;
            _musicSource.loop = true;
            _musicSource.Play();
            PlayLevelStartAudio();
        }

        public void PlayBGM()
        {
            _musicSource.clip = musicClip;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public void StartBossAudio()
        {
            _musicSource.clip = bossClip;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public void PlayLevelStartAudio()
        {
            _voiceSource.clip = levelStart;
            _voiceSource.Play();
        }

        public void PlayLevelCompleteAudio()
        {
            _voiceSource.clip = levelComplete;
            _voiceSource.Play();
        }

        public void PlayGameOverAudio()
        {
            _musicSource.clip = gameOverClip;
            _musicSource.loop = false;
            _musicSource.Play();
        }

        public void PlayDeathAudio()
        {
            _playerSource.clip = marcoDeathClip;
            _playerSource.Play();
        }

        private bool IsPlayingOtherAudio(AudioClip clip, AudioSource source)
        {
            if (source.clip != clip && source.isPlaying) return true;
            return false;
        }

        public void PlayNormalShotAudio()
        {
            AudioClip clip = normalShotClip;
            AudioSource source = _playerSource;

            //Don't overshadow the other sounds
            if (IsPlayingOtherAudio(clip, source)) return;

            //Set the clip for music audio, and then tell it to play
            source.clip = clip;
            source.Play();
        }

        public void PlayHeavyMachineShotAudio()
        {
            AudioClip clip = heavyMachineShotClip;
            AudioSource source = _playerSource;

            //Don't overshadow the other sounds
            if (IsPlayingOtherAudio(clip, source)) return;

            //Set the clip for music audio, and then tell it to play
            source.clip = clip;
            source.Play();
        }

        public void PlayEnemyAttackAudio(AudioClip attackClip)
        {
            _enemySource.clip = attackClip;
            _enemySource.Play();
        }

        public void PlayEnemyDeathAudio(AudioClip deathClip)
        {
            _enemySource.clip = deathClip;
            _enemySource.Play();
        }

        public void PlayShotHitAudio()
        {
            _playerSource.clip = shotHitClip;
            _playerSource.Play();
        }

        public void PlayGrenadeHitAudio()
        {
            _playerSource.clip = grenadeHitClip;
            _playerSource.Play();
        }

        public void PlayMeleeHitAudio()
        {
            _playerSource.clip = meleeHitClip;
            _playerSource.Play();
        }

        public void PlayMeleeTakeAudio()
        {
            _playerSource.clip = meleeTakeClip;
            _playerSource.Play();
        }

        public void PlayInsertCoin()
        {
            _effectSource.clip = insertCoin;
            _effectSource.Play();
        }

        public void PlayCharSelect()
        {
            _musicSource.clip = charSelect;
            _musicSource.Play();
        }

        public void PlayPreSelect()
        {
            _effectSource.clip = preselect;
            _effectSource.Play();
        }

        public void PlaySelectMarco()
        {
            _effectSource.clip = marco;
            _effectSource.Play();
        }

        public void PlayMenuSelect()
        {
            _effectSource.clip = select;
            _effectSource.Play();
        }

        public void PlayMenuBGM()
        {
            _musicSource.clip = menuSound;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public void PlayAmmoGrab()
        {
            _playerSource.clip = grenadeGrabClip;
            _playerSource.Play();
        }

        public void PlayHeavyMachineGunVoice()
        {
            _voiceSource.clip = heavyMachineGunGrab;
            _voiceSource.Play();
        }

        public void PlayOkayVoice()
        {
            _voiceSource.clip = okayClip;
            _voiceSource.Play();
        }

        public void PlayMedKitGrab()
        {
            _playerSource.clip = collectibleGrabClip;
            _playerSource.Play();
        }

        public void PlayMetalSlugDestroy1()
        {
            _effectSource.clip = metalSlugDestroy1;
            _effectSource.Play();
        }

        public void PlayMetalSlugDestroy2()
        {
            _effectSource.clip = metalSlugDestroy2;
            _effectSource.Play();
        }

        public void PlayMetalSlugDestroy3()
        {
            _effectSource.clip = metalSlugDestroy3;
            _effectSource.Play();
        }

        public void PlayContinueSiren()
        {
            _effectSource.clip = continueSiren;
            _effectSource.Play();
        }

        public void ClearEffectSource()
        {
            _effectSource = null;
        }
    }
}
