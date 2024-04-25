﻿using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Equipments
{
    public sealed class Magnet : SS2Equipment
    {
        private const string token = "SS2_EQUIP_MAGNET_DESC";

        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Range at which Simple Magnet can pull pickups, in meters.")]
        [FormatToken(token, 0)]
        public static float magnetRadius = 150f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Percent chance for Simple Magnet to unearth treasure.")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float treasureChance = .1f;

        public static float pullSpeed = 75f;

        public static float pullDuration = 3f;
        //distance away from player that pickups will land
        public static float destinationRadius = 8f;

        private GameObject _magnetPrefab;
        public override bool Execute(EquipmentSlot slot)
        {
            NetworkServer.Spawn(GameObject.Instantiate<GameObject>(_magnetPrefab, slot.characterBody.corePosition + (Vector3.up * 3f), Quaternion.identity));
            return true;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * EquipmentDef - "Magnet" - Equipments
             * GameObject - "PickupMagnetController" - Equipments
             */
            yield break;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
    }
}