# 🐚 PromptShell

PromptShell is an intelligent, cross-platform desktop autobot assistant designed to bridge the gap between human intent and complex terminal execution. Instead of forcing you to remember cryptic CLI commands, flags, or syntax, PromptShell lets you speak plain human language, safely translates it into executable code, and coordinates the heavy lifting for you.

Built from the ground up for average users and developers alike, it aims to deliver a zero-configuration, "just works" experience on any standard PC or laptop.

---

## ✨ Core Features (What It Does Today)

*   **🗣️ Natural Language Translation:** Type what you want to achieve (e.g., *"clean up my workspace"* or *"build this app"*), and the AI automatically generates the precise shell command.
*   **📂 Intent-Aware Directory Tracking:** You can select your active folder using a visual file picker. The app dynamically scans the workspace and feeds the directory structure to the AI, allowing it to realize exactly what project stack you are in.
*   **🛡️ Multi-Tier Safety Guard:** Destructive keywords (like `rm`, `sudo`, `>>`) are automatically caught. The app pauses execution and prompts you for manual approval before touching your system.
*   **❓ Smart Clarification (Two-Way Conversations):** If your request is ambiguous (for example, you type *"build"* but there are multiple solution files present), the AI doesn't guess or fail—it pauses and asks you a short, helpful question to specify your goal.
*   **📖 RAG Manual Integration (`man` pages):** PromptShell secretly reads the built-in system manuals (`man`) of CLI tools in the background to ensure generated arguments and flags are perfectly tailored to your current operating system.
*   **📋 Developer Quality of Life:** Full command history navigation (using Up/Down arrows), quick clear, and a one-click clipboard copy for AI smart interpretations.
*   **🪵 Self-Maintaining Diagnostics:**
    *   `history_commands.log`: Logs every single command executed with timestamps and status (auto-truncates at 5MB to save space).
    *   `latest_session.log`: Overwrites every session to give a detailed, structured look at the raw exchange for easy troubleshooting.

---

## 🚀 The Vision: Upcoming Architecture Upgrades

We are evolving PromptShell from a standard developer utility tool into a seamless, universal consumer product. Tomorrow's development focuses on two main pillars:

### 1. 🤖 Hybrid Cloud/Local AI Router (Zero-Configuration)
To make sure any average user can download the app and use it instantly without installing local terminal frameworks (like Ollama), we are introducing a flexible multi-engine routing system:
*   **Shared Token Proxy (Default):** A built-in cloud routing proxy that lets users immediately communicate with hyper-fast cloud models (like Groq or Hugging Face) out of the box.
*   **Bring Your Own Key:** If the shared proxy runs out of limits, the app gracefully asks the user to insert their own free API key.
*   **Automatic Local Detection:** The app automatically smells if local tools like `Ollama` are running on the machine and switches to them for 100% free, private workloads.
*   **On-Demand Offline AI:** An "Enable Offline Mode" button that dynamically downloads a lightweight, compressed 2GB model (like Microsoft Phi-3 via ONNX Runtime) directly into the app storage. Once downloaded, PromptShell becomes completely autonomous and runs without internet.

---

### 2. 🎨 Upcoming UI Transformation (The Autobot Experience)
We are sunsetting the old two-column "developer dashboard" style to unlock a modern, zero-jargon **Autobot / Chatbot experience** based on an elegant Avalonia XAML concept. 

The user interface will be completely rebuilt around a single conversational flow:
*   **Central Bubble Chat Panel:** A beautiful, clean chat interface (resembling ChatGPT or Claude) showing the history of your conversation in neat bubbles.
*   **Hidden Underlying Code:** Raw green terminals, exit codes, and technical jargon are completely hidden from plain sight to avoid intimidating non-technical users.
*   **Interactive Action Cards:** When the AI decides to run a task, it renders a sleek interface card:
    
    ```text
    🛠️ PromptShell wants to build your project.
    [ Approve ]   [ Cancel ]
    
    ▼ Show raw output / technical details
    ```
    
*   **Expander Blocks:** Clicking on *Show raw output* smoothly slides down a dark, rich container containing the exact background shell command and the standard console responses for advanced auditing.

---

## 🛠️ Technology Stack

*   **Framework:** .NET 9.0 & Avalonia UI (MVVM Architecture)
*   **Platforms:** Native macOS (Apple Silicon M1/M2/M3 & Intel), Windows (Ready for Cross-Platform deployment)
*   **Guidelines followed:** Clean Code, Separation of Concerns, and Open-Closed Principles.

---

*PromptShell is built to make computing accessible, automated, and secure. Stay tuned as we build the future of desktop interaction!*