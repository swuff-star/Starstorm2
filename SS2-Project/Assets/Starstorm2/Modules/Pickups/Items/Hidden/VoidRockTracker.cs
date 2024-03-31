﻿using R2API;
using RoR2;
using RoR2.Items;

using Moonstorm;
namespace SS2.Items
{
    public sealed class VoidRockTracker : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("VoidRockTracker", SS2Bundle.Interactables);

    }
}
