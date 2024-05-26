﻿using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using RoR2.UI;
using MSU;
using System.Collections;
using R2API;
using System.Reflection;
using UnityEngine.Networking;
using RoR2.ContentManagement;
using UnityEngine.AddressableAssets;

namespace SS2.Survivors
{
    public sealed class NemCommando : SS2Survivor, IContentPackModifier
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNemCommando", SS2Bundle.NemCommando);

        private static float gougeDuration = 2;
        public static DamageAPI.ModdedDamageType GougeDamageType { get; private set; }
        public static DotController.DotIndex GougeDotIndex { get; private set; }
        private BuffDef _gougeBuffDef;
        private GameObject distantGashProjectile;
        private GameObject distantGashProjectileBlue;
        private GameObject distantGashProjectileYellow;
        private GameObject grenadeProjectile;
        private GameObject bodyPrefab;
        private CharacterBody characterBody;
        public GameObject nemesisPodPrefab;
        public GameObject podPanelPrefab;

        public override void Initialize()
        {
            _gougeBuffDef = AssetCollection.FindAsset<BuffDef>("BuffGouge");
            distantGashProjectile = AssetCollection.FindAsset<GameObject>("NemCommandoSwordBeamProjectile");
            distantGashProjectileBlue = AssetCollection.FindAsset<GameObject>("NemCommandoSwordBeamProjectileBlue");
            distantGashProjectileYellow = AssetCollection.FindAsset<GameObject>("NemCommandoSwordBeamProjectileYellow");
            bodyPrefab = AssetCollection.FindAsset<GameObject>("NemCommandoBody");

            characterBody = bodyPrefab.GetComponent<CharacterBody>();

            On.RoR2.CharacterSelectBarController.Awake += CharacterSelectBarController_Awake;

            GougeDamageType = DamageAPI.ReserveDamageType();
            GougeDotIndex = DotAPI.RegisterDotDef(0.25f, 0.25f, DamageColorIndex.SuperBleed, _gougeBuffDef);
            On.RoR2.HealthComponent.TakeDamage += TakeDamageGouge;
            GlobalEventManager.onServerDamageDealt += ApplyGouge;

            ModifyProjectiles();
            CreatePod();

            characterBody.preferredPodPrefab = nemesisPodPrefab;
        }

        private void TakeDamageGouge(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool triggerGougeProc = false;
            if (NetworkServer.active)
            {
                if (damageInfo.dotIndex == GougeDotIndex && damageInfo.procCoefficient == 0f && self.alive)
                {
                    if (damageInfo.attacker)
                    {
                        CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (attackerBody)
                        {
                            damageInfo.crit = Util.CheckRoll(attackerBody.crit, attackerBody.master);
                        }
                    }
                    damageInfo.procCoefficient = 0.2f;
                    triggerGougeProc = true;
                }
            }

            orig(self, damageInfo);

            if (NetworkServer.active && !damageInfo.rejected && self.alive)
            {
                if (triggerGougeProc)
                {
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, self.gameObject);
                }
            }
        }

        //N: I should make this an utility idk
        private void CharacterSelectBarController_Awake(On.RoR2.CharacterSelectBarController.orig_Awake orig, CharacterSelectBarController self)
        {
            //hide nemcommando from css proper
            SS2Content.Survivors.NemMerc.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.NemMerc.survivorIndex); // hello nem comado
            SS2Content.Survivors.survivorNemCommando.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.survivorNemCommando.survivorIndex);
            orig(self);
        }

        private void ApplyGouge(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, GougeDamageType))
            {
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = attackerBody.gameObject,
                    victimObject = victimBody.gameObject,
                    dotIndex = GougeDotIndex,
                    duration = gougeDuration,
                    damageMultiplier = 1,
                };
                DotController.InflictDot(ref dotInfo);
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public void CreatePod()
        {
            Material podMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/TrimSheets/matTrimSheetMetal.mat").WaitForCompletion();
            nemesisPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion().InstantiateClone("NemesisSurvivorPod", true);

            Transform modelTransform = nemesisPodPrefab.GetComponent<ModelLocator>().modelTransform;

            modelTransform.Find("EscapePodArmature/Base/Door/EscapePodDoorMesh").GetComponent<MeshRenderer>().material = podMat;
            modelTransform.Find("EscapePodArmature/Base/ReleaseExhaustFX/Door,Physics").GetComponent<MeshRenderer>().material = podMat;
            modelTransform.Find("EscapePodArmature/Base/EscapePodMesh").GetComponent<MeshRenderer>().material = podMat;
            modelTransform.Find("EscapePodArmature/Base/RotatingPanel/EscapePodMesh.002").GetComponent<MeshRenderer>().material = podMat;

            podPanelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPodBatteryPanel.prefab").WaitForCompletion().InstantiateClone("NemesisPanel", true);
            podPanelPrefab.GetComponent<Highlight>().targetRenderer.material = podMat;

            InstantiatePrefabBehavior[] ipb;
            ipb = nemesisPodPrefab.GetComponents<InstantiatePrefabBehavior>();
            foreach (InstantiatePrefabBehavior prefab in ipb)
            {
                if (prefab.prefab == Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPodBatteryPanel.prefab").WaitForCompletion())
                    prefab.prefab = podPanelPrefab;
            }
        }

        private void ModifyProjectiles()
        {
            var damageAPIComponent = distantGashProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(GougeDamageType);
            damageAPIComponent = distantGashProjectileBlue.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(GougeDamageType);
            damageAPIComponent = distantGashProjectileYellow.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(GougeDamageType);

            //var pie = grenadeProjectile.GetComponent<RoR2.Projectile.ProjectileImpactExplosion>();
            //pie.impactEffect = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/OmniExplosionVFXCommandoGrenade.prefab").WaitForCompletion();
        }
    }
}
