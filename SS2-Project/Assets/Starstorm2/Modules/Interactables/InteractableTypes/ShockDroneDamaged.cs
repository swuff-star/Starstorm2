﻿using EntityStates;
using SS2.Components;
using R2API;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

using Moonstorm;
namespace SS2.Interactables
{
    //[DisabledContent]
    public sealed class ShockDroneDamaged : InteractableBase
    {
        public override GameObject Interactable { get; } = SS2Assets.LoadAsset<GameObject>("ShockDroneBroken", SS2Bundle.Interactables);
        private GameObject interactable;
        private SummonMasterBehavior smb;
        private CharacterMaster cm;
        private GameObject bodyPrefab;
        private AkEvent[] droneAkEvents;

        public override List<MSInteractableDirectorCard> InteractableDirectorCards => new List<MSInteractableDirectorCard>
        {
            SS2Assets.LoadAsset<MSInteractableDirectorCard>("msidcShockDrone", SS2Bundle.Interactables)
        };

        public override void Initialize()
        {
            base.Initialize();

            On.EntityStates.Drone.DeathState.OnImpactServer += SpawnShockCorpse;

            //add sound events, the bad way
            interactable = InteractableDirectorCards[0].prefab;
            smb = interactable.GetComponent<SummonMasterBehavior>();
            cm = smb.masterPrefab.GetComponent<CharacterMaster>();
            bodyPrefab = cm.bodyPrefab;

            var droneBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Body.prefab").WaitForCompletion();

            droneAkEvents = droneBody.GetComponents<AkEvent>();

            foreach (AkEvent akEvent in droneAkEvents)
            {
                var akEventType = akEvent.GetType();
                var newComponent = bodyPrefab.AddComponent(akEventType);

                var fields = akEventType.GetFields();

                foreach (var field in fields)
                {
                    var value = field.GetValue(akEvent);
                    field.SetValue(newComponent, value);
                }
            }
        }

        private void SpawnShockCorpse(On.EntityStates.Drone.DeathState.orig_OnImpactServer orig, EntityStates.Drone.DeathState self, Vector3 contactPoint)
        {
            if(self.characterBody.bodyIndex == BodyCatalog.FindBodyIndexCaseInsensitive("ShockDroneBody"))
            {
                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    position = contactPoint
                };
                GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(InteractableDirectorCards[0], placementRule, new Xoroshiro128Plus(0UL)));
                if (gameObject)
                {
                    PurchaseInteraction component = gameObject.GetComponent<PurchaseInteraction>();
                    if (component && component.costType == CostTypeIndex.Money)
                    {
                        component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
                    }
                }

            }
            else
            {
                orig(self, contactPoint);
            }
        }
    }
}
