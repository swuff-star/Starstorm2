using EntityStates;
using RoR2;
using UnityEngine;

namespace Starstorm2.Cores.States.Wayfarer
{
    class CloakState : BaseSkillState
    {
        public static float baseDuration = 3.0f;

        private Animator animator;
        private float duration;
        private bool hasCloaked = false;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = base.GetModelAnimator();
            this.duration = baseDuration / attackSpeedStat;
            base.PlayCrossfade("FullBody, Override", "Cloak", "Cloak.playbackRate", duration, 0.2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hasCloaked && Time.deltaTime > 0.5)
            {
                characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.Cloak.buffIndex, 10f);
                hasCloaked = true;
                Debug.Log("it's cloaking time");
            }

            if (base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}