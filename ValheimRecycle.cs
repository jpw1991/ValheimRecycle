﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using ValheimRecycle.GameClasses;

namespace ValheimRecycle
{
    [BepInPlugin("org.lafchi.plugins.valheim_recycle", "Valheim Recycle", "2.4.0")]
    [BepInProcess("valheim.exe")]
    public class ValheimRecycle : BaseUnityPlugin
    {

        internal static ValheimRecycle instance;
        internal GameObject recycleObject;
        internal Button recycleButton;
        internal float width;
        Vector3 craftingPos;
        Harmony harmony;

        #region Config
        internal ConfigEntry<RecycleConfig.TabPositions> tabPosition;
        internal ConfigEntry<float> resourceMultiplier;
        internal ConfigEntry<bool> preserveOriginalItem;
        internal ConfigEntry<int> nexusID;
        #endregion

        internal bool InTabDeconstruct()
        {
            return !recycleButton.interactable;
        }

        internal void Awake()
        {
            Logger.LogInfo("AWAKE");
            instance = this;
            harmony = Harmony.CreateAndPatchAll(typeof(InventoryGuiPatch));
            Harmony.CreateAndPatchAll(typeof(HumanoidPatch));
            
            tabPosition = Config.Bind("General",   
                             "TabPosition",  
                             RecycleConfig.TabPositions.Left,
                             "The Recycle tab's position in the crafting menu after Upgrade. (Requires restart)");
            resourceMultiplier = Config.Bind("General",
                 "ResourceMultiplier",
                 1f,
                 new ConfigDescription("The amount of resources to return from recycling (0 to 1, where 1 returns 100% of the resources and 0 returns 0%)", new AcceptableValueRange<float>(0,1))
                 );
            preserveOriginalItem = Config.Bind("General", "PreserveOriginalItem", true, "Whether the original item's data should be preserved when downgrading. Useful for mods which add extra properties to items like EpicLoot.\nTurn off if experiencing problems.");
            nexusID = Config.Bind<int>("General", "NexusID", 425, "Nexus mod ID for updates");

        }
        internal void OnDestroy()
        {
            Logger.LogInfo("DESTROY");
            Destroy(recycleObject);
            harmony.UnpatchSelf();
            Logger.LogInfo("Unpatched InventoryGui");
        }

        internal GameObject GetOrCreateRecycleTab()
        {
            if (instance.recycleObject != null)
            {
                return instance.recycleObject;

            }
            Logger.LogInfo("CreateRecycleButton");

            recycleObject = Instantiate(InventoryGui.instance.m_tabUpgrade.gameObject, InventoryGui.instance.m_tabUpgrade.gameObject.transform.parent);
            if (recycleObject is null)
            {
                Logger.LogError($"RecycletButton couldn't be instantiated.");
                return null;
            }
            recycleObject.name = "Recycle";
            recycleObject.GetComponentInChildren<Text>().text = "RECYCLE";
            width = recycleObject.GetComponent<RectTransform>().rect.width;
            craftingPos = new Vector3(recycleObject.transform.localPosition.x + ((width + 10f) * ((int)tabPosition.Value + 1)), recycleObject.transform.localPosition.y, recycleObject.transform.localPosition.z);
            recycleButton = recycleObject.GetComponent<Button>();
            recycleButton.transform.localPosition = craftingPos;
            recycleButton.interactable = true;
            recycleButton.name = "RecycleButton";
            recycleButton.onClick.RemoveAllListeners();
            recycleButton.onClick.AddListener(SelectRecycleTab);
            recycleObject.SetActive(false);
            return recycleObject;
        }

        internal void SelectRecycleTab()
        {
            Logger.LogDebug("Selected recycle");
            recycleButton.interactable = false;
            InventoryGui.m_instance.m_tabCraft.interactable = true;
            InventoryGui.m_instance.m_tabUpgrade.interactable = true;
            InventoryGui.m_instance.UpdateCraftingPanel(false);

        }

        internal void RebuildRecycleTab()
        {
            GetOrCreateRecycleTab();
        }

    }
}