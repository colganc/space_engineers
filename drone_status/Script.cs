public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateSource) {
    Echo("Build-12");

    if (Me.CustomData == null || Me.CustomData.Trim() == "") {
        Echo("No custom data value, stopping");
        return;
    }

    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);
 
    List<IMyTextPanel> displays = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);
 
    List<IMyTerminalBlock> cargoContainers = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers);

    List<IMyTerminalBlock> remoteControls = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(remoteControls);

    string displayText = "";

    string droneName = Me.CustomData;
    bool foundBatteries = false;
    bool foundCargoContainers = false;

    Echo(droneName);
    displayText = droneName;

    bool inventoryEmpty = true;
    for (int c = 0; c < cargoContainers.Count; c++) {
        if (cargoContainers[c].CustomData != droneName) {
            continue;
        }

        foundCargoContainers = true;

        Echo(cargoContainers[c].CustomName);
        IMyInventory cargoContainerInventory = cargoContainers[c].GetInventory();
        if (cargoContainerInventory.ItemCount > 0) {
            inventoryEmpty = false;
            Echo("Not empty");
        }
        else {
            Echo("Empty");
        }
    }

    if (cargoContainers.Count > 0 && foundCargoContainers) {
        if (inventoryEmpty) {
            displayText += "\nInv N";
        }
        else {
            displayText += "\nInv Y";
        }
    }
    else {
        displayText += "\nInv ?";
    }

        float currentStoredPower = 0;
        float maxStoredPower = 0;

        for (int c = 0; c < batteries.Count; c++) {
            if (batteries[c].CustomData != droneName) {
                continue;
            }

            foundBatteries = true;

            currentStoredPower += batteries[c].CurrentStoredPower;
            maxStoredPower += batteries[c].MaxStoredPower;
        }

        int storedPowerPercent = (int)Math.Round((currentStoredPower / maxStoredPower) * 100, 0, MidpointRounding.AwayFromZero);

        displayText += "\nE ";
        if (batteries.Count > 0 && foundBatteries) {
            displayText += storedPowerPercent.ToString();
        }
        else {
            displayText += "?";
        }

        for (int c = 0; c < displays.Count; c++) {
            if (displays[c].CustomData != droneName) {
                continue;
            }
            Echo(displays[c].CustomName);

            displays[c].WriteText(displayText);
        }
}
