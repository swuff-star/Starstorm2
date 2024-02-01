﻿using RoR2;
using RoR2.UI;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Text;
using TMPro;
using MonoMod.RuntimeDetour;
using R2API;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;

namespace Moonstorm.Starstorm2.Items
{
	// hell
    public sealed class CompositeInjector : ItemBase
    {
        private const string token = "SS2_ITEM_COMPOSITEINJECTOR_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("CompositeInjector", SS2Bundle.Items);

        public static int funnyNumber = 16;
        public static float funnyNumber2 = 60f; // fuck unity ui. its going off the screen. i dont fcare
		public static Vector3 funnyVector = new Vector3(-72f,0f,0f);

		public override void Initialize()
        {
			var hook = new Hook(typeof(EquipmentIcon).GetMethod(nameof(EquipmentIcon.GenerateDisplayData), (System.Reflection.BindingFlags)(-1)), typeof(CompositeInjector).GetMethod(nameof(EquipmentIcon_GenerateDisplayData), (System.Reflection.BindingFlags)(-1)));
            On.RoR2.UI.HUD.Awake += AddIcons;
            On.RoR2.EquipmentDef.AttemptGrant += EquipmentDef_AttemptGrant;
            IL.RoR2.CharacterBody.OnEquipmentLost += CharacterBody_OnEquipmentLost;
            EquipmentSlot.onServerEquipmentActivated += ActivateAllEquipment;
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;
        }

        #region Gameplay Mechanics
        private void ActivateAllEquipment(EquipmentSlot self, EquipmentIndex _)
		{
			Inventory inventory = self.characterBody.inventory;
			if (inventory?.GetItemCount(SS2Content.Items.CompositeInjector) > 0)
			{
				for (int i = 0; i < inventory.GetEquipmentSlotCount(); i++)
				{
					EquipmentState state = inventory.GetEquipment((uint)i);
					if (i != inventory.activeEquipmentSlot && state.equipmentIndex != EquipmentIndex.None)
					{
						self.PerformEquipmentAction(EquipmentCatalog.GetEquipmentDef(state.equipmentIndex));
					}
				}
			}
		}

		// new equipment to first slot, move old equipment to third slot, move evreything else upwards, pop out last equipment
		private void EquipmentDef_AttemptGrant(On.RoR2.EquipmentDef.orig_AttemptGrant orig, ref PickupDef.GrantContext context)
		{
			Inventory inventory = context.body.inventory;
			EquipmentState oldEquipmentState = inventory.currentEquipmentState;

			orig(ref context);
			if (oldEquipmentState.Equals(EquipmentState.empty)) return;

			int stack = inventory.GetItemCount(SS2Content.Items.CompositeInjector);
			if (stack <= 0) return;

			EquipmentState lastEquipmentState = inventory.GetEquipment(2 + (uint)stack - 1);

			//move equipment upwards
			for (int i = 2; i < 2 + stack; i++)
			{
				EquipmentState newEquipmentState = oldEquipmentState;
				oldEquipmentState = inventory.GetEquipment((uint)i);
				SS2Log.Info($"Setting slot {i} from {oldEquipmentState.equipmentDef} to {newEquipmentState.equipmentDef}.");
				// SetEquipmentInternal doesnt call onInventoryChanged each time like SetEquipment does, so its ideal.
				inventory.SetEquipmentInternal(newEquipmentState, (uint)i);

				if (oldEquipmentState.Equals(EquipmentState.empty))
				{
					SS2Log.Info("Reached empty slot.");
					context.shouldDestroy = true; // destroy pickup since its in our inventory now
					return;
				}
			}

			//pop out last equipment			
			PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(lastEquipmentState.equipmentIndex);
			context.controller.NetworkpickupIndex = pickupIndex;
			if (pickupIndex == PickupIndex.none)
			{
				context.shouldDestroy = true;
			}
		}

		// vanilla only adds passivebuffdef of the active equipmentslot
		// if body has composite injector, only remove passivebuffdef if the equipment isnt in any slot.
		private void CharacterBody_OnEquipmentLost(ILContext il)
        {
			ILCursor c = new ILCursor(il);
			bool b = c.TryGotoNext(MoveType.After,
				x => x.MatchLdarg(1),
				x => x.MatchLdfld<EquipmentDef>(nameof(EquipmentDef.passiveBuffDef))); // if(NetworkServer.active && passiveBuffDef != null)
			if (b)
			{
				c.Emit(OpCodes.Ldarg_0); // body
				c.EmitDelegate<Func<EquipmentDef, CharacterBody, bool>>((ed, body) =>
				{
					// return TRUE if we want to remove the buff
					// return FALSE if we want to skip removing the buff
					if (ed == null || body.inventory.GetItemCount(SS2Content.Items.CompositeInjector) <= 0) return true;
					
					for(int i = 0; i < body.inventory.GetEquipmentSlotCount(); i++)
                    {
						if (body.inventory.GetEquipment((uint)i).equipmentDef == ed) return false;
                    }
					return true;
				});
			}
			else
			{
				SS2Log.Warning("CompositeInjector.CharacterBody_OnEquipmentLost: ILHook failed.");
			}
		}
        #endregion

        #region Item Displays
        //add a component to CharacterModel that handles extra equipment displays
        private void CharacterModel_Awake(On.RoR2.CharacterModel.orig_Awake orig, CharacterModel self)
		{
			orig(self);
			self.gameObject.AddComponent<ExtraEquipmentDisplays>();
		}

		// keep itemdisplays of all equipment in inventory, if body has composite injector.
		// mostly copypasted functionality of charactermodel, except it keeps more than one equipment display active
		private class ExtraEquipmentDisplays : MonoBehaviour
		{
			private CharacterModel model;
			private ChildLocator childLocator;
			private List<CharacterModel.ParentedPrefabDisplay> parentedPrefabDisplays = new List<CharacterModel.ParentedPrefabDisplay>();
			private List<CharacterModel.LimbMaskDisplay> limbMaskDisplays = new List<CharacterModel.LimbMaskDisplay>();
			private List<EquipmentIndex> enabledEquipmentDisplays = new List<EquipmentIndex>();

			private void Awake()
			{
				this.model = base.GetComponent<CharacterModel>();
				this.childLocator = base.GetComponent<ChildLocator>();
			}
			private void OnEnable()
			{
				if (model.body) model.body.onInventoryChanged += OnInventoryChanged;
			}
			private void OnDisable()
			{
				if (model.body) model.body.onInventoryChanged -= OnInventoryChanged;
			}

			private void OnInventoryChanged()
			{
				EquipmentIndex equipmentIndex = 0;
				EquipmentIndex equipmentCount = (EquipmentIndex)EquipmentCatalog.equipmentCount;
				while (equipmentIndex < equipmentCount)
				{
					// only show multiple equipments with composite injector, to not interfere with toolbot
					// also dont want to re enable CharacterModel's equipment display
					if (HasEquipment(equipmentIndex) && model.body.inventory.GetItemCount(SS2Content.Items.CompositeInjector) > 0)// && equipmentIndex != model.currentEquipmentDisplayIndex)
					{
						SS2Log.Info("ATTEMPT ENABLE DISPLAY::::: " + equipmentIndex);
						this.EnableEquipmentDisplay(equipmentIndex);
					}
					else
					{
						this.DisableEquipmentDisplay(equipmentIndex);
					}
					equipmentIndex++;
				}
			}
			private void EnableEquipmentDisplay(EquipmentIndex index)
			{
				if (this.enabledEquipmentDisplays.Contains(index))
				{
					return;
				}
				this.enabledEquipmentDisplays.Add(index);
				if (model.itemDisplayRuleSet)
				{
					SS2Log.Info("ENABLED DISPLAY::::: " + index);
					DisplayRuleGroup itemDisplayRuleGroup = model.itemDisplayRuleSet.GetEquipmentDisplayRuleGroup(index);
					this.InstantiateDisplayRuleGroup(itemDisplayRuleGroup, index);
				}
			}
			private void DisableEquipmentDisplay(EquipmentIndex index)
			{
				if (!this.enabledEquipmentDisplays.Contains(index))
				{
					return;
				}
				SS2Log.Info("REMOVING DISPLAY::::: " + index);
				this.enabledEquipmentDisplays.Remove(index);
				for (int i = this.parentedPrefabDisplays.Count - 1; i >= 0; i--)
				{
					if (this.parentedPrefabDisplays[i].equipmentIndex == index)
					{
						this.parentedPrefabDisplays[i].Undo();
						this.parentedPrefabDisplays.RemoveAt(i);
					}
				}
				for (int j = this.limbMaskDisplays.Count - 1; j >= 0; j--)
				{
					if (this.limbMaskDisplays[j].equipmentIndex == index)
					{
						this.limbMaskDisplays[j].Undo(model);
						this.limbMaskDisplays.RemoveAt(j);
					}
				}
			}
			private bool HasEquipment(EquipmentIndex index)
			{
				Inventory inventory = model.body.inventory;
				for (int i = 0; i < inventory.GetEquipmentSlotCount(); i++)
				{
					if (inventory.GetEquipment((uint)i).equipmentIndex == index) return true;
				}
				return false;
			}

			private void InstantiateDisplayRuleGroup(DisplayRuleGroup displayRuleGroup, EquipmentIndex equipmentIndex)
			{
				if (displayRuleGroup.rules != null)
				{
					for (int i = 0; i < displayRuleGroup.rules.Length; i++)
					{
						ItemDisplayRule itemDisplayRule = displayRuleGroup.rules[i];
						ItemDisplayRuleType ruleType = itemDisplayRule.ruleType;
						if (ruleType != ItemDisplayRuleType.ParentedPrefab)
						{
							if (ruleType == ItemDisplayRuleType.LimbMask)
							{
								CharacterModel.LimbMaskDisplay item = new CharacterModel.LimbMaskDisplay
								{
									equipmentIndex = equipmentIndex
								};
								item.Apply(model, itemDisplayRule.limbMask);
								this.limbMaskDisplays.Add(item);
							}
						}
						else if (this.childLocator)
						{
							Transform transform = this.childLocator.FindChild(itemDisplayRule.childName);
							if (transform)
							{
								CharacterModel.ParentedPrefabDisplay item2 = new CharacterModel.ParentedPrefabDisplay
								{
									equipmentIndex = equipmentIndex
								};
								item2.Apply(model, itemDisplayRule.followerPrefab, transform, itemDisplayRule.localPos, Quaternion.Euler(itemDisplayRule.localAngles), itemDisplayRule.localScale);
								this.parentedPrefabDisplays.Add(item2);
							}
						}
					}
				}
			}
		}
        #endregion

        #region	HUD stuff
        // dont display mul-t's alt equipment slot if we just have composite injector
        private static EquipmentIcon.DisplayData EquipmentIcon_GenerateDisplayData(Func<EquipmentIcon, EquipmentIcon.DisplayData> orig, EquipmentIcon self)
        {
			EquipmentIcon.DisplayData result = orig(self);
			if (self.displayAlternateEquipment && self.targetInventory)
			{				
				int stacks = self.targetInventory.GetItemCount(SS2Content.Items.CompositeInjector);
				bool shouldHide;
				if (stacks <= 0) shouldHide = (self.targetInventory.GetEquipmentSlotCount() <= 1); // vanilla condition. this could just be replaced with our condition but im being safe.
				else shouldHide = self.targetInventory.GetEquipment(1).Equals(EquipmentState.empty); // otherwise only hide it if empty

				result.hideEntireDisplay = shouldHide;
			}
			return result;
		}

       
    
		//its fuckeed is so fucked
        private void AddIcons(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);

			IconHolder epic = self.gameObject.AddComponent<IconHolder>();
			epic.hud = self;
			epic.icons = new EquipmentIconButEpic[funnyNumber];
            Transform scaler = self.transform.Find("MainContainer/MainUIArea/SpringCanvas/BottomRightCluster/Scaler");
            Transform slot = scaler.Find("AltEquipmentSlot");
            for(int i = 1; i <= funnyNumber; i++)
            {
                Transform newIcon = GameObject.Instantiate(slot.gameObject).transform;
                newIcon.SetParent(slot.parent, false);
                newIcon.transform.position += funnyVector + (Vector3.up * funnyNumber2 * i);

				EquipmentIcon oldIcon = newIcon.GetComponent<EquipmentIcon>();
				EquipmentIconButEpic icon = newIcon.gameObject.AddComponent<EquipmentIconButEpic>();
				icon.targetSlotIndex = (uint)i + 1;
				epic.icons[i - 1] = icon;

				GameObject.Destroy(oldIcon.cooldownText);
				GameObject.Destroy(oldIcon.stockText);
				icon.displayRoot = oldIcon.displayRoot;
				icon.iconImage = oldIcon.iconImage;
				icon.isAutoCastPanelObject = oldIcon.isAutoCastPanelObject;
				icon.tooltipProvider = oldIcon.tooltipProvider;
				icon.displayAlternateEquipment = true;
				GameObject.Destroy(oldIcon);
			}
        }


		public class IconHolder : MonoBehaviour
		{
			public HUD hud;
			public EquipmentIconButEpic[] icons;

			private void Update()
			{
				foreach (EquipmentIconButEpic epic in icons)
					epic.targetInventory = hud.targetMaster?.inventory;
			}
		}

		// copypaste of ror2.ui.equipmenticon but we get to choose the equipmentslot
		// keeping the hex tokens and redudant stuff to remind me of my failure

		// Token: 0x02000CFA RID: 3322
		public class EquipmentIconButEpic : MonoBehaviour
		{
			// Token: 0x170006DC RID: 1756
			// (get) Token: 0x06004BC2 RID: 19394 RVA: 0x00137FA8 File Offset: 0x001361A8
            public bool hasEquipment
			{
				get
				{
					return this.currentDisplayData.hasEquipment;
				}
			}

			// Token: 0x06004BC3 RID: 19395 RVA: 0x00137FB8 File Offset: 0x001361B8
			private void SetDisplayData(EquipmentIconButEpic.DisplayData newDisplayData)
			{
				if (!this.currentDisplayData.isReady && newDisplayData.isReady)
				{
					this.DoStockFlash();
				}
				if (this.displayRoot)
				{
					this.displayRoot.SetActive(!newDisplayData.hideEntireDisplay);
				}
				if (newDisplayData.stock > this.currentDisplayData.stock)
				{
					Util.PlaySound("Play_item_proc_equipMag", RoR2Application.instance.gameObject);
					this.DoStockFlash();
				}
				if (this.isReadyPanelObject)
				{
					this.isReadyPanelObject.SetActive(newDisplayData.isReady);
				}
				if (this.isAutoCastPanelObject)
				{
					if (this.targetInventory)
					{
						this.isAutoCastPanelObject.SetActive(this.targetInventory.GetItemCount(RoR2Content.Items.AutoCastEquipment) > 0);
					}
					else
					{
						this.isAutoCastPanelObject.SetActive(false);
					}
				}
				if (this.iconImage)
				{
					Texture texture = null;
					Color color = Color.clear;
					if (newDisplayData.equipmentDef != null)
					{
						color = ((newDisplayData.stock > 0) ? Color.white : Color.gray);
						texture = newDisplayData.equipmentDef.pickupIconTexture;
					}
					this.iconImage.texture = texture;
					this.iconImage.color = color;
				}
				if (this.cooldownText)
				{
					this.cooldownText.gameObject.SetActive(newDisplayData.showCooldown);
					if (newDisplayData.cooldownValue != this.currentDisplayData.cooldownValue)
					{
						StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
						stringBuilder.AppendInt(newDisplayData.cooldownValue, 1U, uint.MaxValue);
						this.cooldownText.SetText(stringBuilder);
						HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
					}
				}
				if (this.stockText)
				{
					if (newDisplayData.hasEquipment && (newDisplayData.maxStock > 1 || newDisplayData.stock > 1))
					{
						this.stockText.gameObject.SetActive(true);
						StringBuilder stringBuilder2 = HG.StringBuilderPool.RentStringBuilder();
						stringBuilder2.AppendInt(newDisplayData.stock, 1U, uint.MaxValue);
						this.stockText.SetText(stringBuilder2);
						HG.StringBuilderPool.ReturnStringBuilder(stringBuilder2);
					}
					else
					{
						this.stockText.gameObject.SetActive(false);
					}
				}
				string titleToken = null;
				string bodyToken = null;
				Color titleColor = Color.white;
				Color gray = Color.gray;
				if (newDisplayData.equipmentDef != null)
				{
					titleToken = newDisplayData.equipmentDef.nameToken;
					bodyToken = newDisplayData.equipmentDef.pickupToken;
					titleColor = ColorCatalog.GetColor(newDisplayData.equipmentDef.colorIndex);
				}
				if (this.tooltipProvider)
				{
					this.tooltipProvider.titleToken = titleToken;
					this.tooltipProvider.titleColor = titleColor;
					this.tooltipProvider.bodyToken = bodyToken;
					this.tooltipProvider.bodyColor = gray;
				}
				this.currentDisplayData = newDisplayData;
			}

			// Token: 0x06004BC4 RID: 19396 RVA: 0x0013825C File Offset: 0x0013645C
			private void DoReminderFlash()
			{
				if (this.reminderFlashPanelObject)
				{
					AnimateUIAlpha component = this.reminderFlashPanelObject.GetComponent<AnimateUIAlpha>();
					if (component)
					{
						component.time = 0f;
					}
					this.reminderFlashPanelObject.SetActive(true);
				}
				this.equipmentReminderTimer = 5f;
			}

			// Token: 0x06004BC5 RID: 19397 RVA: 0x001382AC File Offset: 0x001364AC
			private void DoStockFlash()
			{
				this.DoReminderFlash();
				if (this.stockFlashPanelObject)
				{
					AnimateUIAlpha component = this.stockFlashPanelObject.GetComponent<AnimateUIAlpha>();
					if (component)
					{
						component.time = 0f;
					}
					this.stockFlashPanelObject.SetActive(true);
				}
			}

			// Token: 0x06004BC6 RID: 19398 RVA: 0x001382F8 File Offset: 0x001364F8
			private EquipmentIconButEpic.DisplayData GenerateDisplayData()
			{
				EquipmentIconButEpic.DisplayData result = default(EquipmentIconButEpic.DisplayData);
				EquipmentIndex equipmentIndex = EquipmentIndex.None;
				if (this.targetInventory)
				{
					EquipmentState equipmentState;
					if (this.displayAlternateEquipment)
					{
						//equipmentState = this.targetInventory.alternateEquipmentState; // VANILLA CODE /////////////////////////////////////////////////////////
						//result.hideEntireDisplay = (this.targetInventory.GetEquipmentSlotCount() <= 1);
						equipmentState = this.targetInventory.GetEquipment(this.targetSlotIndex); // NEW /////////////////////////////////////////////////////////
						result.hideEntireDisplay = (this.targetInventory.GetEquipmentSlotCount() - 1 < this.targetSlotIndex);
					}
					else
					{
						equipmentState = this.targetInventory.currentEquipmentState;
						result.hideEntireDisplay = false;
					}
					Run.FixedTimeStamp now = Run.FixedTimeStamp.now;
					Run.FixedTimeStamp chargeFinishTime = equipmentState.chargeFinishTime;
					equipmentIndex = equipmentState.equipmentIndex;
					result.cooldownValue = (chargeFinishTime.isInfinity ? 0 : Mathf.CeilToInt(chargeFinishTime.timeUntilClamped));
					result.stock = (int)equipmentState.charges;
					result.maxStock = (this.targetEquipmentSlot ? this.targetEquipmentSlot.maxStock : 1);
				}
				else if (this.displayAlternateEquipment)
				{
					result.hideEntireDisplay = true;
				}
				result.equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
				return result;
			}

			// Token: 0x06004BC7 RID: 19399 RVA: 0x001383E4 File Offset: 0x001365E4
			private void Update()
			{
				this.SetDisplayData(this.GenerateDisplayData());
				this.equipmentReminderTimer -= Time.deltaTime;
				if (this.currentDisplayData.isReady && this.equipmentReminderTimer < 0f && this.currentDisplayData.equipmentDef != null)
				{
					this.DoReminderFlash();
				}
			}

			public uint targetSlotIndex;
			// Token: 0x04004870 RID: 18544
			public Inventory targetInventory;

			// Token: 0x04004871 RID: 18545
			public EquipmentSlot targetEquipmentSlot;

			// Token: 0x04004872 RID: 18546
			public GameObject displayRoot;

			// Token: 0x04004873 RID: 18547
			public PlayerCharacterMasterController playerCharacterMasterController;

			// Token: 0x04004874 RID: 18548
			public RawImage iconImage;

			// Token: 0x04004875 RID: 18549
			public TextMeshProUGUI cooldownText;

			// Token: 0x04004876 RID: 18550
			public TextMeshProUGUI stockText;

			// Token: 0x04004877 RID: 18551
			public GameObject stockFlashPanelObject;

			// Token: 0x04004878 RID: 18552
			public GameObject reminderFlashPanelObject;

			// Token: 0x04004879 RID: 18553
			public GameObject isReadyPanelObject;

			// Token: 0x0400487A RID: 18554
			public GameObject isAutoCastPanelObject;

			// Token: 0x0400487B RID: 18555
			public TooltipProvider tooltipProvider;

			// Token: 0x0400487C RID: 18556
			public bool displayAlternateEquipment;

			// Token: 0x0400487D RID: 18557
			private int previousStockCount;

			// Token: 0x0400487E RID: 18558
			private float equipmentReminderTimer;

			// Token: 0x0400487F RID: 18559
			private EquipmentIconButEpic.DisplayData currentDisplayData;

			// Token: 0x02000CFB RID: 3323
			private struct DisplayData
			{
				// Token: 0x170006DD RID: 1757
				// (get) Token: 0x06004BC9 RID: 19401 RVA: 0x00138442 File Offset: 0x00136642
				public bool isReady
				{
					get
					{
						return this.stock > 0;
					}
				}

				// Token: 0x170006DE RID: 1758
				// (get) Token: 0x06004BCA RID: 19402 RVA: 0x0013844D File Offset: 0x0013664D
				public bool hasEquipment
				{
					get
					{
						return this.equipmentDef != null;
					}
				}

				// Token: 0x170006DF RID: 1759
				// (get) Token: 0x06004BCB RID: 19403 RVA: 0x0013845B File Offset: 0x0013665B
				public bool showCooldown
				{
					get
					{
						return !this.isReady && this.hasEquipment;
					}
				}

				// Token: 0x04004880 RID: 18560
				public EquipmentDef equipmentDef;

				// Token: 0x04004881 RID: 18561
				public int cooldownValue;

				// Token: 0x04004882 RID: 18562
				public int stock;

				// Token: 0x04004883 RID: 18563
				public int maxStock;

				// Token: 0x04004884 RID: 18564
				public bool hideEntireDisplay;
			}
		}
        #endregion
    }
}