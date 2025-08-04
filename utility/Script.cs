public Program()
{

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

public void Main(string argument, UpdateType updateSource) {
    string[] arguments = argument.Split(' ');
    if (arguments.Length == 0 || arguments == null) {
        return;
    }

    if (arguments[0] == "rename") {
        if (arguments.Length != 3 && arguments.Length != 4) {
            Echo("Incorrect arguments (" + arguments.Length.ToString() + ") - rename [find] [replace] ([whatif])");
            return;
        }

        if (arguments.Length == 4 && arguments[3] != "whatif") {
            return;
        }

        bool whatIf = false;
        if (arguments.Length == 4 && arguments[3] == "whatif") {
            whatIf = true;
        }

        Echo("Checking for " + arguments[1]);
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
        GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);
        for (int i = 0; i < blocks.Count; i++) {
            string name = blocks[i].CustomName;
            if (name.Contains(arguments[1])) {
                Echo("Was: " + name);
                name = name.Replace(arguments[1], arguments[2]);
                Echo("Now: " + name);

                if (!whatIf) {
                    blocks[i].CustomName = name;
                    Echo("Updated");
                }
            }
        }
    }

    // 0   1           2          3     4           5                6
    // set [attribute] [to_value] where [attribute] [contains_value] (whatif)
    if (arguments[0] == "set") {
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
        GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks);

        if (arguments[1] == "customdata") {
            if (arguments[3] == "where" && arguments[4] == "name") {
                for (int i = 0; i < blocks.Count; i++) {
                    string name = blocks[i].CustomName;
                    if (name.Contains(arguments[5])) {
                        Echo(name + ".CustomData -> " + arguments[2]);
                        blocks[i].CustomData = arguments[2];
                    }
                }
            }
        }
    }
}
