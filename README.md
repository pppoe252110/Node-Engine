# 🎮 In‑Game Node Engine

**A complete runtime visual scripting system for Unity games.**  
Allow your players to create, edit, and execute node‑based logic directly inside your game.

[![Unity Version](https://img.shields.io/badge/Unity-6000.1+-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

⚠️ **Active Development** – features and APIs are evolving.

---

## 📖 Table of Contents

- [What is This?](#-what-is-this)
- [Features](#-features)
- [Installation](#-installation)
- [Quick Setup](#-quick-setup)
- [Controls](#-controls)
- [Save & Load System](#-save--load-system)
- [Node Categories](#-node-categories)
- [Creating Custom Nodes](#-creating-custom-nodes)
- [Architecture Overview](#-architecture-overview)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 What is This?

The **In‑Game Node Engine** is a **player‑facing visual scripting framework**.  
Players can build their own logic using a node graph, without writing code.  
It runs entirely at runtime, making it perfect for modding, user‑generated content, or in‑game automation.

- 🎨 Visual drag‑and‑drop interface
- ⚡ Real‑time execution during gameplay
- 🧩 Fully extensible – create your own node types
- 💾 Built‑in save/load for node graphs
- 🔌 Strongly typed connection system with visual feedback

---

## ✨ Features

### For Players
- **Intuitive Node Editor** – pan, zoom, connect, and arrange nodes.
- **Type‑Safe Connections** – colour‑coded ports prevent invalid links.
- **Flow & Data Connections** – control execution order and pass values.
- **Live Execution** – graphs run inside the game world.

### For Developers
- **Zero‑Editor‑Only Code** – everything is built for runtime.
- **Attribute‑Based Node Definition** – define ports with simple C# attributes.
- **Compilation Pipeline** – graphs are compiled into efficient, stack‑based bytecode‑like instructions.
- **Dependency Injection Ready** – uses `VContainer` for clean, testable architecture.
- **Extensible Persistence** – custom `IValuePersister` for any data type.
- **Save/Load API** – easily store and restore complete node graphs.

---

## 📦 Installation

The Node Engine depends on three lightweight libraries. Install them in order:

### 1️⃣ UniTask
```
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

### 2️⃣ VContainer
```
https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer
```

### 3️⃣ UniMediator
```
https://github.com/pppoe252110/UniMediator.git?path=Assets/Plugins/UniMediator
```

### 4️⃣ Node Engine
```
https://github.com/pppoe252110/Node-Engine.git?path=/Assets/NodeEngine
```

> **Method:** In Unity, open **Window → Package Manager → + → Add package from git URL…** and paste each URL.  
> Alternatively, add all four entries directly to `Packages/manifest.json`.

---


## ⚒ Quick Setup

1. Run Setup `Tools > Node Engine > Setup`

2. **Add the Prefab**  
 Drag `Assets/Resources/NodeEngine/NodeEngine.prefab` into your starting scene.

3. **Press Play**  
 The node editor is ready. Press `Space` to open the spawn menu.

4. **Create Your First Graph**  
 - Spawn an `Update` node (from **Events**)  
 - Add a `Debug/Log` node  
 - Connect the flow output of `Update` to the input of `Log`  
 - Connect a `String Variable` node to the `Message` port  
 - Enter some text in the variable node and watch the console.

---

## 🎮 Controls

| Action | Control |
|--------|---------|
| Open spawn menu | `Space` (at mouse position) |
| Close spawn menu | Click outside |
| Pan canvas | `Middle Mouse` drag |
| Zoom | `Mouse Wheel` |
| Select / Move node | `Left Click` drag on node header |
| Create connection | Drag from an output port to an input port |
| Delete connection | `Right Click` on a port |
| Delete node | `Right Click` → Delete (context menu) |
| Save / Load | UI buttons (Quick Save `F5`, Quick Load `F9`) |

---

## 💾 Save & Load System

Graphs are serialised to JSON and can be saved/loaded at runtime.

| Method | Description |
|--------|-------------|
| `SaveGraph(string name)` | Save current graph to persistent storage. |
| `LoadGraph(string name)` | Load and restore a previously saved graph. |
| `QuickSave()` / `QuickLoad()` | Use a fixed quick‑save slot. |
| `GetSaveFiles()` | List all available saves. |

- Saved files are stored in `Application.persistentDataPath/NodeGraphs/`.  
- Each save includes node positions, port connections, and variable values.

---

## 📊 Node Categories

### 🎯 Control Flow
| Node | Description |
|------|-------------|
| **Start** | Entry point executed once when graph is marked ready. |
| **Update** | Fires every frame. |
| **Branch** | If‑Else condition. |
| **For Loop** | Iterates a fixed number of times. |
| **While** | Loops while condition is true. |
| **Sequence** | Executes multiple outputs in order. |
| **Throttle** | Limits execution rate by time interval. |

### ➕ Math
- Add, Subtract, Multiply, Divide, Modulo  
- Abs, Ceil, Floor, Round, Sign, Clamp  
- Sin, Cos, Tan, Asin, Acos, Atan, Atan2  
- Lerp, InverseLerp, Power, Sqrt, Deg/Rad conversion  
- Min, Max, PI constant

### 🧠 Logic
- **Comparison** (Equal, NotEqual, Greater, Less, etc.)  

### 🔄 Conversion
- **Convert** – change type (e.g. int → float)  
- **ToString** – convert any value to string  

### 🎮 Game Objects
- **Move GameObject** – set Transform position(not done yet)  

### 💾 Variables
- **Int, Float, Bool, String, Vector3, Type, ComparisonOperation**  
- **Get Global / Set Global** – share values across graphs  

### 🐛 Debug
- **Log** – print message to Unity console  

### ⏱️ Engine
- **Time** – provides `deltaTime`, `time`, `realtimeSinceStartup`

---

## 🔧 Creating Custom Nodes

Define a new node by inheriting from `BaseNode` and using attributes.

```csharp
[NodePath("String/Replace")]
public class StringReplaceNode : BaseNode
{
    [NodePort("In", true, true)]
    public void Enter() { }

    [NodePort("Source", true)]
    public string source;

    [NodePort("Old", true)]
    public string oldValue;

    [NodePort("New", true)]
    public string replacement;

    [NodePort("Result", false)]
    public string result;

    [NodePort("Out", false, true)]
    public void Exit() { }

    public override Func<GraphContext, ExecutionResult> Compile(NodeCompilationContext context)
    {
        int sourceId = context.GetInputId("Source");
        int oldId    = context.GetInputId("Old");
        int newId    = context.GetInputId("New");
        int resultId = context.GetOutputId("Result");
        int exitFlow = context.GetFlowId("Out");

        return ctx =>
        {
            string src = ctx.Read<string>(sourceId) ?? string.Empty;
            string old = ctx.Read<string>(oldId) ?? string.Empty;
            string repl = ctx.Read<string>(newId) ?? string.Empty;

            string replaced = src.Replace(old, repl);
            ctx.Write(resultId, replaced);

            return ExecutionResult.Continue(exitFlow);
        };
    }
}
```
### Port Attributes

- `[NodePort("Name", isInput, isFlow = false, order = 0)]`  
- Flow ports (`isFlow = true`) are used for execution control.  
- Non‑flow ports carry typed data.

### Dynamic Type Binding

If your node’s output type depends on an input type (e.g. a `Convert` node), use `BindDynamicType` in the constructor to automatically update port types when a `Type` variable is connected.

---

## 🦺 Architecture Overview

| Component | Responsibility |
|-----------|----------------|
| `BaseNode` | Core node logic, port definition, compilation. |
| `NodeCompiler` / `CompilationPipeline` | Transforms graph into executable delegates. |
| `GraphContext` | Holds runtime memory and execution stack. |
| `ConnectionManager` / `ConnectionService` | Manages data & flow connections. |
| `NodeSpawnerService` | Instantiates and manages node UI. |
| `NodeRunner` | Executes compiled graph (Start/Update/Test). |
| `GraphSnapshotBuilder` / `GraphRestorer` | Serialise / deserialise graph state. |
| `PersistenceService` | Delegates variable serialisation to `IValuePersister` implementations. |

### Execution Flow
1. Graph is compiled into a `CompiledGraph` containing a delegate per node.  
2. `NodeRunner` calls `ExecuteNode(startNode)` → stack‑based execution begins.  
3. Data flows through shared memory (array of `object`).  
4. Flow connections determine which nodes are executed next.

---

## 🤝 Contributing

Contributions are welcome!  
Open an issue or a pull request for:

- New node types  
- Bug fixes  
- Performance improvements  
- Documentation updates  

---

## 📄 License

MIT License – see [LICENSE](LICENSE) for full text.

---

<div align="center">

**Happy Building!** 🎉  
Made with ❤️ by [pppoe252110](https://github.com/pppoe252110)

</div>
