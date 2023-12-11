﻿using System;
using EntityStates;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;

namespace EntityStates.Chirr
{
	// probably worth to turn this into a general-use "body launch" state, and set the parameters when instantiating the state.
	public class DroppedState : BaseState
	{
		public static float bounceForce = 2000f;
		public static float force = 800f;
		public static float blastRadius = 10f;
		public static float procCoefficient = 1f;
		public static float damageCoefficient = 15f;
		public GameObject inflictor;
		public Vector3 initialVelocity;
		public static GameObject hitGroundEffect = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardGroundSlam.prefab").WaitForCompletion();

		public float extraGravity;
		public bool friendlyDrop;

		public bool detonateNextFrame;

		private DetonateOnImpact detonateOnImpact;
		private bool bodyHadGravity = true;
		private bool bodyWasKinematic = true;
		private bool bodyCouldTakeImpactDamage = true;
		private Rigidbody tempRigidbody;
		private SphereCollider tempSphereCollider;
		public override void OnEnter()
		{
			base.OnEnter();
			Animator modelAnimator = base.GetModelAnimator();

			if (base.characterBody && NetworkServer.active) base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

			if (modelAnimator)
			{
				int layerIndex = modelAnimator.GetLayerIndex("Body");
				modelAnimator.enabled = false;
				modelAnimator.CrossFadeInFixedTime((UnityEngine.Random.Range(0, 2) == 0) ? "Hurt1" : "Hurt2", 0.1f);
				modelAnimator.Update(0f);
			}

			// figure out if we should disable gravity/kinematic on exit
			GameObject prefab = BodyCatalog.GetBodyPrefab(this.characterBody.bodyIndex);
			if (prefab)
			{
				Rigidbody rigidbody = prefab.GetComponent<Rigidbody>();
				if (rigidbody)
				{
					this.bodyHadGravity = rigidbody.useGravity;
					this.bodyWasKinematic = rigidbody.isKinematic;
				}
			}

			if(base.characterMotor)
            {
                base.characterMotor.onMovementHit += DoSplashDamage;
				base.characterMotor.disableAirControlUntilCollision = true;
				base.characterMotor.velocity = initialVelocity;
            }
			else
            {
				Rigidbody rigidbody = base.rigidbody; // CONSTRUCTS AND SHIT DONT HAVE RIGIDBODIES
				if(!rigidbody)
                {
					rigidbody = base.gameObject.AddComponent<Rigidbody>();
					this.tempRigidbody = rigidbody;
					this.tempSphereCollider = base.gameObject.AddComponent<SphereCollider>();
                }
				rigidbody.velocity = initialVelocity;
				rigidbody.useGravity = true;
				//rigidbody.isKinematic = true; // we should force shit downwards instead of lettign gravity do it
				this.detonateOnImpact = base.gameObject.AddComponent<DetonateOnImpact>();
				this.detonateOnImpact.droppedState = this;
				if(base.rigidbodyMotor)
                {
					this.bodyCouldTakeImpactDamage = base.rigidbodyMotor.canTakeImpactDamage;
					base.rigidbodyMotor.canTakeImpactDamage = false;
                }
			}

		}

        private void DoSplashDamage(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
			this.detonateNextFrame = true;
		}

        public override void OnExit()
		{
			if(base.characterMotor)
            {
				base.characterMotor.onMovementHit -= DoSplashDamage;
            }

			if (this.detonateOnImpact) Destroy(this.detonateOnImpact);
			if (this.tempRigidbody) Destroy(this.tempRigidbody);
			if (this.tempSphereCollider) Destroy(this.tempSphereCollider);

			if (base.rigidbodyMotor)
			{
				base.rigidbodyMotor.canTakeImpactDamage = this.bodyCouldTakeImpactDamage;
			}

			base.rigidbody.useGravity = bodyHadGravity;
			base.rigidbody.isKinematic = bodyWasKinematic;

			if (NetworkServer.active) base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage; // fuck u loader

			Animator modelAnimator = base.GetModelAnimator();
			if (modelAnimator)
			{
				modelAnimator.enabled = true;
			}
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.characterMotor)
			{
				base.characterMotor.velocity += Vector3.up * extraGravity * Time.fixedDeltaTime;
			}
			else
            {
				Rigidbody rigidbody = base.rigidbody ? base.rigidbody : this.tempRigidbody;
				rigidbody.velocity += Vector3.up * extraGravity * Time.fixedDeltaTime;
			}

			if (detonateNextFrame && (!base.characterMotor || (base.characterMotor.Motor.GroundingStatus.IsStableOnGround && !base.characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
            {
				if (base.characterMotor)
					base.characterMotor.velocity = Vector3.zero;
				else if (base.rigidbody)
					base.rigidbody.velocity = Vector3.zero;
				else if (this.tempRigidbody)
					this.tempRigidbody.velocity = Vector3.zero;
				
				Util.PlaySound("Hit2", base.gameObject);
				if (NetworkServer.active)
				{
					EffectManager.SpawnEffect(hitGroundEffect, new EffectData
					{
						origin = base.characterBody.footPosition,
						scale = 1.25f,
					}, true);


					if (this.inflictor)
					{
						CharacterBody inflictorBody = this.inflictor.GetComponent<CharacterBody>();
						float damageStat = inflictorBody ? inflictorBody.damage : 12f;

						BlastAttack blastAttack = new BlastAttack();
						blastAttack.position = base.characterBody.footPosition;
						blastAttack.baseDamage = damageCoefficient * damageStat;
						blastAttack.baseForce = force;
						blastAttack.bonusForce = Vector3.up * bounceForce;
						blastAttack.radius = blastRadius;
						blastAttack.attacker = this.inflictor;
						blastAttack.inflictor = this.inflictor;
						blastAttack.teamIndex = inflictorBody.teamComponent.teamIndex;
						blastAttack.crit = inflictorBody.RollCrit();
						blastAttack.procChainMask = default(ProcChainMask);
						blastAttack.procCoefficient = procCoefficient;
						blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
						blastAttack.damageColorIndex = DamageColorIndex.Default;
						blastAttack.damageType = DamageType.Stun1s;
						blastAttack.attackerFiltering = AttackerFiltering.Default;
						//blastAttack.impactEffect = 
						BlastAttack.Result result = blastAttack.Fire();

					};
				}

				this.outer.SetNextStateToMain();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return friendlyDrop ? InterruptPriority.Skill : InterruptPriority.Vehicle; //////////////////////////////////
		}


		public class DetonateOnImpact : MonoBehaviour
        {
			public DroppedState droppedState;
            private void OnCollisionEnter(Collision collision)
            {
                if(collision.gameObject.layer == LayerIndex.world.intVal)
                {
					this.droppedState.detonateNextFrame = true;
					Destroy(this);
                }
            }
        }
	}
}