﻿using RoR2;
using UnityEngine;

using Moonstorm;
namespace SS2.Unlocks.NemMercenary
{
    public sealed class NemMercenaryMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.nemmerc.mastery", SS2Bundle.NemMercenary);

        public sealed class NemMercenaryMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.0f;

            public override CharacterBody RequiredCharacterBody { get; set; } = SS2Assets.LoadAsset<GameObject>("NemMercBody", SS2Bundle.NemMercenary).GetComponent<CharacterBody>();
        }
    }
}
