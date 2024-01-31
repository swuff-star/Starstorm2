﻿using RoR2;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class UraniumHorseshoe : ItemBase
    {

        private const string token = "SS2_ITEM_URANIUMHORSESHOE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("UraniumHorseshoe", SS2Bundle.Items);

        public override void Initialize()
        {

        }

    }
}