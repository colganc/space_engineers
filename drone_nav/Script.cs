IMyBroadcastListener broadcastListener;
string messageToTag = "";
string messageToDroneControllerTag = "drone_controller";
string currentAction = "";
string droneName = "";
double degreesOffCenter = .1;
int rpmReduction = 16;
int safeMiningVelocity = 2;
string powerMode = "?";
float[] downSensorDynamicDistances = {50, 40, 30, 25, 20, 15, 12, 9, 7, 5};
bool[] downSensorDynamicDetections = new bool[10];
Vector3D orientDownTowards;
Vector3D orientForwardTowards;
bool useOrientTowards;
Vector3D dockingPosition;
Vector3D miningPosition;
float safeUpPercentage = .0001f;
int safeUndockVelocity = 4;
int safeDockVelocity = 2;
double safeDockVelocityVh = .2;
double metersOffCenter = .05;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    if (Me.CustomData == null || Me.CustomData.Trim() == "") {
        Echo("No custom data value, stopping");
        return;
    }

    droneName = Me.CustomData;
    messageToTag = Me.CustomData;
    broadcastListener = IGC.RegisterBroadcastListener(messageToTag);
}

public void checkConnector () {
    if (currentAction == "UD") {
        return;
    }

    List<IMyShipConnector> connector = new List<IMyShipConnector>();
    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(connector);
    for (int c = 0; c < connector.Count; c++) {
        if (connector[c].CustomData != droneName) {
            continue;
        }

        if (connector[c].Status == MyShipConnectorStatus.Connectable) {
            setPowerMode(false);
            connector[c].Connect();
        };

        if (connector[c].Status == MyShipConnectorStatus.Connected) {
            currentAction = "CN";
        };
    }

    return;
}

public void extendPistons () {
    List<IMyPistonBase> pistons = new List<IMyPistonBase>();
    GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(pistons);
    foreach (IMyPistonBase piston in pistons) {
        if (piston.CustomData != droneName) {
            continue;
        }
        piston.Extend();
    }
}

public void retractPistons () {
    List<IMyPistonBase> pistons = new List<IMyPistonBase>();
    GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(pistons);
    foreach (IMyPistonBase piston in pistons) {
        if (piston.CustomData != droneName) {
            continue;
        }
        piston.Retract();
    }
}

public void setPowerMode (bool mode) {
    List<IMyThrust> thrust = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrust);
    for (int c = 0; c < thrust.Count; c++) {
        if (thrust[c].CustomData != droneName) {
            continue;
        }
        thrust[c].Enabled = mode;
    }

    List<IMyGyro> gyro = new List<IMyGyro>();
    GridTerminalSystem.GetBlocksOfType<IMyGyro>(gyro);
    for (int c = 0; c < gyro.Count; c++) {
        if (gyro[c].CustomData != droneName) {
            continue;
        }
        gyro[c].Enabled = mode;
    }

    List<IMyLightingBlock> lighting = new List<IMyLightingBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(lighting);
    for (int c = 0; c < lighting.Count; c++) {
        if (lighting[c].CustomData != droneName) {
            continue;
        }
        lighting[c].Enabled = mode;
    }

    List<IMySensorBlock> sensor = new List<IMySensorBlock>();
    GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(sensor);
    for (int c = 0; c < sensor.Count; c++) {
        if (sensor[c].CustomData != droneName) {
            continue;
        }
        sensor[c].Enabled = mode;
    }

    List<IMyOreDetector> oreDetectors = new List<IMyOreDetector>();
    GridTerminalSystem.GetBlocksOfType<IMyOreDetector>(oreDetectors);
    foreach (IMyOreDetector oreDetector in oreDetectors) {
        if (oreDetector.CustomData != droneName) {
            continue;
        }
        oreDetector.Enabled = mode;
    }

    List<IMyRadioAntenna> antennas = new List<IMyRadioAntenna>();
    GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(antennas);
    foreach (IMyRadioAntenna antenna in antennas) {
        if (antenna.CustomData != droneName) {
            continue;
        }

        if (mode == false) {
            antenna.Radius = 500;
        }
        else {
            antenna.Radius = 5000;
        }
    }

    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);
    foreach (IMyBatteryBlock battery in batteries) {
        if (battery.CustomData != droneName) {
            continue;
        }

        if (mode == false) {
            battery.ChargeMode = ChargeMode.Recharge;
        }
        else {
            battery.ChargeMode = ChargeMode.Auto;
        }
    }

    if (mode == false) {
        powerMode = "Minimal";
    }
    else {
        powerMode = "All";
    }

    return;
}


public void changeUpThrustersThrustOverride (float thrustInPercent) {
    List<IMyThrust> thrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
    foreach (IMyThrust thruster in thrusters) {
        if (thruster.CustomData != droneName || thruster.GridThrustDirection != Vector3I.Down) {
            continue;
        }
        thruster.ThrustOverridePercentage = thruster.CurrentThrustPercentage + thrustInPercent;
    }
}

public void disableUpThrustersThrustOverride () {
    List<IMyThrust> thrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
    foreach (IMyThrust thruster in thrusters) {
        if (thruster.CustomData != droneName || thruster.GridThrustDirection != Vector3I.Down) {
            continue;
        }
        thruster.ThrustOverride = 0;
    }
}

public void setDownThrustersThrustOverride (float thrustInNewtons) {
    List<IMyThrust> thrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
    foreach (IMyThrust thruster in thrusters) {
        if (thruster.CustomData != droneName || thruster.GridThrustDirection != Vector3I.Up) {
            continue;
        }
        thruster.ThrustOverride = thrustInNewtons;
    }
}

public void setForwardThrustersThrustOverride (float thrustInNewtons) {
    List<IMyThrust> thrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
    foreach (IMyThrust thruster in thrusters) {
        if (thruster.CustomData != droneName || thruster.GridThrustDirection != Vector3I.Backward) {
            continue;
        }
        thruster.ThrustOverride = thrustInNewtons;
    }
}

public void setBackwardThrustersThrustOverride (float thrustInNewtons) {
    List<IMyThrust> thrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
    foreach (IMyThrust thruster in thrusters) {
        if (thruster.CustomData != droneName || thruster.GridThrustDirection != Vector3I.Forward) {
            continue;
        }
        thruster.ThrustOverride = thrustInNewtons;
    }
}

public void setLeftThrustersThrustOverride (float thrustInNewtons) {
    List<IMyThrust> thrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
    foreach (IMyThrust thruster in thrusters) {
        if (thruster.CustomData != droneName || thruster.GridThrustDirection != Vector3I.Right) {
            continue;
        }
        thruster.ThrustOverride = thrustInNewtons;
    }
}

public void setRightThrustersThrustOverride (float thrustInNewtons) {
    List<IMyThrust> thrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(thrusters);
    foreach (IMyThrust thruster in thrusters) {
        if (thruster.CustomData != droneName || thruster.GridThrustDirection != Vector3I.Left) {
            continue;
        }
        thruster.ThrustOverride = thrustInNewtons;
    }
}

public void setGyroscopeOverride (bool gyroOverride) {
    List<IMyGyro> gyros = new List<IMyGyro>();
    GridTerminalSystem.GetBlocksOfType<IMyGyro>(gyros);
    foreach (IMyGyro gyro in gyros) {
            if (gyro.CustomData != droneName) {
                continue;
            }
            gyro.GyroOverride = gyroOverride;
        }
}

public Vector3D getPosition() {

    List<IMyRemoteControl> remoteControls = new List<IMyRemoteControl>();
    GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(remoteControls);

    Vector3D worldPosition = new Vector3D();
    foreach (IMyRemoteControl remoteControl in remoteControls) {
        if (remoteControl.CustomData != droneName) {
            continue;
        }
        worldPosition = remoteControl.GetPosition();
    }
    return worldPosition;
}

public void setRemoteControlsAutoPilotEnabled (bool enabled) {
    List<IMyRemoteControl> remoteControls = new List<IMyRemoteControl>();
    GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(remoteControls);

    foreach (IMyRemoteControl remoteControl in remoteControls) {
        if (remoteControl.CustomData != droneName) {
            continue;
        }

        remoteControl.SetAutoPilotEnabled(enabled);
    }
}

public void setDrillsPower (bool enabled) {
    List<IMyShipDrill> drills = new List<IMyShipDrill>();
    GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(drills);
    foreach (IMyShipDrill drill in drills) {
        if (drill.CustomData != droneName) {
            continue;
        }
        drill.Enabled = enabled;
    }

    List<IMyMotorAdvancedStator> rotors = new List<IMyMotorAdvancedStator>();
    GridTerminalSystem.GetBlocksOfType<IMyMotorAdvancedStator>(rotors);
    foreach (IMyMotorAdvancedStator rotor in rotors) {
        if (rotor.CustomData != droneName) {
            continue;
        }

        if (enabled) {
            rotor.LowerLimitDeg = -361;
            rotor.UpperLimitDeg = 361;
        }
        else {
            rotor.LowerLimitDeg = 90;
            rotor.UpperLimitDeg = 90;
        }
    }
}

public void setDynamicSensorForDocking () {
    // List<IMySensorBlock> sensors = new List<IMySensorBlock>();
    // GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(sensors);

    // foreach (IMySensorBlock sensor in sensors) {
    //     if (sensor.CustomData != droneName) {
    //         continue;
    //     }
        
    //     if (sensor.FrontExtend >= 5) {
    //         sensor.DetectLargeShips = true;
    //         sensor.DetectSmallShips = true;
    //     }
    // }
}

public void setDynamicSensorForMining () {
    // List<IMySensorBlock> sensors = new List<IMySensorBlock>();
    // GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(sensors);

    // foreach (IMySensorBlock sensor in sensors) {
    //     if (sensor.CustomData != droneName) {
    //         continue;
    //     }
        
    //     if (sensor.FrontExtend >= 5) {
    //         sensor.DetectLargeShips = false;
    //         sensor.DetectSmallShips = false;
    //     }
    // }
}


public bool isInventoryFull () {
    List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers);

    long currentVolume = 0;
    long maxVolume = 0;

    foreach (IMyCargoContainer cargoContainer in cargoContainers) {
        if (cargoContainer.CustomData != droneName) {
            continue;
        }

        IMyInventory cargoContainerInventory = cargoContainer.GetInventory();
        currentVolume += cargoContainerInventory.CurrentVolume.RawValue;
        maxVolume += cargoContainerInventory.MaxVolume.RawValue;
    }

    if (currentVolume < maxVolume) {
        return false;
    }
    return true;
}

public void setOrientation (string value) {
    if (value == "natural_gravity") {
        useOrientTowards = false;
    }
    else {
        useOrientTowards = true;
        orientDownTowards.X = Convert.ToDouble(value.Split(',')[0]);
        orientDownTowards.Y = Convert.ToDouble(value.Split(',')[1]);
        orientDownTowards.Z = Convert.ToDouble(value.Split(',')[2]);
        orientForwardTowards.X = Convert.ToDouble(value.Split(',')[3]);
        orientForwardTowards.Y = Convert.ToDouble(value.Split(',')[4]);
        orientForwardTowards.Z = Convert.ToDouble(value.Split(',')[5]);
    }
}

public void setActionWaypoint(string value) {
    currentAction = "WP";

    setGyroscopeOverride(false);

    List<IMyRemoteControl> remoteControls = new List<IMyRemoteControl>();
    GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(remoteControls);

    for (int c = 0; c < remoteControls.Count; c++) {
        if (remoteControls[c].CustomData != droneName) {
            continue;
        }

        remoteControls[c].ClearWaypoints();
        double waypointX = Convert.ToDouble(value.Split(',')[0]);
        double waypointY = Convert.ToDouble(value.Split(',')[1]);
        double waypointZ = Convert.ToDouble(value.Split(',')[2]);
        string waypointName = value.Split(',')[3];
        Vector3D waypoint = new Vector3D(waypointX, waypointY, waypointZ);
        remoteControls[c].AddWaypoint(waypoint, waypointName);
        remoteControls[c].SetAutoPilotEnabled(true);
    }
}

public void setActionMine() {
    currentAction = "MN";

    setDynamicSensorForMining();
    setGyroscopeOverride(true);
    setRemoteControlsAutoPilotEnabled(false);
    miningPosition = getPosition();
}

public void setActionDock(string value) {
    currentAction = "DK";
    extendPistons();
    setRemoteControlsAutoPilotEnabled(false);
    setGyroscopeOverride(true);
    setOrientation(value);
    setDynamicSensorForDocking();
    dockingPosition = new Vector3D(Convert.ToDouble(value.Split(',')[6]), Convert.ToDouble(value.Split(',')[7]), Convert.ToDouble(value.Split(',')[8]));
}

public void setActionUndock(string value) {
    currentAction = "UD";

    setOrientation(value);
    dockingPosition = getPosition();
    setRemoteControlsAutoPilotEnabled(false);
    setGyroscopeOverride(true);
    setPowerMode(true);

    List<IMyShipConnector> connectors = new List<IMyShipConnector>();
    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(connectors);
    foreach (IMyShipConnector connector in connectors) {
        if (connector.CustomData != droneName) {
            continue;
        }
        connector.Disconnect();
    }
}

public string receiveToMessages () {
    string lastMessageData = "";
    while (broadcastListener.HasPendingMessage) {
        MyIGCMessage igcMessage = broadcastListener.AcceptMessage();
        if (igcMessage.Tag == messageToTag) {
            Echo("Reading broadcast message");
            lastMessageData = igcMessage.Data.ToString();
        }
    }
    return lastMessageData;
}

public void processActionRequest (string message) {
    if (message == "") {
        return;
    }

    string action = "";
    string value = "";

    string[] splitMessage = message.Split('\n')[0].Split(' ');
    if (splitMessage.Length >= 1) {
        action = splitMessage[0];
    }

    if (splitMessage.Length >= 2) {
        value = splitMessage[1];
    }

    if (action == "waypoint") {
        setActionWaypoint(value);
    }

    if (action == "undock") {
        setActionUndock(value);
    }

    if (action == "dock") {
        setActionDock(value);
    }

    if (action == "disable_autopilot") {
        currentAction = "NA";
        setRemoteControlsAutoPilotEnabled(false);
    }

    if (action == "mine") {
        setActionMine();
    }

    if (action == "orientation") {
        setOrientation(value);
    }

    if (action == "unload") {
        List<IMyCargoContainer> containers = new List<IMyCargoContainer>();
        GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(containers);
        foreach (IMyCargoContainer container in containers) {
            if (container.CustomData == value) {
                IMyInventory primaryInventory = container.GetInventory();
                List<IMyShipConnector> connectors = new List<IMyShipConnector>();
                GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(connectors);
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                foreach (IMyShipConnector connector in connectors) {
                    if (connector.CustomData != droneName) {
                        continue;
                    }
                    IMyInventory inventory = connector.GetInventory();
                    inventory.GetItems(items);
                    foreach (MyInventoryItem item in items) {
                        inventory.TransferItemTo(primaryInventory, item);
                    }
                }

                foreach (IMyCargoContainer droneContainer in containers) {
                    if (droneContainer.CustomData != droneName) {
                        continue;
                    }
                    IMyInventory droneInventory = droneContainer.GetInventory();
                    droneInventory.GetItems(items);
                    foreach (MyInventoryItem item in items) {
                        droneInventory.TransferItemTo(primaryInventory, item);
                    }
                }
            }
        }
    }

    return;
}

public void Main(string argument, UpdateType updateSource) {
    if (Me.CustomData == null || Me.CustomData.Trim() == "") {
        Echo("No custom data value, stopping");
        return;
    }

    Echo(droneName);

    string message = "";

    double positionX = 0;
    double positionY = 0;
    double positionZ = 0;

    string pitchAngleStatus = "?";
    string rollAngleStatus = "?";
    string yawAngleStatus = "?";
    string drillStatus = "?";
    string downSensorCloseStatus = "?";
    string downSensorDynamicStatus = "?";
    string shipMaxVelocityStatus = "?";
    string shipVelocityStatus = "?";
    string waypointNameStatus = "?";
    Vector3D waypointPosition = new Vector3D();

    string lastMessage = receiveToMessages();
    processActionRequest(lastMessage);

    checkConnector();

    int detectedEntitiesCount = 0;
    downSensorCloseStatus = "Not detected";
    downSensorDynamicStatus = "N:?";
    List<IMySensorBlock> sensors = new List<IMySensorBlock>();
    GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(sensors);
    foreach (IMySensorBlock sensor in sensors) {
        if (sensor.CustomData != droneName) {
            continue;
        }

        List<MyDetectedEntityInfo> detectedEntities = new List<MyDetectedEntityInfo>();
        sensor.DetectedEntities(detectedEntities);
        
        if (sensor.FrontExtend >= 5) {
            float nextDistance = 5;
            float farthestDetection = -1;
            for (int distanceCount = 0; distanceCount < downSensorDynamicDistances.Length; distanceCount++) {
                if (sensor.FrontExtend == downSensorDynamicDistances[distanceCount]) {
                    nextDistance = downSensorDynamicDistances[(distanceCount + 1) % downSensorDynamicDistances.Length];

                    if (detectedEntities != null && detectedEntities.Count > 0) {
                        downSensorDynamicDetections[distanceCount] = true;
                    }
                    else {
                        downSensorDynamicDetections[distanceCount] = false;
                    }
                }
                if (downSensorDynamicDetections[distanceCount] == true) {
                    farthestDetection = downSensorDynamicDistances[distanceCount];
                }
            }
            sensor.FrontExtend = nextDistance;
            if (farthestDetection == -1) {
                downSensorDynamicStatus = "?";
            }
            else {
                downSensorDynamicStatus = farthestDetection.ToString();
            }
            continue;
        }

        detectedEntitiesCount += detectedEntities.Count;
        
        if (detectedEntities.Count > 0) {
            downSensorCloseStatus = "Detected";
        }

        if (detectedEntities.Count > 0 && isInventoryFull() == false && currentAction == "MN") {
            setDrillsPower(true);
            drillStatus = "On";
        }
        else {
            setDrillsPower(false);
            drillStatus = "Off";
        }
    }

    List<IMyRemoteControl> remoteControls = new List<IMyRemoteControl>();
    GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(remoteControls);

    for (int c = 0; c < remoteControls.Count; c++) {
        if (remoteControls[c].CustomData != droneName) {
            continue;
        }
        positionX = remoteControls[c].CenterOfMass.X;
        positionY = remoteControls[c].CenterOfMass.Y;
        positionZ = remoteControls[c].CenterOfMass.Z;

        List<MyWaypointInfo> waypoints = new List<MyWaypointInfo>();
        remoteControls[c].GetWaypointInfo(waypoints);
        foreach (MyWaypointInfo waypoint in waypoints) {
            waypointNameStatus = waypoint.Name;
            waypointPosition = waypoint.Coords;
        }

        Vector3D temporaryDownOrientation = orientDownTowards;
        Vector3D temporaryForwardOrientation = orientForwardTowards;
        if (useOrientTowards == false) {
            temporaryDownOrientation = remoteControls[c].GetNaturalGravity().Normalized();
            temporaryForwardOrientation = remoteControls[c].GetNaturalGravity().Normalized();
        }

        Vector3D relativeDifference = Vector3D.TransformNormal(temporaryDownOrientation, MatrixD.Transpose(remoteControls[c].WorldMatrix));
        Vector3D localForwardOrientation = Vector3D.TransformNormal(temporaryForwardOrientation, MatrixD.Transpose(remoteControls[c].WorldMatrix));
        
        Vector3D pitchVector = relativeDifference;
        pitchVector.X = 0;
  
        double pitchAngle = Vector3D.Angle(pitchVector, Vector3D.Down);
        pitchAngle = pitchAngle * (relativeDifference.Z / Math.Abs(relativeDifference.Z));
        pitchAngle = pitchAngle * (180 / Math.PI);
        pitchAngleStatus = pitchAngle.ToString();

        float gyroPitchRpm = (float) Math.Sqrt(Math.Abs(pitchAngle)) / rpmReduction;
        if (pitchAngle > degreesOffCenter) {
            gyroPitchRpm = gyroPitchRpm * -1;
        }
        else if (pitchAngle < degreesOffCenter * -1) {
            gyroPitchRpm = gyroPitchRpm * 1;
        }
        else {
            gyroPitchRpm = 0;
        }

        Vector3D rollVector = relativeDifference;
        rollVector.Z = 0;

        double rollAngle = Vector3D.Angle(rollVector, Vector3D.Down);
        rollAngle = rollAngle * (relativeDifference.X / Math.Abs(relativeDifference.X));
        rollAngle = rollAngle * (180 / Math.PI);
        rollAngleStatus = rollAngle.ToString();

        float gyroRollRpm = (float) Math.Sqrt(Math.Abs(rollAngle)) / rpmReduction;
        if (rollAngle > degreesOffCenter) {
            gyroRollRpm = gyroRollRpm * 1;
        }
        else if (rollAngle < degreesOffCenter * -1) {
            gyroRollRpm = gyroRollRpm * -1;
        }
        else {
            gyroRollRpm = 0;
        }

        Vector3D yawVector = localForwardOrientation;
        yawVector.Y = 0;

        double yawAngle = Vector3D.Angle(yawVector, Vector3D.Forward);
        yawAngle = yawAngle * (yawVector.X / Math.Abs(yawVector.X));
        yawAngle = yawAngle * (180 / Math.PI);
        yawAngleStatus = yawAngle.ToString();

        float gyroYawRpm = (float) Math.Sqrt(Math.Abs(yawAngle)) / rpmReduction;
        if (yawAngle > degreesOffCenter) {
            gyroYawRpm = gyroYawRpm * 1;
        }
        else if (yawAngle < degreesOffCenter * -1) {
            gyroYawRpm = gyroYawRpm * -1;
        }
        else {
            gyroYawRpm = 0;
        }

        List<IMyGyro> gyro = new List<IMyGyro>();
        GridTerminalSystem.GetBlocksOfType<IMyGyro>(gyro);
        for (int g = 0; g < gyro.Count; g++) {
            if (gyro[g].CustomData != droneName) {
                continue;
            }
            gyro[g].Roll = gyroRollRpm;
            gyro[g].Pitch = gyroPitchRpm;
            if (useOrientTowards) {
                gyro[g].Yaw = gyroYawRpm;
            }
            else {
                gyro[g].Yaw = 0;
            }
        }

        if (detectedEntitiesCount == 0
            && isInventoryFull() == false
            && currentAction == "MN"
            && remoteControls[c].GetShipVelocities().LinearVelocity.Length() < safeMiningVelocity) {
            setDownThrustersThrustOverride(1);
        }
        else if (currentAction == "MN" &&
                    isInventoryFull() == true &&
                    remoteControls[c].GetShipVelocities().LinearVelocity.Length() < safeMiningVelocity) {
            setDownThrustersThrustOverride(0);
            changeUpThrustersThrustOverride(safeUpPercentage);
            double miningDistance = Vector3D.Distance(getPosition(), miningPosition);
            if (miningDistance < 10 || miningDistance > 100) {
                currentAction = "";
            }
        }
        else if (currentAction == "UD" && remoteControls[c].GetShipVelocities().LinearVelocity.Length() < safeUndockVelocity) {
            changeUpThrustersThrustOverride(safeUpPercentage);
            if (Vector3D.Distance(getPosition(), dockingPosition) > 75) {
                currentAction = "";
                retractPistons();
                Vector3D isNaturalGravity = remoteControls[c].GetNaturalGravity();
                if (isNaturalGravity != null) {
                    setOrientation("natural_gravity");
                    setGyroscopeOverride(true);
                }
            }
        }
        else if (currentAction == "DK") {
            if (remoteControls[c].GetShipVelocities().LinearVelocity.Length() < safeDockVelocity
                && downSensorDynamicDetections[downSensorDynamicDetections.Length - 4] == false) {
                setDownThrustersThrustOverride(1);
            }

            if (remoteControls[c].GetShipVelocities().LinearVelocity.Length() >= safeDockVelocity
                || downSensorDynamicDetections[downSensorDynamicDetections.Length - 4] == true) {
                setDownThrustersThrustOverride(0);
            }

            float adjustment = remoteControls[c].CalculateShipMass().TotalMass * 1.1;
            Vector3D linearVelocity = Vector3D.TransformNormal(remoteControls[c].GetShipVelocities().LinearVelocity, MatrixD.Transpose(remoteControls[c].WorldMatrix));
            Vector3D dockingDifference = dockingPosition - remoteControls[c].GetPosition();
            Vector3D bodyPosition = Vector3D.TransformNormal(dockingDifference, MatrixD.Transpose(remoteControls[c].WorldMatrix));
            if (bodyPosition.X > metersOffCenter && linearVelocity.X < safeDockVelocityVh) {
                setRightThrustersThrustOverride(adjustment);
                setLeftThrustersThrustOverride(0);
            }
            else if (bodyPosition.X < -metersOffCenter && linearVelocity.X > -safeDockVelocityVh) {
                setRightThrustersThrustOverride(0);
                setLeftThrustersThrustOverride(adjustment);
            }
            else {
                setRightThrustersThrustOverride(0);
                setLeftThrustersThrustOverride(0);
            }

            if (bodyPosition.Z > metersOffCenter && linearVelocity.Z < safeDockVelocityVh) {
                setForwardThrustersThrustOverride(0);
                setBackwardThrustersThrustOverride(adjustment);
            }
            else if (bodyPosition.Z < -metersOffCenter && linearVelocity.Z > -safeDockVelocityVh) {
                setForwardThrustersThrustOverride(adjustment);
                setBackwardThrustersThrustOverride(0);
            }
            else {
                setForwardThrustersThrustOverride(0);
                setBackwardThrustersThrustOverride(0);
            }
        }
        else if (currentAction == "WP" && remoteControls[c].IsAutoPilotEnabled == false) {
            currentAction = "";
            Vector3D isNaturalGravity = remoteControls[c].GetNaturalGravity();
            if (isNaturalGravity != null) {
                setOrientation("natural_gravity");
                setGyroscopeOverride(true);
            }
        }
        else {
            setDownThrustersThrustOverride(0);
            setForwardThrustersThrustOverride(0);
            setBackwardThrustersThrustOverride(0);
            setLeftThrustersThrustOverride(0);
            setRightThrustersThrustOverride(0);
            disableUpThrustersThrustOverride();
        }
        shipMaxVelocityStatus = remoteControls[c].SpeedLimit.ToString();
        shipVelocityStatus = remoteControls[c].GetShipVelocities().LinearVelocity.Length().ToString();
    }

    List<IMyTerminalBlock> cargoContainers = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers);

    bool foundCargoContainers = false;

    long currentVolume = 0;
    long maxVolume = 0;

    for (int c = 0; c < cargoContainers.Count; c++) {
        if (cargoContainers[c].CustomData != droneName) {
            continue;
        }

        foundCargoContainers = true;
        IMyInventory cargoContainerInventory = cargoContainers[c].GetInventory();
        currentVolume += cargoContainerInventory.CurrentVolume.RawValue;
        maxVolume += cargoContainerInventory.MaxVolume.RawValue;
    }

    string cargoContainersPercent = "?";
    if (foundCargoContainers) {
        float intermediate = (float)currentVolume / (float)maxVolume;
        cargoContainersPercent = Math.Round(intermediate * 100, 0, MidpointRounding.AwayFromZero).ToString();
    }

    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);
 
    bool foundBatteries = false;

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

    string storedPowerPercent = "?";
    if (foundBatteries) {
        storedPowerPercent = Math.Round((currentStoredPower / maxStoredPower) * 100, 0, MidpointRounding.AwayFromZero).ToString();
    }

    string orientationStatus = "Coordinates";
    if (!useOrientTowards) {
        orientationStatus = "Natural gravity";
    }

    message =        droneName;
    message += "," + positionX.ToString();
    message += "," + positionY.ToString();
    message += "," + positionZ.ToString();
    message += "," + cargoContainersPercent;
    message += "," + storedPowerPercent;
    message += "," + currentAction;
    message += "," + drillStatus;
    message += "," + downSensorCloseStatus;
    message += "," + powerMode;
    message += "," + rollAngleStatus;
    message += "," + pitchAngleStatus;
    message += "," + downSensorDynamicStatus;
    message += "," + shipMaxVelocityStatus;
    message += "," + shipVelocityStatus;
    message += "," + waypointNameStatus;
    message += "," + orientationStatus;
    message += "," + yawAngleStatus;
    message += "," + waypointPosition.X.ToString();
    message += "," + waypointPosition.Y.ToString();
    message += "," + waypointPosition.Z.ToString();
    // Echo("Outbound Message - " + messageToDroneControllerTag);
    // Echo(message);
    IGC.SendBroadcastMessage(messageToDroneControllerTag, message, TransmissionDistance.AntennaRelay);
}
