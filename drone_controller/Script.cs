﻿IMyBroadcastListener broadcastListener;
string messageToDroneControllerTag = "drone_controller";
int displayScreenOption = 0;
int maxDisplayScreenOptions = 0;
string selectedOption;
int droneMaxLogSize = 15;
string selectedDrone = "";

public struct droneTelemetry {
    public string droneName { get; set; }
    public string inventoryStatus { get; set; }
    public string energyStatus { get; set; }
    public string actionStatus { get; set; }
    public string drillStatus { get; set; }
    public string downSensorCloseStatus { get; set; }
    public string powerStatus { get; set; }
    public string rollAngleStatus { get; set; }
    public string pitchAngleStatus { get; set; }
    public string yawAngleStatus { get; set; }
    public string downSensorDynamicStatus { get; set; }
    public string shipMaxVelocityStatus { get; set; }
    public string shipVelocityStatus { get; set; }
    public string waypointNameStatus { get; set; }
    public string orientationStatus { get; set; }
    public string positionX { get; set; }
    public string positionY { get; set; }
    public string positionZ { get; set; }
    public string waypointX { get; set; }
    public string waypointY { get; set; }
    public string waypointZ { get; set; }
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
    messageTelemetry.waypointX = messageData[18];
    messageTelemetry.waypointY = messageData[19];
    messageTelemetry.waypointZ = messageData[20];
    messageTelemetry.wasActedOn = false;

    return messageTelemetry;
}

public droneStatus getDroneStatus (string droneName, List<droneStatus> drones) {
    droneStatus status = new droneStatus();
    foreach (droneStatus drone in drones) {
        if (drone.droneName == droneName) {
            status = drone;
        }
    }
    return status;
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

        foreach (IMyTextPanel display in displays) {
            if (display.CustomData != "Dock" + droneName[droneName.Length - 1]) {
                continue;
            }
            Vector3D waypoint = Vector3D.Transform(Vector3D.Right, display.WorldMatrix);
            message += "," + waypoint.X.ToString();
            message += "," + waypoint.Y.ToString();
            message += "," + waypoint.Z.ToString();
        }

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

    if (command.name == "unload") {
        return new droneMessage(droneName, "unload " + command.value);
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

public List<MyWaypointInfo> getWaypointLibrary (string controllerName) {
    List<MyWaypointInfo> waypoints = new List<MyWaypointInfo>();

    List<IMyRemoteControl> remoteControls = new List<IMyRemoteControl>();
    GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(remoteControls);

    foreach (IMyRemoteControl remoteControl in remoteControls) {
        if (remoteControl.CustomData != controllerName) {
            continue;
        }
        remoteControl.GetWaypointInfo(waypoints);
    }
    return waypoints;
}

public Vector2 getMapPosition (Vector3D position, int metersPerPixel, float mapRatio, RectangleF viewport) {
    Vector3D homePosition = Me.GetPosition();
    Vector3D relativeDifference = position - homePosition;
    Vector3D bodyPosition = Vector3D.TransformNormal(relativeDifference, MatrixD.Transpose(Me.WorldMatrix));
    float offsetX = (float)bodyPosition.X / metersPerPixel;
    float offsetY = (float)bodyPosition.Z / metersPerPixel;
    Vector2 mapPosition = new Vector2((viewport.Center.X * mapRatio) + offsetX, (viewport.Center.Y * mapRatio) + offsetY);

    return mapPosition;
}

public void guiScreen (IMyTextPanel display, List<droneTelemetry> drones, List<droneStatus> statuses, List<IMyTextPanel> displays, string selectedDrone, float antennaRadius, string controllerName) {
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

    int logWidth = 23;

    string mapRange = ((int)antennaRadius / 1000).ToString();
    int desiredMapWidth = (int)((double)viewport.Width / 3.5);
    int metersPerPixel = (int)antennaRadius / desiredMapWidth;
    float mapRatio = 1.40f;

    var frame = display.DrawFrame();
    Vector2 position = new Vector2(10, 10);
    MySprite sprite = new MySprite() {
        Type = SpriteType.TEXT,
        Data = DateTime.UtcNow.ToString(@"hh\:mm\:ss"),
        Position = position,
        RotationOrScale = .75f,
        Color = Color.White,
        Alignment = TextAlignment.LEFT,
        FontId = "White"
    };
    frame.Add(sprite);

    sprite = new MySprite() {
        Type = SpriteType.TEXTURE,
        Data = "Grid",
        Position = new Vector2(viewport.Center.X * mapRatio, viewport.Center.Y * mapRatio),
        Size = new Vector2(desiredMapWidth * 2, desiredMapWidth * 2),
        Color = drawingSurface.ScriptBackgroundColor.Alpha(1f),
        Alignment = TextAlignment.CENTER
    };
    frame.Add(sprite);

    sprite = new MySprite() {
        Type = SpriteType.TEXTURE,
        Data = "Cross",
        Position = new Vector2(viewport.Center.X * mapRatio, viewport.Center.Y * mapRatio),
        Size = new Vector2(20, 20),
        Color = drawingSurface.ScriptForegroundColor.Alpha(1f),
        Alignment = TextAlignment.CENTER
    };
    frame.Add(sprite);

    sprite = new MySprite() {
        Type = SpriteType.TEXT,
        Data = ". -" + mapRange + "k, -" + mapRange + "k",
        Position = new Vector2(viewport.Center.X * mapRatio - desiredMapWidth, viewport.Center.Y * mapRatio - desiredMapWidth - 10),
        RotationOrScale = 0.5f,
        Color = Color.DarkGray,
        Alignment = TextAlignment.LEFT,
        FontId = "White"
    };
    frame.Add(sprite);

    sprite = new MySprite() {
        Type = SpriteType.TEXT,
        Data = "+" + mapRange + "k, -" + mapRange + "k .",
        Position = new Vector2(viewport.Center.X * mapRatio + desiredMapWidth, viewport.Center.Y * mapRatio - desiredMapWidth - 10),
        RotationOrScale = 0.5f,
        Color = Color.DarkGray,
        Alignment = TextAlignment.RIGHT,
        FontId = "White"
    };
    frame.Add(sprite);

    sprite = new MySprite() {
        Type = SpriteType.TEXT,
        Data = ". -" + mapRange + "k, +" + mapRange + "k",
        Position = new Vector2(viewport.Center.X * mapRatio - desiredMapWidth, viewport.Center.Y * mapRatio + desiredMapWidth - 10),
        RotationOrScale = 0.5f,
        Color = Color.DarkGray,
        Alignment = TextAlignment.LEFT,
        FontId = "White"
    };
    frame.Add(sprite);

    sprite = new MySprite() {
        Type = SpriteType.TEXT,
        Data = "+" + mapRange + "k, +" + mapRange + "k .",
        Position = new Vector2(viewport.Center.X * mapRatio + desiredMapWidth, viewport.Center.Y * mapRatio + desiredMapWidth - 10),
        RotationOrScale = 0.5f,
        Color = Color.DarkGray,
        Alignment = TextAlignment.RIGHT,
        FontId = "White"
    };
    frame.Add(sprite);

    foreach (MyWaypointInfo waypoint in getWaypointLibrary(controllerName)) {
        Vector2 waypointMapPosition = getMapPosition(waypoint.Coords, metersPerPixel, mapRatio, viewport);
        if (waypointMapPosition.X < viewport.Center.X * mapRatio - desiredMapWidth || waypointMapPosition.Y < viewport.Center.Y * mapRatio - desiredMapWidth) {
            continue;
        }
        sprite = new MySprite() {
            Type = SpriteType.TEXTURE,
            Data = "Cross",
            Position = waypointMapPosition,
            Size = new Vector2(15, 15),
            Color = drawingSurface.ScriptForegroundColor.Alpha(0.5f),
            Alignment = TextAlignment.CENTER
        };
        frame.Add(sprite);

        string waypointName = waypoint.Name;
        if (waypointName.Length > 8) {
            waypointName = waypointName.Substring(0, 8) + "...";
        }
        sprite = new MySprite() {
            Type = SpriteType.TEXT,
            Data = waypointName,
            Position = waypointMapPosition,
            RotationOrScale = 0.5f,
            Color = Color.DarkGray,
            Alignment = TextAlignment.LEFT,
            FontId = "White"
        };
        frame.Add(sprite);  
    } 

    position += new Vector2(0, 20);

    foreach (droneTelemetry drone in drones) {
        Vector3D dronePosition;
        dronePosition.X = Convert.ToDouble(drone.positionX);
        dronePosition.Y = Convert.ToDouble(drone.positionY);
        dronePosition.Z = Convert.ToDouble(drone.positionZ);

        Vector3D homePosition = Me.GetPosition();
        Vector3D relativeDifference = dronePosition - homePosition;
        Vector3D bodyPosition = Vector3D.TransformNormal(relativeDifference, MatrixD.Transpose(Me.WorldMatrix));

        int offsetX = (int)bodyPosition.X / metersPerPixel;
        int offsetY = (int)bodyPosition.Z / metersPerPixel;
        float rotation = (float)bodyPosition.Y / 1.5f;
        if (rotation > 90) {
            rotation = 90;
        }
        if (rotation < -90) {
            rotation = 90;
        }
        rotation += 90 * -1;
        Vector2 droneMapPosition = new Vector2((viewport.Center.X * mapRatio) + offsetX, (viewport.Center.Y * mapRatio) + offsetY);
        if (drone.actionStatus != "CN") {
            sprite = new MySprite() {
                Type = SpriteType.TEXTURE,
                Data = "Arrow",
                Position = droneMapPosition,
                Size = new Vector2(25, 25),
                RotationOrScale = rotation * ((float)Math.PI / 180),
                Color = drawingSurface.ScriptForegroundColor.Alpha(1f),
                Alignment = TextAlignment.CENTER
            };
            if (drone.drillStatus == "On") {
                sprite.Data = "Danger";
                sprite.RotationOrScale = 0;
            }
            if (drone.droneName == selectedDrone) {
                sprite.Size = new Vector2(30, 30);
            }
            frame.Add(sprite);

            sprite = new MySprite() {
                Type = SpriteType.TEXT,
                Data = drone.shipVelocityStatus + "/" + drone.shipMaxVelocityStatus,
                Position = droneMapPosition - new Vector2(15, 15),
                RotationOrScale = 0.5f,
                Color = Color.DarkGray,
                Alignment = TextAlignment.RIGHT,
                FontId = "White"
            };
            if (drone.droneName == selectedDrone) {
                sprite.Color = Color.White;
            }
            frame.Add(sprite);

            sprite = new MySprite() {
                Type = SpriteType.TEXT,
                Data = drone.downSensorDynamicStatus,
                Position = droneMapPosition + new Vector2(0, 10),
                RotationOrScale = 0.5f,
                Color = Color.DarkGray,
                Alignment = TextAlignment.CENTER,
                FontId = "White"
            };
            if (drone.droneName == selectedDrone) {
                sprite.Color = Color.White;
            }
            frame.Add(sprite);

            sprite = new MySprite() {
                Type = SpriteType.TEXT,
                Data = drone.droneName,
                Position = droneMapPosition - new Vector2(0, 30),
                RotationOrScale = 0.5f,
                Color = Color.DarkGray,
                Alignment = TextAlignment.CENTER,
                FontId = "White"
            };
            if (drone.droneName == selectedDrone) {
                sprite.Color = Color.White;
            }
            frame.Add(sprite);

            sprite = new MySprite() {
                Type = SpriteType.TEXT,
                Data = drone.actionStatus,
                Position = droneMapPosition + new Vector2(15, -15),
                RotationOrScale = 0.5f,
                Color = Color.DarkGray,
                Alignment = TextAlignment.LEFT,
                FontId = "White"
            };
            if (drone.droneName == selectedDrone) {
                sprite.Color = Color.White;
            }
            frame.Add(sprite);

            sprite = new MySprite() {
                Type = SpriteType.TEXT,
                Data = "I " + drone.inventoryStatus,
                Position = droneMapPosition + new Vector2(15, 0),
                RotationOrScale = 0.5f,
                Color = Color.DarkGray,
                Alignment = TextAlignment.LEFT,
                FontId = "White"
            };
            if (drone.droneName == selectedDrone) {
                sprite.Color = Color.White;
            }
            frame.Add(sprite);
        }

        if (drone.actionStatus == "WP") { // && drone.droneName == selectedDrone) {
            Vector3D waypointPosition = new Vector3D(Convert.ToDouble(drone.waypointX), Convert.ToDouble(drone.waypointY), Convert.ToDouble(drone.waypointZ));
            Vector2 waypointMapPosition = getMapPosition(waypointPosition, metersPerPixel, mapRatio, viewport);
            Vector2 halfMapPosition = Vector2.Lerp(droneMapPosition, waypointMapPosition, .25f);
            sprite = new MySprite() {
                Type = SpriteType.TEXTURE,
                Data = "Cross",
                Position = halfMapPosition,
                Size = new Vector2(7, 7),
                Color = drawingSurface.ScriptForegroundColor.Alpha(1f),
                Alignment = TextAlignment.CENTER
            };
            frame.Add(sprite);
            halfMapPosition = Vector2.Lerp(droneMapPosition, waypointMapPosition, .5f);
            sprite = new MySprite() {
                Type = SpriteType.TEXTURE,
                Data = "Cross",
                Position = halfMapPosition,
                Size = new Vector2(7, 7),
                Color = drawingSurface.ScriptForegroundColor.Alpha(1f),
                Alignment = TextAlignment.CENTER
            };
            frame.Add(sprite);
            halfMapPosition = Vector2.Lerp(droneMapPosition, waypointMapPosition, .75f);
            sprite = new MySprite() {
                Type = SpriteType.TEXTURE,
                Data = "Cross",
                Position = halfMapPosition,
                Size = new Vector2(7, 7),
                Color = drawingSurface.ScriptForegroundColor.Alpha(1f),
                Alignment = TextAlignment.CENTER
            };
            frame.Add(sprite);
        }

        string boxText =  drone.droneName;
        boxText += "\nE " + drone.energyStatus;
        boxText += "\nI " + drone.inventoryStatus;

        sprite = new MySprite() {
            Type = SpriteType.TEXT,
            Data = boxText,
            Position = position,
            RotationOrScale = 0.5f,
            Color = Color.DarkGray,
            Alignment = TextAlignment.LEFT,
            FontId = "White"
        };
        if (drone.droneName == selectedDrone) {
            sprite.Color = Color.White;
        }
        frame.Add(sprite);
        position += new Vector2(80, 0);
    }

    position = new Vector2(10, 80);
    sprite = new MySprite() {
        Type = SpriteType.TEXT,
        Data = "Command Queue",
        Position = position,
        RotationOrScale = 0.75f,
        Color = Color.White,
        Alignment = TextAlignment.LEFT,
        FontId = "White"
    };
    frame.Add(sprite);

    int counter = 0;
    droneStatus status = getDroneStatus(selectedDrone, statuses);
    position += new Vector2(0, 20);
    if (status.commandSequence != null) {
        foreach (string command in status.commandSequence.Split('\n')) {
            if (counter > 4) {
                break;
            }
            string adjusted = command;
            if (adjusted.Length > logWidth) {
                adjusted = adjusted.Substring(0, logWidth - 3) + "...";
            }
            sprite = new MySprite() {
                Type = SpriteType.TEXT,
                Data = adjusted,
                Position = position,
                RotationOrScale = 0.75f,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = "White"
            };
            position += new Vector2(0, 20);
            frame.Add(sprite);
            counter++;
        }
    }

    position = new Vector2(viewport.Center.X + 10, 80);
    sprite = new MySprite() {
        Type = SpriteType.TEXT,
        Data = "Log",
        Position = position,
        RotationOrScale = 0.75f,
        Color = Color.White,
        Alignment = TextAlignment.LEFT,
        FontId = "White"
    };
    frame.Add(sprite);

    counter = 0;
    position += new Vector2(0, 20);
    if (status.commandLog != null) {
        foreach (string command in status.commandLog.Split('\n')) {
            if (counter > 4) {
                break;
            }
            string adjusted = command;
            if (adjusted.Length > logWidth) {
                adjusted = adjusted.Substring(0, logWidth - 3) + "...";
            }
            sprite = new MySprite() {
                Type = SpriteType.TEXT,
                Data = adjusted,
                Position = position,
                RotationOrScale = 0.75f,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = "White"
            };
            position += new Vector2(0, 20);
            frame.Add(sprite);
            counter++;
        }
    }

    List<string> commands = new List<string>(new[] { "Hold", "Undock", "Mine", "Ant Rng" });
    foreach (IMyTextPanel commandDisplay in displays) {
        if (commandDisplay.CustomData != "action_sequence") {
            continue;
        }
        string header = commandDisplay.GetText().Split('\n')[0];
        if (header.StartsWith("action_sequence ")) {
            commands.Add(header.Split(' ')[1]);
        }
    }
    maxDisplayScreenOptions = commands.Count;
    
    position = new Vector2(10, 280);
    sprite = new MySprite() {
        Type = SpriteType.TEXT,
        Data = "Commands",
        Position = position,
        RotationOrScale = 0.75f,
        Color = Color.White,
        Alignment = TextAlignment.LEFT,
        FontId = "White"
    };
    frame.Add(sprite);

    position += new Vector2(0, 20);
    int optionCounter = 0;
    foreach (string command in commands) {
        sprite = new MySprite() {
            Type = SpriteType.TEXT,
            Data = command,
            Position = position,
            RotationOrScale = 0.75f,
            Color = Color.DarkGray,
            Alignment = TextAlignment.LEFT,
            FontId = "White"
        };
        if (optionCounter == displayScreenOption) {
            selectedOption = command;
            sprite.Color = Color.White;
            sprite.RotationOrScale = 0.8f;
        }
        frame.Add(sprite);
        position += new Vector2(0, 20);
        optionCounter++;
    }

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

    while (broadcastListener.HasPendingMessage) {
        MyIGCMessage igcMessage = broadcastListener.AcceptMessage();
        if (igcMessage.Tag == messageToDroneControllerTag) {
            string messageData = igcMessage.Data.ToString();
            if (messageData != null && messageData != "") {
                setDroneTelemetry(messageToDroneTelemetry(messageData), droneTelemetryList);
            }
        }
    }

    float antennaRadius = 1000;
    List<IMyRadioAntenna> antennas = new List<IMyRadioAntenna>();
    GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(antennas);
    foreach (IMyRadioAntenna antenna in antennas) {
        if (antenna.CustomData != controllerName) {
            continue;
        }
        
        antennaRadius = antenna.Radius;
    }

    List<IMyTextPanel> displays = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(displays);

    foreach (IMyTextPanel display in displays) {
        if (!display.CustomData.StartsWith(controllerName)) {
            continue;
        }

        string[] customDataParts = display.CustomData.Split('_');
        if (customDataParts.Length == 2) {
            int screenChoice = Int32.Parse(display.CustomData.Split('_')[1]);
            if (screenChoice == 6) {
                guiScreen(display, droneTelemetryList, droneStatusList, displays, selectedDrone, antennaRadius, controllerName);
            }
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

    droneCommand command = new droneCommand(argument);
    droneMessage selectedDroneMessage = getDroneCommandMessage(selectedDrone, command, displays);
    if (selectedDroneMessage.message != null) {
        messages.Enqueue(selectedDroneMessage);
    }

    if (command.name == "next_drone") {
        selectedDrone = getNextDroneName(selectedDrone, droneTelemetryList);
    }

    if (command.name == "next_option") {
        displayScreenOption++;
        if (displayScreenOption >= maxDisplayScreenOptions) {
            displayScreenOption = 0;
        }
    }

    if (command.name == "select_option") {
        if (selectedOption == "Hold") {
            droneStatusList = clearCommandSequence(selectedDrone, droneStatusList);
            messages.Enqueue(new droneMessage(selectedDrone, "disable_autopilot"));
        }
        else if (selectedOption == "Undock") {
            messages.Enqueue(new droneMessage(selectedDrone, getUndockMessage(displays)));
        }
        else if (selectedOption == "Mine") {
            messages.Enqueue(new droneMessage(selectedDrone, "mine"));
        }
        else if (selectedOption == "Ant Rng") {
            foreach (IMyRadioAntenna antenna in antennas) {
                if (antenna.CustomData != controllerName) {
                    continue;
                }
                
                antenna.Radius += 1000;
                if (antenna.Radius > 5000) {
                    antenna.Radius = 1000;
                }
            }
        }
        else {
            droneStatusList = addCommandSequence(selectedDrone, droneStatusList, getCommandSequenceFromDisplay(selectedOption, displays));
        }
    }

    foreach (droneMessage message in messages) {
        IGC.SendBroadcastMessage(message.droneName, message.message, TransmissionDistance.AntennaRelay);
    }
}
