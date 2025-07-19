IMyBroadcastListener broadcastListener;
string messageToDroneControllerTag = "drone_controller";
int displayScreen = 1;
int maxDisplayScreens = 5;
int displayScreenOption = 0;
int maxDisplayScreenOptions = 0;
string selectedOption;
string[] displayScreenTitles = { "Compact", "Dbg", "Gen", "Cmd", "Seq", "Gui" };
int droneMaxLogSize = 15;
string selectedDrone = "";

public struct droneTelemetry {
    public string droneName { get; set; }               // messageData[0];
    public string inventoryStatus { get; set; }         // messageData[4];
    public string energyStatus { get; set; }            // messageData[5];
    public string actionStatus { get; set; }            // messageData[6];
    public string drillStatus { get; set; }             // messageData[7];
    public string downSensorCloseStatus { get; set; }   // messageData[8];
    public string powerStatus { get; set; }             // messageData[9];
    public string rollAngleStatus { get; set; }         // messageData[10];
    public string pitchAngleStatus { get; set; }        // messageData[11];
    public string yawAngleStatus { get; set; }          // messageData[17];
    public string downSensorDynamicStatus { get; set; } // messageData[12];
    public string shipMaxVelocityStatus { get; set; }   // messageData[13];
    public string shipVelocityStatus { get; set; }      // messageData[14];
    public string waypointNameStatus { get; set; }      // messageData[15];
    public string orientationStatus { get; set; }       // messageData[16];
    public string positionX { get; set; }               // messageData[1]
    public string positionY { get; set; }               // messageData[2]
    public string positionZ { get; set; }               // messageData[3]
    public bool wasActedOn { get; set; }
}
List<droneTelemetry> droneTelemetryList = new List<droneTelemetry>();

public struct droneStatus {
    public string droneName { get; set; }
    public string commandSequence { get; set; }
    public string commandLog { get; set; }
}
List<droneStatus> droneStatusList = new List<droneStatus>();

public struct droneMessage {
    public droneMessage (string initDroneName, string initMessage) {
        droneName = initDroneName;
        message = initMessage;
    }
    public string droneName { get; set; }
    public string message { get; set; }
}

public struct droneCommand {
    public droneCommand (string argument) {
        name = null;
        value = null;
        string[] splitArgument = argument.Split(' ');
        if (splitArgument.Length >= 1) {
            name = splitArgument[0];
        }
        if (splitArgument.Length >= 2) {
            value = splitArgument[1];
        }
    }
    public string name { get; set; }
    public string value { get; set; }
}

public droneTelemetry messageToDroneTelemetry (string message) {
    string[] messageData = message.Split(',');

    droneTelemetry messageTelemetry = new droneTelemetry();
    messageTelemetry.droneName = messageData[0];
    messageTelemetry.inventoryStatus = messageData[4];
    messageTelemetry.energyStatus = messageData[5];
    messageTelemetry.actionStatus = messageData[6];
    messageTelemetry.drillStatus = messageData[7];
    messageTelemetry.downSensorCloseStatus = messageData[8];
    messageTelemetry.powerStatus = messageData[9];
    messageTelemetry.rollAngleStatus = messageData[10];
    if (messageTelemetry.rollAngleStatus != "?") {
        double rollAngle = Math.Round(Convert.ToDouble(messageTelemetry.rollAngleStatus), 0, MidpointRounding.AwayFromZero);
        messageTelemetry.rollAngleStatus = rollAngle.ToString();
    }
    messageTelemetry.pitchAngleStatus = messageData[11];
    if (messageTelemetry.pitchAngleStatus != "?") {
        double pitchAngle = Math.Round(Convert.ToDouble(messageTelemetry.pitchAngleStatus), 0, MidpointRounding.AwayFromZero);
        messageTelemetry.pitchAngleStatus = pitchAngle.ToString();
    }
    messageTelemetry.yawAngleStatus = messageData[17];
    if (messageTelemetry.yawAngleStatus != "?") {
        double yawAngle = Math.Round(Convert.ToDouble(messageTelemetry.yawAngleStatus), 0, MidpointRounding.AwayFromZero);
        messageTelemetry.yawAngleStatus = yawAngle.ToString();
    }
    messageTelemetry.downSensorDynamicStatus = messageData[12];
    messageTelemetry.shipMaxVelocityStatus = messageData[13];
    if (messageTelemetry.shipMaxVelocityStatus == "" || messageTelemetry.shipMaxVelocityStatus == "?") {
        messageTelemetry.shipMaxVelocityStatus = "0";
    }
    messageTelemetry.shipVelocityStatus = messageData[14];
    messageTelemetry.shipMaxVelocityStatus = Math.Round(Convert.ToDouble(messageTelemetry.shipMaxVelocityStatus), 0, MidpointRounding.AwayFromZero).ToString();
    if (messageTelemetry.shipVelocityStatus == "" || messageTelemetry.shipVelocityStatus == "?") {
        messageTelemetry.shipVelocityStatus = "0";
    }
    messageTelemetry.shipVelocityStatus = Math.Round(Convert.ToDouble(messageTelemetry.shipVelocityStatus), 0, MidpointRounding.AwayFromZero).ToString();

    messageTelemetry.waypointNameStatus = messageData[15];
    messageTelemetry.orientationStatus = messageData[16];
    messageTelemetry.positionX = messageData[1];
    messageTelemetry.positionY = messageData[2];
    messageTelemetry.positionZ = messageData[3];
    messageTelemetry.wasActedOn = false;

    return messageTelemetry;
}

public droneTelemetry getDroneTelemetry (string droneName, List<droneTelemetry> drones) {
    droneTelemetry telemetry = new droneTelemetry();
    foreach (droneTelemetry drone in drones) {
        if (drone.droneName == droneName) {
            telemetry = drone;
        }
    }
    return telemetry;
}

public string getNextDroneName (string droneName, List<droneTelemetry> drones) {
    if (drones.Count == 0) {
        return "";
    }

    for (int i = 0; i < drones.Count - 1; i++) {
        if (drones[i].droneName == droneName) {
            int nextDrone = i + 1;
            if (nextDrone > drones.Count - 1) {
                nextDrone = 0;
            }
            return drones[nextDrone].droneName;
        }
    }

    return drones[0].droneName;
}

public List<droneTelemetry> setDroneTelemetry (droneTelemetry setTelemetry, List<droneTelemetry> drones) {
    if (setTelemetry.droneName == "" || setTelemetry.droneName == null) {
        return drones;
    }

    for (int i = 0; i < drones.Count; i++) {
        if (drones[i].droneName == setTelemetry.droneName) {
            drones[i] = setTelemetry;
            return drones;
        }
    }

    drones.Add(setTelemetry);
    return drones;
}


public string getDisplayHeader (int headerForDisplayScreen, string droneName) {
    string utcNow = DateTime.UtcNow.ToString(@"hh\:mm\:ss");
    string header = droneName;
    if (headerForDisplayScreen > 0) {
        for (int i = 1; i <= displayScreenTitles.Length; i++) {
            if (i == headerForDisplayScreen) {
                header += " | " + displayScreenTitles[i].ToUpper();
                continue;
            }
            header += " | " + displayScreenTitles[i].ToLower();
        }
        header += " | " + utcNow;
    }
    return header;
}


public string[] getDisplayScreenText (droneTelemetry telemetry) {
    string[] displayScreens = new string[maxDisplayScreens];

    Vector3D homePosition = Me.GetPosition();
    Vector3D dronePosition;
    dronePosition.X = Convert.ToDouble(telemetry.positionX);
    dronePosition.Y = Convert.ToDouble(telemetry.positionY);
    dronePosition.Z = Convert.ToDouble(telemetry.positionZ);
    double distance = Math.Round(Vector3D.Distance(homePosition, dronePosition) / 1000, 1, MidpointRounding.AwayFromZero);

    displayScreens[0] = getDisplayHeader(0, telemetry.droneName);
    for (int i = 1; i < maxDisplayScreens; i++) {
        displayScreens[i] = getDisplayHeader(i, telemetry.droneName);
    }

    displayScreens[0] += "\nI " + telemetry.inventoryStatus;
    displayScreens[0] += "\nE " + telemetry.energyStatus;
    displayScreens[0] += "\nD " + distance.ToString();
    displayScreens[0] += "\nA " + telemetry.actionStatus;

    for (int i = 1; i < maxDisplayScreens; i++) {
        displayScreens[i] += "\nI:" + telemetry.inventoryStatus;
        displayScreens[i] += "   E:" + telemetry.energyStatus;
        displayScreens[i] += "   D:" + distance;
        displayScreens[i] += "   A:" + telemetry.actionStatus;
    }

    displayScreens[1] += "\nOrientation: " + telemetry.orientationStatus;
    displayScreens[1] += "\nVelocity: " + telemetry.shipVelocityStatus + "/" + telemetry.shipMaxVelocityStatus;
    displayScreens[1] += "\nRoll: " + telemetry.rollAngleStatus;
    displayScreens[1] += "\nPitch: " + telemetry.pitchAngleStatus;
    displayScreens[1] += "\nYaw: " + telemetry.yawAngleStatus;

    displayScreens[2] += "\nWaypoint: " + telemetry.waypointNameStatus;
    displayScreens[2] += "\nDrill: " + telemetry.drillStatus;
    displayScreens[2] += "\nDrill Sensor: " + telemetry.downSensorCloseStatus;
    displayScreens[2] += "\nVelocity: " + telemetry.shipVelocityStatus + "/" + telemetry.shipMaxVelocityStatus;
    displayScreens[2] += "\nSensor: " + telemetry.downSensorDynamicStatus;

    return displayScreens;
}

public void setCommandLogDisplays (string controllerName, string droneName, List<droneStatus> statuses, List<IMyTextPanel> displays) {
    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != controllerName + "_command_log") {
            continue;
        }

        bool foundStatus = false;
        foreach (droneStatus status in statuses) {
            if (status.droneName != droneName) {
                continue;
            }
            display.WriteText(droneName + " Command Log\n" + status.commandLog);
            foundStatus = true;
            break;
        }

        if (!foundStatus) {
            display.WriteText(droneName + " Command Log\nNo log found");
        }
    }  
}

public List<droneStatus> addCommandSequence (string droneName, List<droneStatus> statuses, string commandSequence) {
    if (commandSequence == null) {
        commandSequence = "";
    }

    bool foundDroneStatus = false;
    for (int i = 0; i < statuses.Count; i++) {
        if (statuses[i].droneName == droneName) {
            droneStatus status = statuses[i];
            status.commandSequence += "\n" + commandSequence;
            status.commandSequence = status.commandSequence.Trim();
            statuses[i] = status;
            foundDroneStatus = true;
            break;
        }
    }

    if (!foundDroneStatus) {
        droneStatus status = new droneStatus();
        status.droneName = droneName;
        status.commandSequence = commandSequence.Trim();
        status.commandLog = "";
        statuses.Add(status);
    }

    return statuses;
}

public List<droneStatus> setCommandSequence (string droneName, string commandSequence, List<droneStatus> statuses) {
    for (int i = 0; i < statuses.Count; i++) {
        if (statuses[i].droneName == droneName) {
            droneStatus status = new droneStatus();
            status = statuses[i];
            status.commandSequence = commandSequence;
            statuses[i] = status;
            break;
        }
    }
    return statuses;
}

public string getCommandSequence (string droneName, List<droneStatus> statuses) {
    for (int i = 0; i < statuses.Count; i++) {
        if (statuses[i].droneName == droneName) {
            return statuses[i].commandSequence;
        }
    }
    return "";
}

public List<droneStatus> clearCommandSequence (string droneName, List<droneStatus> statuses) {
    for (int i = 0; i < statuses.Count; i++) {
        if (statuses[i].droneName == droneName) {
            droneStatus status = new droneStatus();
            status = statuses[i];
            status.commandSequence = "";
            statuses[i] = status;
            break;
        }
    }
    return statuses;
}

public void setCommandSequenceDisplays (string controllerName, string droneName, List<droneStatus> statuses, List<IMyTextPanel> displays) {
    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != controllerName + "_command_sequence") {
            continue;
        }

        bool foundStatus = false;
        foreach (droneStatus status in statuses) {
            if (status.droneName != droneName) {
                continue;
            }
            display.WriteText(droneName + " Command Sequence\n" + status.commandSequence);
            foundStatus = true;
            break;
        }

        if (!foundStatus) {
            display.WriteText(droneName + " Command Sequence\nNo command sequence found");
        }
    }  
}

public string getCommandSequenceFromDisplay (string commandSequenceTitle, List<IMyTextPanel> displays) {
    string actionsOnly = "";
    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != "action_sequence") {
            continue;
        }
        string actionSequenceHeader = display.GetText().Split('\n')[0];
        string actionSequenceHeaderTitle = actionSequenceHeader.Split(' ')[1];
        if (commandSequenceTitle == actionSequenceHeaderTitle) {
            string[] actionSequence = display.GetText().Split('\n');
            for (int i = 1; i < actionSequence.Length; i++) {
                actionsOnly += actionSequence[i] + "\n";
            }
        }
    }
    return actionsOnly;
}

public droneMessage getDroneCommandMessage(string droneName, droneCommand command, List<IMyTextPanel> displays) {
    if (command.name == "waypoint") {
        string message = "waypoint";

        if (command.value != "home") {
            message += " " + command.value.Split(':')[2];
            message += "," + command.value.Split(':')[3];
            message += "," + command.value.Split(':')[4];
            message += "," + command.value.Split(':')[1];
        }
        else {    
            foreach (IMyTextPanel display in displays) {
                if (display.CustomData != "Dock" + droneName[droneName.Length - 1]) {
                    continue;
                }
                Vector3D tempVector = Vector3D.Forward * 100;
                Vector3D waypoint = Vector3D.Transform(Vector3D.Right * 100, display.WorldMatrix);
                message += " " + waypoint.X;
                message += "," + waypoint.Y;
                message += "," + waypoint.Z;
                message += ",Dock" + droneName[droneName.Length - 1];
            }
        }
        return new droneMessage(droneName, message);
    }

    if (command.name == "undock") {
        return new droneMessage(droneName, getUndockMessage(displays));
    }

    if (command.name == "dock") {
        Vector3D startWorldPosition = Vector3D.Transform(Vector3D.Zero, Me.WorldMatrix);
        Vector3D endDownWorldPosition = Vector3D.Transform(Vector3D.Down, Me.WorldMatrix);
        Vector3D endForwardWorldPosition = Vector3D.Transform(Vector3D.Forward, Me.WorldMatrix);
        Vector3D downWorldDirection = Vector3D.Subtract(endDownWorldPosition, startWorldPosition);
        Vector3D forwardWorldDirection = Vector3D.Subtract(endForwardWorldPosition, startWorldPosition);
        downWorldDirection.Normalize();
        forwardWorldDirection.Normalize();
        string message = "dock";
        message += " " + downWorldDirection.X.ToString();
        message += "," + downWorldDirection.Y.ToString();
        message += "," + downWorldDirection.Z.ToString();
        message += "," + forwardWorldDirection.X.ToString();
        message += "," + forwardWorldDirection.Y.ToString();
        message += "," + forwardWorldDirection.Z.ToString();
        return new droneMessage(droneName, message);
    }

    if (command.name == "disable_autopilot") {
        return new droneMessage(droneName, "disable_autopilot");
    }

    if (command.name == "low_power") {
        return new droneMessage(droneName, "low_power");
    }

    if (command.name == "mine") {
        return new droneMessage(droneName, "mine");
    }

    if (command.name == "orientation") {
        string message = "orientation";

        if (command.value == "home") {
            Vector3D startWorldPosition = Vector3D.Transform(Vector3D.Zero, Me.WorldMatrix);
            Vector3D endDownWorldPosition = Vector3D.Transform(Vector3D.Down, Me.WorldMatrix);
            Vector3D endForwardWorldPosition = Vector3D.Transform(Vector3D.Forward, Me.WorldMatrix);
            Vector3D downWorldDirection = Vector3D.Subtract(endDownWorldPosition, startWorldPosition);
            Vector3D forwardWorldDirection = Vector3D.Subtract(endForwardWorldPosition, startWorldPosition);
            downWorldDirection.Normalize();
            forwardWorldDirection.Normalize();
            message += " " + downWorldDirection.X.ToString();
            message += "," + downWorldDirection.Y.ToString();
            message += "," + downWorldDirection.Z.ToString();
            message += "," + forwardWorldDirection.X.ToString();
            message += "," + forwardWorldDirection.Y.ToString();
            message += "," + forwardWorldDirection.Z.ToString();
        }

        if (command.value == "natural_gravity") {
            message += " natural_gravity";
        }
        return new droneMessage(droneName, message);
    }

    return new droneMessage();
}


public string getUndockMessage (List<IMyTextPanel> displays) {
    string message = "";
    Vector3D startWorldPosition = Vector3D.Transform(Vector3D.Zero, Me.WorldMatrix);
    Vector3D endDownWorldPosition = Vector3D.Transform(Vector3D.Down, Me.WorldMatrix);
    Vector3D endForwardWorldPosition = Vector3D.Transform(Vector3D.Forward, Me.WorldMatrix);
    Vector3D downWorldDirection = Vector3D.Subtract(endDownWorldPosition, startWorldPosition);
    Vector3D forwardWorldDirection = Vector3D.Subtract(endForwardWorldPosition, startWorldPosition);
    downWorldDirection.Normalize();
    forwardWorldDirection.Normalize();
    message = "undock";
    message += " " + downWorldDirection.X.ToString();
    message += "," + downWorldDirection.Y.ToString();
    message += "," + downWorldDirection.Z.ToString();
    message += "," + forwardWorldDirection.X.ToString();
    message += "," + forwardWorldDirection.Y.ToString();
    message += "," + forwardWorldDirection.Z.ToString();
    return message;
}

public List<droneStatus> addCommandLogEntry (string droneName, string newEntry, List<droneStatus> statuses) {
    if (newEntry.Trim() == "") {
        return statuses;
    }

    string utcNow = DateTime.UtcNow.ToString(@"hh\:mm\:ss");
    bool foundDroneStatus = false;

    for (int i = 0; i < statuses.Count; i++) {
        if (statuses[i].droneName != droneName) {
            continue;
        }

        string[] commandLog = statuses[i].commandLog.Split('\n');
        string updatedLog = utcNow + " " + newEntry;
        for (int c = 0; c < commandLog.Length; c++) {
            if (c > droneMaxLogSize) {
                break;
            }
            updatedLog += "\n" + commandLog[c];
        }

        droneStatus status = statuses[i];
        status.commandLog = updatedLog;
        statuses[i] = status;
        foundDroneStatus = true;
        break;
    }

    if (!foundDroneStatus) {
        droneStatus status = new droneStatus();
        status.droneName = droneName;
        status.commandSequence = "";
        status.commandLog = utcNow + " " + newEntry;
        statuses.Add(status);
    }

    return statuses;
}

public void guiScreen (IMyTextPanel display, List<droneTelemetry> drones, List<droneStatus> statuses, string selectedDrone) {
    // if (display.ContentType != ContentType.SCRIPT) {
    //     display.ContentType = ContentType.SCRIPT;
    //     display.Script = "";
    // }
    IMyTextSurface drawingSurface = Me.GetSurface(0);
    RectangleF viewport;
    viewport = new RectangleF(
        (drawingSurface.TextureSize - drawingSurface.SurfaceSize) / 2f,
        drawingSurface.SurfaceSize
    );

    var frame = display.DrawFrame();
    Vector2 position = new Vector2(10, 10); // + viewport.Position;
    MySprite sprite = new MySprite() {
        Type = SpriteType.TEXT,
        Data = DateTime.UtcNow.ToString(@"hh\:mm\:ss"),
        Position = position,
        RotationOrScale = .75f,
        Color = Color.White,
        Alignment = TextAlignment.LEFT, //TextAlignment.CENTER /* Center the text on the position */,
        FontId = "White"
    };
    frame.Add(sprite);

    position += new Vector2(0, 20);

    foreach (droneTelemetry drone in drones) {
        sprite = new MySprite() {
            Type = SpriteType.TEXT,
            Data = drone.droneName,
            Position = position,
            RotationOrScale = 0.75f,
            Color = Color.White,
            Alignment = TextAlignment.LEFT,
            FontId = "White"
        };
        if (drone.droneName == selectedDrone) {
            sprite.Color = Color.Blue;
        }
        frame.Add(sprite);
        position += new Vector2(64, 0);
    }
    // sprite = new MySprite() {
    //     Type = SpriteType.TEXTURE,
    //     Data = "Arrow",
    //     Position = viewport.Center,
    //     Size = viewport.Size,
    //     Color = drawingSurface.ScriptForegroundColor.Alpha(0.66f),
    //     Alignment = TextAlignment.CENTER
    // };
    // // Add the sprite to the frame
    // frame.Add(sprite);

    frame.Dispose();
}

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    if (Me.CustomData == null || Me.CustomData.Trim() == "") {
        Echo("No custom data value, stopping");
        return;
    }

    broadcastListener = IGC.RegisterBroadcastListener(messageToDroneControllerTag);
}

public void Main(string argument, UpdateType updateSource)
{
    if (Me.CustomData == null || Me.CustomData.Trim() == "") {
        Echo("No custom data value, stopping");
        return;
    }

    string controllerName = Me.CustomData;
    
    string[] displayScreenText = new string[maxDisplayScreens];

    while (broadcastListener.HasPendingMessage) {
        MyIGCMessage igcMessage = broadcastListener.AcceptMessage();
        if (igcMessage.Tag == messageToDroneControllerTag) {
            string messageData = igcMessage.Data.ToString();
            if (messageData != null && messageData != "") {
                setDroneTelemetry(messageToDroneTelemetry(messageData), droneTelemetryList);
            }
        }
    }

    List<IMyTextPanel> displays = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);

    displayScreenText = getDisplayScreenText(getDroneTelemetry(selectedDrone, droneTelemetryList));

    maxDisplayScreenOptions = 0;

    string[] displayScreenCmdOptions = { "Hold", "Clear Seq", "Undock", "Mine" };
    for (int i = 0; i < displayScreenCmdOptions.Length; i++) {
        if (i == displayScreenOption) {
            displayScreenText[3] += "\n[ " + displayScreenCmdOptions[i] + " ]";
            selectedOption = displayScreenCmdOptions[i];
        }
        else {
            displayScreenText[3] += "\n" + displayScreenCmdOptions[i];
        }
    }
    if (displayScreen == 3) {
        maxDisplayScreenOptions = displayScreenCmdOptions.Length;
    }

    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != "action_sequence") {
            continue;
        }
        string actionSequenceHeader = display.GetText().Split('\n')[0];
        if (actionSequenceHeader.StartsWith("action_sequence ")) {
            if (displayScreenOption == maxDisplayScreenOptions && displayScreen == 4) {
                displayScreenText[4] += "\n[ " + actionSequenceHeader.Split(' ')[1] + " ]";
                selectedOption = actionSequenceHeader.Split(' ')[1];
            }
            else {
                displayScreenText[4] += "\n" + actionSequenceHeader.Split(' ')[1];
            }

            if (displayScreen == 4) {
                maxDisplayScreenOptions++;
            }
        }
    }

    if (selectedDrone != "") {
        foreach (IMyTextPanel display in displays) {
            if (!display.CustomData.StartsWith(selectedDrone)) {
                continue;
            }

            string[] customDataParts = display.CustomData.Split('_');
            if (customDataParts.Length == 2) {
                int screenChoice = Int32.Parse(display.CustomData.Split('_')[1]);
                display.WriteText(displayScreenText[screenChoice]);
            }
        }
    }

    foreach (IMyTextPanel display in displays) {
        if (!display.CustomData.StartsWith(controllerName)) {
            continue;
        }

        string[] customDataParts = display.CustomData.Split('_');
        if (customDataParts.Length == 2) {
            int screenChoice = Int32.Parse(display.CustomData.Split('_')[1]);
            if (screenChoice == 6) {
                guiScreen(display, droneTelemetryList, droneStatusList, selectedDrone);
            }
            else {
                display.WriteText(displayScreenText[screenChoice]);
            }
        }
    }

    List<IMyCockpit> cockpits = new List<IMyCockpit>();
    GridTerminalSystem.GetBlocksOfType<IMyCockpit>(cockpits);
    foreach (IMyCockpit cockpit in cockpits) {
        if (cockpit.CustomData != controllerName) {
            continue;
        }

        for (int c = 0; c < cockpit.SurfaceCount; c++){
            IMyTextSurface display = cockpit.GetSurface(c);
            display.WriteText(displayScreenText[displayScreen]);
        }
    }

    Queue<droneMessage> messages = new Queue<droneMessage>();

    for (int i = 0; i < droneTelemetryList.Count; i++) {
        if (!droneTelemetryList[i].wasActedOn) {
            List<string> commandSequence = new List<string>(getCommandSequence(droneTelemetryList[i].droneName, droneStatusList).Split('\n'));
            if (commandSequence.Count > 0 && commandSequence[0] != "" && (droneTelemetryList[i].actionStatus == "" || droneTelemetryList[i].actionStatus == "CN")) {
                droneStatusList = addCommandLogEntry(droneTelemetryList[i].droneName, commandSequence[0], droneStatusList);
                droneMessage message = getDroneCommandMessage(droneTelemetryList[i].droneName, new droneCommand(commandSequence[0]), displays);
                if (message.message != null) {
                    messages.Enqueue(message);
                }
                commandSequence.RemoveAt(0);
                setCommandSequence(droneTelemetryList[i].droneName, String.Join("\n", commandSequence.ToArray()), droneStatusList);
                droneTelemetry telemetry = droneTelemetryList[i];
                telemetry.wasActedOn = true;
                droneTelemetryList[i] = telemetry;
            }
        }
    }

    setCommandSequenceDisplays(controllerName, selectedDrone, droneStatusList, displays);
    setCommandLogDisplays(controllerName, selectedDrone, droneStatusList, displays);

    droneCommand command = new droneCommand(argument);
    droneMessage selectedDroneMessage = getDroneCommandMessage(selectedDrone, command, displays);
    if (selectedDroneMessage.message != null) {
        messages.Enqueue(selectedDroneMessage);
    }

    if (command.name == "next_drone") {
        selectedOption = "";
        displayScreenOption = 0;
        selectedDrone = getNextDroneName(selectedDrone, droneTelemetryList);
    }

    if (command.name == "next_screen") {
        selectedOption = "";
        displayScreenOption = 0;
        displayScreen++;
        if (displayScreen >= maxDisplayScreens) {
            displayScreen = 1;
        }
    }

    if (command.name == "next_option") {
        displayScreenOption++;
        if (displayScreenOption >= maxDisplayScreenOptions) {
            displayScreenOption = 0;
        }
    }

    if (command.name == "select_option") {
        if (displayScreen == 3) {
            if (selectedOption == "Clear Seq") {
                droneStatusList = clearCommandSequence(selectedDrone, droneStatusList);
            }
            
            if (selectedOption == "Undock") {
                messages.Enqueue(new droneMessage(selectedDrone, getUndockMessage(displays)));
            }

            if (selectedOption == "Mine") {
                messages.Enqueue(new droneMessage(selectedDrone, "mine"));
            }
        }
        if (displayScreen == 4) {
            droneStatusList = addCommandSequence(selectedDrone, droneStatusList, getCommandSequenceFromDisplay(selectedOption, displays));
        }
    }

    foreach (droneMessage message in messages) {
        IGC.SendBroadcastMessage(message.droneName, message.message, TransmissionDistance.AntennaRelay);
    }
}
