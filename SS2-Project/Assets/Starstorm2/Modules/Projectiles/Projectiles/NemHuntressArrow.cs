﻿using UnityEngine;
using static R2API.DamageAPI;

using Moonstorm;
namespace SS2.Projectiles
{
    public sealed class NemHuntressArrow : ProjectileBase
    {
        public override GameObject ProjectilePrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemHuntressArrowProjectile", SS2Bundle.Indev);

        public override void Initialize()
        {
            var damageAPIComponent = ProjectilePrefab.AddComponent<ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(DamageTypes.WeakPointProjectile.weakPointProjectile);
        }
    }
}
