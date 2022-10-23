using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using Moonstorm.Starstorm2;
using UnityEngine.Networking;
using RoR2.Projectile;

namespace EntityStates.DivineRightGadget
{
    public class DivineRightAttachmentState : BaseBodyAttachmentState
    {
        public static float projectileEraserRadius = 20f;
        public static float minimumFireFrequency = 2f;
        public static float baseRechargeFrequency = 1f;

        public static GameObject tracerEffectPrefab = SS2Assets.LoadAsset<GameObject>("GadgetTracer");
        public static GameObject hitEffectPrefab = SS2Assets.LoadAsset<GameObject>("GadgetImpactEffectVFX");

        private float rechargeTimer;
        private float rechargeFrequency
        {
            get
            {
                return baseRechargeFrequency * (attachedBody ? attachedBody.attackSpeed : 1f);
            }
        }

        private float fireFrequency
        {
            get
            {
                return Mathf.Max(minimumFireFrequency, rechargeFrequency);
            }
        }

        private float timeBetweenfiring
        {
            get
            {
                return 1f / fireFrequency;
            }
        }

        private bool isReadyTofire
        {
            get
            {
                return rechargeTimer <= 0f;
            }
        }

        protected int GetItemStack()
        {
            if (!attachedBody || !attachedBody.inventory)
            {
                return 1;
            }
            return attachedBody.inventory.GetItemCount(Moonstorm.Starstorm2.SS2Content.Items.ErraticGadget);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!NetworkServer.active)
            {
                return;
            }
            rechargeTimer -= Time.fixedDeltaTime;
            if (fixedAge > timeBetweenfiring)
            {
                fixedAge -= timeBetweenfiring;
                if (isReadyTofire && DeleteNearbyProjectile())
                {
                    rechargeTimer = 1f / rechargeFrequency;
                }
            }
        }

        private bool DeleteNearbyProjectile()
        {
            Vector3 vector = attachedBody ? attachedBody.corePosition : Vector3.zero;
            TeamIndex teamIndex = attachedBody ? attachedBody.teamComponent.teamIndex : TeamIndex.None;
            float num = projectileEraserRadius * projectileEraserRadius;
            int num2 = 0;
            int itemStack = GetItemStack();
            bool result = false;
            List<ProjectileController> instanceList = InstanceTracker.GetInstancesList<ProjectileController>();
            List<ProjectileController> list = new List<ProjectileController>();
            int num3 = 0;
            int count = instanceList.Count;
            while (num3 < count && num2 < itemStack)
            {
                ProjectileController projectileController = instanceList[num3];
                if (!projectileController.cannotBeDeleted && projectileController.teamFilter.teamIndex != teamIndex && (projectileController.transform.position - vector).sqrMagnitude < num)
                {
                    list.Add(projectileController);
                    num2++;
                }
                num3++;
            }
            int i = 0;
            int count2 = list.Count;
            while (i < count2)
            {
                ProjectileController projectileController2 = list[i];
                if (projectileController2)
                {
                    result = true;
                    Vector3 position = projectileController2.transform.position;
                    Vector3 start = vector;
                    if (tracerEffectPrefab)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = position,
                            start = start
                        };
                        EffectManager.SpawnEffect(tracerEffectPrefab, effectData, true);
                    }
                    if (hitEffectPrefab)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = position,
                        };
                        EffectManager.SpawnEffect(hitEffectPrefab, effectData, true);
                    }
                    Destroy(projectileController2.gameObject);
                }
                i++;
            }
            return result;
        }
    }
}
