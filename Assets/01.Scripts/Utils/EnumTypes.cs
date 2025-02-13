using UnityEngine;

namespace _01.Scripts.Utils
{
    public enum BodyPosture { Stand, Running, InTheAir, Crouch }
    public enum Layers
    {
        Default,
        TransparentFX,
        IgnoreRaycast,
        Reserved1,
        Water,
        UI,
        Reserved2,
        Reserved3,
        Player,
        World,
        Enemy,
        EnemySolid,
        FreeMan,
        Building,
        Walkable,
    }

    public enum Missions { Home = 0, Mission1, Mission2, Mission3, Mission3Boss }

    public enum LauncherType
    {
        Player,
        Enemy
    };

    public enum ThrowableType
    {
        Grenade,
        BossBomb,
        BossHeavyBomb,
        EnemyGrenade,
    };

    public enum GlobalEvents
    {
        SoldierDead,

        WaveEventEnd,
        
        ShowRecordingUI
    }

    public enum GameEvents
    {
        GameOver,
        Restart,
        Home,
        GameReset,
        PointsEarned,
    }

    public enum MissionEvents
    {
        MissionStartRequest,
        MissionStart,
        MissionEnd,
        MissionSuccess,
    }

    public enum PlayerEvents
    {
        PlayerDead,
        PlayerSpawned,
        PlayerStabbed,
        PlayerInactive,
        PlayerDamaged,
    }

    public enum ItemEvents
    {
        ItemPickedUp,
    }

    public enum BossEvents
    {
        BossSpawn,
        BossDead,
    }

    public enum AttackEvents
    {
        KnifeUsed,
        GunUsed,
        GrenadeUsed,
    }

    public class EnumTypes : MonoBehaviour { }
}