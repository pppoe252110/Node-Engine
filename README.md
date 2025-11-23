# In-Game Node Engine

A complete in-game visual scripting system that allows players to create, edit, and execute node-based logic directly within your Unity game.
# *<div align="center"> ⚠️ IN DEVELOPMENT ⚠️ </div>*
<img width="1917" height="881" alt="image" src="https://github.com/user-attachments/assets/1c3b1f9d-b538-4435-b676-343d1a4704c3"/>


## 🎮 What is This?

This is a **player-facing node system** that enables:
- **In-game logic creation** - Players build their own scripts without coding
- **Real-time execution** - Node graphs run directly in gameplay
- **Visual programming** - Drag-and-drop interface accessible to non-programmers
- **Modding & customization** - Players can create custom game mechanics

## ✨ Player Features
### 🎯 Core Gameplay
- **Space Key** - Open/close node spawn menu at mouse position
- **Right Click** - Context menus for node management
- **Middle Mouse** - Pan around the node canvas
- **Mouse Wheel** - Zoom in/out

### 🔗 Connection System
- **Drag connectors** between nodes to create data flow
- **Color-coded types** (Blue=Void, Green=Float, Red=Int, etc.)
- **Type validation** - Prevents incompatible connections
- **Right-click on connectors** to clear connections

### 📊 Available Node Types
| Category | Nodes | Description |
|----------|-------|-------------|
| **Control Flow** | Update, For Loop, If-Else | Game loop and logic control |
| **Variables** | Int, Float, Bool, String | Data storage with UI inputs |
| **Math** | Add, Subtract, Multiply | Basic arithmetic operations |
| **Game Objects** | Move GameObject | Scene object manipulation |
| **Debug** | Debug Log, ToString | Testing and output |
| **Time** | Time | Time-based values |
