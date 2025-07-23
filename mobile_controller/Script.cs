public Program()
{
    // The constructor, called only once every session and
    // always before any other method is called. Use it to
    // initialize your script. 
    //     
    // The constructor is optional and can be removed if not
    // needed.
    // 
    // It's recommended to set RuntimeInfo.UpdateFrequency 
    // here, which will allow your script to run itself without a 
    // timer block.

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
    //public float
}

public void Main(string argument, UpdateType updateSource) {

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

    long currentVolume = 0;
    long maxVolume = 0;
    List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers);
    foreach (IMyCargoContainer cargoContainer in cargoContainers) {
        if (cargoContainer.CustomData != Me.CustomData) {
            continue;
        }
        IMyInventory cargoContainerInventory = cargoContainer.GetInventory();
        currentVolume += cargoContainerInventory.CurrentVolume.RawValue;
        maxVolume += cargoContainerInventory.MaxVolume.RawValue;
    }

    float intermediate = (float)currentVolume / (float)maxVolume;
    string cargoContainersPercent = Math.Round(intermediate * 100, 0, MidpointRounding.AwayFromZero).ToString();
    displayText += "\nContainer Capacity: " + cargoContainersPercent + "%    " + (currentVolume / 1000000).ToString() + "/" + (maxVolume / 1000000).ToString() + " tonnes";

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

}
