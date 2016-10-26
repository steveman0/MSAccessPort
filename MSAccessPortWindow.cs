using UnityEngine;
using FortressCraft.Community.Utilities;

public class MSAccessPortWindow : BaseMachineWindow
{
    public const string InterfaceName = "steveman0.MSAccessPortWindow";
    public const string InterfaceWithdrawItem = "WithdrawItem";
    public const string InterfaceDepositItem = "DepositItem";
    public static bool dirty;
    public static bool networkredraw;
    public bool WithdrawMode = true;
    public int listcount;

    public override void SpawnWindow(SegmentEntity targetEntity)
    {
        MSAccessPort port = targetEntity as MSAccessPort;
        //Catch for when the window is called on an inappropriate machine
        if (port == null)
        {
            //GenericMachinePanelScript.instance.Hide();
            UIManager.RemoveUIRules("Machine");
            return;
        }

        UIUtil.ScaleUIWindow(3f);

        float x = GenericMachinePanelScript.instance.Label_Holder.transform.position.x;
        float y = GenericMachinePanelScript.instance.Label_Holder.transform.position.y;
        GenericMachinePanelScript.instance.Label_Holder.transform.position = new Vector3(x, y, 69.3f);

        if (this.WithdrawMode)
        {
            this.manager.SetTitle("Access Port - Withdraw");
            int globalxoffset = 75;
            //int buttonoffset = 175;
            //int buttonspacing = 175;
            int ItemRowSpacing = 60;
            int ItemColSpacing = 60;
            int yoffset = 80;

            this.manager.AddButton("modetoggle", "Deposit Mode", 100, 0);
            this.manager.AddPowerBar("powerbar", 900, 0);

            if (port.MasterItemList == null)
                return;
            listcount = port.MasterItemList.Count;
            for (int n = 0; n < listcount; n++)
            {
                int row = n / 15;
                int col = n % 15;
                this.manager.AddIcon("itemicon" + n, "empty", Color.white, globalxoffset + col * ItemColSpacing + 35, row * ItemRowSpacing + yoffset);
                this.manager.AddLabel(GenericMachineManager.LabelType.OneLineHalfWidth, "StackSize" + n, "", Color.white, false, globalxoffset + col * ItemColSpacing + 27 + 25, row * ItemRowSpacing + 22 + yoffset);
            }
        }
        else
        {
            this.manager.SetTitle("Access Port - Deposit");
            int globalxoffset = 235;
            //int buttonoffset = 175;
            //int buttonspacing = 175;
            int ItemRowSpacing = 60;
            int ItemColSpacing = 60;

            this.manager.AddButton("modetoggle", "Withdraw Mode", 100, 0);
            this.manager.AddPowerBar("powerbar", 900, 0);

            for (int n = 0; n < 80; n++)
            {
                int row = n / 10;
                int col = n % 10;
                this.manager.AddIcon("itemicon" + n, "empty", Color.white, globalxoffset + col * ItemColSpacing + 35, row * ItemRowSpacing);
                this.manager.AddLabel(GenericMachineManager.LabelType.OneLineHalfWidth, "StackSize" + n, "", Color.white, false, globalxoffset + col * ItemColSpacing + 27 + 25, row * ItemRowSpacing + 22);
            }
        }

        dirty = true;
    }

    public override void UpdateMachine(SegmentEntity targetEntity)
    {
        MSAccessPort port = targetEntity as MSAccessPort;

        if (port == null)
        {
            GenericMachinePanelScript.instance.Hide();
            UIManager.RemoveUIRules("Machine");
            return;
        }

        if (networkredraw)
        {
            networkredraw = false;
            this.manager.RedrawWindow();
        }
        this.manager.UpdatePowerBar("powerbar", port.mrCurrentPower, port.mrMaxPower);

        if (!dirty)
            return;

        if (WithdrawMode)
        {
            if (listcount != port.MasterItemList.Count)
                this.manager.RedrawWindow();
            if (port.AccessState != MSAccessPort.eState.Withdraw)
                port.AccessState = MSAccessPort.eState.Withdraw;

            for (int n = 0; n < listcount; n++)
            {
                int row = n / 15;
                int col = n % 15;
                ItemBase item = port.MasterItemList[n].itembase;
                if (item != null)
                {
                    string itemicon = ItemManager.GetItemIcon(item);
                    int stacksize = port.MasterItemList[n].Quantity;

                    this.manager.UpdateIcon("itemicon" + n, itemicon, Color.white);
                    this.manager.UpdateLabel("StackSize" + n, this.FormatStackText(stacksize), Color.white);
                }
            }
        }
        else
        {
            for (int n = 0; n < 80; n++)
            {
                int row = n / 10;
                int col = n % 10;
                ItemBase item = WorldScript.mLocalPlayer.mInventory.maItemInventory[col, row];
                if (item != null)
                {
                    string itemicon = ItemManager.GetItemIcon(item);
                    int stacksize = item.GetAmount();

                    this.manager.UpdateIcon("itemicon" + n, itemicon, Color.white);
                    this.manager.UpdateLabel("StackSize" + n, this.FormatStackText(stacksize), Color.white);
                }
            }
        }
        dirty = false;
    }

    private string FormatStackText(int count)
    {
        string label = "";
        if (count < 10)
            label = "     " + count.ToString();
        else if (count < 100)
            label = "   " + count.ToString();
        else if (count < 1000)
            label = "  " + count.ToString();
        else if (count < 10000)
            label = " " + ((double)count / 1000f).ToString("N1") + "k";
        else if (count < 100000)
            label = ((double)count / 1000f).ToString("N1") + "k";
        else if (count < 1000000)
            label = " " + (count / 1000).ToString() + "k";
        return label;
    }

    public override bool ButtonClicked(string name, SegmentEntity targetEntity)
    {
        MSAccessPort port = targetEntity as MSAccessPort;

        if (name.Contains("itemicon"))
        {
            int slotNum = -1;
            int.TryParse(name.Replace("itemicon", ""), out slotNum); //Get slot name as number
            if (slotNum > -1)
            {
                //Enable Shift click and control click to only take 10/1 item at a time, default to whole stack
                int amount = 100;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    amount = 10;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    amount = 1;

                // Set stack size for the withdraw and handle window refresh appropriately (restoring the empty slot as needed by redrawing)
                int available;
                ItemBase item = this.GetItemForSlot(name, out available, port);
                if (item != null)
                    item = item.NewInstance();
                else
                {
                    Debug.LogWarning("MSAccessPort Attempted to withdraw null item?");
                    return true;
                }
                if (item == null)
                {
                    Debug.LogWarning("accessport item wasn't null but new instance is!?");
                }
                if (amount < available && item.IsStack())
                {
                    ItemManager.SetItemCount(item, amount);
                    dirty = true;
                }
                else
                    networkredraw = true;

                if ((float)amount * port.mrTransportCost > port.mrCurrentPower)
                {
                    FloatingCombatTextManager.instance.QueueText(port.mnX, port.mnY + 1L, port.mnZ, 1f, "Low Power!", Color.red, 1.5f);
                    port.ErrorText = "---INSUFFICIENT POWER---\n";
                    port.ErrorTime = 1.5f;
                    port.ErrorResult = true;
                    return true;
                }

                if (WithdrawMode)
                {
                    MSAccessPortWindow.WithdrawItem(WorldScript.mLocalPlayer, port, item, amount);
                    port.BuildInventoryList();
                }
                else
                    MSAccessPortWindow.DepositItem(WorldScript.mLocalPlayer, port, item, amount);

                return true;
            }
        }
        else if (name == "modetoggle")
        {
            WithdrawMode = !WithdrawMode;
            this.manager.RedrawWindow();
        }

        return false;
    }


    public override void ButtonEnter(string name, SegmentEntity targetEntity)
    {
        MSAccessPort port = targetEntity as MSAccessPort;

        if (!name.Contains("itemicon"))
            return;
        string str = string.Empty;
        int count;
        ItemBase itemForSlot = this.GetItemForSlot(name, out count, port);
        if (itemForSlot == null)
            return;
        if (HotBarManager.mbInited)
        {
            HotBarManager.SetCurrentBlockLabel(ItemManager.GetItemName(itemForSlot));
        }
        else
        {
            if (!SurvivalHotBarManager.mbInited)
                return;
            string name1 = !WorldScript.mLocalPlayer.mResearch.IsKnown(itemForSlot) ? "Unknown Material" : ItemManager.GetItemName(itemForSlot);
            if (count > 1)
                SurvivalHotBarManager.SetCurrentBlockLabel(string.Format("{0} {1}", count, name1));
            else
                SurvivalHotBarManager.SetCurrentBlockLabel(name1);
        }
    }

    private ItemBase GetItemForSlot(string name, out int count, MSAccessPort port)
    {
        ItemBase itemForSlot = null;
        count = 0;
        int slotNum = -1;
        int.TryParse(name.Replace("itemicon", ""), out slotNum); //Get slot name as number
        if (slotNum > -1)
        {
            if (WithdrawMode)
            {
                itemForSlot = port.MasterItemList[slotNum].itembase;
                count = port.MasterItemList[slotNum].Quantity;
            }
            else
            {
                int row = slotNum / 10;
                int col = slotNum % 10;
                itemForSlot = WorldScript.mLocalPlayer.mInventory.maItemInventory[col, row];
                if (itemForSlot != null)
                    count = itemForSlot.GetAmount();
            }
        }
        return itemForSlot;
    }

    public override void OnClose(SegmentEntity targetEntity)
    {
        UIUtil.RestoreUIScale();
        base.OnClose(targetEntity);
    }

    public static int WithdrawItem(Player player, MSAccessPort port, ItemBase item, int amount)
    {
        if (item == null)
        {
            Debug.LogWarning("MSAccessPort is trying to withdraw null item!");
            return amount;
        }
        if (!WorldScript.mbIsServer)
            NetworkManager.instance.SendInterfaceCommand(InterfaceName, InterfaceWithdrawItem, amount.ToString(), item, port, 0.0f);
        ItemBase itemout;
        //Handle special items by copying the full data on retrieval and inserting into the player inventory
        if (item.mType != ItemType.ItemStack && item.mType != ItemType.ItemCubeStack)
        {
            for (int index = 0; index < amount; index++)
            {
                if (!player.mInventory.CanFit(item))
                {
                    if (player.mbIsLocalPlayer)
                        FloatingCombatTextManager.instance.QueueText(port.mnX, port.mnY + 1L, port.mnZ, 1f, "Inventory full!", Color.red, 1.5f);
                    if (index != 0)
                    {
                        player.mInventory.MarkEverythingDirty();
                        port.MarkDirtyDelayed();
                        port.mbJustTransmitted = true;
                    }
                    return amount - index;
                }
                if (port.WithdrawItem(item, out itemout) == 0)
                {
                    if (itemout == null)
                    {
                        Debug.LogWarning("MSAccessPort attempted to withdraw item: " + item.ToString() + " but found null on withdraw");
                        return amount - index;
                    }
                    port.mrCurrentPower -= port.mrTransportCost;
                    if (itemout != null && !player.mInventory.AddItem(itemout))
                        ItemManager.instance.DropItem(itemout, player.mnWorldX, player.mnWorldY, player.mnWorldZ, Vector3.zero);
                    else if (itemout == null)
                        Debug.LogWarning("MSAccessPort tried to withdraw single item but returned null itemout?");
                }
                else
                {
                    if (index != 0)
                    {
                        player.mInventory.MarkEverythingDirty();
                        port.MarkDirtyDelayed();
                        port.mbJustTransmitted = true;
                    }
                    return amount - index;
                }
            }
        }
        else
        {
            //Stacked items have all necessary info stored in the passed itembase and don't need the original from mass storage
            if (!player.mInventory.CanFit(item))
            {
                if (player.mbIsLocalPlayer)
                    FloatingCombatTextManager.instance.QueueText(port.mnX, port.mnY + 1L, port.mnZ, 1f, "Inventory full!", Color.red, 1.5f);
                return item.GetAmount();
            }
            int remainder = port.WithdrawItem(item, out itemout);
            if (remainder != 0)
            {
                port.mrCurrentPower -= port.mrTransportCost * (item.GetAmount() - remainder);
                if (itemout != null && !player.mInventory.AddItem(itemout))
                    ItemManager.instance.DropItem(item, player.mnWorldX, player.mnWorldY, player.mnWorldZ, Vector3.zero);
                else if (itemout == null)
                    Debug.LogWarning("MSAccessPort tried to withdraw item stack but returned null itemout?");
                player.mInventory.MarkEverythingDirty();
                port.MarkDirtyDelayed();
                port.mbJustTransmitted = true;
            }
            else
            {
                if (itemout != null && !player.mInventory.AddItem(itemout))
                    ItemManager.instance.DropItem(item, player.mnWorldX, player.mnWorldY, player.mnWorldZ, Vector3.zero);
                else if (itemout == null)
                    Debug.LogWarning("MSAccessPort tried to withdraw item stack but returned null itemout?");
                port.mrCurrentPower -= port.mrTransportCost * item.GetAmount();
            }
        }
        player.mInventory.MarkEverythingDirty();
        port.MarkDirtyDelayed();
        port.mbJustTransmitted = true;
        return 0;
    }

    public static int DepositItem(Player player, MSAccessPort port, ItemBase item, int amount)
    {
        if (!WorldScript.mbIsServer)
            NetworkManager.instance.SendInterfaceCommand(InterfaceName, InterfaceDepositItem, amount.ToString(), item, port, 0.0f);
        //Handle special case items by getting the exact item from inventory based on the example ID
        if (item.mType != ItemType.ItemStack && item.mType != ItemType.ItemCubeStack)
        {
            for (int index = 0; index < amount; index++)
            {
                if (item != null)
                {
                    // This is required due to the dubious item type associated with MasterList
                    item = player.mInventory.TryAndGetItem(item.mnItemID);
                    if (!player.mInventory.RemoveSpecificItem(item))
                    {
                        if (player.mbIsLocalPlayer)
                            FloatingCombatTextManager.instance.QueueText(port.mnX, port.mnY + 1L, port.mnZ, 1f, "Item not found!", Color.red, 1.5f);
                        if (index != 0)
                        {
                            player.mInventory.MarkEverythingDirty();
                            port.MarkDirtyDelayed();
                            port.mbJustTransmitted = true;
                        }
                        return amount-index;
                    }
                    if (port.DepositItem(item) != 0)
                    {
                        if (player.mbIsLocalPlayer)
                            FloatingCombatTextManager.instance.QueueText(port.mnX, port.mnY + 1L, port.mnZ, 1f, "Mass Storage full!", Color.red, 1.5f);
                        //Return item to player if it failed to deposit
                        player.mInventory.AddItem(item);
                        if (index != 0)
                        {
                            player.mInventory.MarkEverythingDirty();
                            port.MarkDirtyDelayed();
                            port.mbJustTransmitted = true;
                        }
                        return amount-index;
                    }
                    else
                        port.mrCurrentPower -= port.mrTransportCost;
                }
            }
        }
        else
        {
            if (!player.mInventory.RemoveItemByExample(item))
            {
                if (player.mbIsLocalPlayer)
                    FloatingCombatTextManager.instance.QueueText(port.mnX, port.mnY + 1L, port.mnZ, 1f, "Item not found!", Color.red, 1.5f);
                return item.GetAmount();
            }
            int remainder = port.DepositItem(item);
            if (remainder != 0)
            {
                if (player.mbIsLocalPlayer)
                    FloatingCombatTextManager.instance.QueueText(port.mnX, port.mnY + 1L, port.mnZ, 1f, "Mass Storage full!", Color.red, 1.5f);
                //Return item to player if it failed to deposit
                player.mInventory.AddItem(ItemManager.CloneItem(item).SetAmount(remainder));
                port.mrCurrentPower -= port.mrTransportCost * (item.GetAmount() - remainder);
                player.mInventory.MarkEverythingDirty();
                port.MarkDirtyDelayed();
                port.mbJustTransmitted = true;
                return remainder;
            }
            else
                port.mrCurrentPower -= port.mrTransportCost * item.GetAmount();
        }

        player.mInventory.MarkEverythingDirty();
        port.MarkDirtyDelayed();
        port.mbJustTransmitted = true;
        return 0;
    }

    public static NetworkInterfaceResponse HandleNetworkCommand(Player player, NetworkInterfaceCommand nic)
    {
        MSAccessPort port = nic.target as MSAccessPort;
        string key = nic.command;
        if (key != null)
        {
            if (key == InterfaceWithdrawItem)
            {
                int result;
                if (int.TryParse(nic.payload ?? "0", out result))
                    MSAccessPortWindow.WithdrawItem(player, port, nic.itemContext, result);
            }
            else if (key == InterfaceDepositItem)
            {
                int result;
                if (int.TryParse(nic.payload ?? "0", out result))
                    MSAccessPortWindow.DepositItem(player, port, nic.itemContext, result);
            }
        }
        return new NetworkInterfaceResponse()
        {
            entity = port,
            inventory = player.mInventory
        };
    }
}