using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace CreateBatchFilesForSegaModel3Games;

public partial class MainWindow
{
    // The local BugReportService instance and constants have been removed.
    // The service is now accessed via App.BugReportService.

    public MainWindow()
    {
        InitializeComponent();

        LogMessage("Welcome to the Batch File Creator for Sega Model 3 Games.");
        LogMessage("");
        LogMessage("This program creates batch files to launch your Sega Model 3 games.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the Supermodel emulator executable file (Supermodel.exe)");
        LogMessage("2. Select the folder containing your Sega Model 3 ROM zip files");
        LogMessage("3. Click 'Create Batch Files' to generate the batch files");
        LogMessage("");
        UpdateStatusBarMessage("Ready");
    }

    private void UpdateStatusBarMessage(string message)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            StatusBarMessage.Text = message;
        });
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // This is the standard, graceful way to shut down a WPF application.
        // The redundant and forceful Environment.Exit(0) has been removed.
        Application.Current.Shutdown();
    }

    private void LogMessage(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        });
    }

    private void BrowseSupermodelButton_Click(object sender, RoutedEventArgs e)
    {
        var supermodelExePath = SelectFile();
        if (string.IsNullOrEmpty(supermodelExePath)) return;

        SupermodelPathTextBox.Text = supermodelExePath;
        LogMessage($"Supermodel executable selected: {supermodelExePath}");
        UpdateStatusBarMessage("Supermodel executable selected.");

        if (supermodelExePath.EndsWith("Supermodel.exe", StringComparison.OrdinalIgnoreCase)) return;

        LogMessage("Warning: The selected file does not appear to be Supermodel.exe.");
        _ = ReportBugAsync("User selected a file that doesn't appear to be Supermodel.exe: " + supermodelExePath);
    }

    private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var romFolder = SelectFolder();
        if (string.IsNullOrEmpty(romFolder)) return;

        RomFolderTextBox.Text = romFolder;
        LogMessage($"ROM folder selected: {romFolder}");
        UpdateStatusBarMessage("ROM folder selected.");
    }

    private async void CreateBatchFilesButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var supermodelExePath = SupermodelPathTextBox.Text;
            var romFolder = RomFolderTextBox.Text;

            if (string.IsNullOrEmpty(supermodelExePath))
            {
                LogMessage("Error: No Supermodel executable selected.");
                ShowError("Please select the Supermodel executable file (Supermodel.exe).");
                UpdateStatusBarMessage("Error: Supermodel executable not selected.");
                return;
            }

            if (!File.Exists(supermodelExePath))
            {
                LogMessage($"Error: Supermodel executable not found at path: {supermodelExePath}");
                ShowError("The selected Supermodel executable file does not exist.");
                await ReportBugAsync("Supermodel executable not found", new FileNotFoundException("The Supermodel executable was not found", supermodelExePath));
                UpdateStatusBarMessage("Error: Supermodel executable not found.");
                return;
            }

            if (string.IsNullOrEmpty(romFolder))
            {
                LogMessage("Error: No ROM folder selected.");
                ShowError("Please select the folder containing your Sega Model 3 ROM zip files.");
                UpdateStatusBarMessage("Error: ROM folder not selected.");
                return;
            }

            if (!Directory.Exists(romFolder))
            {
                LogMessage($"Error: ROM folder not found at path: {romFolder}");
                ShowError("The selected ROM folder does not exist.");
                await ReportBugAsync("ROM folder not found", new DirectoryNotFoundException($"ROM folder not found: {romFolder}"));
                UpdateStatusBarMessage("Error: ROM folder not found.");
                return;
            }

            try
            {
                CreateBatchFilesForModel3Games(romFolder, supermodelExePath);
            }
            catch (Exception ex)
            {
                LogMessage($"Error creating batch files: {ex.Message}");
                ShowError($"An error occurred while creating batch files: {ex.Message}");
                await ReportBugAsync("Error creating batch files", ex);
                UpdateStatusBarMessage("Process failed with an error.");
            }
        }
        catch (Exception ex)
        {
            await ReportBugAsync("Error creating batch files", ex);
            UpdateStatusBarMessage("An unexpected error occurred.");
        }
    }

    private static string? SelectFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Please select the folder where your Sega Model 3 ROM zip files are located."
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    private static string? SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Please select the Supermodel executable file (Supermodel.exe)",
            Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private void CreateBatchFilesForModel3Games(string romFolder, string supermodelExePath)
    {
        try
        {
            var romFiles = Directory.GetFiles(romFolder, "*.zip");
            var filesCreated = 0;

            LogMessage("");
            LogMessage("Starting batch file creation process...");
            UpdateStatusBarMessage("Creating batch files...");

            if (romFiles.Length == 0)
            {
                LogMessage("No ROM zip files found. No batch files were created.");
                ShowError("No ROM zip files found. No batch files were created.");
                UpdateStatusBarMessage("No ROM zip files found.");
                _ = ReportBugAsync("No ROM zip files found in selected folder",
                    new FileNotFoundException("No *.zip files found in ROM folder", romFolder));
                return;
            }

            foreach (var romFilePath in romFiles)
            {
                try
                {
                    var romFileName = Path.GetFileNameWithoutExtension(romFilePath);
                    var batchFilePath = Path.Combine(romFolder, romFileName + ".bat");

                    using (StreamWriter sw = new(batchFilePath))
                    {
                        sw.WriteLine("@echo off");
                        sw.WriteLine($"cd /d \"{Path.GetDirectoryName(supermodelExePath)}\"");
                        sw.WriteLine($"start \"\" \"{Path.GetFileName(supermodelExePath)}\" \"{romFilePath}\" -fullscreen -show-fps");
                    }

                    LogMessage($"Batch file created: {batchFilePath}");
                    filesCreated++;
                }
                catch (Exception ex)
                {
                    LogMessage($"Error creating batch file for {romFilePath}: {ex.Message}");
                    _ = ReportBugAsync($"Error creating batch file for {Path.GetFileName(romFilePath)}", ex);
                }
            }

            if (filesCreated > 0)
            {
                LogMessage("");
                LogMessage($"{filesCreated} batch files have been successfully created.");
                LogMessage("They are located in the same folder as your ROM zip files.");
                UpdateStatusBarMessage($"{filesCreated} batch files created successfully.");

                ShowMessageBox($"{filesCreated} batch files have been successfully created.\n\n" +
                               "They are located in the same folder as your ROM zip files.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                LogMessage("Failed to create any batch files.");
                ShowError("Failed to create any batch files.");
                UpdateStatusBarMessage("Failed to create any batch files.");
                _ = ReportBugAsync("Failed to create any batch files despite finding zip files",
                    new Exception($"Found {romFiles.Length} zip files but created 0 batch files"));
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error accessing ROM folder: {ex.Message}");
            UpdateStatusBarMessage("Error accessing ROM folder.");
            _ = ReportBugAsync("Error accessing ROM folder during batch file creation", ex);
            throw; // Rethrow to be caught by the outer try-catch
        }
    }

    private void ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        Dispatcher.Invoke(() =>
            MessageBox.Show(this, message, title, buttons, icon));
    }

    private void ShowError(string message)
    {
        ShowMessageBox(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async Task ReportBugAsync(string message, Exception? exception = null)
    {
        try
        {
            var fullReport = new StringBuilder();
            var assemblyName = GetType().Assembly.GetName();

            // Add system information
            fullReport.AppendLine("=== Bug Report ===");
            fullReport.AppendLine(CultureInfo.InvariantCulture, $"Application: {assemblyName.Name}");
            fullReport.AppendLine(CultureInfo.InvariantCulture, $"Version: {assemblyName.Version}");
            fullReport.AppendLine(CultureInfo.InvariantCulture, $"OS: {Environment.OSVersion}");
            fullReport.AppendLine(CultureInfo.InvariantCulture, $".NET Version: {Environment.Version}");
            fullReport.AppendLine(CultureInfo.InvariantCulture, $"Date/Time: {DateTime.Now}");
            fullReport.AppendLine();

            // Add a message
            fullReport.AppendLine("=== Error Message ===");
            fullReport.AppendLine(message);
            fullReport.AppendLine();

            // Add exception details if available
            if (exception != null)
            {
                fullReport.AppendLine("=== Exception Details ===");
                fullReport.AppendLine(CultureInfo.InvariantCulture, $"Type: {exception.GetType().FullName}");
                fullReport.AppendLine(CultureInfo.InvariantCulture, $"Message: {exception.Message}");
                fullReport.AppendLine(CultureInfo.InvariantCulture, $"Source: {exception.Source}");
                fullReport.AppendLine("Stack Trace:");
                fullReport.AppendLine(exception.StackTrace);

                // Add inner exception if available
                if (exception.InnerException != null)
                {
                    fullReport.AppendLine("Inner Exception:");
                    fullReport.AppendLine(CultureInfo.InvariantCulture, $"Type: {exception.InnerException.GetType().FullName}");
                    fullReport.AppendLine(CultureInfo.InvariantCulture, $"Message: {exception.InnerException.Message}");
                    fullReport.AppendLine("Stack Trace:");
                    fullReport.AppendLine(exception.InnerException.StackTrace);
                }
            }

            // Add log contents if available
            if (LogTextBox != null)
            {
                var logContent = await Dispatcher.InvokeAsync(() => LogTextBox.Text);
                if (!string.IsNullOrEmpty(logContent))
                {
                    fullReport.AppendLine().AppendLine("=== Application Log ===").Append(logContent);
                }
            }

            // Add Supermodel and ROM folder paths if available
            if (SupermodelPathTextBox != null && RomFolderTextBox != null)
            {
                var (supermodelPath, romFolderPath) = await Dispatcher.InvokeAsync(() => (SupermodelPathTextBox.Text, RomFolderTextBox.Text));
                fullReport.AppendLine().AppendLine("=== Configuration ===").AppendLine(CultureInfo.InvariantCulture, $"Supermodel Path: {supermodelPath}").AppendLine(CultureInfo.InvariantCulture, $"ROM Folder: {romFolderPath}");
            }

            // Silently send the report using the shared service from the App class
            if (App.BugReportService != null)
            {
                await App.BugReportService.SendBugReportAsync(fullReport.ToString());
            }
        }
        catch
        {
            // Silently fail if error reporting itself fails
        }
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            LogMessage($"Error opening About window: {ex.Message}");
            _ = ReportBugAsync("Error opening About window", ex);
        }
    }
}
