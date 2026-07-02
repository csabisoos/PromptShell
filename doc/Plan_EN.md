# PromptShell Development Checklist (Plan.md)

This document serves as a step-by-step development checklist for PromptShell. The tasks are optimized to allow the underlying business logic and the user interface to evolve in parallel while maintaining a clean separation of concerns (MVVM, Clean Code).

## Phase 1: Core Terminal Logic (Console / Service-Oriented Approach)
- [ ] **Create the foundational terminal service (`ITerminalService`)**
  - Implement an asynchronous service within the `Services` folder that utilizes `System.Diagnostics.Process` to launch `/bin/zsh`. It should accept a string input command and return the captured `stdout` and `stderr` outputs.
- [ ] **Validate via Console/Tests (UI-Independent)**
  - Before binding the service to the UI, thoroughly test `TerminalService` using a temporary console entry point or by adding automated tests inside the existing `PromptShell.Tests` project. Ensure that raw commands (e.g., `ls -la`) execute reliably and return the output string without any thread deadlocks.

## Phase 2: Avalonia UI Foundations (Phase 1.5)
- [ ] **Establish UI and MVVM Data Binding (`MainWindow.axaml` & `MainWindowViewModel`)**
  - Design a lightweight, functional layout: an input `TextBox` for entering text/commands and a read-only, scrollable container (`TextBox` with text wrapping or a `TextBlock` inside a `ScrollViewer`) for displaying outputs. Ensure strict adherence to `CompiledBindings`.
- [ ] **Connect `ITerminalService` to the Avalonia Window**
  - Invoke the terminal service directly from the ViewModel via a `[RelayCommand]`. At this stage, any entered string will run directly as a raw terminal command, and its `plain output` will be immediately printed to the UI. This provides a reliable graphical shell wrapper before introducing AI components.

## Phase 3: Input Interpretation via Local LLM (Phase 2)
- [ ] **Integrate the Ollama HTTP Client (`IOllamaService`)**
  - Configure an asynchronous `HttpClient` to communicate with the local Ollama REST API (`http://localhost:11434/api/generate` or `/api/chat`), parsing JSON data structures natively using `System.Text.Json`.
- [ ] **Design the Command-Generation Prompt (System Prompt)**
  - Author a strict System Prompt that forces the local model (e.g., Llama 3 or Mistral) to interpret natural language requests (e.g., *"What is my local IP address?"*) and return *only* the executable terminal command (e.g., `ifconfig` or `ipconfig`), stripping out any markdown formatting or introductory text.
- [ ] **Link the End-to-End Workflow (Human -> AI -> Terminal -> Plain Output)**
  - Intercept the user input from the UI, pass it to `IOllamaService` to resolve the actual bash/zsh command, feed that command into `ITerminalService`, and safely display the raw terminal output back to the user.

## Phase 4: UI Enhancements & Flow Control (Phase 2.5)
- [ ] **Implement Asynchronous State Handling (Loading / Busy States)**
  - Introduce an `IsBusy` property into the ViewModel. Use it to disable input fields during active operations and display a `ProgressBar` or loading spinner while the AI is generating or the terminal process is running.
- [ ] **Incorporate a Command Preview Panel**
  - Display the exact command suggested by the AI in a dedicated, subtle UI label before it gets executed. (Optionally, add a "Confirm and Execute" button for safety-critical interactions).

## Phase 5: Intelligent Output Analysis (Phase 3)
- [ ] **Build the Output Interpreter Service (`IResultInterpreterService`)**
  - Capture both the raw terminal output and its exit code. Bundle them into a secondary prompt payload and transmit it back to Ollama for evaluation.
- [ ] **Display Human-Readable Summaries on the UI**
  - Instruct the AI to translate cryptic shell dumps or verbose error stack traces into clear, actionable human phrasing (e.g., *"The git push failed because no upstream branch is configured. Run: ..."*). Render this polished explanation prominently alongside or in place of the raw console logs.

## Phase 6: UI Refinement & Production Styling (Phase 3.5)
- [ ] **Finalize Theme, Fluent Design, and Dark Mode**
  - Style PromptShell into a sleek, professional application. Apply a monospace font family (e.g., Cascadia Code, JetBrains Mono) for console elements, configure comfortable paddings, and implement consistent borders aligned with the macOS desktop guidelines.
- [ ] **Add Quality-of-Life (UX) Features**
  - Implement a Command History tracker (allowing users to navigate past prompts using the Up/Down arrow keys).
  - Add a "Copy to Clipboard" button for quick command and output extraction.
  - Integrate a "Clear Console" action to flush the current session view.

## Phase 7: Compilation, Publishing, and Deployment (Phase 4)
- [ ] **Configure Self-Contained Single File Builds**
  - Parameterize the `dotnet publish` command (e.g., `-r osx-arm64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true`) to compile a compressed standalone binary that runs on target devices without requiring a pre-installed .NET Runtime.
- [ ] **Assemble the macOS `.app` Bundle Structure**
  - Organize the compiled production binaries into a native macOS folder structure (`PromptShell.app/Contents/MacOS/`).
  - Wire up the `Info.plist` configuration metadata and associate a high-resolution icon asset (`.icns`) for an integrated desktop appearance.
