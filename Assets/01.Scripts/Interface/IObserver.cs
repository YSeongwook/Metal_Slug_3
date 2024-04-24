public enum SlugEvents
{
    Fall,
    Jump,
    JumpLowSpeed,
    JumpHighSpeed,
    HitGround,
    MovingRight,
    MovingLeft,
    StartMoving,
    StopMoving,
    Turn,
    Sit,
    Stand,
    LookUp,
    Shoot,
    Attack,
    Grenade
};

// 이벤트가 일어나는지 관찰하는 인터페이스
public interface IObserver
{
    void Observe(SlugEvents ev);
}