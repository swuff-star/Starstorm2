﻿using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;

using Moonstorm;
namespace SS2.Projectiles
{
    public sealed class NemMercenaryKnife : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = SS2Assets.LoadAsset<GameObject>("KnifeProjectile", SS2Bundle.NemMercenary);

        public override void Initialize()
        {

        }
    }
}