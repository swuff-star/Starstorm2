﻿using RoR2;
using UnityEngine;
namespace SS2.Equipments
{
    public sealed class WhiteFlag : SS2Equipment
    {
        private const string token = "SS2_EQUIP_WHITEFLAG_DESC";
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("WhiteFlag", SS2Bundle.Equipments);
        public GameObject FlagObject { get; } = SS2Assets.LoadAsset<GameObject>("WhiteFlagWard", SS2Bundle.Equipments);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Radius of the White Flag's effect, in meters.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float flagRadius = 25f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of White Flag when used, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float flagDuration = 8f;

        public override bool FireAction(EquipmentSlot slot)
        {
            //To do: make better placement system
            GameObject gameObject = Object.Instantiate(FlagObject, slot.characterBody.corePosition, Quaternion.identity);
            BuffWard buffWard = gameObject.GetComponent<BuffWard>();
            buffWard.expireDuration = flagDuration;
            buffWard.radius = flagRadius;
            gameObject.GetComponent<TeamFilter>().teamIndex = slot.teamComponent.teamIndex;

            return true;
        }
    }

}
