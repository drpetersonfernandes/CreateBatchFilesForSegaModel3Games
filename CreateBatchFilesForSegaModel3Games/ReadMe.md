# Create Batch Files for Sega Model 3 Games

## Overview

This application simplifies the process of creating batch files for launching Sega Model 3 games using the Supermodel emulator. It allows users to select the Supermodel executable and a folder containing ROM files, and then automatically generates batch files for each ROM. The application also includes silent bug reporting to assist with development and maintenance.

## Features

*   **User-Friendly Interface:** Simple and intuitive graphical interface with a top menu and status bar for easy operation.
*   **Supermodel Executable Selection:** Allows users to select the path to the `Supermodel.exe` emulator.
*   **ROM Folder Selection:** Enables users to specify the folder containing their Sega Model 3 ROM zip files.
*   **Automatic Batch File Generation:** Creates batch files for each ROM in the selected folder, configured to launch the game in fullscreen mode with FPS display.
*   **Logging:** Provides a log window to display status messages, errors, and warnings.
*   **Status Bar:** Displays real-time status messages about the application's state.
*   **Silent Bug Reporting:** Automatically reports unhandled exceptions and errors to a remote API for debugging and improvement purposes.

## Usage

1.  **Select Supermodel Executable:** Click the "Browse" button next to "Supermodel Path" and select the `Supermodel.exe` file.
2.  **Select ROM Folder:** Click the "Browse" button next to "ROM Folder" and select the folder containing your Sega Model 3 ROM zip files.
3.  **Create Batch Files:** Click the "Create Batch Files" button to generate the batch files. The created batch files will be located in the same folder as your ROM zip files.
4.  **Check Log:** Monitor the log text box for any messages, warnings, or errors during the process.

## System Requirements

-   Windows Operating System
-   .NET 9 Desktop Runtime

## Technical Details

### Technologies Used

*   **C#:** The primary programming language.
*   **WPF (Windows Presentation Foundation):** Used for building the graphical user interface.
*   **.NET 9:** The application targets the .NET 9 runtime.
*   **System.IO:** Used for file and directory operations.
*   **System.Net.Http:** Used for sending bug reports to the API.

### File Structure

*   **App.xaml/App.xaml.cs:** Defines the application entry point, handles global exception handling, and centrally manages the `BugReportService`.
*   **MainWindow.xaml/MainWindow.xaml.cs:** Contains the main application window, including UI elements and event handlers for creating batch files.
*   **AboutWindow.xaml/AboutWindow.xaml.cs:** A separate window that displays application information, version, and credits.
*   **BugReportService.cs:** Implements a service for silently sending bug reports to a remote API using a shared, static `HttpClient`.

### Bug Reporting

The application includes a `BugReportService` that silently sends bug reports to a remote API. This helps in identifying and fixing issues without requiring user intervention. The API configuration is centralized in `App.xaml.cs`.

**Important:** The `BugReportApiUrl` and `BugReportApiKey` are placeholders. A valid API key is required for the bug reporting feature to function correctly.
