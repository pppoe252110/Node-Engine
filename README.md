# 🎮 In-Game Node Engine

<div align="center">

A complete in-game visual scripting system that allows players to create, edit, and execute node-based logic directly within your Unity game.

[![Unity Version](https://img.shields.io/badge/Unity-6000.1+-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

⚠️ **IN DEVELOPMENT** ⚠️

</div>

![Node Engine Screenshot](https://github.com/user-attachments/assets/1c3b1f9d-b538-4435-b676-343d1a4704c3)

## 📖 Table of Contents
- [What is This?](#-what-is-this)
- [✨ Features](#-features)
- [📦 Installation](#-installation)
- [🚀 Getting Started](#-getting-started)
- [🎮 Controls](#-controls)
- [🔗 Connection System](#-connection-system)
- [📊 Available Node Types](#-available-node-types)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)

## 🎯 What is This?

The In-Game Node Engine is a **player-facing visual scripting system** that empowers your players to become creators without writing a single line of code. It provides a powerful node-based interface directly within your game, enabling:

- 🛠️ **In-game logic creation** - Players build their own scripts without coding
- ⚡ **Real-time execution** - Node graphs run directly in gameplay
- 🎨 **Visual programming** - Intuitive drag-and-drop interface accessible to non-programmers
- 🔧 **Modding & customization** - Players can create custom game mechanics and content
- 🧩 **Extensible architecture** - Easily add new node types to fit your game's needs

## ✨ Features

### 🎮 Player Experience
- **Intuitive Interface**: Clean, user-friendly design that's easy to learn
- **No Coding Required**: Create complex behaviors through visual connections

### 🛠️ Developer Tools
- **Easy Integration**: Simple setup process for any Unity project
- **Custom Node Creation**: Extend the system with your own specialized nodes
- **Performance Optimized**: Efficient execution even with complex node graphs
- **Robust Type System**: Prevents errors with strong type validation

/*
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

## 🎮 Controls

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

- **Color-coded Types**: Different connection types are represented by different colors:
  - 🔵 **Blue**: Void/Events
  - 🟢 **Green**: Float values
  - 🔴 **Red**: Integer values
  - 🟡 **Yellow**: Boolean values
  - 🟣 **Purple**: String values
  - 🟠 **Orange**: GameObject references

- **Type Validation**: Prevents incompatible connections with visual feedback
- **Connection Flow**: Data flows from right to left (output ports on the right, input ports on the left)
- **Multi-connections**: One output can connect to multiple inputs, but inputs can only have one connection

## 📊 Available Node Types

### 🎯 Control Flow
| Node | Description |
|------|-------------|
| **Update** | Executes every frame |
| **For Loop** | Iterates through a range of values |
| **If-Else** | Conditional branching based on boolean input |

### 💾 Variables
| Node | Description |
|------|-------------|
| **Int** | Integer variable with UI input |
| **Float** | Floating-point variable with UI input |
| **Bool** | Boolean variable with toggle UI |
| **String** | Text variable with text field UI |

### ➕ Math
| Node | Description |
|------|-------------|
| **Add** | Adds two values together |
| **Subtract** | Subtracts one value from another |
| **Multiply** | Multiplies two values |
| **Divide** | Divides one value by another |
| **Clamp** | Restricts a value between min and max |

### 🎮 Game Objects
| Node | Description |
|------|-------------|
| **Move GameObject** | Changes the position of a GameObject |
| ~~**Rotate GameObject**~~ | ~~Changes the rotation of a GameObject~~ |
| ~~**Get Position**~~ | ~~Retrieves the position of a GameObject~~ |
| ~~**Set Active**~~ | ~~Enables or disables a GameObject~~ |

### 🐛 Debug
| Node | Description |
|------|-------------|
| **Debug Log** | Outputs a message to the console |
| **ToString** | Converts any value to a string |

### ⏱️ Time
| Node | Description |
|------|-------------|
| **Time** | Provides time-based values (delta time, fixed time, etc.) |
| ~~**Wait**~~ | ~~Pauses execution for a specified duration~~ |
| ~~**Timer**~~ | ~~Measures elapsed time~~ |

## 🤝 Contributing

We welcome contributions from the community! Whether you're fixing bugs, adding new features, or improving documentation, your help is appreciated.

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add some amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

Please make sure to follow our [Code of Conduct](CODE_OF_CONDUCT.md) and [Contributing Guidelines](CONTRIBUTING.md).

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Thanks to all the contributors who have helped make this project possible
- Inspired by other visual scripting tools like Unreal Blueprint and Unity Bolt
- Special thanks to our beta testers for their valuable feedback

---

<div align="center">
Made by pppoe252110
</div>
