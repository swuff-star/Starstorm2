﻿using RoR2;
using UnityEngine;
namespace SS2.Unlocks.VanillaSurvivors
{
    public class CrocoGrandMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef => SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.croco.grandmastery", SS2Bundle.Vanilla);

        public sealed class CrocoGrandMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.5f;

            public override CharacterBody RequiredCharacterBody { get; set; } = Resources.Load<GameObject>("prefabs/characterbodies/crocobody").GetComponent<CharacterBody>();
        }
    }
}