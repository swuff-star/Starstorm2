﻿using RoR2;
namespace SS2.Scenes
{
    [DisabledContent]
    public sealed class RedPlane : SceneBase
    {
        public override SceneDef SceneDef { get; } = SS2Assets.LoadAsset<SceneDef>("redplane", SS2Bundle.Indev);
    }
}
