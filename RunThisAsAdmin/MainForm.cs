using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using static RunThisAsAdmin.Program;

namespace RunThisAsAdmin;

internal partial class MainForm : Form
{
    private AboutForm FormAboutForm { get; set; }
    private AdminCommand CommandType { get; }
    private string CommandPath { get; }
    private string CommandArguments { get; }
    private static bool IsFormAboutFormVisible => Application.OpenForms.OfType<AboutForm>().FirstOrDefault()?.Visible ?? false;
    private bool IsOpenFileDialogRunFileVisible { get; set; }
    private bool IsOpenFileDialogDeleteFileVisible { get; set; }
    private bool IsFolderBrowserDialogDeleteFolderVisible { get; set; }

    public MainForm(AdminCommand commandType = AdminCommand.None, string commandPath = "", string commandArguments = "")
    {
        InitializeComponent();
        Size = new Size(0, 0);

        CommandType = commandType;
        CommandPath = commandPath.Trim();
        CommandArguments = commandArguments.Trim();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            // Change extended window style to WS_EX_TOOLWINDOW to prevent the window from appearing in the Windows Task Manager.
            var cp = base.CreateParams;
            cp.ExStyle |= 0x80; // Turn on WS_EX_TOOLWINDOW
            return cp;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        var contextMenuStrip = new ContextMenuStrip();
        contextMenuStrip.Items.Add("Run file", Properties.Resources.Select, OnRunFileClicked);
        contextMenuStrip.Items.Add(new ToolStripSeparator());
        var deleteItem = contextMenuStrip.Items.Add("Delete", Properties.Resources.Delete);
        (deleteItem as ToolStripMenuItem)?.DropDownItems.AddRange(
            new ToolStripItem[]
            {
                new ToolStripMenuItem("Delete file", Properties.Resources.DeleteFile, OnDeleteFileClicked),
                new ToolStripMenuItem("Delete folder", Properties.Resources.DeleteFolder, OnDeleteFolderClicked)
            });
        var deleteItemSeparator = contextMenuStrip.Items.Add(new ToolStripSeparator());
        var settingsItem = contextMenuStrip.Items.Add("Settings", Properties.Resources.Settings);
        var contextMenuItem = ((ToolStripMenuItem)settingsItem).DropDownItems.Add("Context menu", Properties.Resources.Menu);
        (contextMenuItem as ToolStripMenuItem)?.DropDownItems.AddRange(
            new ToolStripItem[]
            {
                new ToolStripMenuItem("Add shortcuts", Properties.Resources.Add, OnAddShortcutsClicked),
                new ToolStripMenuItem("Remove shortcuts", Properties.Resources.Remove, OnRemoveShortcutsClicked)
            });
        contextMenuStrip.Items.Add(new ToolStripSeparator());
        contextMenuStrip.Items.Add("About", Properties.Resources.Info, OnAboutClicked);
        contextMenuStrip.Items.Add(new ToolStripSeparator());
        contextMenuStrip.Items.Add("Exit", Properties.Resources.Exit, OnExitClicked);
        contextMenuStrip.Opening += (obj, eventArgs) =>
        {
            deleteItem.Available = ModifierKeys == Keys.Shift;
            contextMenuStrip.Items[deleteItemSeparator].Available = ModifierKeys == Keys.Shift;

            Location = Screen.PrimaryScreen.WorkingArea.Location;
            Activate();
        };
        var notifyIcon = new NotifyIcon();
        notifyIcon.ContextMenuStrip = contextMenuStrip;
        notifyIcon.Icon = Properties.Resources.Icon;
        notifyIcon.Text = $@"{Program.ProductName} {Program.ProductVersion}";
        notifyIcon.Visible = true;
        notifyIcon.MouseDoubleClick += OnRunFileClicked;
        notifyIcon.BalloonTipTitle = @"Welcome!";
        notifyIcon.BalloonTipText = @"The application started and running in the background.";
        notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
        notifyIcon.ShowBalloonTip(10000);

        if (CommandType != AdminCommand.None &&
            CommandPath != string.Empty)
        {
            ExecuteCommand(CommandType, CommandPath, CommandArguments);
        }

        var isPipeRunning = Directory.GetFiles(@"\\.\\pipe\\").Contains($@"\\.\\pipe\\{PipeName}");
        if (!isPipeRunning)
        {
            StartServerAsync();
        }
    }

    private void OnRunFileClicked(object sender, EventArgs e)
    {
        if (IsOpenFileDialogRunFileVisible)
            return;

        IsOpenFileDialogRunFileVisible = true;

        var openFileDialogRunFile = new OpenFileDialog
        {
            Title = @"Run file",
            Multiselect = false,
            Filter = @"All files (*.*)|*.*",
            FilterIndex = 1,
        };
        if (openFileDialogRunFile.ShowDialog() == DialogResult.OK)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = openFileDialogRunFile.FileName,
                UseShellExecute = true,
            };
            Process.Start(processStartInfo);
        }

        IsOpenFileDialogRunFileVisible = false;
    }

    private void OnDeleteFileClicked(object sender, EventArgs e)
    {
        if (IsOpenFileDialogDeleteFileVisible)
            return;

        IsOpenFileDialogDeleteFileVisible = true;

        var openFileDialogDeleteFile = new OpenFileDialog
        {
            Title = @"Delete file",
            Multiselect = false,
            Filter = @"All files (*.*)|*.*",
            FilterIndex = 1,
        };
        if (openFileDialogDeleteFile.ShowDialog() == DialogResult.OK)
        {
            DeleteFile(openFileDialogDeleteFile.FileName);
        }

        IsOpenFileDialogDeleteFileVisible = false;
    }

    private void OnDeleteFolderClicked(object sender, EventArgs e)
    {
        if (IsFolderBrowserDialogDeleteFolderVisible)
            return;

        IsFolderBrowserDialogDeleteFolderVisible = true;

        var folderBrowserDialogDeleteFolder = new FolderBrowserDialog
        {
            Description = @"Select the folder that you want to delete.",
            ShowNewFolderButton = false,
        };
        if (folderBrowserDialogDeleteFolder.ShowDialog() == DialogResult.OK)
        {
            DeleteDirectory(folderBrowserDialogDeleteFolder.SelectedPath);
        }

        IsFolderBrowserDialogDeleteFolderVisible = false;
    }

    private void OnAddShortcutsClicked(object sender, EventArgs e)
    {
        try
        {
            AddShortcutRunFile();
            AddShortcutDeleteFile();
            AddShortcutDeleteLnkFile();
            AddShortcutDeleteDirectory();
        }
        catch
        {
            ShowErrorMessage("Failed to add context menu shortcuts");
        }
    }

    private void OnRemoveShortcutsClicked(object sender, EventArgs e)
    {
        try
        {
            RemoveShortcutRunFile();
            RemoveShortcutDeleteFile();
            RemoveShortcutDeleteLnkFile();
            RemoveShortcutDeleteDirectory();
        }
        catch
        {
            ShowErrorMessage("Failed to remove context menu shortcuts");
        }
    }

    private async void StartServerAsync()
    {
        while (true)
        {
            var pipeSecurity = new PipeSecurity();
            pipeSecurity.SetAccessRule(
                new PipeAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    PipeAccessRights.ReadWrite,
                    AccessControlType.Allow));

            using var namedPipeServerStream = new NamedPipeServerStream(
                PipeName,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Message,
                PipeOptions.None,
                1,
                0,
                pipeSecurity);

            await namedPipeServerStream.WaitForConnectionAsync();
            using var streamReader = new StreamReader(namedPipeServerStream);

            // Read command type
            var commandType = await streamReader.ReadLineAsync();
            if (commandType == null ||
                !Enum.TryParse(commandType, true, out AdminCommand type))
            {
                namedPipeServerStream.Disconnect();
                continue;
            }

            // Read command path
            var commandPath = await streamReader.ReadLineAsync();
            if (commandPath == null)
            {
                namedPipeServerStream.Disconnect();
                continue;
            }

            // Read command arguments
            var commandArguments = await streamReader.ReadLineAsync();
            if (commandArguments == null)
            {
                commandArguments = "";
            }

            try
            {
                ExecuteCommand(type, commandPath, commandArguments);
            }
            catch (Exception exception)
            {
                ShowErrorMessage(exception.Message);
            }
            finally
            {
                namedPipeServerStream.Disconnect();
            }
        }
    }

    private void ExecuteCommand(AdminCommand type, string path, string arguments)
    {
        switch (type)
        {
            case AdminCommand.Run:
                RunFile(path, arguments);
                break;

            case AdminCommand.DeleteFile:
                DeleteFile(path);
                break;

            case AdminCommand.DeleteDirectory:
                DeleteDirectory(path);
                break;
        }
    }

    private void RunFile(string filePath, string arguments)
    {
        if (!File.Exists(filePath))
        {
            ShowErrorMessage("File not found!");
            return;
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true,
            Arguments = arguments,
        };
        Process.Start(processStartInfo);
    }

    private void DeleteFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            ShowErrorMessage("File not found!");
            return;
        }

        Activate();
        var dialogResult = MessageBox.Show(
            @"  Are you sure you want to delete this file?" +
            Environment.NewLine +
            @$"  '{filePath}'",
            $@"{Program.ProductName} - Delete File",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (dialogResult == DialogResult.Yes)
            File.Delete(filePath);
    }

    private void DeleteDirectory(string directoryPath, bool recursive = true)
    {
        if (!Directory.Exists(directoryPath))
        {
            ShowErrorMessage("Directory not found!");
            return;
        }

        Activate();
        var dialogResult = MessageBox.Show(
            @"  Are you sure you want to delete this folder?" +
            Environment.NewLine +
            @$"  '{directoryPath}'",
            $@"{Program.ProductName} - Delete Folder",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (dialogResult == DialogResult.Yes)
            Directory.Delete(directoryPath, recursive);
    }

    private void AddShortcutRunFile()
    {
        var executableFilePath = Application.ExecutablePath;

        //HKEY_CLASSES_ROOT\*
        var key = Registry.ClassesRoot.OpenSubKey("*", true);
        //HKEY_CLASSES_ROOT\*\shell
        var shellKey = key!.OpenSubKey("shell", true) ?? key.CreateSubKey("shell");
        //HKEY_CLASSES_ROOT\*\shell\0_RunThisAsAdmin
        key = shellKey?.CreateSubKey(RegistryKeyRunName);
        //HKEY_CLASSES_ROOT\*\shell\0_RunThisAsAdmin\(Icon)
        key?.SetValue("Icon", $"\"{executableFilePath}\",0", RegistryValueKind.ExpandString);
        //HKEY_CLASSES_ROOT\*\shell\0_RunThisAsAdmin\(MUIVerb)
        key?.SetValue("MUIVerb", "Run this as admin", RegistryValueKind.String);
        //HKEY_CLASSES_ROOT\*\shell\0_RunThisAsAdmin\command
        key = key?.CreateSubKey("command", true);
        //HKEY_CLASSES_ROOT\*\shell\0_RunThisAsAdmin\command\()
        key?.SetValue("", $"\"{executableFilePath}\" {AdminCommand.Run} \"%1\" \"%*\"", RegistryValueKind.ExpandString);
    }

    private void AddShortcutDeleteFile()
    {
        var executableFilePath = Application.ExecutablePath;

        //HKEY_CLASSES_ROOT\*
        var key = Registry.ClassesRoot.OpenSubKey("*", true);
        //HKEY_CLASSES_ROOT\*\shell
        var shellKey = key!.OpenSubKey("shell", true) ?? key.CreateSubKey("shell");
        //HKEY_CLASSES_ROOT\*\shell\1_RunThisAsAdmin
        key = shellKey?.CreateSubKey(RegistryKeyDeleteName);
        //HKEY_CLASSES_ROOT\*\shell\1_RunThisAsAdmin\(Icon)
        key?.SetValue("Icon", $"\"{executableFilePath}\",0", RegistryValueKind.ExpandString);
        //HKEY_CLASSES_ROOT\*\shell\1_RunThisAsAdmin\(MUIVerb)
        key?.SetValue("MUIVerb", "Delete this as admin", RegistryValueKind.String);
        //HKEY_CLASSES_ROOT\*\shell\1_RunThisAsAdmin\(Extended)
        key?.SetValue("Extended", "", RegistryValueKind.String);
        //HKEY_CLASSES_ROOT\*\shell\1_RunThisAsAdmin\command
        key = key?.CreateSubKey("command", true);
        //HKEY_CLASSES_ROOT\*\shell\1_RunThisAsAdmin\command\()
        key?.SetValue("", $"\"{executableFilePath}\" {AdminCommand.DeleteFile} \"%1\"", RegistryValueKind.String);
    }

    private void AddShortcutDeleteLnkFile()
    {
        var executableFilePath = Application.ExecutablePath;

        //HKEY_CLASSES_ROOT\lnkfile
        var key = Registry.ClassesRoot.OpenSubKey("lnkfile", true);
        //HKEY_CLASSES_ROOT\lnkfile\shell
        var shellKey = key!.OpenSubKey("shell", true) ?? key.CreateSubKey("shell");
        //HKEY_CLASSES_ROOT\lnkfile\shell\1_RunThisAsAdmin
        key = shellKey?.CreateSubKey(RegistryKeyDeleteName);
        //HKEY_CLASSES_ROOT\lnkfile\shell\1_RunThisAsAdmin\(Icon)
        key?.SetValue("Icon", $"\"{executableFilePath}\",0", RegistryValueKind.ExpandString);
        //HKEY_CLASSES_ROOT\lnkfile\shell\1_RunThisAsAdmin\(MUIVerb)
        key?.SetValue("MUIVerb", "Delete this as admin", RegistryValueKind.String);
        //HKEY_CLASSES_ROOT\lnkfile\shell\1_RunThisAsAdmin\(Extended)
        key?.SetValue("Extended", "", RegistryValueKind.String);
        //HKEY_CLASSES_ROOT\lnkfile\shell\1_RunThisAsAdmin\command
        key = key?.CreateSubKey("command", true);
        //HKEY_CLASSES_ROOT\lnkfile\shell\1_RunThisAsAdmin\command\()
        key?.SetValue("", $"\"{executableFilePath}\" {AdminCommand.DeleteFile} \"%1\"", RegistryValueKind.String);
    }

    private void AddShortcutDeleteDirectory()
    {
        var executableFilePath = Application.ExecutablePath;

        //HKEY_CLASSES_ROOT\Directory
        var key = Registry.ClassesRoot.OpenSubKey("Directory", true);
        //HKEY_CLASSES_ROOT\Directory\shell
        var shellKey = key!.OpenSubKey("shell", true) ?? key.CreateSubKey("shell");
        //HKEY_CLASSES_ROOT\Directory\shell\1_RunThisAsAdmin
        key = shellKey?.CreateSubKey(RegistryKeyDeleteName);
        //HKEY_CLASSES_ROOT\Directory\shell\1_RunThisAsAdmin\(Icon)
        key?.SetValue("Icon", $"\"{executableFilePath}\",0", RegistryValueKind.ExpandString);
        //HKEY_CLASSES_ROOT\Directory\shell\1_RunThisAsAdmin\(MUIVerb)
        key?.SetValue("MUIVerb", "Delete this as admin", RegistryValueKind.String);
        //HKEY_CLASSES_ROOT\Directory\shell\1_RunThisAsAdmin\(Extended)
        key?.SetValue("Extended", "", RegistryValueKind.String);
        //HKEY_CLASSES_ROOT\Directory\shell\1_RunThisAsAdmin\command
        key = key?.CreateSubKey("command", true);
        //HKEY_CLASSES_ROOT\Directory\shell\1_RunThisAsAdmin\command\()
        key?.SetValue("", $"\"{executableFilePath}\" {AdminCommand.DeleteDirectory} \"%1\"", RegistryValueKind.String);
    }

    private void RemoveShortcutRunFile()
    {
        //HKEY_CLASSES_ROOT\*\shell
        var key = Registry.ClassesRoot.OpenSubKey("*")!.OpenSubKey("shell", true);
        //HKEY_CLASSES_ROOT\*\shell\0_RunThisAsAdmin
        if (key?.OpenSubKey(RegistryKeyRunName) != null)
        {
            key.DeleteSubKeyTree(RegistryKeyRunName);
        }
    }

    private void RemoveShortcutDeleteFile()
    {
        //HKEY_CLASSES_ROOT\*\shell
        var key = Registry.ClassesRoot.OpenSubKey("*")!.OpenSubKey("shell", true);
        //HKEY_CLASSES_ROOT\*\shell\1_RunThisAsAdmin
        if (key?.OpenSubKey(RegistryKeyDeleteName) != null)
        {
            key.DeleteSubKeyTree(RegistryKeyDeleteName);
        }
    }

    private void RemoveShortcutDeleteLnkFile()
    {
        //HKEY_CLASSES_ROOT\lnkfile\shell
        var key = Registry.ClassesRoot.OpenSubKey("lnkfile")!.OpenSubKey("shell", true);
        //HKEY_CLASSES_ROOT\lnkfile\shell\1_RunThisAsAdmin
        if (key?.OpenSubKey(RegistryKeyDeleteName) != null)
        {
            key.DeleteSubKeyTree(RegistryKeyDeleteName);
        }
    }

    private void RemoveShortcutDeleteDirectory()
    {
        //HKEY_CLASSES_ROOT\Directory\shell
        var key = Registry.ClassesRoot.OpenSubKey("Directory")!.OpenSubKey("shell", true);
        //HKEY_CLASSES_ROOT\Directory\shell\1_RunThisAsAdmin
        if (key?.OpenSubKey(RegistryKeyDeleteName) != null)
        {
            key.DeleteSubKeyTree(RegistryKeyDeleteName);
        }
    }

    private void OnAboutClicked(object sender, EventArgs e)
    {
        if (!IsFormAboutFormVisible)
        {
            FormAboutForm = new AboutForm();
        }

        FormAboutForm.Show();
        FormAboutForm.BringToFront();
    }

    private void OnExitClicked(object sender, EventArgs e)
    {
        Close();
    }
}
