bool requestedStateHangarClosed = false;
bool requestedStateArmorClosed = false;
int ticksBeforeDoorsClose = 60 * 3;
float hangarDoorSpeed = 3;
float armorSpeed = 3;
Dictionary<string, int> inventoryRequests = new Dictionary<string, int>();
Dictionary<string, string> componentToBlueprint = new Dictionary<string, string>();

public struct doorStatus {
    public string name { get; set; }
    public int ticksOpen { get; set; }
}
List<doorStatus> doorStatuses = new List<doorStatus>();


public Program() {
    componentToBlueprint.Add("Girder", "GirderComponent");
    componentToBlueprint.Add("Computer", "ComputerComponent");
    componentToBlueprint.Add("Motor", "MotorComponent");
    componentToBlueprint.Add("Construction", "ConstructionComponent");
    componentToBlueprint.Add("RadioCommunication", "RadioCommunicationComponent");
    componentToBlueprint.Add("Detector", "DetectorComponent");
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Save()
{
    // Called when the program needs to save its state. Use
    // this method to save your state to the Storage field
    // or some other means. 
    // 
    // This method is optional and can be removed if not
    // needed.
}

public struct blockStatus {
    public string name { get; set; }
    public float mass { get; set; }
}

public void Main(string argument, UpdateType updateSource) {

    if (argument == "change_state_hangar") {
        requestedStateHangarClosed = !requestedStateHangarClosed;
    }

    if (argument == "change_state_armor") {
        requestedStateArmorClosed = !requestedStateArmorClosed;
    }

    List<IMyAirtightSlideDoor> doors = new List<IMyAirtightSlideDoor>();
    GridTerminalSystem.GetBlocksOfType<IMyAirtightSlideDoor>(doors);
    foreach (IMyAirtightSlideDoor door in doors) {
        if (door.CustomData != Me.CustomData) {
            continue;
        }

        bool foundDoorStatus = false;
        doorStatus doorStatus = new doorStatus();
        for (int i = 0; i < doorStatuses.Count; i++) {
            doorStatus = doorStatuses[i];
            if (doorStatus.name == door.DisplayNameText) {
                foundDoorStatus = true;
                if (door.OpenRatio < 1) {
                    doorStatus.ticksOpen = 0;
                }
                else {
                    doorStatus.ticksOpen += 100;
                    if (doorStatus.ticksOpen >= ticksBeforeDoorsClose) {
                        door.CloseDoor();
                    }
                }
                doorStatuses[i] = doorStatus;
            }
        }
        if (!foundDoorStatus) {
            doorStatus newDoorStatus = new doorStatus();
            newDoorStatus.name = door.DisplayNameText;
            doorStatuses.Add(newDoorStatus);
        }
    }

    string hangarState = "Unknown";
    float openedHangar = 0;
    float closedHangar = -90 * (float)(Math.PI / 180);
    float bottomHangarHingeAngle = 0;
    float topHangarHingeAngle = 0;

    string armorState = "Unknown";
    float openedArmor = 0;
    float closedArmor = -90 * (float)(Math.PI / 180);
    float armorHingeAngle = 0;

    List<IMyMotorAdvancedStator> hinges = new List<IMyMotorAdvancedStator>();
    GridTerminalSystem.GetBlocksOfType<IMyMotorAdvancedStator>(hinges);
    foreach (IMyMotorAdvancedStator hinge in hinges) {
        if (hinge.CustomData == Me.CustomData + "_hangar_bottom") {
            bottomHangarHingeAngle = hinge.Angle;
        }

        if (hinge.CustomData == Me.CustomData + "_hangar_top") {
            topHangarHingeAngle = hinge.Angle;
        }

        if (hinge.CustomData == Me.CustomData + "_armor") {
            armorHingeAngle = hinge.Angle;
        }
    }

    if ((int)bottomHangarHingeAngle == (int)closedHangar && (int)topHangarHingeAngle == (int)closedHangar) {
        hangarState = "Closed";
    }
    else if ((int)bottomHangarHingeAngle == (int)openedHangar && (int)topHangarHingeAngle == (int)openedHangar) {
        hangarState = "Opened";
    }

    if (armorHingeAngle == closedArmor) {
        armorState = "Closed";
    }
    else if (armorHingeAngle == openedArmor) {
        armorState = "Opened";
    }

    foreach (IMyMotorAdvancedStator hinge in hinges) {
        if (requestedStateHangarClosed && hinge.CustomData == Me.CustomData + "_hangar_bottom" && (int)topHangarHingeAngle == (int)openedHangar) {
            hinge.RotateToAngle(MyRotationDirection.AUTO, closedHangar * (float)(180 / Math.PI), hangarDoorSpeed);
        }

        if (requestedStateHangarClosed && hinge.CustomData == Me.CustomData + "_hangar_top" && (int)bottomHangarHingeAngle == (int)closedHangar) {
            hinge.RotateToAngle(MyRotationDirection.AUTO, closedHangar * (float)(180 / Math.PI), hangarDoorSpeed);
        }

        if (!requestedStateHangarClosed && hinge.CustomData == Me.CustomData + "_hangar_bottom" && (int)topHangarHingeAngle == (int)openedHangar) {
            hinge.RotateToAngle(MyRotationDirection.AUTO, openedHangar * (float)(180 / Math.PI), hangarDoorSpeed);
        }

        if (!requestedStateHangarClosed && hinge.CustomData == Me.CustomData + "_hangar_top" && (int)bottomHangarHingeAngle == (int)closedHangar) {
            hinge.RotateToAngle(MyRotationDirection.AUTO, openedHangar * (float)(180 / Math.PI), hangarDoorSpeed);
        }

        if (requestedStateArmorClosed && hinge.CustomData == Me.CustomData + "_armor") {
            hinge.RotateToAngle(MyRotationDirection.AUTO, closedArmor * (float)(180 / Math.PI), armorSpeed);
        }

        if (!requestedStateArmorClosed && hinge.CustomData == Me.CustomData + "_armor") {
            hinge.RotateToAngle(MyRotationDirection.AUTO, openedArmor * (float)(180 / Math.PI), armorSpeed);
        }
    }

    string displayText = "";

    // List<blockStatus> blockStatuses = new List<blockStatus>();
    // List<IMyCubeBlock> blocks = new List<IMyCubeBlock>();
    // GridTerminalSystem.GetBlocksOfType<IMyCubeBlock>(blocks);
    // foreach (IMyCubeBlock block in blocks) {
    //     if (!block.DisplayNameText.StartsWith("Mobile")) {
    //         continue;
    //     }
    //     if (block.Mass < 2000f) {
    //         continue;
    //     }
    //     blockStatus status = new blockStatus();
    //     status.name = block.DisplayNameText;
    //     status.mass = block.Mass;
    //     blockStatuses.Add(status);
    // }

    // foreach (blockStatus status in blockStatuses) {
    //     //displayText += "\n" + status.name + ": " + status.mass.ToString();
    //     displayText += "\n" + status.name + ": " + status.DisassembleRatio.ToString();
    // }

    float currentStoredPower = 0;
    float maxStoredPower = 0;
    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);
    foreach (IMyBatteryBlock battery in batteries) {
        if (battery.CustomData != Me.CustomData) {
            continue;
        }
        currentStoredPower += battery.CurrentStoredPower;
        maxStoredPower += battery.MaxStoredPower;
    }
    string storedPowerPercent = Math.Round((currentStoredPower / maxStoredPower) * 100, 0, MidpointRounding.AwayFromZero).ToString();
    displayText += "\nEnergy: " + storedPowerPercent + "%    " + Math.Round(currentStoredPower, 2).ToString() + "/" + maxStoredPower.ToString() + " MWh";

    int oxygenTankCount = 0;
    double filledRatio = 0;
    List<IMyGasTank> tanks = new List<IMyGasTank>();
    GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks);
    foreach (IMyGasTank tank in tanks) {
        if (tank.CustomData != Me.CustomData) {
            continue;
        }
        oxygenTankCount++;
        filledRatio += tank.FilledRatio;
    }
    double overallRatio = Math.Round(filledRatio / oxygenTankCount * 100, 3);
    displayText += "\nOxygen Tanks: " + overallRatio.ToString() + "%";

    Dictionary<string, int> inventoryActuals = new Dictionary<string, int>();

    long currentVolume = 0;
    long maxVolume = 0;
    List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers);
    foreach (IMyCargoContainer cargoContainer in cargoContainers) {
        if (cargoContainer.CustomData == Me.CustomData) {
            IMyInventory cargoContainerInventory = cargoContainer.GetInventory();
            currentVolume += cargoContainerInventory.CurrentVolume.RawValue;
            maxVolume += cargoContainerInventory.MaxVolume.RawValue;

            for (int i = 0; i < cargoContainerInventory.ItemCount; i++) {
                MyInventoryItem? item = cargoContainerInventory.GetItemAt(i);
                if (item != null) {
                    MyInventoryItem notNullItem = (MyInventoryItem)item;
                    // if (notNullItem.Type.TypeId != "MyObjectBuilder_Component") {
                    //     continue;
                    // }
                    if (inventoryActuals.ContainsKey(notNullItem.Type.SubtypeId)) {
                        inventoryActuals[notNullItem.Type.SubtypeId] += notNullItem.Amount.ToIntSafe();
                    }
                    else {
                        inventoryActuals.Add(notNullItem.Type.SubtypeId, notNullItem.Amount.ToIntSafe());
                    }
                }
            }
        }

        if (cargoContainer.CustomData == Me.CustomData + "_empty") {
            foreach (IMyCargoContainer destinationCargoContainer in cargoContainers) {
                if (destinationCargoContainer.CustomData != Me.CustomData) {
                    continue;
                }
                IMyInventory destinationInventory = destinationCargoContainer.GetInventory();
                if (destinationInventory.VolumeFillFactor < .8) {
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    cargoContainer.GetInventory().GetItems(items);
                    foreach (MyInventoryItem item in items) {
                        cargoContainer.GetInventory().TransferItemTo(destinationInventory, item);
                    }
                }
            }
        }
    }

    float intermediate = (float)currentVolume / (float)maxVolume;
    string cargoContainersPercent = Math.Round(intermediate * 100, 0, MidpointRounding.AwayFromZero).ToString();
    displayText += "\nContainer Capacity: " + cargoContainersPercent + "%    " + (currentVolume / 1000000).ToString() + "/" + (maxVolume / 1000000).ToString() + " tonnes";

    displayText += "\nHangar Doors: " + hangarState;
    displayText += "\nArmor: " + armorState;

    displayText += "\nFactory...";
    List<IMyRefinery> refineries = new List<IMyRefinery>();
    GridTerminalSystem.GetBlocksOfType<IMyRefinery>(refineries);
    foreach (IMyRefinery refinery in refineries) {
        if (refinery.CustomData != Me.CustomData) {
            continue;
        }
        displayText += "\n    " + refinery.DisplayNameText + "    Producing: " + refinery.IsProducing.ToString();
    }

    List<IMyAssembler> assemblers = new List<IMyAssembler>();
    GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblers);
    foreach (IMyAssembler assembler in assemblers) {
        if (assembler.CustomData != Me.CustomData) {
            continue;
        }
        displayText += "\n    " + assembler.DisplayNameText + "    Producing: " + assembler.IsProducing.ToString();

        foreach (IMyCargoContainer cargoContainer in cargoContainers) {
            if (cargoContainer.CustomData != Me.CustomData) {
                continue;
            }
            IMyInventory cargoContainerInventory = cargoContainer.GetInventory();
            if (cargoContainerInventory.VolumeFillFactor < .8) {
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                assembler.OutputInventory.GetItems(items);
                foreach (MyInventoryItem item in items) {
                    assembler.OutputInventory.TransferItemTo(cargoContainerInventory, item);
                }
            }
        }

        if (assembler.IsQueueEmpty) {
            MyDefinitionId blueprint = new MyDefinitionId();
            double amount = 10;
            foreach (KeyValuePair<string, int> item in inventoryRequests) {
                if (inventoryActuals.ContainsKey(item.Key) && inventoryActuals[item.Key] < item.Value) {
                    string blueprintDefinition = item.Key;
                    if (componentToBlueprint.ContainsKey(item.Key)) {
                        blueprintDefinition = componentToBlueprint[item.Key];
                    }
                    blueprint = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + blueprintDefinition);
                    assembler.AddQueueItem(blueprint, amount);
                }
                if (!inventoryActuals.ContainsKey(item.Key) && item.Value > 0) {
                    string blueprintDefinition = item.Key;
                    if (componentToBlueprint.ContainsKey(item.Key)) {
                        blueprintDefinition = componentToBlueprint[item.Key];
                    }
                    blueprint = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + blueprintDefinition);
                    assembler.AddQueueItem(blueprint, amount);
                }
            }
        }
    }

    List<IMyGasGenerator> gasGenerators = new List<IMyGasGenerator>();
    GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(gasGenerators);
    foreach (IMyGasGenerator gasGenerator in gasGenerators) {
        if (gasGenerator.CustomData != Me.CustomData) {
            continue;
        }
        displayText += "\n    " + gasGenerator.DisplayNameText + "    Producing: " + gasGenerator.IsProducing.ToString();
    }

    float maxThrustUp = 0;
    float maxThrustDown = 0;
    float maxThrustForward = 0;
    float maxThrustBackward = 0;
    float maxThrustLeft = 0;
    float maxThrustRight = 0;
    List<IMyThrust> thrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
    foreach (IMyThrust thruster in thrusters) {
        if (thruster.CustomData != Me.CustomData) {
            continue;
        }

        if (thruster.GridThrustDirection == Vector3I.Up) {
            maxThrustUp += thruster.MaxThrust;
        }

        if (thruster.GridThrustDirection == Vector3I.Down) {
            maxThrustDown += thruster.MaxThrust;
        }

        if (thruster.GridThrustDirection == Vector3I.Forward) {
            maxThrustForward += thruster.MaxThrust;
        }

        if (thruster.GridThrustDirection == Vector3I.Backward) {
            maxThrustBackward += thruster.MaxThrust;
        }

        if (thruster.GridThrustDirection == Vector3I.Left) {
            maxThrustLeft += thruster.MaxThrust;
        }

        if (thruster.GridThrustDirection == Vector3I.Right) {
            maxThrustRight += thruster.MaxThrust;
        }
        
    }
    displayText += "\nThrust...";
    displayText += "\n    Up Max: " + (maxThrustUp / 1000).ToString() + " kn";
    displayText += "\n    Down Max: " + (maxThrustDown / 1000).ToString() + " kn";
    displayText += "\n    Forward Max: " + (maxThrustForward / 1000).ToString() + " kn";
    displayText += "\n    Backward Max: " + (maxThrustBackward / 1000).ToString() + " kn";
    displayText += "\n    Left Max: " + (maxThrustLeft / 1000).ToString() + " kn";
    displayText += "\n    Right Max: " + (maxThrustRight / 1000).ToString() + " kn";


    List<IMyTextPanel> displays = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);
    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != Me.CustomData) {
            continue;
        }
        IMyTextSurface drawingSurface = Me.GetSurface(0);
        RectangleF viewport;
        viewport = new RectangleF(
            (drawingSurface.TextureSize - drawingSurface.SurfaceSize) / 2f,
            drawingSurface.SurfaceSize
        );

        var frame = display.DrawFrame();
        Vector2 position = new Vector2(10, 10);
        MySprite sprite = new MySprite() {
            Type = SpriteType.TEXT,
            Data = DateTime.UtcNow.ToString(@"hh\:mm\:ss") + "\n" + displayText,
            Position = position,
            RotationOrScale = .75f,
            Color = Color.White,
            Alignment = TextAlignment.LEFT,
            FontId = "White"
        };
        frame.Add(sprite);
        frame.Dispose();
    }

    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != Me.CustomData + "_inventory_requests") {
            continue;
        }

        if (display.GetText() == "") {
            string message = "";
            message += "SteelPlate 0";
            message += "\nSmallTube 0";
            message += "\nGirder 0";
            display.WriteText(message);
        }

        foreach (string entry in display.GetText().Split('\n')) {
            string name = entry.Split(' ')[0];
            int amount = int.Parse(entry.Split(' ')[1]);
            if (!inventoryRequests.ContainsKey(name)) {
                inventoryRequests.Add(name, amount);
            }
            else {
                inventoryRequests[name] = amount;
            }
        }
    }

    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != Me.CustomData + "_inventory_actuals") {
            continue;
        }

        string message = "Name: Amount (Requested)";
        string[] keys = inventoryActuals.Keys.ToArray();
        Array.Sort(keys);
        foreach (string key in keys) {
            string request = "0";
            if (inventoryRequests.ContainsKey(key)) {
                request = inventoryRequests[key].ToString();
            }
            message += "\n" + key  + ": " + inventoryActuals[key].ToString() + " (" + request + ")";
        }
        display.WriteText(message);
    }
}
