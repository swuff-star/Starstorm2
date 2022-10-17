using RoR2;
using UnityEngine;


namespace Moonstorm.Starstorm2.Survivors
{
    //[DisabledContent]
    public sealed class NemesisBandit : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("NemBanditBody");
        public override GameObject MasterPrefab { get; } = null; //Assets.Instance.MainAssetBundle.LoadAsset<GameObject>("PyroMonsterMaster");
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("survNemBandit");

        public override void ModifyPrefab()
        {
            base.ModifyPrefab();

            var cb = BodyPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }
    }
}
