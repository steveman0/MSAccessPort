using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using FortressCraft.Community.Utilities;

public class MSAccessPort : MachineEntity, PowerConsumerInterface
{
    public float mrTransportCost = 50f;
    public float mrMaxPower = 5000.4f;
    public float mrCurrentPower;
    public float mrRemainingCapacity;
    public float mrSparePowerCapacity;
    private bool mbLinkedToGO;
    //private TextMesh DebugReadout;
    public bool mbJustTransmitted;
    public MassStorageCrate massStorageCrate;
    public float mrScanDelay = 0.0f;
    //public MSAPPanel panel;
    public List<MasterList> MasterItemList;
    public List<MasterList> MasterPlayerItemList;
    public MSAccessPort.eState AccessState;
    public int ListIndex = 0;
    public string ErrorText = "";
    public float ErrorTime = 0.0f;
    public bool ErrorResult = false;
    public float FocusTime = 0.0f;
    public bool TextEntryMode = false;
    public string EntryString = "";
    public float CursorTimer = 0.0f;
    public Vector3 MyPosition;
    private GameObject HoloPreview;
    private GameObject HoloCubePreview;
    private bool mbHoloPreviewDirty;
    public ItemBase Exemplar;
    public MassStorageController Controller;
    public string LastSearch;
    public int LastListCount;
    public MSAccessPortWindow MachineWindow = new MSAccessPortWindow();

    public MSAccessPort(ModCreateSegmentEntityParameters parameters)
    : base(parameters)
    {
        this.mbNeedsLowFrequencyUpdate = true;
        this.mbNeedsUnityUpdate = true;
        this.AccessState = MSAccessPort.eState.Withdraw;
    }

    public override string GetPopupText()
    {
        //UIUtil.HandleThisMachineWindow(this, this.MachineWindow);

        //Non-UI Version
        //string lstr1 = "1\n2\n3\n4\n5\n6\n7\n8";



        string lstr3 = "";
        string lstr4 = "";
        string lstr7 = "";

        this.FocusTime = 1.0f;
        bool chatwindowclose = false;

        string lstr1 = "Mass Storage Access Port\n";
        string lstr2 = "Power: " + this.mrCurrentPower.ToString("F0") + "/" + this.mrMaxPower.ToString("F0") + "\n";

        if (this.AccessState == MSAccessPort.eState.Withdraw)
            lstr3 = "Withdraw Mode - Press (T) to enter Deposit Mode\n";
        else
            lstr3 = "Deposit Mode - Press (Q) to enter Withdraw Mode\n";

        // Check if the list has changed and lock back onto the search result
        if (this.LastListCount != 0 && this.LastListCount != this.MasterItemList.Count)
            this.GetSearchResult(this.LastSearch);

        int amount = 100;
        if (!this.TextEntryMode)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                amount = 10;
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                amount = 1;
            //else if (Input.GetKey(KeyCode.LeftAlt))
            //    amount = 1000;
        }
        if (this.AccessState == MSAccessPort.eState.Withdraw && this.GetListCount() != 0)
        {
            lstr4 = "Press (Q) to Withdraw " + (amount < this.MasterItemList[this.ListIndex].Quantity ? amount : this.MasterItemList[this.ListIndex].Quantity) + " " + this.MasterItemList[this.ListIndex].Name + "\n";
        }
        else if (this.AccessState == MSAccessPort.eState.Deposit && this.GetListCount() != 0)
        {
            lstr4 = "Press (T) to Deposit " + (amount < this.MasterPlayerItemList[this.ListIndex].Quantity ? amount : this.MasterPlayerItemList[this.ListIndex].Quantity) + " " + this.MasterPlayerItemList[this.ListIndex].Name + "\n";
        }

        string lstr5 = "Scroll item list with (shift +) </>\n";
        //string lstr6 = "Press (E) to search inventory by item name\n";
        string lstr6 = "Press (E) to access Inventory Display\n";
       
        if (Input.GetKeyDown(KeyCode.Period) && !this.TextEntryMode)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                this.ListIndex += 10;
            else
                this.ListIndex++;
            if (this.ListIndex > this.GetListCount() - 1)
                this.ListIndex = 0;
            this.UpdateExemplar();
            this.LastListCount = 0;
        }

        if (Input.GetKeyDown(KeyCode.Comma) && !this.TextEntryMode)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                this.ListIndex -= 10;
            else
                this.ListIndex--;
            if (this.ListIndex < 0)
                this.ListIndex = this.GetListCount() - 1;
            this.UpdateExemplar();
            this.LastListCount = 0;
        }
        if (Input.GetButtonDown("Extract") && this.AccessState == MSAccessPort.eState.Withdraw && this.MasterItemList[this.ListIndex] != null && !this.TextEntryMode)
        {
            MasterList masterlist = MasterItemList[this.ListIndex];
            ItemBase itembase = ItemManager.CloneItem(masterlist.itembase);
            if (masterlist.Quantity < amount)
                amount = masterlist.Quantity;
            itembase.SetAmount(amount);
            float powercost = amount * this.mrTransportCost;

            //Check for sufficient power and flag accordingly
            if (this.mrCurrentPower >= powercost)
            {
                int remainder = MSAccessPortWindow.WithdrawItem(WorldScript.mLocalPlayer, this, itembase, amount);
                if (remainder == 0)
                {
                    FloatingCombatTextManager.instance.QueueText(this.mnX, this.mnY + 1L, this.mnZ, 1f, itembase.ToString(), ItemColor(itembase), 1.5f);
                    this.ErrorText = amount + " " + ItemManager.GetItemName(itembase) + " withdrawn!";
                    this.ErrorTime = 1.5f;
                }
                else
                {
                    FloatingCombatTextManager.instance.QueueText(this.mnX, this.mnY + 1L, this.mnZ, 1f, itembase.SetAmount(amount - remainder).ToString(), ItemColor(itembase), 1.5f);
                    this.ErrorText = "Unable to complete transaction! " + remainder.ToString("N0") + " " + ItemManager.GetItemName(itembase) + " were not withdrawn!";
                    this.ErrorTime = 1.5f;
                    this.ErrorResult = true;
                }
            }
            else
            {
                FloatingCombatTextManager.instance.QueueText(this.mnX, this.mnY + 1L, this.mnZ, 1f, "Low Power!", Color.red, 1.5f);
                this.ErrorText = "---INSUFFICIENT POWER---\n";
                this.ErrorTime = 1.5f;
                this.ErrorResult = true;
            }
        }

        if (Input.GetButtonDown("Store") && this.AccessState == MSAccessPort.eState.Deposit && this.MasterPlayerItemList[this.ListIndex] != null && !this.TextEntryMode)
        {
            MasterList masterlist = MasterPlayerItemList[this.ListIndex];
            ItemBase itembase = ItemManager.CloneItem(masterlist.itembase);
            if (masterlist.Quantity < amount)
                amount = masterlist.Quantity;
            itembase.SetAmount(amount);
            float powercost = amount * this.mrTransportCost;
            
            //Check for sufficient power and flag accordingly
            if (this.mrCurrentPower >= powercost)
            {
                int remainder = MSAccessPortWindow.DepositItem(WorldScript.mLocalPlayer, this, itembase, amount);
                if (remainder == 0)
                {
                    FloatingCombatTextManager.instance.QueueText(this.mnX, this.mnY + 1L, this.mnZ, 1f, itembase.ToString(), ItemColor(itembase), 1.5f);
                    this.ErrorText = amount + " " + ItemManager.GetItemName(itembase) + " deposited!";
                    this.ErrorTime = 1.5f;
                }
                else
                {
                    FloatingCombatTextManager.instance.QueueText(this.mnX, this.mnY + 1L, this.mnZ, 1f, itembase.SetAmount(amount - remainder).ToString(), ItemColor(itembase), 1.5f);
                    this.ErrorText = "Unable to complete transaction! " + remainder.ToString("N0") + " " + ItemManager.GetItemName(itembase) + " were not deposited!";
                    this.ErrorTime = 1.5f;
                    this.ErrorResult = true;
                }
            }
            else
            {
                FloatingCombatTextManager.instance.QueueText(this.mnX, this.mnY + 1L, this.mnZ, 1f, "Low Power!", Color.red, 1.5f);
                this.ErrorText = "---INSUFFICIENT POWER---\n";
                this.ErrorTime = 1.5f;
                this.ErrorResult = true;
            }
        }



        //Must happen after other input lest weird item exchanges occur without intent
        if (Input.GetButtonDown("Extract") && this.AccessState == MSAccessPort.eState.Deposit && !this.TextEntryMode)
        {
            this.AccessState = MSAccessPort.eState.Withdraw;
            if (this.MasterItemList == null)
                this.BuildInventoryList();
            if (this.MasterPlayerItemList != null)
            {
                if (!this.GetSearchResult(this.MasterPlayerItemList[this.ListIndex].Name))
                {
                    this.ListIndex = 0;
                    this.UpdateExemplar();
                }
            }
        }
        if (Input.GetButtonDown("Store") && this.AccessState == MSAccessPort.eState.Withdraw && !this.TextEntryMode)
        {
            this.AccessState = MSAccessPort.eState.Deposit;
            if (this.MasterPlayerItemList == null)
                this.BuildPlayerInventoryList();
            if (this.MasterItemList != null)
            {
                if (!this.GetSearchResult(this.MasterItemList[this.ListIndex].Name))
                {
                    this.ListIndex = 0;
                    this.UpdateExemplar();
                }
            }
        }

        //Handle text entry for item searching
        if (this.TextEntryMode)
        {
            this.CursorTimer += Time.deltaTime;
            foreach (char c in Input.inputString)
            {
                if (c == "\b"[0])
                {
                    if (this.EntryString.Length != 0)
                        this.EntryString = this.EntryString.Substring(0, this.EntryString.Length - 1);
                }
                else if (c == "\n"[0] || c == "\r"[0])
                {
                    this.GetSearchResult(this.EntryString);
                    this.TextEntryMode = false;
                    this.LastSearch = this.EntryString;
                    this.LastListCount = this.MasterItemList.Count;
                    this.EntryString = "";
                    UIManager.RemoveUIRules("TextEntry");
                    chatwindowclose = true;
                }
                else
                    this.EntryString += c;
            }
            lstr3 = "Item search mode - enter desired item name.\n";
            lstr4 = "Press Enter to search.\n";
            lstr5 = "Press ESC to cancel.\n";
            lstr6 = "Search for: " + this.EntryString;

            if ((int)this.CursorTimer % 2 == 1)
                lstr6 += "_";

            lstr6 += "\n";

            //Hide unwanted UI when typing
            if (Input.GetKeyDown(KeyCode.P))
                UIManager.instance.UnpauseGame();
            if (Input.GetKeyDown(KeyCode.H))
                UIManager.instance.mHelpPanel.Hide();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.TextEntryMode = false;
                this.EntryString = "";
                UIManager.RemoveUIRules("TextEntry");
                UIManager.instance.UnpauseGame();
            }

        }
        if (chatwindowclose)
            UIManager.instance.mChatPanel.StopEdit();

        //DISABLED FOR UI
        //Activate text entry mode
        //if (Input.GetButtonDown("Interact") && !this.TextEntryMode)
        //{
        //    this.TextEntryMode = true;
        //    this.EntryString = "";
        //    this.CursorTimer = 0.0f;
        //    UIManager.AddUIRules("TextEntry", UIRules.RestrictMovement | UIRules.RestrictLooking | UIRules.RestrictBuilding | UIRules.RestrictInteracting | UIRules.SetUIUpdateRate);
        //}

        if (this.ErrorTime > 0)
        {
            this.ErrorTime -= Time.deltaTime;
            lstr7 = this.ErrorText;
        }

            string lstr9 = string.Concat(new object[7]
            {
                lstr1,
                lstr2,
                lstr3,
                lstr4,
                lstr5,
                lstr6,
                lstr7
            });

        return lstr9;
    }

    private int GetListCount()
    {
        if (this.AccessState == MSAccessPort.eState.Deposit && this.MasterPlayerItemList != null)
            return this.MasterPlayerItemList.Count;
        if (this.AccessState == MSAccessPort.eState.Withdraw && this.MasterItemList != null)
            return this.MasterItemList.Count;
        return 0;
    }

    private Color ItemColor(ItemBase itemBase)
    {
        Color lCol = Color.green;
        if (itemBase.mType == ItemType.ItemCubeStack)
        {
            ItemCubeStack itemCubeStack = itemBase as ItemCubeStack;
            if (CubeHelper.IsGarbage(itemCubeStack.mCubeType))
                lCol = Color.red;
            if (CubeHelper.IsSmeltableOre(itemCubeStack.mCubeType))
                lCol = Color.green;
        }
        if (itemBase.mType == ItemType.ItemStack)
            lCol = Color.cyan;
        if (itemBase.mType == ItemType.ItemSingle)
            lCol = Color.white;
        if (itemBase.mType == ItemType.ItemCharge)
            lCol = Color.magenta;
        if (itemBase.mType == ItemType.ItemDurability)
            lCol = Color.yellow;
        if (itemBase.mType == ItemType.ItemLocation)
            lCol = Color.gray;

        return lCol;
    }

    private void UpdateExemplar()
    {
        if (this.AccessState == MSAccessPort.eState.Deposit && this.MasterPlayerItemList != null)
            this.SetExemplar(this.MasterPlayerItemList[this.ListIndex].itembase);
        if (this.AccessState == MSAccessPort.eState.Withdraw && this.MasterItemList != null)
            this.SetExemplar(this.MasterItemList[this.ListIndex].itembase);
    }

    private bool GetSearchResult(string search)
    {
        if (this.AccessState == MSAccessPort.eState.Deposit && this.MasterPlayerItemList != null)
        {
            int testsearch = this.MasterPlayerItemList.FindIndex(x => x.Name.ToLower().StartsWith(search.ToLower()));
            if (testsearch == -1)
            {
                testsearch = this.MasterPlayerItemList.FindIndex(x => x.Name.ToLower().Contains(search.ToLower()));
            }
            if (testsearch == -1)
                return false;
            this.ListIndex = testsearch;
            this.UpdateExemplar();
            return true;
        }
        if (this.AccessState == MSAccessPort.eState.Withdraw && this.MasterItemList != null)
        {
            int testsearch = this.MasterItemList.FindIndex(x => x.Name.ToLower().StartsWith(search.ToLower()));
            if (testsearch == -1)
            {
                testsearch = this.MasterItemList.FindIndex(x => x.Name.ToLower().Contains(search.ToLower()));
            }
            if (testsearch == -1)
                return false;
            this.ListIndex = testsearch;
            this.UpdateExemplar();
            return true;
        }
        return false;
    }

    public override void LowFrequencyUpdate()
    {
        if (this.massStorageCrate == null)
            this.SearchForCrateNeighbours(this.mnX, this.mnY, this.mnZ);
        if (this.massStorageCrate != null && this.massStorageCrate.mbDelete)
        {
            this.massStorageCrate = null;
            return;
        }
        if (this.Controller != null && this.Controller.mbDelete)
            this.Controller = null;
        if (this.Controller == null && this.massStorageCrate != null)
            this.Controller = this.massStorageCrate.GetController();
        if (this.Controller != null && this.mrCurrentPower < this.mrMaxPower)
        {
            float power = this.Controller.mrCurrentPower;
            if (power > this.GetRemainingPowerCapacity())
            {
                this.Controller.RemovePower(this.GetRemainingPowerCapacity());
                this.mrCurrentPower = this.mrMaxPower;
            }
            else
            {
                if (this.DeliverPower(this.Controller.mrCurrentPower))
                    this.Controller.mrCurrentPower = 0;
            }
        }

        if ((double)this.mrScanDelay > 0.0)
        {
            this.mrScanDelay -= LowFrequencyThread.mrPreviousUpdateTimeStep;
        }
        else if (this.FocusTime > 0)
        {
            if (this.AccessState == MSAccessPort.eState.Withdraw)
            {
                this.BuildInventoryList();
                MSAccessPortWindow.dirty = true;
            }
            else if (this.AccessState == MSAccessPort.eState.Deposit)
                this.BuildPlayerInventoryList();
            int newlimit = GetListCount() - 1;
            if (this.ListIndex > newlimit)
            {
                this.ListIndex = 0;
                this.UpdateExemplar();
            }
        }
        this.FocusTime -= LowFrequencyThread.mrPreviousUpdateTimeStep;
    }

    public void BuildInventoryList()
    {
        //ScanDelay sets the refresh interval
        this.mrScanDelay = 1f;
        //Clear arrays from last use so we don't have phantom items
        ItemBase pickeditem = null;
        Dictionary<ulong, int> items = new Dictionary<ulong, int>();
        List<MasterList> itemlist = new List<MasterList>();
        ulong mergeditem = 0;
        MassStorageCrate crate;

        //Loop over all crates and collect all items into a list
        if (this.massStorageCrate != null)
        {
            for (int index = 0; index < this.massStorageCrate.mConnectedCrates.Count + 1; ++index)
            {
                for (int index2 = 0; index2 < massStorageCrate.STORAGE_CRATE_SIZE; ++index2)
                {
                    if (index == this.massStorageCrate.mConnectedCrates.Count) //Center crate!
                        crate = this.massStorageCrate;
                    else
                        crate = this.massStorageCrate.mConnectedCrates[index];
                    if (crate.mMode == MassStorageCrate.CrateMode.Items)
                        pickeditem = crate.mItems[index2];
                    else
                        pickeditem = crate.mItem;

                    if (pickeditem != null)
                    {
                        if (pickeditem.mType == ItemType.ItemCubeStack)
                            mergeditem = (ulong)pickeditem.mnItemID << 32 | (ulong)(pickeditem as ItemCubeStack).mCubeType << 16 | (ulong)(pickeditem as ItemCubeStack).mCubeValue;
                        else if (pickeditem.mType == ItemType.ItemStack)
                            mergeditem = (ulong)pickeditem.mnItemID << 32 | (ulong)0 << 16 | (ulong)0;
                        else
                            mergeditem = (ulong)pickeditem.mnItemID << 32 | (ulong)0 << 16 | (ulong)ushort.MaxValue;
                        if (!items.ContainsKey(mergeditem))
                        {
                            items.Add(mergeditem, pickeditem.GetAmount());
                        }
                        else
                        {
                            items[mergeditem] = (int)items[mergeditem] + pickeditem.GetAmount();
                        }
                        pickeditem = null;
                    }
                    if (crate.mMode == MassStorageCrate.CrateMode.SingleStack)
                        break;
                }
            }
        }
        //No mass storage crate associated... find one!
        else
        {
            SearchForCrateNeighbours(this.mnX, this.mnY, this.mnZ);
        }
        //Group like items in the list and output the quantities to arrays
        if (items == null || items.Count == 0)
            return;
        else
        {
            int itemid;
            ushort cubeid;
            ushort cubevalue;

            var keyslist = items.Keys.ToList();
            for (int index = 0; index < items.Count; index++)
            {
                itemid = (int)(keyslist[index] >> 32);
                cubeid = (ushort)(keyslist[index] >> 16 & ushort.MaxValue);
                cubevalue = (ushort)(keyslist[index] & ushort.MaxValue);
                MasterList masterlist = null;

                //Not an Item
                if (itemid < 0)
                {
                    masterlist = new MasterList(TerrainData.GetNameForValue(cubeid, cubevalue), cubeid, cubevalue, items[keyslist[index]], MasterList.eType.Cube);
                }
                else if (cubevalue != ushort.MaxValue)
                {
                    masterlist = new MasterList(ItemManager.GetItemName(itemid), itemid, -1, items[keyslist[index]], MasterList.eType.Item);
                }
                else
                {
                    masterlist = new MasterList(ItemManager.GetItemName(itemid), itemid, -1, items[keyslist[index]], MasterList.eType.Other);
                }
                itemlist.Add(masterlist);
            }
            this.MasterItemList = itemlist.OrderBy(item => item.Name).ToList();
            //this.MasterItemList.Sort((s1, s2) => s1.Name.CompareTo(s2.Name));
        }
    }

    public void BuildPlayerInventoryList()
    {
        //ScanDelay sets the refresh interval
        this.mrScanDelay = 1f;
        //Clear arrays from last use so we don't have phantom items
        ItemBase pickeditem = null;
        Dictionary<ulong, int> playeritems = new Dictionary<ulong, int>();
        List<MasterList> playeritemlist = new List<MasterList>();
        ulong mergeditem = 0;

        //Loop over all crates and collect all items into a list
        for (int index = 0; index < 10; ++index)
        {
            for (int index2 = 0; index2 < 8; ++index2)
            {
                if (WorldScript.mLocalPlayer.mInventory.maItemInventory[index, index2] != null)
                {
                    pickeditem = WorldScript.mLocalPlayer.mInventory.maItemInventory[index, index2];
                    if (pickeditem.mType == ItemType.ItemCubeStack)
                        mergeditem = (ulong)pickeditem.mnItemID << 32 | (ulong)(pickeditem as ItemCubeStack).mCubeType << 16 | (ulong)(pickeditem as ItemCubeStack).mCubeValue;
                    else if (pickeditem.mType == ItemType.ItemStack)
                        mergeditem = (ulong)pickeditem.mnItemID << 32 | (ulong)0 << 16 | (ulong)0;
                    else
                        mergeditem = (ulong)pickeditem.mnItemID << 32 | (ulong)0 << 16 | (ulong)ushort.MaxValue;

                    if (!playeritems.ContainsKey(mergeditem))
                    {
                        playeritems.Add(mergeditem, ItemManager.GetCurrentStackSize(pickeditem));
                    }
                    else
                    {
                        playeritems[mergeditem] = (int)playeritems[mergeditem] + ItemManager.GetCurrentStackSize(pickeditem);
                    }
                    pickeditem = null;
                }
            }
        }
        //Group like items in the list and output the quantities to arrays
        if (playeritems.Count == 0)
            return;
        else
        {
            int itemid;
            ushort cubeid;
            ushort cubevalue;

            var keyslist = playeritems.Keys.ToList();
            for (int index = 0; index < playeritems.Count; index++)
            {
                itemid = (int)(keyslist[index] >> 32);
                cubeid = (ushort)(keyslist[index] >> 16 & ushort.MaxValue);
                cubevalue = (ushort)(keyslist[index] & ushort.MaxValue);
                MasterList masterlist;
                //Not an Item
                if (itemid < 0)
                {
                    masterlist = new MasterList(TerrainData.GetNameForValue(cubeid, cubevalue), cubeid, cubevalue, playeritems[keyslist[index]], MasterList.eType.Cube);
                }
                else if (cubevalue != ushort.MaxValue)
                {
                    masterlist = new MasterList(ItemManager.GetItemName(itemid), itemid, -1, playeritems[keyslist[index]], MasterList.eType.Item);
                }
                else
                {
                    masterlist = new MasterList(ItemManager.GetItemName(itemid), itemid, -1, playeritems[keyslist[index]], MasterList.eType.Other);
                }
                playeritemlist.Add(masterlist);
            }
            this.MasterPlayerItemList = playeritemlist.OrderBy(item => item.Name).ToList();
            //this.MasterPlayerItemList.Sort((s1, s2) => s1.Name.CompareTo(s2.Name));
        }
    }

    public int DepositItem(ItemBase item)
    {
        int remainder = item.GetAmount();
        if (this.massStorageCrate == null || this.massStorageCrate.mbDelete)
            return remainder;

        if (item.IsStack())
        {
            //Try to find a matching crate before switching a crate
            for (int index = 0; index <= this.massStorageCrate.mConnectedCrates.Count; index++)
            {
                MassStorageCrate crate;
                if (index == this.massStorageCrate.mConnectedCrates.Count)
                    crate = this.massStorageCrate;
                else
                    crate = this.massStorageCrate.mConnectedCrates[index];

                if (crate.mrInputLockTimer <= 0 && crate.mMode == MassStorageCrate.CrateMode.SingleStack)
                {
                    int freespace = crate.mnLocalFreeStorage;
                    bool match = false;
                    if (crate.mItem != null)
                        match = crate.mItem.Compare(item);
                    if (freespace > 0 && (crate.mnLocalUsedStorage == 0 || match))
                    {
                        if (remainder > freespace)
                        {
                            if (crate.AddItem(ItemManager.CloneItem(item).SetAmount(freespace)))
                                remainder -= freespace;
                            else
                                Debug.LogWarning("MS Access Port tried to deposit partial stack to crate but failed!");
                        }
                        else
                        {
                            if (crate.AddItem(ItemManager.CloneItem(item).SetAmount(remainder)))
                                return 0;
                            else
                                Debug.LogWarning("MS Access Port tried to deposit remaining stack to crate but failed!");
                        }
                    }
                }
            }
            //Repeat trial with crate switching allowed
            for (int index = 0; index <= this.massStorageCrate.mConnectedCrates.Count; index++)
            {
                MassStorageCrate crate;
                if (index == this.massStorageCrate.mConnectedCrates.Count)
                    crate = this.massStorageCrate;
                else
                    crate = this.massStorageCrate.mConnectedCrates[index];

                if (crate.mrInputLockTimer <= 0 && crate.mMode != MassStorageCrate.CrateMode.SingleStack && crate.SwitchMode(MassStorageCrate.CrateMode.SingleStack))
                {
                    int freespace = crate.mnLocalFreeStorage;
                    bool match = false;
                    if (crate.mItem != null)
                        match = crate.mItem.Compare(item);
                    if (freespace > 0 && (crate.mnLocalUsedStorage == 0 || match))
                    {
                        if (remainder > freespace)
                        {
                            if (crate.AddItem(ItemManager.CloneItem(item).SetAmount(freespace)))
                                remainder -= freespace;
                            else
                                Debug.LogWarning("MS Access Port tried to deposit partial stack to crate but failed!");
                        }
                        else
                        {
                            if (crate.AddItem(ItemManager.CloneItem(item).SetAmount(remainder)))
                                return 0;
                            else
                                Debug.LogWarning("MS Access Port tried to deposit remaining stack to crate but failed!");
                        }
                    }
                }
            }
        }
        else
        {
            //Try to find a matching crate before switching a crate
            for (int index = 0; index <= this.massStorageCrate.mConnectedCrates.Count; index++)
            {
                MassStorageCrate crate;
                if (index == this.massStorageCrate.mConnectedCrates.Count)
                    crate = this.massStorageCrate;
                else
                    crate = this.massStorageCrate.mConnectedCrates[index];

                if (crate.mrInputLockTimer <= 0 && crate.mMode == MassStorageCrate.CrateMode.Items)
                {
                    if (crate.mnLocalFreeStorage > 0)
                    {
                        if (crate.AddItem(ItemManager.CloneItem(item)))
                            return 0;
                        else
                            Debug.LogWarning("MS Access Port tried to deposit single item to crate but failed!");
                    }
                }
            }
            //Repeat trial with crate switching allowed
            for (int index = 0; index <= this.massStorageCrate.mConnectedCrates.Count; index++)
            {
                MassStorageCrate crate;
                if (index == this.massStorageCrate.mConnectedCrates.Count)
                    crate = this.massStorageCrate;
                else
                    crate = this.massStorageCrate.mConnectedCrates[index];

                if (crate.mrInputLockTimer <= 0 && crate.mMode != MassStorageCrate.CrateMode.Items && crate.SwitchMode(MassStorageCrate.CrateMode.Items))
                {
                    if (crate.mnLocalFreeStorage > 0)
                    {
                        if (crate.AddItem(ItemManager.CloneItem(item)))
                            return 0;
                        else
                            Debug.LogWarning("MS Access Port tried to deposit single item to crate but failed!");
                    }
                }
            }
        }
        return remainder;
    }

    //public bool DepositItem(ItemBase item)
    //{
    //    int itemcount = item.GetAmount();
    //    bool stacktype = item.IsStack();
    //    ItemBase pickeditem = null;
    //    bool match = false;
    //    List<KeyValuePair<int, int>> indexlist = new List<KeyValuePair<int, int>>();

    //    if (this.massStorageCrate != null)
    //    {
    //        for (int index = 0; index < this.massStorageCrate.mConnectedCrates.Count + 1; ++index)
    //        {
    //            if (itemcount == 0)
    //                break;
    //            for (int index2 = 0; index2 < massStorageCrate.STORAGE_CRATE_SIZE; ++index2)
    //            {
    //                if (index == this.massStorageCrate.mConnectedCrates.Count) //Center crate!
    //                {
    //                    if (massStorageCrate.mMode == MassStorageCrate.CrateMode.Items && !stacktype)
    //                    {
    //                        if (massStorageCrate.mrInputLockTimer > 0 && index2 == 0)
    //                            break;
    //                        if (massStorageCrate.mItems[index2] != null)
    //                            pickeditem = (massStorageCrate.mItems[index2]);
    //                    }
    //                    else
    //                    {
    //                        if (massStorageCrate.mrInputLockTimer > 0)
    //                            break;
    //                        if (massStorageCrate.mItem.Compare(item) && massStorageCrate.mnLocalFreeStorage > 0)
    //                            match = true;
    //                    }
    //                }
    //                else
    //                {
    //                    if (massStorageCrate.mMode == MassStorageCrate.CrateMode.Items && !stacktype)
    //                    {
    //                        if (massStorageCrate.mConnectedCrates[index].mrInputLockTimer > 0 && index2 == 0)
    //                            continue;
    //                        if (massStorageCrate.mConnectedCrates[index].mItems[index2] != null)
    //                        {
    //                            pickeditem = (massStorageCrate.mConnectedCrates[index].mItems[index2]);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (massStorageCrate.mConnectedCrates[index].mrInputLockTimer > 0)
    //                            continue;
    //                        if (massStorageCrate.mConnectedCrates[index].mItem.Compare(item) && massStorageCrate.mConnectedCrates[index].mnLocalFreeStorage > 0)
    //                            match = true;
    //                    }
    //                }
    //                if (pickeditem == null)
    //                {
    //                    indexlist.Add(new KeyValuePair<int, int>(index, index2));
    //                    itemcount--;
    //                    if (index == massStorageCrate.mConnectedCrates.Count)
    //                        massStorageCrate.mrInputLockTimer = 0.5f;
    //                    else
    //                        massStorageCrate.mConnectedCrates[index].mrInputLockTimer = 0.5f;
    //                    if (itemcount == 0)
    //                    {
    //                        break;
    //                    }
    //                }
    //                else
    //                    pickeditem = null;
    //                if (match)
    //                {

    //                }
    //            }
    //        }
    //    }
    //    else
    //        return false;
    //    if (itemcount > 0)
    //        return false;
    //    //Otherwise found enough slots for the items - add the items and return true
    //    for (int index = 0; index < indexlist.Count; ++index)
    //    {
    //        ItemBase itembase = ItemManager.CloneItem(item);
    //        itembase.SetAmount(1);
    //        if (indexlist[index].Key == massStorageCrate.mConnectedCrates.Count)
    //        {
    //            massStorageCrate.mItems[indexlist[index].Value] = itembase;
    //        }
    //        else
    //        {
    //            massStorageCrate.mConnectedCrates[indexlist[index].Key].mItems[indexlist[index].Value] = itembase;
    //        }
    //    }
    //    //Code to only count storage on crates that changed for efficiency
    //    var distinctkeys = indexlist.GroupBy(x => x.Key).Select(g => g.First()).ToList();

    //    for (int index = 0; index < distinctkeys.Count(); index++)
    //    {
    //        if (distinctkeys[index].Key == massStorageCrate.mConnectedCrates.Count)
    //            massStorageCrate.CountUpFreeStorage(false);
    //        else
    //            massStorageCrate.mConnectedCrates[distinctkeys[index].Key].CountUpFreeStorage(false);
    //    }
    //    return true;
    //}

    public int WithdrawItem(ItemBase item, out ItemBase itemout)
    {
        int remainder = item.GetAmount();
        itemout = null;
        if (this.massStorageCrate == null || this.massStorageCrate.mbDelete)
            return remainder;

        if (item.IsStack())
        {
            for (int index = 0; index <= this.massStorageCrate.mConnectedCrates.Count; index++)
            {
                MassStorageCrate crate;
                if (index == this.massStorageCrate.mConnectedCrates.Count)
                    crate = this.massStorageCrate;
                else
                    crate = this.massStorageCrate.mConnectedCrates[index];

                if (crate.mrOutputLockTimer <= 0 && crate.mMode == MassStorageCrate.CrateMode.SingleStack)
                {
                    int available = crate.mnLocalUsedStorage;
                    bool match = false;
                    if (crate.mItem != null)
                        match = crate.mItem.Compare(item);
                    if (available > 0 && match)
                    {
                        if (remainder >= available)
                        {
                            remainder -= available;
                            crate.mItem = null;
                            itemout = ItemManager.CloneItem(item).SetAmount(item.GetAmount() - remainder);
                            crate.CountUpFreeStorage(false);
                            crate.MarkDirtyDelayed();
                        }
                        else
                        {
                            crate.mItem.DecrementStack(remainder);
                            itemout = ItemManager.CloneItem(item);
                            crate.CountUpFreeStorage(false);
                            crate.MarkDirtyDelayed();
                            return 0;
                        }
                    }
                }
            }
        }
        else
        {
            if (item.mnItemID == -1)
            {
                Debug.LogWarning("MSAccessPort tried to withdraw non-stack but found invalid item ID?");
                return remainder;
            }
            for (int index = 0; index <= this.massStorageCrate.mConnectedCrates.Count; index++)
            {
                MassStorageCrate crate;
                if (index == this.massStorageCrate.mConnectedCrates.Count)
                    crate = this.massStorageCrate;
                else
                    crate = this.massStorageCrate.mConnectedCrates[index];

                if (crate.mrOutputLockTimer <= 0 && crate.mMode == MassStorageCrate.CrateMode.Items && crate.mnLocalUsedStorage > 0)
                {
                    for (int index2 = 0; index2 < crate.STORAGE_CRATE_SIZE; index2++)
                    {
                        bool match = false;
                        // Compare only item ID because type is dubious due to MasterList construction
                        if (crate.mItems[index2] != null)
                            match = crate.mItems[index2].mnItemID == item.mnItemID;
                        if (match)
                        {
                            itemout = ItemManager.CloneItem(crate.mItems[index2]);
                            crate.mItems[index2] = null;
                            crate.CountUpFreeStorage(false);
                            crate.MarkDirtyDelayed();
                            return 0;
                        }
                    }
                }
            }
        }
        return remainder;
    }

    //public bool WithdrawItem(ItemBase item, out ItemBase itemout)
    //{
    //    int itemcount = item.GetAmount();
    //    ItemBase pickeditem = null;
    //    itemout = null;
    //    List<KeyValuePair<int, int>> indexlist = new List<KeyValuePair<int, int>>();

    //    if (this.massStorageCrate != null)
    //    {
    //        for (int index = 0; index < this.massStorageCrate.mConnectedCrates.Count + 1; ++index)
    //        {
    //            if (itemcount == 0)
    //                break;
    //            for (int index2 = 0; index2 < massStorageCrate.STORAGE_CRATE_SIZE; ++index2)
    //            {
    //                if (index == this.massStorageCrate.mConnectedCrates.Count) //Center crate!
    //                {
    //                    if (massStorageCrate.mrOutputLockTimer > 0 && index2 == 0)
    //                        break;
    //                    if (massStorageCrate.mItems[index2] != null)
    //                        pickeditem = (massStorageCrate.mItems[index2]);
    //                }
    //                else
    //                {
    //                    if (massStorageCrate.mConnectedCrates[index].mrOutputLockTimer > 0 && index2 == 0)
    //                        continue;
    //                    if (massStorageCrate.mConnectedCrates[index].mItems[index2] != null)
    //                    {
    //                        pickeditem = (massStorageCrate.mConnectedCrates[index].mItems[index2]);
    //                    }
    //                }
    //                if (pickeditem != null)
    //                {
    //                    if (pickeditem.Compare(item))
    //                    {
    //                        if (pickeditem.mType != ItemType.ItemStack || pickeditem.mType != ItemType.ItemCubeStack)
    //                            itemout = ItemManager.CloneItem(pickeditem);
    //                        indexlist.Add(new KeyValuePair<int, int>(index, index2));
    //                        itemcount--;
    //                        if (index == massStorageCrate.mConnectedCrates.Count)
    //                            massStorageCrate.mrOutputLockTimer = 0.5f;
    //                        else
    //                            massStorageCrate.mConnectedCrates[index].mrOutputLockTimer = 0.5f;
    //                        if (itemcount == 0)
    //                        {
    //                            break;
    //                        }
    //                    }
    //                    pickeditem = null;
    //                }
    //            }
    //        }
    //    }
    //    else
    //        return false;
    //    if (itemcount > 0)
    //        return false;
    //    //Otherwise found all the items - set null and return true
    //    for (int index = 0; index < indexlist.Count; ++index)
    //    {
    //        if (indexlist[index].Key == massStorageCrate.mConnectedCrates.Count)
    //        {
    //            massStorageCrate.mItems[indexlist[index].Value] = null;
    //        }
    //        else
    //        {
    //            massStorageCrate.mConnectedCrates[indexlist[index].Key].mItems[indexlist[index].Value] = null;
    //        }
    //    }
    //    //Code to only count storage on crates that changed for efficiency
    //    var distinctkeys = indexlist.GroupBy(x => x.Key).Select(g => g.First()).ToList();

    //    for (int index = 0; index < distinctkeys.Count(); index++)
    //    {
    //        if (distinctkeys[index].Key == massStorageCrate.mConnectedCrates.Count)
    //            massStorageCrate.CountUpFreeStorage(false);
    //        else
    //            massStorageCrate.mConnectedCrates[distinctkeys[index].Key].CountUpFreeStorage(false);
    //    }
    //    return true;
    //}

    public void SetExemplar(ItemBase lItem)
    {
        if (this.Exemplar == null || lItem == null)
        {
            this.mbHoloPreviewDirty = true;
            //if (lItem == null)
            //Debug.Log((object)"MSOP Cleared Exemplar");
        }
        else if (lItem.mType == ItemType.ItemCubeStack)
        {
            if (this.Exemplar.mType == ItemType.ItemCubeStack)
            {
                if ((int)(lItem as ItemCubeStack).mCubeType == (int)(this.Exemplar as ItemCubeStack).mCubeType)
                    return;
                this.mbHoloPreviewDirty = true;
                //Debug.Log((object)"MSOP Set Exemplar to different CubeStack type");
            }
            else
            {
                this.mbHoloPreviewDirty = true;
                //Debug.Log((object)"MSOP Set Exemplar to CubeStack type");
            }
        }
        else
        {
            if (lItem.mnItemID == this.Exemplar.mnItemID)
                return;
            this.mbHoloPreviewDirty = true;
            //Debug.Log((object)("MSOP Set Exemplar to " + ItemManager.GetItemName(lItem.mnItemID)));
        }
        this.Exemplar = lItem;
        this.MarkDirtyDelayed();
    }

    public override void DropGameObject()
    {
        base.DropGameObject();
        this.mbLinkedToGO = false;
    }

    public override void UnitySuspended()
    {
        if ((Object)this.HoloPreview != (Object)null)
            Object.Destroy((Object)this.HoloPreview);
        this.HoloPreview = (GameObject)null;
    }

    public override void UnityUpdate()
    {
        //UIUtil.DisconnectUI(this);
        if (!this.mbLinkedToGO)
        {
            if (this.mWrapper == null || !this.mWrapper.mbHasGameObject || this.mWrapper.mGameObjectList == null)
                return;
            if (this.mWrapper.mGameObjectList == null)
                Debug.LogError((object)"MF missing game object #0?");
            if ((Object)this.mWrapper.mGameObjectList[0].gameObject == (Object)null)
                Debug.LogError((object)"MF missing game object #0 (GO)?");

            //Debug.Log("Before holocube");
            GameObject lObj = SpawnableObjectManagerScript.instance.maSpawnableObjects[(int)SpawnableObjectEnum.MassStorageOutputPort].transform.Search("HoloCube").gameObject;
            GameObject HoloCubePreview = (GameObject)GameObject.Instantiate(lObj, this.mWrapper.mGameObjectList[0].gameObject.transform.position + new Vector3(0.0f, 0.75f, 0.0f), Quaternion.identity);
            this.HoloCubePreview = HoloCubePreview;
            //Debug.Log("After holo cube");
            this.HoloCubePreview.SetActive(false);
            this.mbLinkedToGO = true;
        }
        else
        {
            if (this.mbHoloPreviewDirty)
            {
                if ((Object)this.HoloPreview != (Object)null)
                    Object.Destroy((Object)this.HoloPreview);
                if (this.Exemplar != null)
                {
                    if (this.Exemplar.mType == ItemType.ItemCubeStack)
                    {
                        this.HoloCubePreview.SetActive(true);
                    }
                    else
                    {
                        int index = (int)ItemEntry.mEntries[this.Exemplar.mnItemID].Object;
                        this.HoloPreview = (GameObject)Object.Instantiate((Object)SpawnableObjectManagerScript.instance.maSpawnableObjects[index], this.mWrapper.mGameObjectList[0].gameObject.transform.position + new Vector3(0.0f, 0.75f, 0.0f), Quaternion.identity);
                        this.HoloPreview.transform.parent = this.mWrapper.mGameObjectList[0].gameObject.transform;
                        if ((Object)this.HoloPreview.GetComponent<Renderer>() != (Object)null)
                        {
                            this.HoloPreview.GetComponent<Renderer>().material = PrefabHolder.instance.HoloPreviewMaterial;
                            this.HoloPreview.GetComponent<Renderer>().castShadows = false;
                            this.HoloPreview.GetComponent<Renderer>().receiveShadows = false;
                        }
                        this.HoloPreview.gameObject.AddComponent<RotateConstantlyScript>();
                        this.HoloPreview.gameObject.GetComponent<RotateConstantlyScript>().YRot = 1f;
                        this.HoloPreview.gameObject.GetComponent<RotateConstantlyScript>().XRot = 0.35f;
                        this.HoloPreview.SetActive(true);
                        this.HoloCubePreview.SetActive(false);
                    }
                }
                else
                    this.HoloCubePreview.SetActive(false);
                this.mbHoloPreviewDirty = false;
            }

            if (this.mbJustTransmitted)
            {
                Vector3 pos = new Vector3(WorldScript.instance.localPlayerInstance.mPosition.x, WorldScript.instance.localPlayerInstance.mPosition.y, WorldScript.instance.localPlayerInstance.mPosition.z);
                //AudioSoundEffectManager.instance.Play(AudioSoundEffectManager.instance.T1MatterMoverTransmit, 0.65f, 0.5f, pos);
                AudioSoundEffectManager.instance.PlayPositionEffect(AudioSoundEffectManager.instance.T1MatterMoverTransmit, pos, 1.0f, 8f);
                //AudioSoundEffectManager.instance.PlayPositionEffect(AudioSoundEffectManager.instance.T1MatterMoverTransmit, this.MyPosition, 10.0f, 8f);
                this.mbJustTransmitted = false;
            }
            if (this.ErrorResult)
            {
                //Vector3 pos = new Vector3(WorldScript.instance.localPlayerInstance.mPosition.x, WorldScript.instance.localPlayerInstance.mPosition.y, WorldScript.instance.localPlayerInstance.mPosition.z);
                //AudioHUDManager.instance.Play(AudioHUDManager.instance., 1.0f);
                this.ErrorResult = false;
                AudioHUDManager.instance.HUDFailAlt();
            }
        }
    }

    private void SearchForCrateNeighbours(long x, long y, long z)
    {
        for (int index = 0; index < 6; ++index)
        {
            //Debug.Log("Searching for crate");
            long x1 = x;
            long y1 = y;
            long z1 = z;
            if (index == 0)
                --x1;
            if (index == 1)
                ++x1;
            if (index == 2)
                --y1;
            if (index == 3)
                ++y1;
            if (index == 4)
                --z1;
            if (index == 5)
                ++z1;
            Segment segment = this.AttemptGetSegment(x1, y1, z1);
            if (segment == null)
            {
                segment = WorldScript.instance.GetSegment(x1, y1, z1);
                if (segment == null)
                {
                    Debug.Log((object)"SearchForCrateNeighbours did not find segment");
                    continue;
                }
            }
            if ((int)segment.GetCube(x1, y1, z1) == 527)
            {
                MassStorageCrate massStorageCrate = segment.FetchEntity(eSegmentEntity.MassStorageCrate, x1, y1, z1) as MassStorageCrate;
                if (massStorageCrate == null)
                    return;
                this.massStorageCrate = massStorageCrate.GetCenter();
            }
        }
    }

    public float GetRemainingPowerCapacity()
    {
        return this.mrMaxPower - this.mrCurrentPower;
    }

    public float GetMaximumDeliveryRate()
    {
        return float.MaxValue;
    }

    public float GetMaxPower()
    {
        return this.mrMaxPower;
    }

    public bool DeliverPower(float amount)
    {
        if ((double)amount > (double)this.GetRemainingPowerCapacity())
            return false;
        this.mrCurrentPower += amount;
        this.MarkDirtyDelayed();
        return true;
    }

    public bool WantsPowerFromEntity(SegmentEntity entity)
    {
        return true;
    }

    public override bool ShouldSave()
    {
        return true;
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(this.mrCurrentPower);
    }

    public override void Read(BinaryReader reader, int entityVersion)
    {
        this.mrCurrentPower = reader.ReadSingle();
        if (this.mrCurrentPower > this.mrMaxPower)
            this.mrCurrentPower = this.mrMaxPower;
    }

    public enum eState
    {
        Withdraw,
        Deposit,
    }
}

public class MasterList
{
    public string Name;
    public int Id;
    public int Value;
    public int Quantity;
    public MasterList.eType Type;
    public ItemBase itembase;

    public MasterList(string name, int id, int value, int quantity, eType type)
    {
        this.Name = name;
        this.Id = id;
        this.Value = value;
        this.Quantity = quantity;
        this.Type = type;


        if (this.Type == MasterList.eType.Cube)
        {
            this.itembase = new ItemCubeStack((ushort)id, (ushort)value, quantity);
        }
        else if (this.Type == MasterList.eType.Item)
        {
            this.itembase = new ItemStack(id, quantity);
        }
        else
        {
            this.itembase = new ItemSingle(id);
        }
    }

    public enum eType
    {
        Cube,
        Item,
        Other,
    }
}