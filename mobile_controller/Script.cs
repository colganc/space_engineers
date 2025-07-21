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

    List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers);

    long currentVolume = 0;
    long maxVolume = 0;

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

    List<IMyAssembler> assemblers = new List<IMyAssembler>();
    GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblers);
    foreach (IMyAssembler assembler in assemblers) {
        if (assembler.CustomData != Me.CustomData) {
            continue;
        }

        displayText += "\nAssembler...";
        displayText += "\n    Producing: " + assembler.IsProducing.ToString();
        displayText += "\n    Empty Queue: " + assembler.IsQueueEmpty.ToString();
        displayText += "\n    Working: " + assembler.IsWorking.ToString();
    }

    List<IMyRefinery> refineries = new List<IMyRefinery>();
    GridTerminalSystem.GetBlocksOfType<IMyRefinery>(refineries);
    foreach (IMyRefinery refinery in refineries) {
        if (refinery.CustomData != Me.CustomData) {
            continue;
        }

        displayText += "\nRefinery...";
        displayText += "\n    Producing: " + refinery.IsProducing.ToString();
        displayText += "\n    Empty Queue: " + refinery.IsQueueEmpty.ToString();
        displayText += "\n    Working: " + refinery.IsWorking.ToString();
    }

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
