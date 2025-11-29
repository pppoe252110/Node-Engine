# 🎮 In-Game Node Engine

<div align="center">

A complete in-game visual scripting system that allows players to create, edit, and execute node-based logic directly within your Unity game.

[![Unity Version](https://img.shields.io/badge/Unity-6000.1+-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

⚠️ **IN DEVELOPMENT** ⚠️
</div>

## ⚡ Performance Improvements

**Version 0.3 is 1.5x faster than Version 0.2**

This performance boost comes from two key optimizations:

1. **UI Variables Caching** - Variable nodes now cache their values, eliminating redundant UI updates and value conversions
2. **Execution Flow Refactoring** - Complete overhaul of node execution logic with optimized value propagation and reduced processing overhead

## 📖 Table of Contents
- [What is This?](#-what-is-this)
- [✨ Features](#-features)
- [📦 Installation](#-installation)
- [🚀 Getting Started](#-getting-started)
- [🛠️ Setup Instructions](#️-setup-instructions)
- [💾 Save/Load System](#-saveload-system)
- [🎮 Controls](#-controls)
- [🔗 Connection System](#-connection-system)
- [📊 Available Node Types](#-available-node-types)
- [🔧 Advanced Usage](#-advanced-usage)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)

## 🎯 What is This?

The In-Game Node Engine is a **player-facing visual scripting system** that empowers your players to become creators without writing a single line of code. It provides a powerful node-based interface directly within your game, enabling:

- 🛠️ **In-game logic creation** - Players build their own scripts without coding
- ⚡ **Real-time execution** - Node graphs run directly in gameplay
- 🎨 **Visual programming** - Intuitive drag-and-drop interface accessible to non-programmers
- 🔧 **Modding & customization** - Players can create custom game mechanics and content
- 🧩 **Extensible architecture** - Easily add new node types to fit your game's needs
<img width="1918" height="882" alt="image" src="https://github.com/user-attachments/assets/6c14af94-4fc4-4615-b5f6-c43e431b6bb2" />
<img width="1917" height="881" alt="image" src="https://github.com/user-attachments/assets/7e37b673-802f-438e-b7de-9ca81e1ce16a" />

## ✨ Features

### 🎮 Player Experience
- **Intuitive Interface**: Clean, user-friendly design that's easy to learn
- **No Coding Required**: Create complex behaviors through visual connections
- **Real-time Preview**: See results immediately as you build
- **Drag & Drop**: Simple node creation and connection system

### 🛠️ Developer Tools
- **Easy Integration**: Simple setup process for any Unity project
- **Custom Node Creation**: Extend the system with your own specialized nodes
- **Performance Optimized**: Efficient execution even with complex node graphs
- **Robust Type System**: Prevents errors with strong type validation
- **Save/Load System**: Persistent storage of node graphs
- **Console Debugging**: Built-in console for debugging node execution

## 📦 Installation

This package has a dependency on **UniTask**. Because of how Unity's Package Manager handles Git dependencies, you must install UniTask first.

Please follow one of the methods below carefully.

### Option 1: Using Package Manager UI (Recommended)

This method uses the Unity Editor's interface and gives you visual feedback.

**Step 1: Install UniTask Dependency**

1.  In Unity, navigate to **Window > Package Manager**.
2.  Click the **`+`** icon in the top-left corner and select **"Add package from git URL..."**.
3.  Enter the following URL and click **Add**:
    ```
    https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
    ```
4.  Wait for the package to install. You should see "UniTask" appear in your Package Manager list.

**Step 2: Install the Node Engine**

1.  With UniTask successfully installed, click the **`+`** icon in the Package Manager again.
2.  Select **"Add package from git URL..."**.
3.  Enter the following URL and click **Add**:
    ```
    https://github.com/pppoe252110/Node-Engine.git?path=/Assets/NodeEngine
    ```

The installation should now complete without errors.

### Option 2: Using `manifest.json` (Advanced)

This method involves editing a project file directly.

1.  **Open your project's `manifest.json` file.** You can find it at `YourProject/Packages/manifest.json`.
2.  **Add both packages** to the `dependencies` object. It should look something like this:

```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.parity.nodeengine": "https://github.com/pppoe252110/Node-Engine.git?path=/Assets/NodeEngine",
    "com.unity.modules.ui": "1.0.0",
    "com.unity.modules.imgui": "1.0.0"
  }
}
```
## 🚀 Getting Started

1. **Install the package** using one of the methods above
2. **Add the NodeEngine component** to a GameObject in your scene
3. **Configure the component** with your desired settings
4. **Run your game** and press the **Space key** to open the node editor
5. **Start creating!** Right-click to add nodes and connect them to build logic

## 🛠️ Setup Instructions

### Quick Setup (5 minutes)

1. **Install the package** using one of the methods above
2. **Run the Setup Wizard**:
   - In Unity Editor, go to **Tools → Node Engine → Setup**
   - This will open the setup window automatically on first import
   - Check the options you want (recommended: both options enabled)
   - Click **"Run Setup"** to copy resources and fix references

3. **Add the NodeEngine to your scene**:
   - Navigate to `Assets/Resources/NodeEngine/` in your Project window
   - Drag the `NodeEngine.prefab` into your scene

4. **Test it out**:
   - Press **Play** in Unity
   - Press **Space** to open the node spawn menu
   - Start creating nodes and connecting them!

### Setup Wizard Features

The setup wizard automatically handles:

- **Resource Copying**: Copies necessary files from the package to your project
- **GUID Fixing**: Updates all internal references to work in your project
- **Example Graphs**: Provides sample node graphs to learn from
- **Version Checking**: Automatically checks for updates
- **Status Tracking**: Shows current setup status

## 💾 Save/Load System

The Node Engine includes a robust save/load system that allows players to:

- **Save Node Graphs**: Store complete node configurations for later use
- **Load Node Graphs**: Restore previously saved node configurations
- **Quick Save/Load**: Fast access to frequently used graphs
- **Export/Import**: Share node graphs between projects or with other players

### Save/Load Controls

| Action | Control | Description |
|--------|---------|-------------|
| **Quick Save** | `F5` | Save the current node graph as "quicksave" |
| **Quick Load** | `F9` | Load the "quicksave" graph |
| **Save Menu** | `Save Button` | Access the full save/load interface |

### Save File Management

- **Location**: Save files are stored in your project's `Assets` folder as `.json` files
- **Compatibility**: Saved graphs are compatible across different Unity versions
- **Backup**: Automatic backup system prevents data loss
- **Versioning**: Save files include version information for future compatibility
<img width="1918" height="882" alt="image" src="https://github.com/user-attachments/assets/23ea0681-59d4-4ef5-b6f9-eab61b018624" />

## 🎮 Controls

### Basic Navigation

| Action | Control | Description |
|--------|---------|-------------|
| **Open/Close Menu** | `Space` | Open/close node spawn menu at mouse position |
| **Context Menu** | `Right Click` | Access context menus for node management |
| **Pan Canvas** | `Middle Mouse` | Move around the node canvas |
| **Zoom** | `Mouse Wheel` | Zoom in/out of the canvas |
| **Connect Nodes** | `Drag` | Create connections between node ports |
| **Delete Connection** | `Right Click on Connector` | Remove existing connections |

## 🔗 Connection System

The node engine features a robust connection system with visual feedback:

### Connection Types

- **Color-coded Types**: Different connection types are represented by different colors:
  - 🔴 **Red**: Integer values (`Int32`) - whole numbers
  - 🟢 **Green**: Float values (`Single`) - decimal numbers  
  - 🔵 **Blue**: Boolean values (`Boolean`) - true/false
  - 🟡 **Yellow**: String values (`String`) - text
  - 🟣 **Purple**: Void/Events (`Void`) - execution flow
  - 🟠 **Orange**: Vector3 values (`Vector3`) - 3D vectors
  - 🔷 **Cyan**: GameObject references (`GameObject`) - Unity objects
  - 🟣 **Magenta**: Object/Generic types (`Object`) - any object
  - ⚫ **Gray**: Fallback/Unknown types

### Connection Rules

- **Type Validation**: Prevents incompatible connections with visual feedback
- **Connection Flow**: Data flows from left to right
- **Multi-connections**: One output can connect to multiple inputs, but inputs can only have one connection

### Connection Creation

1. **Click and drag** from any output connector (right side of node)
2. **Drag to** any compatible input connector (left side of another node)
3. **Release** to create the connection
4. **Right-click** on any connector to remove all its connections

## 📊 Available Node Types

### 🎯 Control Flow
| Node | Description | Inputs | Outputs |
|------|-------------|--------|---------|
| **Update** | Executes on play event | None | Execute |
| **For Loop** | Iterates through a range of values | Count, Execute | Body, Index, End |
| **If-Else** | Conditional branching | Condition, Execute | True, False |

### 💾 Variables
| Node | Description | Inputs | Outputs |
|------|-------------|--------|---------|
| **Int** | Integer variable | None | Value |
| **Float** | Floating-point variable | None | Value |
| **Bool** | Boolean variable | None | Value |
| **String** | Text variable | None | Value |
| **Vector3** | 3D vector variable | None | Value |
| **Set Variable** | Store value in variable | Value, Name, Execute | Execute |

### ➕ Math Operations
| Node | Description | Inputs | Outputs |
|------|-------------|--------|---------|
| **Add** | Adds two values | A, B | Result |
| **Subtract** | Subtracts one value from another | A, B | Result |
| **Multiply** | Multiplies two values | A, B | Result |
| **Divide** | Divides one value by another | A, B | Result |
| **Clamp** | Restricts a value between min and max | Value, Min, Max | Result |

### 🎮 Game Objects
| Node | Description | Inputs | Outputs |
|------|-------------|--------|---------|
| **Move GameObject** | Changes position of GameObject | Target, Position, Speed, Execute | Execute |
| *More GameObject nodes coming soon* | | | |

### 🔄 Conversion
| Node | Description | Inputs | Outputs |
|------|-------------|--------|---------|
| **ToString** | Converts any value to string | Input | Output |

### 🐛 Debug & Output
| Node | Description | Inputs | Outputs |
|------|-------------|--------|---------|
| **Debug Log** | Outputs message to console | LogString, Execute | Execute |

### ⏱️ Time & Events
| Node | Description | Inputs | Outputs |
|------|-------------|--------|---------|
| **Time** | Provides time-based values | None | DeltaTime, Time, RealTime |

## 🔧 Advanced Usage

## 🔧 Creating Custom Nodes

Here's an example of creating a custom Debug node that outputs messages to the console:

```csharp
using UnityEngine;

// NodePath attribute defines where this node appears in the node menu
[NodePath("Debug/Log")]
public class DebugNode : ExecutableNodeBase
{
    private ConnectorValueString _logText;

    // NodeValue attribute defines an input connector with name and type
    [NodeValue("LogString", typeof(string))]
    public void LogString(ConnectorValueString value)
    {
        _logText = value; // Store the input value for later use
    }

    // Execute is called when the node is triggered
    public override void Execute()
    {
        // Get the text value, with null safety
        string text = _logText?.GetValue() ?? "null";

        // Output to the in-game console if available, otherwise use Unity's console
        if (ConsoleUI.Instance != null)
        {
            ConsoleUI.Instance.LogMessage($"[Debug] {text}");
        }
        else
        {
            Debug.Log($"[Debug] {text}");
        }

        // Important: Always call base.Execute() to trigger default output connections
        base.Execute();
    }

    // Setup defines the node's input and output connectors
    public override void Setup()
    {
        // Define input fields - true means it's an input connector
        inputFields = new()
        {
            new NodeField<ConnectorValueString>(true)
                .SetHandler(LogString)                    // Method to call when value changes
                .SetDefaultValue(new ConnectorValueString("")) // Default value
        };

        // Important: Always call base.Setup() to include default execution connectors
        base.Setup();
    }
}
```

## 🔧 Advanced Usage

### Performance Optimization

- Implement **caching** for frequently accessed values
- Use **void connections** for execution flow to avoid unnecessary data processing
- **Batch process** nodes when possible to reduce per-frame overhead

### Debugging Tips

- Use the **Console UI** to see real-time execution logs
- Check **connection colors** to verify data types are compatible
- Use **Debug nodes** to output intermediate values
- Enable **execution visualization** to see data flow through the graph

## 🤝 Contributing

I'm open to contributions! Whether you're fixing bugs, adding new features, or improving documentation, your help is appreciated.

### How to Contribute

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add some amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### Contribution Areas

- **New Node Types**: Add functionality specific to your game
- **UI Improvements**: Enhance the user experience
- **Performance Optimizations**: Make the system faster
- **Documentation**: Help others learn to use the system
- **Bug Fixes**: Squash those pesky issues

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Thanks to all the contributors who have helped make this project possible
- Inspired by other visual scripting tools like Unreal Blueprint and Unity Bolt
- Built with [UniTask](https://github.com/Cysharp/UniTask) for async operations
- Uses custom UI components for optimal performance

---

<div align="center">

**Happy Node Building!** 🎉

Made with ❤️ by pppoe252110

*If you use this in your project, let me know! I'd love to see what you create.*

</div>
