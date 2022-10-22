using RoR2;
using UnityEngine;

namespace EntityStates.NemHuntress
{
    public class ChargeExplosiveArrow : BaseSkillState
    {
        public static float baseChargeDuration = 1.75f;
        public static float maxEmission;
        public static float minEmission;
        public static GameObject lightningEffect;

        //private Material swordMat;
        private float chargeDuration;
        //private ChildLocator childLocator;
        //private Animator animator;
        //private Transform modelBaseTransform;
        //private GameObject effectInstance;
        //private GameObject defaultCrosshair;
        //private uint chargePlayID;

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration / attackSpeedStat;
            //childLocator = GetModelChildLocator();
            ////modelBaseTransform = GetModelBaseTransform();
            //animator = GetModelAnimator();
            //defaultCrosshair = characterBody.defaultCrosshairPrefab;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            characterBody.SetSpreadBloom(Util.Remap(charge, 0f, 1f, 0f, 3f), true);

            StartAimMode();

            if (isAuthority && (charge >= 1f || (!IsKeyDownAuthority() && fixedAge >= 0.1f)))
            {
                FireExplosiveArrow nextState = new FireExplosiveArrow();
                nextState.charge = charge;
                outer.SetNextState(nextState);
            }
        }

        protected float CalcCharge()
        {
            return Mathf.Clamp01(fixedAge / chargeDuration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}