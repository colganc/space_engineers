bool requestedStatePressurized = true;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    if (Me.CustomData == null || Me.CustomData.Trim() == "") {
        Echo("No custom data value, stopping");
        return;
    }
}

public void Main(string argument, UpdateType updateSource)
{
    if (Me.CustomData == null || Me.CustomData.Trim() == "") {
        Echo("No custom data value, stopping");
        return;
    }

    if (argument == "change_state") {
        requestedStatePressurized = !requestedStatePressurized;
    }

    string airlockName = Me.CustomData;
    float oxygenLevel = 0;
    float exteriorDoorsOpenRatio = 0;
    float interiorDoorsOpenRatio = 0;
    int oxygenTankCount = 0;
    double fillRatio = 0;

    List<IMyGasTank> tanks = new List<IMyGasTank>();
    GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks);
    foreach (IMyGasTank tank in tanks) {
        if (tank.CustomData != "MobileController1") {
            continue;
        }
        oxygenTankCount++;
        fillRatio += tank.FilledRatio;
    }
    double overallTankFillRatio = fillRatio / oxygenTankCount;

    List<IMyAirVent> vents = new List<IMyAirVent>();
    GridTerminalSystem.GetBlocksOfType<IMyAirVent>(vents);
    foreach (IMyAirVent vent in vents) {
        if (vent.CustomData != airlockName) {
            continue;
        }
        oxygenLevel = vent.GetOxygenLevel(); 
    }

    List<IMyAirtightSlideDoor> doors = new List<IMyAirtightSlideDoor>();
    GridTerminalSystem.GetBlocksOfType<IMyAirtightSlideDoor>(doors);
    foreach (IMyAirtightSlideDoor door in doors) {
        if (door.CustomData != airlockName + "_exterior" && door.CustomData != airlockName + "_interior") {
            continue;
        }

        if (door.CustomData == airlockName + "_exterior") {
            exteriorDoorsOpenRatio =door.OpenRatio; 
        }

        if (door.CustomData == airlockName + "_interior") {
            interiorDoorsOpenRatio = door.OpenRatio; 
        }
    }

    foreach (IMyAirVent vent in vents) {
        if (vent.CustomData != airlockName) {
            continue;
        }

        if (requestedStatePressurized && exteriorDoorsOpenRatio == 0) {
            vent.Depressurize = false;
        }

        if (!requestedStatePressurized && interiorDoorsOpenRatio == 0) {
            vent.Depressurize = true;
        }
    }

    foreach (IMyAirtightSlideDoor door in doors) {
        if (door.CustomData != airlockName + "_exterior" && door.CustomData != airlockName + "_interior") {
            continue;
        }

        if (door.CustomData == airlockName + "_exterior") {
            if (requestedStatePressurized) {
                door.CloseDoor();
                if (exteriorDoorsOpenRatio == 0) {
                    door.Enabled = false;
                }
                else {
                    door.Enabled = true;
                }
            }

            if (!requestedStatePressurized && exteriorDoorsOpenRatio == 0 && interiorDoorsOpenRatio == 0 && oxygenLevel > 0 && overallTankFillRatio > .9) {
                door.OpenDoor();
                door.Enabled = true;
            }

            if (!requestedStatePressurized && oxygenLevel == 0) { // && overallTankFillRatio <= .9) {
                door.OpenDoor();
                if (exteriorDoorsOpenRatio == 1) {
                    door.Enabled = false;
                }
                else {
                    door.Enabled = true;
                }
            }
        }

        if (door.CustomData == airlockName + "_interior") {
            if (requestedStatePressurized && oxygenLevel == 1) {
                door.OpenDoor();
                if (interiorDoorsOpenRatio == 1) {
                    door.Enabled = false;
                }
                else {
                    door.Enabled = true;
                }
            }

            if (!requestedStatePressurized) {
                door.CloseDoor();
                if (interiorDoorsOpenRatio == 0) {
                    door.Enabled = false;
                }
                else {
                    door.Enabled = true;
                }
            }
        }
    }

    List<IMyTextPanel> displays = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);
    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != airlockName + "_interior" && display.CustomData != airlockName + "_exterior") {
            continue;
        }

        display.ContentType = ContentType.TEXT_AND_IMAGE;
        display.Alignment = TextAlignment.CENTER;
        display.FontSize = 4;

        string message = "";

        if (display.CustomData == airlockName + "_interior") {
            message = "Interior";
        }

        if (display.CustomData == airlockName + "_exterior") {
            message = "Exterior";
        }

        double roundedOxygenLevel = Math.Round(Convert.ToDouble(oxygenLevel * 100), 0, MidpointRounding.AwayFromZero);
        message += "\nAirlock Pressurization " + roundedOxygenLevel + "%";
        display.WriteText(message);
    }
}
