using Moonstorm.Components;
using RoR2;
using UnityEngine;
using System;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class BuffBulwark : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffBulwark");

        public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation(useOnClient = false, useOnServer = true)]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffBulwark;
            private void OnDisable()
            {
                attachmentActive = false;
            }

            private void FixedUpdate()
            {
                attachmentActive = body.healthComponent.alive;
            }

            private bool attachmentActive
            {
                get
                {
                    return attachment != null;
                }
                set
                {
                    if (value == attachmentActive)
                    {
                        return;
                    }
                    if (value)
                    {
                        attachment = Instantiate(SS2Assets.LoadAsset<GameObject>("DivineRightBodyAttachment")).GetComponent<NetworkedBodyAttachment>();
                        attachment.AttachToGameObjectAndSpawn(body.gameObject, null);
                        return;
                    }
                    Destroy(attachmentGameObject);
                    attachmentGameObject = null;
                    attachment = null;
                }
            }

            private NetworkedBodyAttachment attachment;

            private GameObject attachmentGameObject;
        }
    }
}
