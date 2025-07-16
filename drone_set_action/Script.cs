IMyBroadcastListener broadcastListener;
string messageToTag = "";
string messageFromTag = "";
int displayScreen = 1;
int maxDisplayScreens = 5;
int displayScreenOption = 0;
int maxDisplayScreenOptions = 0;
string selectedOption;
string[] displayScreenTitles = { "Compact", "Dbg", "Gen", "Cmd", "Seq" };
int droneMaxLogSize = 15;
string[] droneList = { "Drone1", "Drone2" }
string selectedDrone = "";

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

    messageToTag = Me.CustomData + "_to";
    messageFromTag = Me.CustomData + "_from";

    broadcastListener = IGC.RegisterBroadcastListener(messageFromTag);
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
    displayScreenText[0] = getDisplayHeader(0, droneName);
    for (int i = 1; i < maxDisplayScreens; i++) {
        displayScreenText[i] = getDisplayHeader(i, droneName);
    }

    string lastMessageData = "";
    while (broadcastListener.HasPendingMessage) {
        MyIGCMessage igcMessage = broadcastListener.AcceptMessage();
        if (igcMessage.Tag == messageFromTag) {
            Echo("Reading broadcast message");
            lastMessageData = igcMessage.Data.ToString();
        }
    }

    List<IMyTextPanel> displays = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);

    string actionStatus = "";
    if (lastMessageData != null && lastMessageData != "") {
        Vector3D homePosition = Me.GetPosition();
        string[] messageData = lastMessageData.Split(',');

        string inventoryStatus = messageData[3];
        string energyStatus = messageData[4];
        actionStatus = messageData[5];
        string drillStatus = messageData[6];
        string downSensorCloseStatus = messageData[7];
        string powerStatus = messageData[8];
        string rollAngleStatus = messageData[9];
        if (rollAngleStatus != "?") {
            double rollAngle = Math.Round(Convert.ToDouble(rollAngleStatus), 0, MidpointRounding.AwayFromZero);
            rollAngleStatus = rollAngle.ToString();
        }
        string pitchAngleStatus = messageData[10];
        if (pitchAngleStatus != "?") {
            double pitchAngle = Math.Round(Convert.ToDouble(pitchAngleStatus), 0, MidpointRounding.AwayFromZero);
            pitchAngleStatus = pitchAngle.ToString();
        }

        string yawAngleStatus = messageData[16];
        if (yawAngleStatus != "?") {
            double yawAngle = Math.Round(Convert.ToDouble(yawAngleStatus), 0, MidpointRounding.AwayFromZero);
            yawAngleStatus = yawAngle.ToString();
        }

        string downSensorDynamicStatus = messageData[11];
        string shipMaxVelocityStatus = messageData[12];
        if (shipMaxVelocityStatus == "" || shipMaxVelocityStatus == "?") {
            shipMaxVelocityStatus = "0";
        }
        shipMaxVelocityStatus = Math.Round(Convert.ToDouble(shipMaxVelocityStatus), 0, MidpointRounding.AwayFromZero).ToString();
        string shipVelocityStatus = messageData[13];
        if (shipVelocityStatus == "" || shipVelocityStatus == "?") {
            shipVelocityStatus = "0";
        }
        shipVelocityStatus = Math.Round(Convert.ToDouble(shipVelocityStatus), 0, MidpointRounding.AwayFromZero).ToString();

        string downSensorStatus = downSensorDynamicStatus;
        string waypointNameStatus = messageData[14];
        string orientationStatus = messageData[15];

        Vector3D dronePosition;
        dronePosition.X = Convert.ToDouble(messageData[0]);
        dronePosition.Y = Convert.ToDouble(messageData[1]);
        dronePosition.Z = Convert.ToDouble(messageData[2]);
        double distance = Math.Round(Vector3D.Distance(homePosition, dronePosition) / 1000, 1, MidpointRounding.AwayFromZero);
       
        displayScreenText[0] += "\nI " + inventoryStatus;
        displayScreenText[0] += "\nE " + energyStatus;
        displayScreenText[0] += "\nD " + distance.ToString();
        displayScreenText[0] += "\nA " + actionStatus;

        for (int i = 1; i < maxDisplayScreens; i++) {
            displayScreenText[i] += "\nI:" + inventoryStatus.PadRight(4);
            displayScreenText[i] += "E:" + energyStatus.PadRight(4);
            displayScreenText[i] += "D:" + distance.ToString().PadRight(4);
            displayScreenText[i] += "A:" + actionStatus;
        }

        displayScreenText[1] += "\nOrientation: " + orientationStatus;
        displayScreenText[1] += "\nVelocity: " + shipVelocityStatus + "/" + shipMaxVelocityStatus;
        displayScreenText[1] += "\nRoll: " + rollAngleStatus;
        displayScreenText[1] += "\nPitch: " + pitchAngleStatus;
        displayScreenText[1] += "\nYaw: " + yawAngleStatus;

        displayScreenText[2] += "\nWaypoint: " + waypointNameStatus;
        displayScreenText[2] += "\nDrill: " + drillStatus;
        displayScreenText[2] += "\nDrill Sensor: " + downSensorCloseStatus;
        displayScreenText[2] += "\nVelocity: " + shipVelocityStatus + "/" + shipMaxVelocityStatus;
        displayScreenText[2] += "\nSensor: " + downSensorStatus;

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
                if (actionStatus != "" && actionStatus != "CN") {
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

    Echo("Outbound Message - " + messageToTag);
    Echo(message);
    IGC.SendBroadcastMessage(messageToTag, message, TransmissionDistance.AntennaRelay);
}
