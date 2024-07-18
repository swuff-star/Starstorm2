using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;
using RoR2.ContentManagement;

#if DEBUG
namespace SS2.Survivors
{

    public sealed class DUT : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acDUT", SS2Bundle.Indev);

        public override void Initialize()
        {
            ModifyPrefab();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePod.prefab").WaitForCompletion(); ;
            cb._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
        }
    }
}
#endif