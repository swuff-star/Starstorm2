﻿namespace EntityStates.MULE
{
    class MULESpinStart : BaseSkillState
    {
        [FormatToken("SS2_MULE_PRIMARY_SPIN_DESCRIPTION", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 0, "100")]
        public static float swingTimeCoefficient = 1f;
        public static float duration = 0.2f;

        public override void OnEnter()
        {
            base.OnEnter();
            PlayCrossfade("Gesture, Override", "Startpin", "Utility.playbackRate", duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                MULESpin nextState = new MULESpin();
                outer.SetNextState(nextState);
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
