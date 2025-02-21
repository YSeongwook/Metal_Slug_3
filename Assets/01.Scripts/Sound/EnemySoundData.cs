using System;

namespace _01.Scripts.Sound
{
    [Serializable]
    public class EnemySoundData
    {
        public string enemyType;
        public string[] deathClips;
        public string[] attackClips;
    }

    [Serializable]
    public class EnemySoundList
    {
        public EnemySoundData[] enemySounds;
    }
}