using RoR2;
using RoR2.Items;
using UnityEngine;

namespace Moonstorm.Starstorm2.Equipments
{
    [DisabledContent]
    public sealed class DivineRight : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("DivineRight");
        public override bool FireAction(EquipmentSlot slot)
        {
            Debug.Log("hello");
            return true;
        }
    }
}
