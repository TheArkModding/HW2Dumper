using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

using Ookii.Dialogs.WinForms;

namespace HW2Dumper
{
    public partial class MainForm : Form
    {
        public State CurrentState { get; private set; } = State.INAPP;
        private readonly string tempFolderPath;
        private readonly string uwpInjectorPath;
        private readonly string uwpDumperPath;
        private readonly BackgroundWorker backgroundWorker;
        private int gameProcessId;
        private Process? uwpInjectorProcess;

        public MainForm()
        {
            InitializeComponent();
            tempFolderPath = Path.Combine(Path.GetTempPath(), "HW2Dumper");
            Directory.CreateDirectory(tempFolderPath);

            uwpInjectorPath = Path.Combine(tempFolderPath, "UWPInjector.exe");
            uwpDumperPath = Path.Combine(tempFolderPath, "UWPDumper.dll");

            ExtractResource(Properties.Resources.UWPInjector, uwpInjectorPath);
            ExtractResource(Properties.Resources.UWPDumper, uwpDumperPath);

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork!;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted!;
        }

        private void CheckGameOpen_Tick(object sender, EventArgs e)
        {
            if (CurrentState != State.DUMPING)
            {
                UpdateGameState();
                UpdateStateUI(CurrentState);
            }
        }

        private void UpdateGameState()
        {
            Process? gameProcess = Process.GetProcessesByName("HaloWars2_WinAppDX12Final").FirstOrDefault();
            CurrentState = gameProcess != null ? State.INGAME : State.INAPP;
            gameProcessId = gameProcess?.Id ?? 0;
        }

        private void UpdateStateUI(State state)
        {
            switch (state)
            {
                case State.INGAME:
                    IndicatorState.BackColor = Color.Green;
                    TxtState.Text = "The game is running, you can start dumping.";
                    BtnDump.Enabled = true;
                    break;
                case State.INAPP:
                    IndicatorState.BackColor = Color.Red;
                    TxtState.Text = "The game is not running, please start the game.";
                    BtnDump.Enabled = false;
                    break;
                case State.DUMPING:
                    IndicatorState.BackColor = Color.LightBlue;
                    TxtState.Text = "The game is currently dumping, this can take a long time..";
                    BtnDump.Enabled = false;
                    break;
                case State.FINISHED:
                    IndicatorState.BackColor = Color.Green;
                    TxtState.Text = "The game finished dumping.";
                    BtnDump.Enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private static void ExtractResource(byte[] resource, string outputPath)
        {
            using FileStream fileStream = new(outputPath, FileMode.Create, FileAccess.Write);
            fileStream.Write(resource, 0, resource.Length);
            fileStream.Close();
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateProcessWithTokenW(
            IntPtr hToken,
            int dwLogonFlags,
            string lpApplicationName,
            string lpCommandLine,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            uint dwDesiredAccess,
            IntPtr lpTokenAttributes,
            SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
            TOKEN_TYPE TokenType,
            out IntPtr phNewToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool OpenProcessToken(
            IntPtr ProcessHandle,
            uint DesiredAccess,
            out IntPtr TokenHandle);

        private const int TOKEN_DUPLICATE = 0x0002;
        private const int TOKEN_QUERY = 0x0008;
        private const int TOKEN_ASSIGN_PRIMARY = 0x0001;
        private const int TOKEN_ADJUST_DEFAULT = 0x0080;
        private const int TOKEN_ADJUST_SESSIONID = 0x0100;

        private const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private const uint STANDARD_RIGHTS_READ = 0x00020000;
        private const uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | STANDARD_RIGHTS_READ | TOKEN_ASSIGN_PRIMARY |
                                               TOKEN_DUPLICATE | TOKEN_QUERY | TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID);

        private enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        private enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        private const int STARTF_USESHOWWINDOW = 0x00000001;
        private const short SW_HIDE = 0;

        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        private void RunUWPInjector()
        {
            if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_DUPLICATE | TOKEN_QUERY, out nint hToken))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (!DuplicateTokenEx(hToken, TOKEN_ALL_ACCESS, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, TOKEN_TYPE.TokenPrimary, out nint hNewToken))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            STARTUPINFO si = new()
            {
                cb = Marshal.SizeOf(typeof(STARTUPINFO)),
                dwFlags = STARTF_USESHOWWINDOW,
                wShowWindow = SW_HIDE
            };

            string commandLine = $"{uwpInjectorPath} -p {gameProcessId} -c -d {TxBoxDumpLocation.Text}";
            if (!CreateProcessWithTokenW(hNewToken, 0, null, commandLine, 0, IntPtr.Zero, null, ref si, out PROCESS_INFORMATION pi))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            _ = CloseHandle(pi.hProcess);
            _ = CloseHandle(pi.hThread);
            _ = CloseHandle(hNewToken);
            _ = CloseHandle(hToken);
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RunUWPInjector();
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateStateUI(State.FINISHED);
            CurrentState = State.FINISHED;

            if (e.Error != null)
            {
                Console.WriteLine($"Error: {e.Error.Message}");
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            CleanUpTemporaryFiles();
            StopUWPInjectorProcess();
        }

        private void CleanUpTemporaryFiles()
        {
            try
            {
                if (File.Exists(uwpInjectorPath)) File.Delete(uwpInjectorPath);
                if (File.Exists(uwpDumperPath)) File.Delete(uwpDumperPath);
                if (Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up temporary files: {ex.Message}");
            }
        }

        private void StopUWPInjectorProcess()
        {
            if (uwpInjectorProcess != null && !uwpInjectorProcess.HasExited)
            {
                try
                {
                    uwpInjectorProcess.Kill();
                    uwpInjectorProcess.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping UWPInjector process: {ex.Message}");
                }
            }
        }

        private void BtnChangeDumpFolder_Click(object sender, EventArgs e)
        {
            using VistaFolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TxBoxDumpLocation.Text = dialog.SelectedPath;
            }
        }

        private void BtnDump_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                CurrentState = State.DUMPING;
                UpdateStateUI(CurrentState);
                backgroundWorker.RunWorkerAsync();
            }
        }
    }
}
