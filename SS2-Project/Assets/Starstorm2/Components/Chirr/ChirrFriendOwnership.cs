﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
namespace Moonstorm.Starstorm2.Components
{
	public class ChirrFriendOwnership : MonoBehaviour
	{
		private CharacterMaster master;
		public static float friendAimSpeedCoefficient = 3f;
		public Friend currentFriend;
		public bool hasFriend
        {
			get => this.currentFriend.master != null; /////////////////////// ?
        }
		public struct Friend
		{
			public Friend(CharacterBody body)
            {
				this.master = body.master;
				this.itemAcquisitionOrder = null;
				this.itemStacks = null;
				this.teamIndex = body.teamComponent.teamIndex;
				if(this.master)
                {
					this.itemStacks = HG.ArrayUtils.Clone(this.master.inventory.itemStacks);
					this.itemAcquisitionOrder = this.master.inventory.itemAcquisitionOrder; // reference fix later
				}
            }
			public TeamIndex teamIndex;
			public CharacterMaster master;
			public List<ItemIndex> itemAcquisitionOrder;
			public int[] itemStacks;
		}
		public void Awake()
		{
			this.currentFriend = default(Friend);
		}
		public void Start()
		{
			this.master = base.GetComponent<CharacterMaster>();
		}

		
		public void AddFriend(CharacterBody body) // this shouldnt use body as a param now that i think about it
		{
			if (!body || (body && !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless)))
			{
				return;
			}

			if(this.hasFriend)
            {
				GameObject oldBodyObject = this.currentFriend.master.GetBodyObject();
				if(oldBodyObject)
                {
					this.RemoveFriend(oldBodyObject.GetComponent<CharacterBody>());
                }
            }
			// change team
			// turn speed
			// stat buffs (?)
			//copy inventory
			//heal
			//cleanse debuffs
			//add to minionownership
			//
			this.currentFriend = new Friend(body);

			TeamIndex newTeam = this.currentFriend.teamIndex;
			body.teamComponent.teamIndex = newTeam;
			body.master.teamIndex = newTeam;

			DontDestroyOnLoad(body.masterObject);

			if (NetworkServer.active)
			{
				body.master.minionOwnership.SetOwner(this.master);
				body.healthComponent.HealFraction(1f, default(ProcChainMask));
				Util.CleanseBody(body, true, false, false, true, true, false); // lol
				body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 3f);

				body.master.inventory.CopyItemsFrom(this.master.inventory);

				if (body.master.aiComponents.Length > 0)
				{
					body.master.aiComponents[0].aimVectorMaxSpeed *= friendAimSpeedCoefficient;
				}
			}

			
		}
		public void RemoveFriend(CharacterBody body)
		{
			if (!body || (body && !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless)))
			{
				return;
			}

			if (this.currentFriend.master != null && this.currentFriend.master != body.master)
			{
				SS2Log.Warning("ChirrFriendTracker.RemoveFriend: Body was not a friend");
				return;
			}

			//undo
			TeamIndex newTeam = this.currentFriend.teamIndex;
			body.teamComponent.teamIndex = newTeam;
			body.master.teamIndex = newTeam;

			if (NetworkServer.active)
			{
				body.master.minionOwnership.SetOwner(this.master); // this apparently unsets the owner if you call it again

				Inventory inventory = body.master.inventory;
				inventory.itemAcquisitionOrder.Clear();
				int[] array = inventory.itemStacks;
				int num = 0;
				HG.ArrayUtils.SetAll<int>(array, num);
				body.master.inventory.AddItemsFrom(this.currentFriend.itemStacks, new Func<ItemIndex, bool>(target => true));

				if (body.master.aiComponents.Length > 0)
				{
					body.master.aiComponents[0].aimVectorMaxSpeed /= friendAimSpeedCoefficient;
				}
			}

			this.currentFriend = default(Friend);

		}


	}
}
