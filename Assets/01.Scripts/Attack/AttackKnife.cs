using UnityEngine;

namespace _01.Scripts.Attack
{
    public class AttackKnife : MonoBehaviour, IAttack
    {
        public Animator anim;
        public AreaOfEffectProjectile knife;
        public AudioManager audioManager;
        public TimeUtils timeUtils;
    
        private bool _attackSwitch;
    
        // 애니메이터 파라미터 캐시 처리
        private static readonly int Knifing = Animator.StringToHash("knifing");
        private static readonly int Knife = Animator.StringToHash("knife");
        private static readonly int Knife2 = Animator.StringToHash("knife2");

        public void Execute(string victimTag, Vector3 unused, Vector3 unused2)
        {
            if (_attackSwitch)
            {
                anim.SetTrigger(Knife2);
                audioManager.PlaySound(4);
            }
            else
            {
                anim.SetTrigger(Knife);
                audioManager.PlaySound(5);
            }
            _attackSwitch = !_attackSwitch;

            knife.CastAOE(victimTag, transform.position);
            timeUtils.TimeDelay(0.2f, () => { anim.SetBool(Knifing, false); });
        }

        public bool InProgress()
        {
            return anim.GetBool(Knifing);
        }
    }
}