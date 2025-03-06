using System;
using System.Collections.Generic;
using _01.Scripts.Utils;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

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

        [Header("EnemyDeath")]
        public AudioClip crabDeathClip;
        public AudioClip[] soldierDeathClips;

        [Header("Mixer Groups")]
        public AudioMixerGroup musicGroup;
        public AudioMixerGroup effectGroup;
        public AudioMixerGroup enemyGroup;
        public AudioMixerGroup playerGroup;
        public AudioMixerGroup voiceGroup;
        public AudioMixerGroup sirenGroup;

        // 단일 사운드를 관리하는 딕셔너리
        private Dictionary<string, AudioClip> _singleSoundDict = new Dictionary<string, AudioClip>();
        // 여러 사운드 배열을 관리하는 딕셔너리
        private Dictionary<string, AudioClip[]> _multiSoundDict = new Dictionary<string, AudioClip[]>();

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
            InitializeSoundDictionaries();
            RefreshAudioVolume();
        }

        private void OnEnable()
        {
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

        private void InitializeSoundDictionaries()
        {
            // 단일 사운드 딕셔너리
            _singleSoundDict["charSelect"] = charSelect;
            _singleSoundDict["musicClip"] = musicClip;
            _singleSoundDict["gameOverClip"] = gameOverClip;
            _singleSoundDict["bossClip"] = bossClip;
            _singleSoundDict["marcoDeathClip"] = marcoDeathClip;
            _singleSoundDict["normalShotClip"] = normalShotClip;
            _singleSoundDict["heavyMachineShotClip"] = heavyMachineShotClip;
            _singleSoundDict["shotHitClip"] = shotHitClip;
            _singleSoundDict["grenadeHitClip"] = grenadeHitClip;
            _singleSoundDict["meleeHitClip"] = meleeHitClip;
            _singleSoundDict["meleeTakeClip"] = meleeTakeClip;
            _singleSoundDict["collectibleGrabClip"] = collectibleGrabClip;
            _singleSoundDict["grenadeGrabClip"] = grenadeGrabClip;
            _singleSoundDict["metalSlugDestroy1"] = metalSlugDestroy1;
            _singleSoundDict["metalSlugDestroy2"] = metalSlugDestroy2;
            _singleSoundDict["metalSlugDestroy3"] = metalSlugDestroy3;
            _singleSoundDict["continueSiren"] = continueSiren;
            _singleSoundDict["levelStart"] = levelStart;
            _singleSoundDict["levelComplete"] = levelComplete;
            _singleSoundDict["heavyMachineGunGrab"] = heavyMachineGunGrab;
            _singleSoundDict["okayClip"] = okayClip;
            _singleSoundDict["insertCoin"] = insertCoin;
            _singleSoundDict["marco"] = marco;
            _singleSoundDict["menuSound"] = menuSound;
            _singleSoundDict["preselect"] = preselect;
            _singleSoundDict["select"] = select;
            _singleSoundDict["crabDeath"] = crabDeathClip;

            // 여러 사운드 배열 딕셔너리
            _multiSoundDict["soldierDeath"] = soldierDeathClips;
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
            _sirenSource.volume = GameManager.Instance.GetSfxAudio();
        }

        // SoundEventType에 정의된 이벤트들을 구독합니다.
        private void SubscribeToSoundEvents()
        {
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.EnemyAttack, OnEnemyAttackSound);
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.EnemyDeath, OnEnemyDeathSound);
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.PlayMusic, OnMusicSound);
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.PlayVoice, OnVoiceSound);
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.PlayEffect, OnEffectSound);
            EventManager<SoundEventType>.StartListening<string>(SoundEventType.ContinueSiren, OnSirenSound);
            EventManager<SoundEventType>.StartListening(SoundEventType.ClearAllSounds, ClearAllSounds);
        }

        private void UnsubscribeFromSoundEvents()
        {
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.EnemyAttack, OnEnemyAttackSound);
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.EnemyDeath, OnEnemyDeathSound);
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.PlayMusic, OnMusicSound);
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.PlayVoice, OnVoiceSound);
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.PlayEffect, OnEffectSound);
            EventManager<SoundEventType>.StopListening<string>(SoundEventType.ContinueSiren, OnSirenSound);
            EventManager<SoundEventType>.StopListening(SoundEventType.ClearAllSounds, ClearAllSounds);
        }

        // 이벤트 리스너들
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

        // 재생 메서드: 먼저 단일 사운드 딕셔너리에서 찾고, 없으면 다중 사운드 딕셔너리에서 찾음
        public void PlaySound(string soundKey, AudioChannel channel)
        {
            AudioClip clip = null;
            if (_singleSoundDict.ContainsKey(soundKey))
            {
                clip = _singleSoundDict[soundKey];
            }
            else if (_multiSoundDict.ContainsKey(soundKey))
            {
                AudioClip[] clips = _multiSoundDict[soundKey];
                if (clips != null && clips.Length > 0)
                {
                    clip = (clips.Length == 1) ? clips[0] : clips[Random.Range(0, clips.Length)];
                }
            }
            else
            {
                DebugLogger.LogWarning($"Sound key not found in any dictionary: {soundKey}");
                return;
            }

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

            // 반복 재생 여부 설정: Music와 Siren 채널은 반복, 나머지는 단발 재생
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
            _musicSource.Stop();
            _effectSource.Stop();
            _enemySource.Stop();
            _playerSource.Stop();
            _voiceSource.Stop();
            _sirenSource.Stop();

            _musicSource.clip = null;
            _effectSource.clip = null;
            _enemySource.clip = null;
            _playerSource.clip = null;
            _voiceSource.clip = null;
            _sirenSource.clip = null;

            DebugLogger.Log("모든 사운드가 중지되고 클리어되었습니다.");
        }

        // 기존 특정 재생 메서드들 (필요에 따라 계속 사용할 수 있음)
        public void PlayBGM() => PlaySound("musicClip", AudioChannel.Music);
        public void StartBossAudio() => PlaySound("bossClip", AudioChannel.Music);
        public void PlayGameOverAudio() => PlaySound("gameOverClip", AudioChannel.Music);
        public void PlayDeathAudio() => PlaySound("marcoDeathClip", AudioChannel.Player);
    }
}
