using MonoMod.Cil;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Items;
using UnityEngine.Networking;
using Mono.Cecil.Cil;
using System;
using RoR2.Navigation;
using RoR2.Projectile;
using static MSU.BaseBuffBehaviour;

namespace SS2.Equipments
{
#if DEBUG
    public sealed class AffixUltra : SS2EliteEquipment
    {
        public override SS2AssetRequest<EliteAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<EliteAssetCollection>("acAffixUltra", SS2Bundle.Equipments);

        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void Initialize()
        {
        }


        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public sealed class BodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.UltraItemAffix;

            public void Start()
            {
                if (NetworkServer.active)
                {
                    body.AddBuff(SS2Content.Buffs.bdUltra);
                }
            }

            private void OnDestroy()
            {
                if (NetworkServer.active && body.enabled)
                {
                    body.RemoveBuff(SS2Content.Buffs.bdUltra);
                }
            }

        }

        public sealed class UltraBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => SS2Content.Buffs.bdUltra;

            public void Start()
            {
            }

            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                    return;

                if (HasAnyStacks)
                {
                    
                }
                
            }

            protected override void OnAllStacksLost()
            {
            }

        }

    }
#endif
}
