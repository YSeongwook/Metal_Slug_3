using System.Collections.Generic;
using _01.Scripts.Utils;
using UnityEngine;
using UnityEngine.Audio;

namespace _01.Scripts.Sound
{
    public enum AudioChannel
    {
        Music,
        Effect,
        Enemy,
        Player,
        Voice,
        Siren
    }

    public class SoundManager : Singleton<SoundManager>
    {
        [Header("Music")]
        public AudioClip charSelect;
        public AudioClip musicClip;
        public AudioClip gameOverClip;
        public AudioClip bossClip;

        [Header("Player")]
        public AudioClip marcoDeathClip;

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
        public AudioClip marco;
        public AudioClip menuSound;
        public AudioClip preselect;
        public AudioClip select;

        [Header("Mixer Groups")]
        public AudioMixerGroup musicGroup;
        public AudioMixerGroup effectGroup;
        public AudioMixerGroup enemyGroup;
        public AudioMixerGroup playerGroup;
        public AudioMixerGroup voiceGroup;
        public AudioMixerGroup sirenGroup;

        // 사운드 데이터를 관리하는 Dictionary
        private Dictionary<string, AudioClip> _soundDictionary = new Dictionary<string, AudioClip>();

        // AudioSource 채널들
        private AudioSource _musicSource;
        private AudioSource _effectSource;
        private AudioSource _enemySource;
        private AudioSource _playerSource;
        private AudioSource _voiceSource;
        private AudioSource _sirenSource;

        private void Start()
        {
            InitializeAudioSources();
            InitializeSoundDictionary();
            RefreshAudioVolume();
            SubscribeToSoundEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromSoundEvents();
        }

        private void InitializeAudioSources()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _effectSource = gameObject.AddComponent<AudioSource>();
            _enemySource = gameObject.AddComponent<AudioSource>();
            _playerSource = gameObject.AddComponent<AudioSource>();
            _voiceSource = gameObject.AddComponent<AudioSource>();
            _sirenSource = gameObject.AddComponent<AudioSource>();

            _musicSource.outputAudioMixerGroup = musicGroup;
            _effectSource.outputAudioMixerGroup = effectGroup;
            _enemySource.outputAudioMixerGroup = enemyGroup;
            _playerSource.outputAudioMixerGroup = playerGroup;
            _voiceSource.outputAudioMixerGroup = voiceGroup;
            _sirenSource.outputAudioMixerGroup = sirenGroup;
        }

        private void InitializeSoundDictionary()
        {
            // Music
            _soundDictionary["charSelect"] = charSelect;
            _soundDictionary["musicClip"] = musicClip;
            _soundDictionary["gameOverClip"] = gameOverClip;
            _soundDictionary["bossClip"] = bossClip;

            // Player
            _soundDictionary["marcoDeathClip"] = marcoDeathClip;

            // Effects
            _soundDictionary["normalShotClip"] = normalShotClip;
            _soundDictionary["heavyMachineShotClip"] = heavyMachineShotClip;
            _soundDictionary["shotHitClip"] = shotHitClip;
            _soundDictionary["grenadeHitClip"] = grenadeHitClip;
            _soundDictionary["meleeHitClip"] = meleeHitClip;
            _soundDictionary["meleeTakeClip"] = meleeTakeClip;
            _soundDictionary["collectibleGrabClip"] = collectibleGrabClip;
            _soundDictionary["grenadeGrabClip"] = grenadeGrabClip;
            _soundDictionary["metalSlugDestroy1"] = metalSlugDestroy1;
            _soundDictionary["metalSlugDestroy2"] = metalSlugDestroy2;
            _soundDictionary["metalSlugDestroy3"] = metalSlugDestroy3;
            _soundDictionary["continueSiren"] = continueSiren;

            // Voice
            _soundDictionary["levelStart"] = levelStart;
            _soundDictionary["levelComplete"] = levelComplete;
            _soundDictionary["heavyMachineGunGrab"] = heavyMachineGunGrab;
            _soundDictionary["okayClip"] = okayClip;

            // Menu
            _soundDictionary["insertCoin"] = insertCoin;
            _soundDictionary["marco"] = marco;
            _soundDictionary["menuSound"] = menuSound;
            _soundDictionary["preselect"] = preselect;
            _soundDictionary["select"] = select;
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

        // SoundEventType에 정의된 이벤트들을 구독합니다.
        private void SubscribeToSoundEvents()
        {
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.EnemyAttack, OnEnemyAttackSound);
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.EnemyDeath, OnEnemyDeathSound);
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.PlayMusic, OnMusicSound);
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.PlayVoice, OnVoiceSound);
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.PlayEffect, OnVoiceSound);
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.ContinueSiren, OnSirenSound);
            EventManager<SoundEventType>.StartListening(SoundEventType.ClearAllSounds, ClearAllSounds);
        }

        // SoundEventType에 정의된 이벤트들을 구독 해제합니다.
        private void UnsubscribeFromSoundEvents()
        {
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.EnemyAttack, OnEnemyAttackSound);
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.EnemyDeath, OnEnemyDeathSound);
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.PlayMusic, OnMusicSound);
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.PlayVoice, OnVoiceSound);
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.PlayEffect, OnVoiceSound);
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.ContinueSiren, OnSirenSound);
            EventManager<SoundEventType>.StopListening(SoundEventType.ClearAllSounds, ClearAllSounds);
        }

        // 이벤트 발생 시 전달된 사운드 키에 따라 재생
        private void OnEnemyAttackSound(string soundKey)
        {
            PlaySound(soundKey, AudioChannel.Enemy);
        }

        private void OnEnemyDeathSound(string soundKey)
        {
            PlaySound(soundKey, AudioChannel.Enemy);
        }
        
        private void OnMusicSound(string soundKey)
        {
            PlaySound(soundKey, AudioChannel.Music);
        }

        private void OnEffectSound(string soundKey)
        {
            PlaySound(soundKey, AudioChannel.Effect);
        }
        
        private void OnVoiceSound(string soundKey)
        {
            PlaySound(soundKey, AudioChannel.Voice);
        }
        
        private void OnSirenSound(string soundKey)
        {
            PlaySound(soundKey, AudioChannel.Siren);
        }

        public void PlaySound(string soundKey, AudioChannel channel)
        {
            if (!_soundDictionary.ContainsKey(soundKey))
            {
                DebugLogger.LogWarning($"Sound key not found: {soundKey}");
                return;
            }

            AudioClip clip = _soundDictionary[soundKey];
            if (clip == null)
            {
                DebugLogger.LogWarning($"AudioClip is null for key: {soundKey}");
                return;
            }

            AudioSource source = GetAudioSource(channel);
            if (source == null)
            {
                DebugLogger.LogWarning($"AudioSource not found for channel: {channel}");
                return;
            }

            // 반복 재생 여부 설정: Music 채널은 반복, 나머지는 단발 재생
            if (channel == AudioChannel.Music || channel == AudioChannel.Siren)
                source.loop = true;
            else
                source.loop = false;

            source.clip = clip;
            source.Play();
        }

        private AudioSource GetAudioSource(AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Music:
                    return _musicSource;
                case AudioChannel.Effect:
                    return _effectSource;
                case AudioChannel.Enemy:
                    return _enemySource;
                case AudioChannel.Player:
                    return _playerSource;
                case AudioChannel.Voice:
                    return _voiceSource;
                case AudioChannel.Siren:
                    return _sirenSource;
                default:
                    return null;
            }
        }
        
        public void ClearAllSounds()
        {
            // 각 AudioSource의 재생을 중지합니다.
            _musicSource.Stop();
            _effectSource.Stop();
            _enemySource.Stop();
            _playerSource.Stop();
            _voiceSource.Stop();

            // (원하는 경우) 각 채널의 클립을 null로 설정하여 메모리에서 해제
            _musicSource.clip = null;
            _effectSource.clip = null;
            _enemySource.clip = null;
            _playerSource.clip = null;
            _voiceSource.clip = null;

            DebugLogger.Log("모든 사운드가 중지되고 클리어되었습니다.");
        }

        // 기존의 특정 재생 메서드들을 통합 메서드 호출로 대체할 수 있음
        public void PlayBGM() => PlaySound("musicClip", AudioChannel.Music);
        public void StartBossAudio() => PlaySound("bossClip", AudioChannel.Music);
        public void PlayGameOverAudio() => PlaySound("gameOverClip", AudioChannel.Music);
        public void PlayDeathAudio() => PlaySound("marcoDeathClip", AudioChannel.Player);
    }
}
