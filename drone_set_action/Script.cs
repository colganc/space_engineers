string messageToTag = "";

public Program()
{
    if (Me.CustomData == null || Me.CustomData.Trim() == "") {
        Echo("No custom data value, stopping");
        return;
    }

    messageToTag = Me.CustomData + "_to";
}

public void Main(string argument, UpdateType updateSource)
{
    if (Me.CustomData == null || Me.CustomData.Trim() == "") {
        Echo("No custom data value, stopping");
        return;
    }

    if (argument == null || argument.Trim() == "") {
        Echo("No argument, stopping");
        return;
    }

    string message = "";
    bool validAction = false;

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
            List<IMyTextPanel> displays = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);
            foreach (IMyTextPanel display in displays) {
                if (display.CustomData != "Dock1") {
                    continue;
                }
                Vector3D tempVector = Vector3D.Forward * 100;
                Echo(tempVector.X + " " + tempVector.Y + " " + tempVector.Z);
                // backward, no
                // down, no
                // forward, no
                // up, no
                // left, no
                Vector3D waypoint = Vector3D.Transform(Vector3D.Right * 100, display.WorldMatrix);
                message += " " + waypoint.X;
                message += "," + waypoint.Y;
                message += "," + waypoint.Z;
                message += ",Dock1";
            }
        }
        validAction = true;
    }

    if (action == "undock") {
        List<IMyTextPanel> displays = new List<IMyTextPanel>();
        GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);
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
            validAction = true;
        }
    }

    if (action == "dock") {
        List<IMyTextPanel> displays = new List<IMyTextPanel>();
        GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);
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
            validAction = true;
        }
    }

    if (action == "disable_autopilot") {
        message = "disable_autopilot";
        validAction = true;
    }

    if (action == "low_power") {
        message = "low_power";
        validAction = true;
    }

    if (action == "mine") {
        message = "mine";
        validAction = true;
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
        validAction = true;
    }

    if (action == "action_sequence") {
        List<IMyTextPanel> displays = new List<IMyTextPanel>();
        GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);
        foreach (IMyTextPanel display in displays) {
            if (display.CustomData != "action_sequence") {
                continue;
            }

            string actionSequence = display.GetText();
            Echo("Action Sequence:\n" + actionSequence);
            // if (actionSequence.Split('\n')[0] != "action_sequence " + value) {
            //     message = actionSequence.Replace("action_sequence " + value + "\n", "");
            //     validAction = true;
            // }
        }
    }

    if (!validAction) {
        Echo("No valid action, stopping");
        return;
    }

    Echo("Outbound Message - " + messageToTag);
    Echo(message);
    //Me.DisplayText(Me.CustomData + "\n" + message);
    IGC.SendBroadcastMessage(messageToTag, message, TransmissionDistance.AntennaRelay);
}
