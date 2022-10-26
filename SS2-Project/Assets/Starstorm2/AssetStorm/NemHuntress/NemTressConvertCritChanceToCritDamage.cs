using R2API;
using RoR2;
using RoR2.Items;
using System;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class NemTressConvertCritChanceToCritDamage : ItemBase
    {
        private const string token = "SS2_ITEM_CONVERTCRITCHANCETOCRITDAMAGE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("NemTressConvertCritChanceToDamage");
        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.NemTressConvertCritChanceToDamage;
            private float oldCrit;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (stack >= 1)
                {
                    args.critDamageMultAdd += body.crit * 0.01f;
                    body.crit = 0f;
                }
            }
        }
    }
}
