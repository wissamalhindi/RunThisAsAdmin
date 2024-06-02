using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace RunThisAsAdmin;

internal static class Program
{
    internal static string ApplicationPath => Assembly.GetExecutingAssembly().Location;
    internal static string ProductName => Application.ProductName;
    internal static string ProductVersion => Application.ProductVersion.Substring(0, Application.ProductVersion.IndexOf('+'));
    internal static string PipeName => $"{ProductName}_NamedPipe";
    internal static string RegistryKeyRunName => $"0_{ProductName}";
    internal static string RegistryKeyDeleteName => $"1_{ProductName}";
    private static bool IsInRoleAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    private static string[] CommandLineArgs { get; set; }
    private static string CommandLineString { get; set; }

    public enum AdminCommand
    {
        None,
        Run,
        DeleteFile,
        DeleteDirectory,
    }

    [STAThread]
    public static void Main()
    {
        if (!Debugger.IsAttached)
        {
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        CommandLineArgs = Environment.GetCommandLineArgs();

        // Remove executing assembly path from commandLine
        var exePath = "\"" + CommandLineArgs[0] + "\"";
        var commandLine = Environment.CommandLine;
        var exePathIndex = commandLine.IndexOf(exePath, StringComparison.Ordinal);
        CommandLineString = exePathIndex == 0 ?
            commandLine.Remove(exePathIndex, exePath.Length).Trim() :
            commandLine.Trim();

        try
        {
            // Add a small delay to avoid the race condition between the first start and the elevated self start
            Thread.Sleep(500);

            // Check if there is another instance of the application running
            var processesCount = Process.GetProcesses().Count(process => process.ProcessName.Contains(ProductName));
            if (processesCount > 1)
            {
                // Run in Client mode
                StartClient(PipeName, CommandLineArgs.Skip(1).ToArray());
            }
            else
            {
                // Run in Server mode
                if (IsInRoleAdministrator)
                {
                    if (CommandLineArgs.Length > 2 &&
                        Enum.TryParse(CommandLineArgs[1], true, out AdminCommand type))
                    {
                        var arguments = CommandLineArgs.Length > 3 ? CommandLineArgs[3] : "";
                        Application.Run(new MainForm(type, CommandLineArgs[2], arguments));
                    }
                    else
                    {
                        Application.Run(new MainForm());
                    }
                }
                else
                {
                    // Start a new instance of the application as elevated and close the current one
                    try
                    {
                        var processStartInfo = new ProcessStartInfo
                        {
                            FileName = ApplicationPath,
                            Verb = "runas",
                            UseShellExecute = true,
                            Arguments = CommandLineString,
                        };
                        Process.Start(processStartInfo);
                    }
                    catch (Win32Exception exception)
                    {
                        const int ERROR_CANCELLED = 1223; // "The operation was canceled by the user"

                        // Check if the exception is "ERROR_CANCELLED" ignore it
                        if (exception.NativeErrorCode != ERROR_CANCELLED)
                            throw;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            ShowErrorMessage(exception.Message);
        }
    }

    private static void StartClient(string pipeName, string[] arguments)
    {
        var isMyPipeRunning = Directory.GetFiles(@"\\.\\pipe\\").Contains($@"\\.\\pipe\\{pipeName}");
        if (!isMyPipeRunning)
            return;

        using var namedPipeClientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.None);
        namedPipeClientStream.Connect(2000);

        if (!namedPipeClientStream.IsConnected)
            return;

        using var streamWriter = new StreamWriter(namedPipeClientStream);
        while (true)
        {
            if (arguments.Length > 1)
            {
                // Write command type
                streamWriter.WriteLine(arguments[0]);

                // Write command path
                streamWriter.WriteLine(arguments[1]);

                if (arguments.Length > 2)
                {
                    // Write command arguments
                    streamWriter.WriteLine(arguments[2]);
                }

                streamWriter.Flush();
            }

            break;
        }
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
        ShowErrorMessage(e.Exception.Message);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        ShowErrorMessage(((Exception)e.ExceptionObject).Message);
    }

    internal static void ShowErrorMessage(string message)
    {
        MessageBox.Show(@$"  {message}", ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
