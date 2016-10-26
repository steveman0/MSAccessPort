using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using FortressCraft.Community;

public class MSAPPanel : MonoBehaviour
{
    private string mSearchString = string.Empty;
    private int mnNumVisibleBluePrints = 7;
    private int mnAmount = 999;
    public const string TierStart = "Tier";
    public const string TierEnd = "_Button";
    public const string MaterialStart = "Materials_Slot#";
    public const string MaterialEnd = "_Icon";
    public const string CategoryTabStart = "Craft_Tab";
    public const string CategoryTabEnd = "_Icon";
    public const string ModuleStart = "Module";
    public const string ModuleEnd = "_Off_Button";
    public const string EntryStart = "Blueprint_Slot_Background";
    public const int CATEGORY_COUNT = 11;
    public const int MODULE_COUNT = 7;
    public const int TIER_COUNT = 9;
    public const int SLOT_OFFSET = 55;
    public const string InterfaceName = "MSAPPanel";
    public const string InterfaceSetRecipe = "SetRecipe";
    public const string InterfaceTakeOutput = "TakeOutput";
    public const string InterfaceClearRecipe = "ClearRecipe";
    public const string InterfaceSetRecipeSeparator = "x";
    public static MSAPPanel instance;
    private List<MSAPPanel.BlueprintEntry> mCurrentEntries;
    public static bool mbQueueDirty;
    public GameObject Crafting_Background_Panel;
    public GameObject Crafting_Graphics_Panel;
    public GameObject Crafting_Label_Panel;
    private UISprite[] Crafting_Background_Blueprint_Tabs;
    private UISprite[] Craft_Tab_Icons;
    private UISprite Blueprint_Left_Arrow;
    private UISprite Blueprint_Right_Arrow;
    private int mnTabDepth;
    private UISprite[] Module_Icons;
    private UISprite[] Module_Off_Buttons;
    private UISprite[] Module_On_Buttons;
    private UISprite Machine_Select_Off;
    private UISprite BluePrintUpArrow;
    private UISprite BluePrintDownArrow;
    [HideInInspector]
    private UIDraggablePanel Blueprints_Scroll_View;
    private UISprite Blueprint_Slot_Background;
    private UISprite Blueprints_Slot_Icon;
    private UILabel Blueprints_Slot_Quantity_Label;
    private UILabel Blueprints_Slot_Title_Label;
    private UISprite[] Ingredients;
    private UISprite[] Tier_Buttons;
    private UISprite ItemInfoBackground;
    private UISprite Info_Preview_Icon;
    private UILabel ItemInfo_Title_Label;
    private UILabel ItemInfo_Description_Label;
    private UISprite MaterialsBackground;
    private UISprite MaterialsUpArrow;
    private UISprite MaterialsDownArrow;
    private UILabel[] Materials_Slot_Title_Label;
    private UILabel[] Materials_Slot_Quantity_Label;
    private UILabel Craft_Count_Label;
    private UISprite Craft_Button_On;
    private UICheckbox HaveMats_CheckBox;
    private bool mbHaveMatsSelected;
    private UILabel Search_Label;
    private UIInput Search_Input;
    private UISprite Craft_Arrow_Left_Button;
    private UISprite Craft_Arrow_Right_Button;
    private UISprite Automate_Button_On;
    private UILabel Craft_Amount_Label;
    private UIInput Craft_Amount_Input;
    private UISprite Loop_On;
    private UICheckbox Loop_On_CheckBox;
    private UISprite QueueBackground;
    private UISprite Queue_Output_Icon;
    private UISprite Queue_ProgressSlot_Highlight;
    private UISprite Queue_ProgressSlot_Icon;
    private UISprite Queue_PowerBar;
    private UILabel Queue_ProcessingSlot_Label;
    private UILabel Queue_Progress_Label;
    private UILabel Queue_ProgressSlot_Count_Label;
    private UISprite[] QueueSlotSprite;
    private UILabel[] QueueSlotCountLabel;
    public MSAPPanel.UseMode mUseMode;
    private int mnBluePrintHighlight;
    private int mnSelectedModuleIndex;
    private eManufacturingPlantModule mSelectedModule;
    private int mnSelectedCategory;
    public ushort mnCurrentTier;
    private List<CraftData> mSearchResults;
    private List<CraftingCategory> mAvailableCategories;
    private bool[] mTierPresent;
    private Dictionary<eManufacturingPlantModule, bool> availableModulesLookup;
    public MSAccessPort mSelectedPort;
    private GameObject[] MaterialsHighlightGlow;
    private int mnCraftNum;
    private bool mbInit;
    private bool mbEditingAmountTextBox;
    private bool mbEditingSearchTextBox;

    private void Init()
    {
        this.Crafting_Background_Panel = GameObjectUtil.GetObjectFromList("Crafting_Background_Panel");
        this.Crafting_Graphics_Panel = GameObjectUtil.GetObjectFromList("Crafting_Graphics_Panel");
        this.Crafting_Label_Panel = GameObjectUtil.GetObjectFromList("Crafting_Label_Panel");

        this.ItemInfoBackground = FindChildren.FindChild(this.Crafting_Background_Panel, "Crafting_Background_InfoBox").GetComponent<UISprite>();
        this.MaterialsBackground = FindChildren.FindChild(this.Crafting_Background_Panel, "Crafting_Background_Materials_Basic").GetComponent<UISprite>();
        this.QueueBackground = FindChildren.FindChild(this.Crafting_Background_Panel, "Crafting_Background_Queue").GetComponent<UISprite>();
        this.Blueprints_Scroll_View = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Blueprints_Scroll_View").GetComponent<UIDraggablePanel>();
        this.Blueprint_Slot_Background = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Blueprint_Slot_Background").GetComponent<UISprite>();
        this.Blueprints_Slot_Icon = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Blueprints_Slot_Icon").GetComponent<UISprite>();
        this.Blueprints_Slot_Quantity_Label = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Blueprints_Slot_Quantity_Label").GetComponent<UILabel>();
        this.Blueprints_Slot_Title_Label = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Blueprints_Slot_Title_Label").GetComponent<UILabel>();
        this.mCurrentEntries = new List<MSAPPanel.BlueprintEntry>();
        if ((UnityEngine.Object)this.BluePrintUpArrow != (UnityEngine.Object)null)
            throw new AssertException("Init called more than once!?!?!");
        this.Blueprint_Left_Arrow = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Blueprint_Left_Arrow").GetComponent<UISprite>();
        this.Blueprint_Right_Arrow = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Blueprint_Right_Arrow").GetComponent<UISprite>();
        this.Blueprint_Left_Arrow.cachedGameObject.SetActive(false);
        this.Blueprint_Right_Arrow.cachedGameObject.SetActive(false);
        this.BluePrintUpArrow = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Blueprint_Button_Up").GetComponent<UISprite>();
        this.BluePrintDownArrow = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Blueprint_Button_Down").GetComponent<UISprite>();
        this.BluePrintUpArrow.gameObject.SetActive(false);
        this.BluePrintDownArrow.gameObject.SetActive(false);
        this.Tier_Buttons = new UISprite[9];
        for (int index = 0; index < 9; ++index)
            this.Tier_Buttons[index] = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Tier" + (index + 1).ToString() + "_Button").GetComponent<UISprite>();
        this.Ingredients = new UISprite[5];
        this.Materials_Slot_Title_Label = new UILabel[5];
        this.Materials_Slot_Quantity_Label = new UILabel[5];
        for (int index = 0; index < 5; ++index)
        {
            this.Materials_Slot_Title_Label[index] = FindChildren.FindChild(this.Crafting_Label_Panel, "Materials_Slot#" + (index + 1).ToString() + "_Title_Label").GetComponent<UILabel>();
            this.Materials_Slot_Title_Label[index].text = string.Empty;
            this.Materials_Slot_Quantity_Label[index] = FindChildren.FindChild(this.Crafting_Label_Panel, "Materials_Slot#" + (index + 1).ToString() + "_Quantity_Label").GetComponent<UILabel>();
            this.Materials_Slot_Quantity_Label[index].text = string.Empty;
            string withName = "Materials_Slot#" + (object)(index + 1) + "_Icon";
            this.Ingredients[index] = FindChildren.FindChild(this.Crafting_Graphics_Panel, withName).GetComponent<UISprite>();
            this.Ingredients[index].spriteName = "empty";
        }
        this.Crafting_Background_Blueprint_Tabs = new UISprite[11];
        this.Craft_Tab_Icons = new UISprite[11];
        for (int index = 0; index < 11; ++index)
        {
            this.Crafting_Background_Blueprint_Tabs[index] = FindChildren.FindChild(this.Crafting_Background_Panel, "Crafting_Background_Blueprint_Tab" + (index + 1).ToString()).GetComponent<UISprite>();
            this.mnTabDepth = this.Crafting_Background_Blueprint_Tabs[index].depth;
            this.Craft_Tab_Icons[index] = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Craft_Tab" + (index + 1).ToString() + "_Icon").GetComponent<UISprite>();
            if (index > 0)
            {
                this.Crafting_Background_Blueprint_Tabs[index].cachedGameObject.SetActive(false);
                this.Craft_Tab_Icons[index].cachedGameObject.SetActive(false);
            }
        }
        this.Craft_Tab_Icons[0].spriteName = "Unknown";
        this.Module_Icons = new UISprite[7];
        this.Module_Off_Buttons = new UISprite[7];
        this.Module_On_Buttons = new UISprite[7];
        for (int index = 0; index < 7; ++index)
        {
            this.Module_Icons[index] = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Module" + (index + 1).ToString() + "_Icon").GetComponent<UISprite>();
            this.Module_Off_Buttons[index] = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Module" + (index + 1).ToString() + "_Off_Button").GetComponent<UISprite>();
            this.Module_On_Buttons[index] = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Module" + (index + 1).ToString() + "_On_Button").GetComponent<UISprite>();
            this.Module_Icons[index].cachedGameObject.SetActive(false);
            this.Module_Off_Buttons[index].cachedGameObject.SetActive(false);
            this.Module_On_Buttons[index].cachedGameObject.SetActive(false);
        }
        this.Machine_Select_Off = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Machine_Select_Off").GetComponent<UISprite>();
        this.Machine_Select_Off.cachedGameObject.SetActive(false);
        this.Craft_Button_On = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Craft_Button_On").GetComponent<UISprite>();
        this.MaterialsUpArrow = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Materials_Button_Up").GetComponent<UISprite>();
        this.MaterialsDownArrow = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Materials_Button_Down").GetComponent<UISprite>();
        this.MaterialsUpArrow.gameObject.SetActive(false);
        this.MaterialsDownArrow.gameObject.SetActive(false);
        this.HaveMats_CheckBox = FindChildren.FindChild(this.Crafting_Graphics_Panel, "HaveMats_Checkbox1").GetComponent<UICheckbox>();
        this.HaveMats_CheckBox.isChecked = false;
        this.Search_Label = FindChildren.FindChild(this.Crafting_Label_Panel, "Search_Label").GetComponent<UILabel>();
        this.Search_Input = this.Search_Label.GetComponent<UIInput>();
        this.Craft_Arrow_Left_Button = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Craft_Arrow_Left_Button").GetComponent<UISprite>();
        this.Craft_Arrow_Right_Button = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Craft_Arrow_Right_Button").GetComponent<UISprite>();
        this.Automate_Button_On = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Automate_Button_On").GetComponent<UISprite>();
        this.Craft_Amount_Label = FindChildren.FindChild(this.Crafting_Label_Panel, "Craft_#_Label").GetComponent<UILabel>();
        this.Craft_Amount_Input = this.Craft_Amount_Label.GetComponent<UIInput>();
        this.Loop_On = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Loop_On").GetComponent<UISprite>();
        this.Loop_On_CheckBox = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Loop_On").GetComponent<UICheckbox>();
        this.QueueSlotSprite = new UISprite[6];
        this.QueueSlotCountLabel = new UILabel[6];
        for (int index = 0; index < 6; ++index)
        {
            this.QueueSlotCountLabel[index] = FindChildren.FindChild(this.Crafting_Label_Panel, "Queue_Slot#" + (index + 1).ToString() + "_Label").GetComponent<UILabel>();
            string withName = "Queue_Slot#" + (object)(index + 1) + "_Icon";
            this.QueueSlotSprite[index] = FindChildren.FindChild(this.Crafting_Graphics_Panel, withName).GetComponent<UISprite>();
        }
        this.Queue_PowerBar = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Queue_PowerBar").GetComponent<UISprite>();
        this.Queue_Output_Icon = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Queue_Output_Icon").GetComponent<UISprite>();
        this.Queue_ProgressSlot_Icon = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Queue_ProgressSlot_Icon").GetComponent<UISprite>();
        this.Queue_ProgressSlot_Highlight = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Queue_ProgressSlot_Highlight").GetComponent<UISprite>();
        this.Queue_ProcessingSlot_Label = FindChildren.FindChild(this.Crafting_Label_Panel, "Queue_ProcessingSlot_Label").GetComponent<UILabel>();
        this.Queue_Progress_Label = FindChildren.FindChild(this.Crafting_Label_Panel, "Queue_Progress_Label").GetComponent<UILabel>();
        this.Queue_ProgressSlot_Count_Label = FindChildren.FindChild(this.Crafting_Label_Panel, "Queue_ProgressSlot_Count_Label").GetComponent<UILabel>();
        this.Info_Preview_Icon = FindChildren.FindChild(this.Crafting_Graphics_Panel, "Info_Preview_Icon").GetComponent<UISprite>();
        this.ItemInfo_Title_Label = FindChildren.FindChild(this.Crafting_Label_Panel, "ItemInfo_Title_Label").GetComponent<UILabel>();
        this.ItemInfo_Description_Label = FindChildren.FindChild(this.Crafting_Label_Panel, "ItemInfo_Description_Label").GetComponent<UILabel>();
        this.Craft_Count_Label = FindChildren.FindChild(this.Crafting_Label_Panel, "Craft_#_Label").GetComponent<UILabel>();
        this.Craft_Count_Label.text = this.mnCraftNum.ToString();
        this.mnBluePrintHighlight = -1;
        this.mUseMode = MSAPPanel.UseMode.Plant;
        this.mSelectedPort = (MSAccessPort)null;
        this.mAvailableCategories = new List<CraftingCategory>();
        this.mTierPresent = new bool[9];


        this.availableModulesLookup = new Dictionary<eManufacturingPlantModule, bool>();
        this.availableModulesLookup.Add(eManufacturingPlantModule.None, true);
        this.availableModulesLookup.Add(eManufacturingPlantModule.Printer, false);
        this.availableModulesLookup.Add(eManufacturingPlantModule.Compressor, false);
        this.availableModulesLookup.Add(eManufacturingPlantModule.Extruder, false);
        this.availableModulesLookup.Add(eManufacturingPlantModule.ChipEtcher, false);
        this.availableModulesLookup.Add(eManufacturingPlantModule.HydrojetCutter, false);
        this.availableModulesLookup.Add(eManufacturingPlantModule.RoboticWelder, false);
        this.availableModulesLookup.Add(eManufacturingPlantModule.Incubator, false);
        this.availableModulesLookup.Add(eManufacturingPlantModule.AssemblyStation, false);
        this.Hide();
        HandbookManager.RegisterContextHandler(new HandbookContextHandler(this.ContextualHelpHandler));
    }

    public void ShowMSAccessPort(MSAccessPort port)
    {
        Debug.Log("Getting in panel");
        UIManager.AddUIRules("Crafting", UIRules.RestrictMovement | UIRules.RestrictLooking | UIRules.RestrictBuilding | UIRules.RestrictInteracting | UIRules.ShowCursor | UIRules.SetUIUpdateRate);
        this.Crafting_Background_Panel.SetActive(true);
        this.Crafting_Graphics_Panel.SetActive(true);
        this.Crafting_Label_Panel.SetActive(true);
        Debug.Log("Panel position: " + this.Crafting_Background_Panel.transform.position.x + ", " + this.Crafting_Background_Panel.transform.position.y + ", " + this.Crafting_Background_Panel.transform.position.z + ", ");
        Debug.Log((object)("Enabling MSAccessPort Panel with BluePrint ID " + (object)this.mnBluePrintHighlight));
        if (this.mUseMode != MSAPPanel.UseMode.Plant || port != this.mSelectedPort)
        {
            this.mnCurrentTier = (ushort)0;
            this.mnBluePrintHighlight = -1;
        }
        this.mSelectedPort = port;
        this.mSelectedModule = eManufacturingPlantModule.None;
        this.mUseMode = MSAPPanel.UseMode.Plant;
        this.ShowQueuePanel(true);
        MSAPPanel.mbQueueDirty = true;
        //this.UpdateModules();
        this.CheckCategories();
        this.UpdateCategoryDisplay();
        this.UpdateSearchResults();
        this.SetBlueprintSlot(this.mnBluePrintHighlight);
        if (this.Crafting_Background_Panel.activeSelf)
            Debug.Log("Background_panel is active");
        this.Crafting_Background_Panel.transform.position = WorldScript.instance.mPlayerFrustrum.GetCoordsToUnity(port.mnX, port.mnY, port.mnZ) + new Vector3(0.5f, 0.5f, 0.5f);
        //this.Crafting_Graphics_Panel.transform.position = WorldScript.instance.mPlayerFrustrum.GetCoordsToUnity(port.mnX, port.mnY, port.mnZ) + new Vector3(0.5f, 0.5f, 0.5f);
        //this.Crafting_Label_Panel.transform.position = WorldScript.instance.mPlayerFrustrum.GetCoordsToUnity(port.mnX, port.mnY, port.mnZ) + new Vector3(0.5f, 0.5f, 0.5f);
        Debug.Log("Panel position: " + this.Crafting_Background_Panel.transform.position.x + ", " + this.Crafting_Background_Panel.transform.position.y + ", " + this.Crafting_Background_Panel.transform.position.z + ", ");
        this.Crafting_Background_Panel.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
        Debug.Log("Panel position: " + this.Crafting_Background_Panel.transform.position.x + ", " + this.Crafting_Background_Panel.transform.position.y + ", " + this.Crafting_Background_Panel.transform.position.z + ", ");



    }

    public void ShowSelfCrafting()
    {
        UIManager.AddUIRules("Crafting", UIRules.RestrictMovement | UIRules.RestrictLooking | UIRules.RestrictBuilding | UIRules.RestrictInteracting | UIRules.ShowCursor | UIRules.SetUIUpdateRate);
        this.Crafting_Background_Panel.SetActive(true);
        this.Crafting_Graphics_Panel.SetActive(true);
        this.Crafting_Label_Panel.SetActive(true);
        if (this.mUseMode != MSAPPanel.UseMode.SelfCrafting)
        {
            this.mnCurrentTier = (ushort)0;
            this.mnBluePrintHighlight = -1;
        }
        this.mSelectedPort = (MSAccessPort)null;
        this.mUseMode = MSAPPanel.UseMode.SelfCrafting;
        this.mSelectedModule = eManufacturingPlantModule.None;
        this.ShowQueuePanel(false);
        //this.UpdateModules();
        this.CheckCategories();
        this.UpdateCategoryDisplay();
        this.UpdateSearchResults();
        this.SetBlueprintSlot(this.mnBluePrintHighlight);
    }

    public void Reshow()
    {
        this.Crafting_Background_Panel.SetActive(true);
        this.Crafting_Graphics_Panel.SetActive(true);
        this.Crafting_Label_Panel.SetActive(true);
    }

    public void Hide()
    {
        UIManager.RemoveUIRules("Crafting");
        this.mSelectedPort = (MSAccessPort)null;
        if (this.mbEditingAmountTextBox)
            this.Craft_Amount_Input.selected = false;
        if (this.mbEditingSearchTextBox)
            this.Search_Input.selected = false;
        this.Crafting_Background_Panel.SetActive(false);
        this.Crafting_Graphics_Panel.SetActive(false);
        this.Crafting_Label_Panel.SetActive(false);
    }

    public void HideOnHelp()
    {
        this.mSelectedPort = (MSAccessPort)null;
        if (this.mbEditingAmountTextBox)
            this.Craft_Amount_Input.selected = false;
        if (this.mbEditingSearchTextBox)
            this.Search_Input.selected = false;
        this.Crafting_Background_Panel.SetActive(false);
        this.Crafting_Graphics_Panel.SetActive(false);
        this.Crafting_Label_Panel.SetActive(false);
    }

    public void HideItemInfoAndMaterials()
    {
        this.ItemInfoBackground.gameObject.SetActive(false);
        this.MaterialsBackground.gameObject.SetActive(false);
    }

    private List<HandbookContextEntry> ContextualHelpHandler()
    {
        List<HandbookContextEntry> list = new List<HandbookContextEntry>();
        if (!this.Crafting_Background_Panel.activeSelf || this.mnBluePrintHighlight < 0 || this.mnBluePrintHighlight >= this.mSearchResults.Count)
            return list;
        CraftData craftData = this.mSearchResults[this.mnBluePrintHighlight];
        if (craftData.CraftableItemType >= 0)
            list.Add(HandbookContextEntry.Material(850, craftData.CraftableItemType, "Selected Recipe"));
        else
            list.Add(HandbookContextEntry.Material(850, craftData.CraftableCubeType, craftData.CraftableCubeValue, "Selected Recipe"));
        return list;
    }

    //private void UpdateModules()
    //{
    //    this.ResetAvailableModules();
    //    if (this.mSelectedPort == null || this.mSelectedPort.modules.Count == 0)
    //    {
    //        for (int index = 0; index < 7; ++index)
    //        {
    //            this.Module_Icons[index].cachedGameObject.SetActive(false);
    //            this.Module_Off_Buttons[index].cachedGameObject.SetActive(false);
    //            this.Module_On_Buttons[index].cachedGameObject.SetActive(false);
    //        }
    //    }
    //    else
    //    {
    //        this.SetModuleGraphic(0, (ushort)520, (ushort)0);
    //        if (this.mnSelectedModuleIndex > this.mSelectedPort.modules.Count + 1)
    //        {
    //            this.mnSelectedModuleIndex = 0;
    //            this.mSelectedModule = eManufacturingPlantModule.None;
    //        }
    //        if (this.mnSelectedModuleIndex > 0)
    //            this.mSelectedModule = (eManufacturingPlantModule)this.mSelectedPort.modules[this.mnSelectedModuleIndex - 1];
    //        int index;
    //        for (index = 0; index < this.mSelectedPort.modules.Count && index < 6; ++index)
    //        {
    //            this.SetModuleGraphic(index + 1, (ushort)521, this.mSelectedPort.modules[index]);
    //            this.availableModulesLookup[(eManufacturingPlantModule)this.mSelectedPort.modules[index]] = true;
    //        }
    //        for (; index < 6; ++index)
    //        {
    //            this.Module_Icons[index + 1].cachedGameObject.SetActive(false);
    //            this.Module_Off_Buttons[index + 1].cachedGameObject.SetActive(false);
    //            this.Module_On_Buttons[index + 1].cachedGameObject.SetActive(false);
    //        }
    //    }
    //}

    private void ResetAvailableModules()
    {
        this.availableModulesLookup[eManufacturingPlantModule.None] = true;
        this.availableModulesLookup[eManufacturingPlantModule.Printer] = false;
        this.availableModulesLookup[eManufacturingPlantModule.Compressor] = false;
        this.availableModulesLookup[eManufacturingPlantModule.Extruder] = false;
        this.availableModulesLookup[eManufacturingPlantModule.ChipEtcher] = false;
        this.availableModulesLookup[eManufacturingPlantModule.HydrojetCutter] = false;
        this.availableModulesLookup[eManufacturingPlantModule.RoboticWelder] = false;
        this.availableModulesLookup[eManufacturingPlantModule.Incubator] = false;
        this.availableModulesLookup[eManufacturingPlantModule.AssemblyStation] = false;
    }

    private void SetModuleGraphic(int index, ushort block, ushort value)
    {
        this.Module_Icons[index].cachedGameObject.SetActive(true);
        this.Module_Off_Buttons[index].cachedGameObject.SetActive(true);
        this.Module_On_Buttons[index].cachedGameObject.SetActive(index == this.mnSelectedModuleIndex);
        Util.SetNGUIIcon(this.Module_Icons[index], TerrainData.GetIconNameForValue(block, value));
    }

    private void SelectModule(int index)
    {
        if (this.mnSelectedModuleIndex == index)
            return;
        this.mnSelectedModuleIndex = index;
        //this.mSelectedModule = index != 0 ? (eManufacturingPlantModule)this.mSelectedPort.modules[index - 1] : eManufacturingPlantModule.None;
        //this.UpdateModules();
        this.CheckCategories();
        this.UpdateCategoryDisplay();
        this.UpdateSearchResults();
        this.SetBlueprintSlot(this.mnBluePrintHighlight);
        this.Blueprints_Scroll_View.ResetPosition();
    }

    public void CheckCategories()
    {
        this.mAvailableCategories.Clear();
        for (int index = 0; index < CraftData.mCraftCategories.Count; ++index)
        {
            CraftingCategory craftingCategory = CraftData.mCraftCategories[index];
            if (CraftingManager.HasEligibleRecipes(WorldScript.mLocalPlayer, craftingCategory.recipes, this.mUseMode == MSAPPanel.UseMode.SelfCrafting, this.mSelectedModule, this.availableModulesLookup))
                this.mAvailableCategories.Add(craftingCategory);
        }
        Debug.Log((object)string.Format("Found {0} available categories", (object)this.mAvailableCategories.Count));
        if (this.mnSelectedCategory < this.mAvailableCategories.Count + 1)
            return;
        this.mnSelectedCategory = 0;
    }

    public void UpdateCategoryDisplay()
    {
        this.Crafting_Background_Blueprint_Tabs[0].cachedGameObject.SetActive(true);
        this.Crafting_Background_Blueprint_Tabs[0].depth = this.mnTabDepth - (this.mnSelectedCategory != 0 ? 0 : -10);
        int index;
        for (index = 0; index < this.mAvailableCategories.Count && index < 10; ++index)
        {
            CraftingCategory craftingCategory = this.mAvailableCategories[index];
            this.Craft_Tab_Icons[index + 1].cachedGameObject.SetActive(true);
            this.Craft_Tab_Icons[index + 1].spriteName = craftingCategory.iconName;
            this.Crafting_Background_Blueprint_Tabs[index + 1].cachedGameObject.SetActive(true);
            this.Crafting_Background_Blueprint_Tabs[index + 1].depth = this.mnTabDepth - (this.mnSelectedCategory != index + 1 ? 0 : -10);
        }
        for (; index < 10; ++index)
        {
            this.Crafting_Background_Blueprint_Tabs[index + 1].cachedGameObject.SetActive(false);
            this.Craft_Tab_Icons[index + 1].cachedGameObject.SetActive(false);
        }
    }

    private void ShowQueuePanel(bool active)
    {
        this.Craft_Arrow_Left_Button.cachedGameObject.SetActive(active);
        this.Craft_Arrow_Right_Button.cachedGameObject.SetActive(active);
        this.Automate_Button_On.cachedGameObject.SetActive(active);
        this.Craft_Amount_Label.cachedGameObject.SetActive(active);
        this.Loop_On.cachedGameObject.SetActive(active);
        this.QueueBackground.cachedGameObject.SetActive(active);
        this.Queue_Output_Icon.cachedGameObject.SetActive(active);
        this.Queue_PowerBar.cachedGameObject.SetActive(active);
        this.Queue_ProgressSlot_Icon.cachedGameObject.SetActive(active);
        this.Queue_ProgressSlot_Highlight.cachedGameObject.SetActive(active);
        this.Queue_ProcessingSlot_Label.cachedGameObject.SetActive(active);
        this.Queue_Progress_Label.cachedGameObject.SetActive(active);
        this.Queue_ProgressSlot_Count_Label.cachedGameObject.SetActive(active);
        for (int index = 0; index < this.QueueSlotSprite.Length; ++index)
        {
            this.QueueSlotSprite[index].cachedGameObject.SetActive(active);
            this.QueueSlotCountLabel[index].cachedGameObject.SetActive(active);
        }
    }

    //private void UpdateQueuePanel()
    //{
    //    CraftData craftData1 = this.mSelectedPort.mSelectedRecipe;
    //    CraftData craftData2 = this.mSelectedPort.mCurrentRecipe;
    //    CraftData entry = craftData2 ?? craftData1;
    //    if (MSAPPanel.mbQueueDirty)
    //    {
    //        if (entry == null)
    //        {
    //            this.Queue_ProgressSlot_Icon.spriteName = "empty";
    //            this.Queue_ProgressSlot_Highlight.spriteName = "empty";
    //            this.Queue_Progress_Label.text = string.Empty;
    //            this.Queue_ProgressSlot_Count_Label.text = string.Empty;
    //            for (int index = 0; index < this.QueueSlotSprite.Length; ++index)
    //            {
    //                this.QueueSlotSprite[index].spriteName = "empty";
    //                this.QueueSlotCountLabel[index].text = string.Empty;
    //            }
    //        }
    //        else
    //        {
    //            this.Queue_Progress_Label.text = craftData1 == null || craftData2 == null || craftData1 == craftData2 ? (craftData1 != null || craftData2 == null ? CraftData.GetName(entry) : "Cancelling") : "Switching";
    //            Util.SetNGUIIcon(this.Queue_ProgressSlot_Icon, CraftData.GetIconName(entry));
    //            this.Queue_ProgressSlot_Icon.color = CraftData.GetIconColor(entry);
    //            Util.SetNGUIIcon(this.Queue_ProgressSlot_Highlight, CraftData.GetIconName(entry));
    //            Color iconColor1 = CraftData.GetIconColor(entry);
    //            iconColor1.r *= 0.5f;
    //            iconColor1.g *= 0.5f;
    //            iconColor1.b *= 0.5f;
    //            this.Queue_ProgressSlot_Highlight.color = iconColor1;
    //            this.Queue_ProgressSlot_Count_Label.text = this.mSelectedPort.mnRequestedAmount <= 0 ? string.Empty : string.Format("{0}x", (object)this.mSelectedPort.mnRequestedAmount);
    //            for (int index = 0; index < this.QueueSlotSprite.Length; ++index)
    //            {
    //                if (index >= entry.Costs.Count)
    //                {
    //                    this.QueueSlotSprite[index].spriteName = "empty";
    //                    this.QueueSlotCountLabel[index].text = string.Empty;
    //                }
    //                else
    //                {
    //                    CraftCost craftCost = entry.Costs[index];
    //                    string iconName = CraftData.GetIconName(craftCost);
    //                    string str = craftCost.Amount.ToString();
    //                    Color iconColor2 = CraftData.GetIconColor(craftCost);
    //                    Color color = Color.white;
    //                    if ((craftData2 == craftData1 || craftData2 == null) && !this.mSelectedPort.mSelectedRecipeCostAvailable[index])
    //                        color = Color.red;
    //                    Util.SetNGUIIcon(this.QueueSlotSprite[index], iconName);
    //                    this.QueueSlotSprite[index].color = iconColor2;
    //                    this.QueueSlotCountLabel[index].text = str;
    //                    this.QueueSlotCountLabel[index].color = color;
    //                }
    //            }
    //        }
    //        ItemBase itemBase = this.mSelectedPort.mOutputHopper;
    //        if (itemBase == null)
    //        {
    //            this.Queue_Output_Icon.spriteName = "empty";
    //            this.Queue_ProcessingSlot_Label.text = string.Empty;
    //        }
    //        else
    //        {
    //            string iconName;
    //            int count;
    //            Color color;
    //            Util.GetIconAndCount(itemBase, out iconName, out count, out color);
    //            Util.SetNGUIIcon(this.Queue_Output_Icon, iconName);
    //            this.Queue_Output_Icon.color = color;
    //            this.Queue_ProcessingSlot_Label.text = count.ToString();
    //        }
    //        MSAPPanel.mbQueueDirty = false;
    //    }
    //    if (entry != null)
    //        this.Queue_ProgressSlot_Highlight.fillAmount = this.mSelectedPort.mProgressTimer / entry.CraftTime;
    //    this.Queue_PowerBar.fillAmount = this.mSelectedPort.mrCurrentPower / this.mSelectedPort.mrMaxPower;
    //}

    private void SelectCategory(int index)
    {
        if (index == this.mnSelectedCategory)
            return;
        this.mnSelectedCategory = index;
        this.UpdateCategoryDisplay();
        this.UpdateSearchResults();
        this.UpdateBlueprintList();
        this.SetBlueprintSlot(-1);
        this.Blueprints_Scroll_View.ResetPosition();
    }

    public void SetTier(int Tier)
    {
        if ((int)(ushort)Tier == (int)this.mnCurrentTier)
            return;
        Debug.Log((object)("Setting to tier: " + (object)Tier));
        this.mnCurrentTier = (ushort)Tier;
        this.UpdateSearchResults();
        this.UpdateBlueprintList();
        this.SetBlueprintSlot(-1);
        this.Blueprints_Scroll_View.ResetPosition();
    }

    public void SetBlueprintSlot(int newHighlight)
    {
        if (newHighlight >= this.mSearchResults.Count)
            return;
        this.mnBluePrintHighlight = newHighlight;
        this.UpdateBlueprintList();
        if (this.mnBluePrintHighlight < 0)
        {
            this.ClearItemInfo();
        }
        else
        {
            CraftData entry1 = this.mSearchResults[newHighlight];
            string str1 = string.Empty;
            if (!string.IsNullOrEmpty(entry1.Description))
                str1 = str1 + entry1.Description + "\n";
            if (CubeHelper.IsDapperDLCBlock(entry1.CraftableCubeType, entry1.CraftableCubeValue))
                str1 = !DLCOwnership.mbHasDapperDLC ? str1 + "This requires the Dapper DLC pack!" : str1 + "Thank you for buying the Dapper DLC pack!";
            if (CubeHelper.IsT4Block(entry1.CraftableCubeType, entry1.CraftableCubeValue) && !DLCOwnership.mbHasPatreon && !DLCOwnership.mbHasT4)
                str1 += "This will require the Frozen Factory Expansion pack! (Coming soon!)";
            this.ItemInfo_Description_Label.text = str1;
            string str2 = CraftData.GetName(entry1);
            int num1 = entry1.CraftedAmount;
            if (num1 > 1)
                str2 = str2 + (object)" x" + (string)(object)num1;
            this.ItemInfo_Title_Label.text = str2;
            Util.SetNGUIIcon(this.Info_Preview_Icon, CraftData.GetIconName(entry1));
            for (int index = 0; index < 5; ++index)
            {
                this.Materials_Slot_Title_Label[index].text = string.Empty;
                this.Materials_Slot_Quantity_Label[index].text = string.Empty;
                this.Ingredients[index].spriteName = "empty";
            }
            int count = entry1.Costs.Count;
            bool flag1 = true;
            for (int index = 0; index < count; ++index)
            {
                if (index >= 5)
                {
                    Debug.Log((object)("Can't display crafting cost: " + (object)index));
                }
                else
                {
                    CraftCost entry2 = entry1.Costs[index];
                    int num2 = 0;
                    if ((int)entry2.CubeType != 0)
                        num2 = WorldScript.mLocalPlayer.mInventory.GetCubeTypeCountValue(entry2.CubeType, entry2.CubeValue);
                    else if (entry2.ItemType != -1)
                        num2 = WorldScript.mLocalPlayer.mInventory.GetItemCount(entry2.ItemType);
                    else
                        Debug.Log((object)"Error, crafting item had neither CubeTypes nor Items?");
                    bool flag2 = (long)num2 >= (long)entry2.Amount;
                    string spriteName = CraftData.GetIconName(entry2);
                    string str3 = CraftData.GetName(entry2);
                    string str4 = entry2.Amount.ToString();
                    if ((int)entry2.CubeType != 0 && !WorldScript.mLocalPlayer.mResearch.IsKnown(entry2.CubeType, entry2.CubeValue))
                    {
                        spriteName = "Unknown";
                        str3 = "Unknown Material";
                        str4 = "?";
                        flag1 = false;
                    }
                    Util.SetNGUIIcon(this.Ingredients[index], spriteName);
                    this.Materials_Slot_Title_Label[index].text = str3;
                    this.Materials_Slot_Quantity_Label[index].text = (string)(object)num2 + (object)" / " + str4;
                    if (flag2)
                    {
                        this.Materials_Slot_Quantity_Label[index].color = Color.green;
                    }
                    else
                    {
                        this.Materials_Slot_Quantity_Label[index].color = Color.red;
                        flag1 = false;
                    }
                }
            }
            this.Craft_Button_On.gameObject.SetActive(flag1);
        }
    }

    private bool AreMatsAvailable(CraftData lData)
    {
        for (int index = 0; index < lData.Costs.Count; ++index)
        {
            CraftCost craftCost = lData.Costs[index];
            if (craftCost.ItemType >= 0)
            {
                if ((long)WorldScript.mLocalPlayer.mInventory.GetItemCount(craftCost.ItemType) < (long)craftCost.Amount)
                    return false;
            }
            else if ((long)WorldScript.mLocalPlayer.mInventory.GetCubeTypeCountValue(craftCost.CubeType, craftCost.CubeValue) < (long)craftCost.Amount)
                return false;
        }
        return true;
    }

    public void Start()
    {
        MSAPPanel.instance = this;
        this.Init();
    }

    private void ClearItemInfo()
    {
        this.ItemInfo_Title_Label.text = string.Empty;
        this.ItemInfo_Description_Label.text = string.Empty;
        this.Info_Preview_Icon.spriteName = "empty";
        for (int index = 0; index < 5; ++index)
        {
            this.Materials_Slot_Title_Label[index].text = string.Empty;
            this.Materials_Slot_Quantity_Label[index].text = string.Empty;
            this.Ingredients[index].spriteName = "empty";
        }
        this.Craft_Button_On.gameObject.SetActive(false);
    }

    private void UpdateSearchResults()
    {
        List<CraftData> sourceList1 = CraftData.maCraftData;
        if (this.mnSelectedCategory > 0)
            sourceList1 = this.mAvailableCategories[this.mnSelectedCategory - 1].recipes;
        List<CraftData> sourceList2 = CraftingManager.GetEligibleRecipes(WorldScript.mLocalPlayer, sourceList1, (int)this.mnCurrentTier, this.mUseMode == MSAPPanel.UseMode.SelfCrafting, this.mSelectedModule, this.availableModulesLookup);
        //if (this.mSelectedPort != null)
        //    sourceList2 = this.mSelectedPort.FilterAvailableRecipes(sourceList2);
        this.mbHaveMatsSelected = this.HaveMats_CheckBox.isChecked;
        string str = (this.mSearchString ?? string.Empty).Trim().ToLower();
        bool isChecked = this.HaveMats_CheckBox.isChecked;
        if (isChecked || str.Length > 0)
        {
            List<CraftData> list = new List<CraftData>();
            list.AddRange((IEnumerable<CraftData>)sourceList2);
            sourceList2.Clear();
            using (List<CraftData>.Enumerator enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    CraftData current = enumerator.Current;
                    if ((!isChecked || this.AreMatsAvailable(current)) && (str.Length <= 0 || current.CraftedName.ToLower().Contains(str)))
                        sourceList2.Add(current);
                }
            }
        }
        for (int index = 0; index < 9; ++index)
            this.mTierPresent[index] = false;
        for (int index = 0; index < sourceList2.Count; ++index)
            this.mTierPresent[(int)sourceList2[index].Tier] = true;
        for (int index = 0; index < 9; ++index)
            this.Tier_Buttons[index].cachedGameObject.SetActive(this.mTierPresent[index]);
        this.mSearchResults = sourceList2;
    }

    private MSAPPanel.BlueprintEntry GetBlueprintEntry(int index)
    {
        if (index < this.mCurrentEntries.Count)
            return this.mCurrentEntries[index];
        MSAPPanel.BlueprintEntry blueprintEntry = new MSAPPanel.BlueprintEntry();
        this.mCurrentEntries.Add(blueprintEntry);
        blueprintEntry.Spawn(this, index);
        return blueprintEntry;
    }

    private void DropSurplusEntries(int usedCount)
    {
        for (int index = this.mCurrentEntries.Count - 1; index >= usedCount; --index)
        {
            this.mCurrentEntries[index].Destroy();
            this.mCurrentEntries.RemoveAt(index);
        }
    }

    private void UpdateBlueprintList()
    {
        for (int index = 0; index < this.mSearchResults.Count; ++index)
        {
            CraftData entry = this.mSearchResults[index];
            if (entry != null)
            {
                MSAPPanel.BlueprintEntry blueprintEntry = this.GetBlueprintEntry(index);
                string str = CraftData.GetName(entry);
                if (entry.CraftedAmount > 1)
                    str = str + " x" + entry.CraftedAmount.ToString();
                blueprintEntry.Blueprints_Slot_Title_Label.text = str;
                if (entry.CraftableItemType >= 0)
                    blueprintEntry.Blueprints_Slot_Quantity_Label.text = WorldScript.mLocalPlayer.mInventory.GetItemCount(entry.CraftableItemType).ToString();
                else if ((int)entry.CraftableCubeType != 0)
                    blueprintEntry.Blueprints_Slot_Quantity_Label.text = WorldScript.mLocalPlayer.mInventory.GetCubeTypeCountValue(entry.CraftableCubeType, entry.CraftableCubeValue).ToString();
                else
                    blueprintEntry.Blueprints_Slot_Title_Label.text = "Null Item Entry " + entry.CraftedKey;
                Util.SetNGUIIcon(blueprintEntry.Blueprints_Slot_Icon, CraftData.GetIconName(entry));
                blueprintEntry.Blueprints_Slot_Icon.color = CraftData.GetIconColor(entry);
                blueprintEntry.Blueprints_Slot_Title_Label.color = Color.gray;
                if (index == this.mnBluePrintHighlight)
                    blueprintEntry.Blueprints_Slot_Title_Label.color = Color.white;
            }
        }
        this.DropSurplusEntries(this.mSearchResults.Count);
    }

    private void Update()
    {
        if (this.mbEditingAmountTextBox)
        {
            int result;
            if (int.TryParse(this.Craft_Amount_Input.text, out result))
                this.mnAmount = result;
            this.Loop_On_CheckBox.isChecked = this.mnAmount == 0;
            if (!this.Craft_Amount_Input.selected)
            {
                this.mbEditingAmountTextBox = false;
                this.Craft_Amount_Input.text = this.mnAmount.ToString();
                this.Craft_Amount_Label.text = this.mnAmount.ToString();
            }
        }
        else if (this.Loop_On_CheckBox.isChecked)
        {
            if (this.mnAmount != 0)
            {
                this.mnAmount = 0;
                this.Craft_Amount_Input.text = this.mnAmount.ToString();
                this.Craft_Amount_Label.text = this.mnAmount.ToString();
            }
        }
        else if (this.mnAmount == 0)
        {
            this.mnAmount = 10;
            this.Craft_Amount_Input.text = this.mnAmount.ToString();
            this.Craft_Amount_Label.text = this.mnAmount.ToString();
        }
        if (this.mbEditingSearchTextBox && this.mSearchString != this.Search_Input.text)
        {
            this.mSearchString = this.Search_Input.text;
            this.UpdateSearchResults();
            this.UpdateBlueprintList();
            this.SetBlueprintSlot(-1);
            this.Blueprints_Scroll_View.ResetPosition();
            this.Search_Label.text = this.Search_Input.text;
        }
        if (this.mbHaveMatsSelected != this.HaveMats_CheckBox.isChecked)
        {
            this.UpdateSearchResults();
            this.UpdateBlueprintList();
            this.SetBlueprintSlot(-1);
            this.Blueprints_Scroll_View.ResetPosition();
        }
        if (!this.mbInit)
        {
            if ((UnityEngine.Object)WorldScript.instance.localPlayerInstance == (UnityEngine.Object)null)
                return;
            this.mnBluePrintHighlight = -1;
            this.mnCurrentTier = (ushort)0;
            this.ClearItemInfo();
            this.UpdateSearchResults();
            this.UpdateBlueprintList();
            this.Blueprints_Scroll_View.ResetPosition();
            this.mbInit = true;
        }
        if (this.mSelectedPort == null)
            return;
        //this.UpdateQueuePanel();
    }

    public CraftData GetCurrentBluePrintHighlight()
    {
        if (this.mnBluePrintHighlight < 0)
            Debug.LogError((object)("Can't get blueprint highlight of " + (object)this.mnBluePrintHighlight + "!"));
        if (this.mnBluePrintHighlight >= this.mSearchResults.Count)
            Debug.LogError((object)("Can't get blueprint highlight of " + (object)this.mnBluePrintHighlight + "!"));
        return this.mSearchResults[this.mnBluePrintHighlight];
    }

    public void DoCraft(bool lbSilent)
    {
        if (this.mnBluePrintHighlight >= this.mSearchResults.Count)
        {
            Debug.LogError((object)string.Concat(new object[4]
            {
    (object) "Can't craft item offset ",
    (object) this.mnBluePrintHighlight,
    (object) " because max was only ",
    (object) this.mSearchResults.Count
            }));
            if (lbSilent)
                return;
            AudioHUDManager.instance.HUDFail();
        }
        else
        {
            CraftData craftData = this.mSearchResults[this.mnBluePrintHighlight];
            bool flag = this.AreMatsAvailable(craftData);
            if (CubeHelper.IsDapperDLCBlock(craftData.CraftableCubeType, craftData.CraftableCubeValue))
            {
                DLCOwnership.CheckDLC();
                if (!DLCOwnership.mbHasDapperDLC)
                {
                    SteamFriends.ActivateGameOverlayToWebPage("http://store.steampowered.com/app/400400/");
                    UIManager.instance.SetInfoText("Dapper DLC not present!", 5f, false);
                    if (lbSilent)
                        return;
                    AudioHUDManager.instance.HUDFail();
                    return;
                }
            }
            if (flag)
            {
                if ((int)craftData.CraftableCubeType != 0)
                {
                    if (!WorldScript.mLocalPlayer.mResearch.IsKnown(craftData.CraftableCubeType, craftData.CraftableCubeValue))
                        WorldScript.mLocalPlayer.mResearch.GiveResearch(craftData.CraftableCubeType, craftData.CraftableCubeValue);
                    if (WorldScript.mLocalPlayer.mInventory.CollectValue(craftData.CraftableCubeType, craftData.CraftableCubeValue, craftData.CraftedAmount))
                    {
                        ++PlayerStats.instance.SurvivalItemCrafted;
                        PlayerStats.instance.MarkStatsDirty();
                    }
                    else
                    {
                        UIManager.instance.SetInfoText("Inventory full!", 1.2f, false);
                        if (lbSilent)
                            return;
                        AudioHUDManager.instance.HUDFail();
                        return;
                    }
                }
                else if (craftData.CraftableItemType != -1)
                {
                    ItemBase lItemToAdd1 = ItemManager.SpawnItem(craftData.CraftableItemType);
                    if (lItemToAdd1.mType == ItemType.ItemStack)
                        (lItemToAdd1 as ItemStack).mnAmount = craftData.CraftedAmount;
                    //this.CheckCraftAchievements(craftData.CraftableItemType);
                    if (WorldScript.mLocalPlayer.mInventory.AddItem(lItemToAdd1))
                    {
                        if (lItemToAdd1.mType != ItemType.ItemStack)
                        {
                            for (int index = 1; index < craftData.CraftedAmount; ++index)
                            {
                                ItemBase lItemToAdd2 = ItemManager.SpawnItem(craftData.CraftableItemType);
                                WorldScript.mLocalPlayer.mInventory.AddItem(lItemToAdd2);
                            }
                        }
                        WorldScript.mLocalPlayer.mInventory.VerifySuitUpgrades();
                        ++PlayerStats.instance.SurvivalItemCrafted;
                        PlayerStats.instance.MarkStatsDirty();
                    }
                    else
                    {
                        if (lbSilent)
                            return;
                        AudioHUDManager.instance.HUDFail();
                        return;
                    }
                }
                else
                    Debug.LogError((object)"Unable to craft, item has neither CubeType nor ItemType!");
                if (!lbSilent)
                    Debug.Log((object)("Player has now crafted " + (object)PlayerStats.instance.SurvivalItemCrafted + " items"));
                if (!lbSilent)
                    AudioHUDManager.instance.CraftingVocal();
                if (!lbSilent)
                    AudioHUDManager.instance.HUDClick();
                UIManager.instance.SetInfoText("Crafted a " + CraftData.GetName(craftData), 1.2f, false);
                for (int index = 0; index < craftData.Costs.Count; ++index)
                {
                    CraftCost craftCost = craftData.Costs[index];
                    if (craftCost.ItemType >= 0)
                    {
                        if ((long)WorldScript.mLocalPlayer.mInventory.GetItemCount(craftCost.ItemType) >= (long)craftCost.Amount)
                        {
                            Debug.LogWarning((object)string.Concat(new object[4]
                            {
            (object) "Removing ",
            (object) (int) craftCost.Amount,
            (object) " ",
            (object) ItemManager.GetItemName(craftCost.ItemType)
                            }));
                            WorldScript.mLocalPlayer.mInventory.RemoveItem(craftCost.ItemType, (int)craftCost.Amount);
                            if (WorldScript.meGameMode == eGameMode.eSurvival && SurvivalPlayerScript.meTutorialState == SurvivalPlayerScript.eTutorialState.CraftSomething)
                                SurvivalPlayerScript.TutorialSectionComplete();
                        }
                        else if (WorldScript.meGameMode == eGameMode.eSurvival && SurvivalPlayerScript.meTutorialState == SurvivalPlayerScript.eTutorialState.CraftSomething)
                            SurvivalPlayerScript.TutorialSectionComplete();
                    }
                    else if ((long)WorldScript.mLocalPlayer.mInventory.GetCubeTypeCountValue(craftCost.CubeType, craftCost.CubeValue) >= (long)craftCost.Amount)
                    {
                        WorldScript.mLocalPlayer.mInventory.RemoveValue(craftCost.CubeType, craftCost.CubeValue, (int)craftCost.Amount);
                        if (WorldScript.meGameMode == eGameMode.eSurvival && SurvivalPlayerScript.meTutorialState == SurvivalPlayerScript.eTutorialState.CraftSomething)
                            SurvivalPlayerScript.TutorialSectionComplete();
                    }
                    else if (WorldScript.meGameMode == eGameMode.eSurvival && SurvivalPlayerScript.meTutorialState == SurvivalPlayerScript.eTutorialState.CraftSomething)
                        SurvivalPlayerScript.TutorialSectionComplete();
                }
                this.UpdateSearchResults();
                this.SetBlueprintSlot(this.mnBluePrintHighlight);
            }
            else
            {
                if (!lbSilent)
                    AudioHUDManager.instance.HUDFail();
                UIManager.instance.SetInfoText("Lacking Materials to craft a " + CraftData.GetName(craftData), 1.8f, false);
            }
        }
    }

    //private void CheckCraftAchievements(int lnID)
    //{
    //    if (lnID == 55)
    //        Achievements.instance.UnlockAchievement(Achievements.eAchievements.eStartedT2, false);
    //    if (lnID == 1000)
    //        Achievements.instance.UnlockAchievement(Achievements.eAchievements.eSolarMK2, false);
    //    if (lnID != 1002)
    //        return;
    //    Achievements.instance.UnlockAchievement(Achievements.eAchievements.eHeaterMK2, false);
    //}

    private void OnSubmitAmount(string input)
    {
        this.mbEditingAmountTextBox = false;
        int result = this.mnAmount;
        if (!int.TryParse(input, out result))
        {
            this.Craft_Amount_Input.text = this.mnAmount.ToString();
            this.Craft_Amount_Label.text = this.mnAmount.ToString();
        }
        else
        {
            if (result < 0)
                result = 0;
            this.mnAmount = result;
            this.Craft_Amount_Label.text = this.mnAmount.ToString();
        }
    }

    private void OnSubmitSearch(string input)
    {
    }

    private void OnInputSelect()
    {
        if (this.Craft_Amount_Input.selected)
        {
            UIManager.EditingTextField(this.Craft_Amount_Input);
            this.mbEditingAmountTextBox = true;
        }
        if (!this.Search_Input.selected)
            return;
        UIManager.EditingTextField(this.Search_Input);
        this.mbEditingSearchTextBox = true;
    }

    private void ButtonClicked(string name)
    {
        if (name.Equals("Blueprint_Button_Up", StringComparison.CurrentCultureIgnoreCase) || name.Equals("Blueprint_Button_Down", StringComparison.CurrentCultureIgnoreCase))
            return;
        if (name.Equals("Automate_Button_On", StringComparison.CurrentCultureIgnoreCase))
        {
            if (this.mSelectedPort == null || this.mnBluePrintHighlight < 0)
                return;
            int amount = this.mnAmount;
            CraftData bluePrintHighlight = this.GetCurrentBluePrintHighlight();
            //if (bluePrintHighlight == null || !MSAPPanel.SetRecipe(WorldScript.mLocalPlayer, this.mSelectedPort, bluePrintHighlight.Key, amount))
            //    return;
            AudioHUDManager.instance.HUDIn();
        }
        else if (name.Equals("Queue_Output_Icon", StringComparison.CurrentCultureIgnoreCase))
        {
            //if (this.mSelectedPort == null || this.mSelectedPort.mOutputHopper == null || !MSAPPanel.ExtractAutomatedOutput(WorldScript.mLocalPlayer, this.mSelectedPort))
            //    return;
            AudioHUDManager.instance.OrePickup();
        }
        else if (name.Equals("Queue_ProgressSlot_Icon", StringComparison.CurrentCultureIgnoreCase))
        {
            if (this.mSelectedPort == null)
                return;
            //MSAPPanel.ClearAutomatedRecipe(this.mSelectedPort);
        }
        else if (name.StartsWith("Module"))
            this.SelectModule(int.Parse(name.Substring("Module".Length, name.Length - "Module".Length - "_Off_Button".Length)) - 1);
        else if (name.StartsWith("Craft_Tab"))
            this.SelectCategory(int.Parse(name.Substring("Craft_Tab".Length, name.Length - "Craft_Tab".Length - "_Icon".Length)) - 1);
        else if (name.StartsWith("Tier"))
        {
            this.SetTier(int.Parse(name.Substring("Tier".Length, name.Length - "Tier".Length - "_Button".Length)) - 1);
        }
        else
        {
            if (!name.StartsWith("Blueprint_Slot_Background"))
                return;
            this.SetBlueprintSlot(int.Parse(name.Substring("Blueprint_Slot_Background".Length, name.Length - "Blueprint_Slot_Background".Length)));
        }
    }

    private void ButtonMiddleClicked(string name)
    {
        Debug.Log((object)("Crafting Panel middle clicked: " + name));
        if (!name.StartsWith("Materials_Slot#"))
            return;
        this.JumpToHelp(int.Parse(name.Substring("Materials_Slot#".Length, name.Length - "Materials_Slot#".Length - "_Icon".Length)) - 1);
    }

    private void JumpToHelp(int index)
    {
        Debug.Log((object)("Attempting to open help for material index: " + (object)index));
        if (this.mnBluePrintHighlight == -1)
            return;
        CraftData craftData = this.mSearchResults[this.mnBluePrintHighlight];
        if (craftData == null || craftData.Costs == null || craftData.Costs.Count <= index)
            return;
        CraftCost craftCost = craftData.Costs[index];
        if ((int)craftCost.CubeType != 0 && !WorldScript.mLocalPlayer.mResearch.IsKnown(craftCost.CubeType, craftCost.CubeValue))
            return;
        this.Hide();
        UIManager.instance.mHelpPanel.ShowMaterial(craftCost.Key);
    }

    public void MarkDirty(MSAccessPort port)
    {
        if (this.mSelectedPort != port)
            return;
        MSAPPanel.mbQueueDirty = true;
    }

    public static bool ExtractAutomatedOutput(Player player, ManufacturingPlant plant)
    {
        ItemBase lItemToAdd = plant.mOutputHopper;
        if (lItemToAdd == null)
            return false;
        if (player == WorldScript.mLocalPlayer)
        {
            if (!WorldScript.mLocalPlayer.mInventory.CanFit(lItemToAdd))
                return false;
            WorldScript.mLocalPlayer.mInventory.AddItem(lItemToAdd);
        }
        plant.ClearOutputHopper();
        if (!WorldScript.mbIsServer)
            NetworkManager.instance.SendInterfaceCommand("MSAPPanel", "TakeOutput", (string)null, (ItemBase)null, (SegmentEntity)plant, 0.0f);
        MSAPPanel.mbQueueDirty = true;
        return true;
    }

    public static bool SetRecipe(Player player, ManufacturingPlant plant, string recipeKey, int amount)
    {
        CraftData recipe = CraftData.SearchForCraftData(recipeKey, CraftData.maCraftData);
        plant.SetSelectedRecipe(recipe, amount);
        MSAPPanel.mbQueueDirty = true;
        if (!WorldScript.mbIsServer)
        {
            string payload = amount.ToString() + "x" + recipeKey;
            NetworkManager.instance.SendInterfaceCommand("MSAPPanel", "SetRecipe", payload, (ItemBase)null, (SegmentEntity)plant, 0.0f);
        }
        return true;
    }

    public static void ClearAutomatedRecipe(ManufacturingPlant plant)
    {
        if (plant == null)
            return;
        plant.ClearSelectedRecipe();
        MSAPPanel.mbQueueDirty = true;
        if (WorldScript.mbIsServer)
            return;
        NetworkManager.instance.SendInterfaceCommand("MSAPPanel", "ClearRecipe", (string)null, (ItemBase)null, (SegmentEntity)plant, 0.0f);
    }

//    public static NetworkInterfaceResponse HandleNetworkCommand(Player player, NetworkInterfaceCommand nic)
//    {
//        ManufacturingPlant plant = nic.target as ManufacturingPlant;
//        string key = nic.command;
//        if (key != null)
//        {
//            // ISSUE: reference to a compiler-generated field
//            if (MSAPPanel.\u003C\u003Ef__switch\u0024map8 == null)
//    {
//                // ISSUE: reference to a compiler-generated field
//                MSAPPanel.\u003C\u003Ef__switch\u0024map8 = new Dictionary<string, int>(3)
//    {
//        {
//        "SetRecipe",
//        0
//        },
//        {
//        "TakeOutput",
//        1
//        },
//        {
//        "ClearRecipe",
//        2
//        }
//    };
//            }
//            int num;
//            // ISSUE: reference to a compiler-generated field
//            if (MSAPPanel.\u003C\u003Ef__switch\u0024map8.TryGetValue(key, out num))
//    {
//                switch (num)
//                {
//                    case 0:
//                        int length = nic.payload.IndexOf("x");
//    int amount = int.Parse(nic.payload.Substring(0, length));
//    string recipeKey = nic.payload.Substring(length + 1, nic.payload.Length - length - 1);
//    MSAPPanel.SetRecipe(player, plant, recipeKey, amount);
//                        break;
//                    case 1:
//                        MSAPPanel.ExtractAutomatedOutput(player, plant);
//                        break;
//                    case 2:
//                        MSAPPanel.ClearAutomatedRecipe(plant);
//                        break;
//                }
//            }
//        }
//        return new NetworkInterfaceResponse()
//{
//    entity = (SegmentEntity)plant,
//            inventory = player.mInventory
//        };
//    }

    public enum UseMode
    {
        SelfCrafting,
        Plant,
    }

    //public GameObject GetGameObjectByName(string name)
    //{
    //    return GameObjectUtil.GetObjectFromList(name);
    //}


    //public bool FindByReflection(string fieldname, GameObject obj)
    //{
    //    bool created = false;
    //    if (!created && CraftingPanelLabel.instance != null)
    //    {
    //        FieldInfo field = CraftingPanelLabel.instance.GetType().GetField(fieldname, BindingFlags.NonPublic | BindingFlags.Instance);
    //        GameObject lala = (GameObject)field.GetValue(CraftingPanelLabel.instance);

    //        obj = GameObject.Instantiate(lala);
    //        obj.SetActive(true);
    //        created = true;
    //    }
    //    return created;
    //}


    public class BlueprintEntry
    {
        public UISprite Blueprint_Slot_Background;
        public UISprite Blueprints_Slot_Icon;
        public UILabel Blueprints_Slot_Quantity_Label;
        public UILabel Blueprints_Slot_Title_Label;

        public void Spawn(MSAPPanel script, int index)
        {
            Vector3 localPosition1 = script.Blueprint_Slot_Background.cachedTransform.localPosition;
            float num = localPosition1.y;
            Vector3 localScale1 = script.Blueprint_Slot_Background.cachedTransform.localScale;
            localPosition1.y -= (float)(55 * index);
            localPosition1.y -= num;
            this.Blueprint_Slot_Background = ((GameObject)UnityEngine.Object.Instantiate((UnityEngine.Object)script.Blueprint_Slot_Background.gameObject, localPosition1, Quaternion.identity)).GetComponent<UISprite>();
            this.Blueprint_Slot_Background.cachedGameObject.SetActive(true);
            this.Blueprint_Slot_Background.cachedGameObject.name = "Blueprint_Slot_Background" + index.ToString();
            this.Blueprint_Slot_Background.cachedTransform.parent = script.Blueprints_Scroll_View.transform;
            this.Blueprint_Slot_Background.cachedTransform.localScale = localScale1;
            this.Blueprint_Slot_Background.cachedTransform.localPosition = localPosition1;
            Vector3 localPosition2 = script.Blueprints_Slot_Icon.cachedTransform.localPosition;
            Vector3 localScale2 = script.Blueprints_Slot_Icon.cachedTransform.localScale;
            localPosition2.y -= (float)(55 * index);
            localPosition2.y -= num;
            GameObject go = (GameObject)UnityEngine.Object.Instantiate((UnityEngine.Object)script.Blueprints_Slot_Icon.gameObject, localPosition2, Quaternion.identity);
            this.Blueprints_Slot_Icon = go.GetComponent<UISprite>();
            this.Blueprints_Slot_Icon.cachedGameObject.SetActive(true);
            this.Blueprints_Slot_Icon.cachedTransform.parent = script.Blueprints_Scroll_View.transform;
            this.Blueprints_Slot_Icon.cachedTransform.localScale = localScale2;
            this.Blueprints_Slot_Icon.cachedTransform.localPosition = localPosition2;
            NGUITools.MarkParentAsChanged(go);
            Vector3 localPosition3 = script.Blueprints_Slot_Title_Label.cachedTransform.localPosition;
            Vector3 localScale3 = script.Blueprints_Slot_Title_Label.cachedTransform.localScale;
            localPosition3.y -= (float)(55 * index);
            localPosition3.y -= num;
            this.Blueprints_Slot_Title_Label = ((GameObject)UnityEngine.Object.Instantiate((UnityEngine.Object)script.Blueprints_Slot_Title_Label.gameObject, localPosition3, Quaternion.identity)).GetComponent<UILabel>();
            this.Blueprints_Slot_Title_Label.cachedGameObject.SetActive(true);
            this.Blueprints_Slot_Title_Label.cachedTransform.parent = script.Blueprints_Scroll_View.transform;
            this.Blueprints_Slot_Title_Label.cachedTransform.localScale = localScale3;
            this.Blueprints_Slot_Title_Label.cachedTransform.localPosition = localPosition3;
            Vector3 localPosition4 = script.Blueprints_Slot_Quantity_Label.cachedTransform.localPosition;
            Vector3 localScale4 = script.Blueprints_Slot_Quantity_Label.cachedTransform.localScale;
            localPosition4.y -= (float)(55 * index);
            localPosition4.y -= num;
            this.Blueprints_Slot_Quantity_Label = ((GameObject)UnityEngine.Object.Instantiate((UnityEngine.Object)script.Blueprints_Slot_Quantity_Label.gameObject, localPosition4, Quaternion.identity)).GetComponent<UILabel>();
            this.Blueprints_Slot_Quantity_Label.cachedGameObject.SetActive(true);
            this.Blueprints_Slot_Quantity_Label.cachedTransform.parent = script.Blueprints_Scroll_View.transform;
            this.Blueprints_Slot_Quantity_Label.cachedTransform.localScale = localScale4;
            this.Blueprints_Slot_Quantity_Label.cachedTransform.localPosition = localPosition4;
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy((UnityEngine.Object)this.Blueprint_Slot_Background.cachedGameObject);
            UnityEngine.Object.Destroy((UnityEngine.Object)this.Blueprints_Slot_Title_Label.cachedGameObject);
            UnityEngine.Object.Destroy((UnityEngine.Object)this.Blueprints_Slot_Quantity_Label.cachedGameObject);
            UnityEngine.Object.Destroy((UnityEngine.Object)this.Blueprints_Slot_Icon.cachedGameObject);
        }
    }
}

//public class GetFullObjectList
//{
//    public bool runalready = false;
//    public List<GameObject> AllObjects;

//    public void run()
//    {
//        // diagnostic dumps 8000+ objects to file (slow)
//        List<GameObject> allobjects = (Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]).ToList();
//        this.AllObjects = allobjects;
//        this.runalready = true;
//    }

//    public GameObject GetObjectFromList(string name)
//    {
//        if (!this.runalready)
//            this.run();
//        for (int n = 0; n < this.AllObjects.Count; n++)
//        {
//            if (this.AllObjects[n].name == name)
//            {
//                return this.AllObjects[n];
//            }
//        }
//        return (GameObject)null;
//    }
//}

//public static class GameObjectUtil
//{
//    private static GameObject[] _allObjects;

//    static GameObjectUtil()
//    {
//        _allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
//    }

//    // Originally By binaryalgorithm & steveman0
//    public static GameObject GetObjectFromList(string name)
//    {
//        for (var i = 0; i < _allObjects.Length; i++)
//        {
//            if (_allObjects[i].name == name)
//                return _allObjects[i];
//        }
//        return null;
//    }
//}
    



