IMyBroadcastListener broadcastListener;
string messageToDroneControllerTag = "drone_controller";
int displayScreen = 1;
int maxDisplayScreens = 5;
int displayScreenOption = 0;
int maxDisplayScreenOptions = 0;
string selectedOption;
string[] displayScreenTitles = { "Compact", "Dbg", "Gen", "Cmd", "Seq" };
int droneMaxLogSize = 15;
string[] droneList = { "Drone1", "Drone2" };
int activeDrone = 0;
string selectedDrone = "";
List<droneTelemetry> displays = new List<droneTelemetry>();

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

public string getDisplayHeader (int headerForDisplayScreen, string droneName) {
    string utcNow = DateTime.UtcNow.ToString(@"hh\:mm\:ss");
    string header = droneName;
    if (headerForDisplayScreen > 0) {
        for (int i = 1; i < displayScreenTitles.Length; i++) {
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
    string[] displayScreens = new string[5];

    Vector3D homePosition = Me.GetPosition();
    Vector3D dronePosition;
    dronePosition.X = Convert.ToDouble(telemetry.positionX);
    dronePosition.Y = Convert.ToDouble(telemetry.positionY);
    dronePosition.Z = Convert.ToDouble(telemetry.positionZ);
    double distance = Math.Round(Vector3D.Distance(homePosition, dronePosition) / 1000, 1, MidpointRounding.AwayFromZero);

    displayScreens[0] = getDisplayHeader(0, droneList[activeDrone]);
    for (int i = 1; i < maxDisplayScreens; i++) {
        displayScreens[i] = getDisplayHeader(i, droneList[activeDrone]);
    }

    displayScreens[0] += "\nI " + telemetry.inventoryStatus;
    displayScreens[0] += "\nE " + telemetry.energyStatus;
    displayScreens[0] += "\nD " + distance.ToString();
    displayScreens[0] += "\nA " + telemetry.actionStatus;

    for (int i = 1; i < maxDisplayScreens; i++) {
        displayScreens[i] += "\nI:" + telemetry.inventoryStatus.PadRight(4);
        displayScreens[i] += "E:" + telemetry.energyStatus.PadRight(4);
        displayScreens[i] += "D:" + distance.ToString().PadRight(4);
        displayScreens[i] += "A:" + telemetry.actionStatus;
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

public string getActionSequenceFromDisplay (string actionSequenceTitle, List<IMyTextPanel> displays) {
    string actionsOnly = "";
    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != "action_sequence") {
            continue;
        }
        string actionSequenceHeader = display.GetText().Split('\n')[0];
        string actionSequenceHeaderTitle = actionSequenceHeader.Split(' ')[1];
        if (actionSequenceTitle == actionSequenceHeaderTitle) {
            string[] actionSequence = display.GetText().Split('\n');
            for (int i = 1; i < actionSequence.Length; i++) {
                actionsOnly += actionSequence[i] + "\n";
            }
        }
    }
    return actionsOnly;
}

public string getUndockMessage (List<IMyTextPanel> displays) {
    string message = "";
    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != "Dock1") {
            continue;
        }
        
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
        
    }
    return message;
}

public void clearActionSequenceDisplay (string droneName, List<IMyTextPanel> displays) {
    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != "action_sequence") {
            continue;
        }

        string actionSequenceDisplayText = display.GetText();
        if (actionSequenceDisplayText.Split('\n')[0] == droneName + " action_sequence") {
            display.WriteText(actionSequenceDisplayText.Split('\n')[0]);
        }
    }
}

public void setActionLog (string droneName, string newEntry, List<IMyTextPanel> displays) {
    if (newEntry.Trim() == "") {
        newEntry = "Sequence complete";
    }
    string utcNow = DateTime.UtcNow.ToString(@"hh\:mm\:ss");
    foreach (IMyTextPanel display in displays) {
        if (display.CustomData != "action_sequence") {
            continue;
        }
        string actionLogHeader = display.GetText().Split('\n')[0];
        string actionLogHeaderDroneName = actionLogHeader.Split(' ')[0];
        string actionLogTitle = actionLogHeader.Split(' ')[1];

        if (actionLogHeaderDroneName != droneName) {
            continue;
        }

        if (actionLogTitle == "action_sequence_log") {
            string[] actionSequenceLog = display.GetText().Split('\n');
            string updatedLog = actionSequenceLog[0];
            updatedLog += "\n" + utcNow + " " + newEntry;
            for (int i = 1; i < actionSequenceLog.Length; i++) {
                if (i > droneMaxLogSize) {
                    break;
                }
                updatedLog += "\n" + actionSequenceLog[i];
            }
            display.WriteText(updatedLog);
        }
    }
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

    string[] displayScreenText = new string[maxDisplayScreens];

    string droneName = Me.CustomData;
    Echo(droneName);

    string lastMessageData = "";
    while (broadcastListener.HasPendingMessage) {
        MyIGCMessage igcMessage = broadcastListener.AcceptMessage();
        if (igcMessage.Tag == messageToDroneControllerTag) {
            Echo("Reading broadcast message");
            lastMessageData = igcMessage.Data.ToString();
        }
    }

    List<IMyTextPanel> displays = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);

    droneTelemetry rawTelemetry = new droneTelemetry();

    if (lastMessageData != null && lastMessageData != "") {
        rawTelemetry = messageToDroneTelemetry(lastMessageData);
        displayScreenText = getDisplayScreenText(rawTelemetry);

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
            maxDisplayScreenOptions = displayScreenCmdOptions.Length + 1;
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
    }

    foreach (IMyTextPanel display in displays) {
        if (!display.CustomData.StartsWith(droneName)) {
            continue;
        }

        string[] customDataParts = display.CustomData.Split('_');
        if (customDataParts.Length == 2) {
            int screenChoice = Int32.Parse(display.CustomData.Split('_')[1]);
            display.WriteText(displayScreenText[screenChoice]);
        }
        else {
            Echo(display.CustomData);
        }
    }

    List<IMyCockpit> cockpits = new List<IMyCockpit>();
    GridTerminalSystem.GetBlocksOfType<IMyCockpit>(cockpits);
    foreach (IMyCockpit cockpit in cockpits) {
        if (cockpit.CustomData != droneName) {
            continue;
        }

        for (int c = 0; c < cockpit.SurfaceCount; c++){
            IMyTextSurface display = cockpit.GetSurface(c);
            display.WriteText(displayScreenText[displayScreen]);
        }
    }

    if (argument == null || argument.Trim() == "" && lastMessageData != "") {
        foreach (IMyTextPanel display in displays) {
            if (display.CustomData != "action_sequence") {
                continue;
            }

            string actionSequenceDisplayText = display.GetText();
            if (actionSequenceDisplayText.Split('\n')[0] == Me.CustomData + " action_sequence") {
                List<string> actionSequence = new List<string>(actionSequenceDisplayText.Split('\n'));
                if (actionSequence.Count < 2) {
                    continue;
                }
                if (rawTelemetry.actionStatus != "" && rawTelemetry.actionStatus != "CN") {
                    continue;
                }
                argument = actionSequence[1];
                setActionLog(droneName, argument, displays);
                actionSequence.RemoveAt(1);
                display.WriteText(String.Join("\n", actionSequence.ToArray()));
            }
        }
    }

    if (argument == null || argument.Trim() == "") {
        Echo("No argument and no action_sequence, stopping");
        return;
    }

    string message = "";
    bool sendMessage = false;

    string action = "";
    string value = "";

    string[] splitArgument = argument.Split(' ');
    if (splitArgument.Length >= 1) {
        action = splitArgument[0];
    }

    if (splitArgument.Length >= 2) {
        value = splitArgument[1];
    }

    if (action == "waypoint") {
        message = "waypoint";

        if (value != "home") {
            message += " " + value.Split(':')[2];
            message += "," + value.Split(':')[3];
            message += "," + value.Split(':')[4];
            message += "," + value.Split(':')[1];
        }
        else {    
            foreach (IMyTextPanel display in displays) {
                if (display.CustomData != "Dock1") {
                    continue;
                }
                Vector3D tempVector = Vector3D.Forward * 100;
                Vector3D waypoint = Vector3D.Transform(Vector3D.Right * 100, display.WorldMatrix);
                message += " " + waypoint.X;
                message += "," + waypoint.Y;
                message += "," + waypoint.Z;
                message += ",Dock1";
            }
        }
        sendMessage = true;
    }

    if (action == "undock") {
        message = getUndockMessage(displays);
        sendMessage = true;
    }

    if (action == "dock") {
        foreach (IMyTextPanel display in displays) {
            if (display.CustomData != "Dock1") {
                continue;
            }
            
            Vector3D startWorldPosition = Vector3D.Transform(Vector3D.Zero, Me.WorldMatrix);
            Vector3D endDownWorldPosition = Vector3D.Transform(Vector3D.Down, Me.WorldMatrix);
            Vector3D endForwardWorldPosition = Vector3D.Transform(Vector3D.Forward, Me.WorldMatrix);
            Vector3D downWorldDirection = Vector3D.Subtract(endDownWorldPosition, startWorldPosition);
            Vector3D forwardWorldDirection = Vector3D.Subtract(endForwardWorldPosition, startWorldPosition);
            downWorldDirection.Normalize();
            forwardWorldDirection.Normalize();
            message = "dock";
            message += " " + downWorldDirection.X.ToString();
            message += "," + downWorldDirection.Y.ToString();
            message += "," + downWorldDirection.Z.ToString();
            message += "," + forwardWorldDirection.X.ToString();
            message += "," + forwardWorldDirection.Y.ToString();
            message += "," + forwardWorldDirection.Z.ToString();
            sendMessage = true;
        }
    }

    if (action == "disable_autopilot") {
        message = "disable_autopilot";
        sendMessage = true;
    }

    if (action == "low_power") {
        message = "low_power";
        sendMessage = true;
    }

    if (action == "mine") {
        message = "mine";
        sendMessage = true;
    }

    if (action == "orientation") {
        message = "orientation";

        if (value == "home") {
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

        if (value == "natural_gravity") {
            message += " natural_gravity";
        }
        sendMessage = true;
    }

    if (action == "next_drone") {
        selectedOption = "";
        displayScreenOption = 0;
        activeDrone++;
        if (activeDrone >= droneList.Length) {
            activeDrone = 0;
        }
        selectedDrone = droneList[activeDrone];
    }

    if (action == "next_screen") {
        selectedOption = "";
        displayScreenOption = 0;
        displayScreen++;
        if (displayScreen >= maxDisplayScreens) {
            displayScreen = 1;
        }
    }

    if (action == "next_option") {
        displayScreenOption++;
        if (displayScreenOption >= maxDisplayScreenOptions) {
            displayScreenOption = 0;
        }
    }

    if (action == "select_option") {
        if (displayScreen == 3) {
            if (selectedOption == "Clear Seq") {
                clearActionSequenceDisplay(droneName, displays);
            }
            
            if (selectedOption == "Undock") {
                message = getUndockMessage(displays);
                sendMessage = true;
            }

            if (selectedOption == "Mine") {
                message = "mine";
                sendMessage = true;
            }
        }
        if (displayScreen == 4) {
            foreach (IMyTextPanel display in displays) {
                if (display.CustomData != "action_sequence") {
                    continue;
                }

                string actionSequenceDisplayText = display.GetText();
                if (actionSequenceDisplayText.Split('\n')[0] == Me.CustomData + " action_sequence") {
                    display.WriteText(Me.CustomData + " action_sequence\n" + getActionSequenceFromDisplay(selectedOption, displays));
                }
            }
        }
    }

    if (!sendMessage) {
        Echo("No message to send");
        return;
    }

    string messageTag = droneList[activeDrone];
    Echo("Outbound Message - " + messageTag);
    Echo(message);
    IGC.SendBroadcastMessage(messageTag, message, TransmissionDistance.AntennaRelay);
}
