using System.Diagnostics;
using Ookii.Dialogs.WinForms;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW2Dumper
{
    public partial class MainForm : Form
    {
        private readonly string tempFolderPath;
        private readonly string uwpInjectorPath;
        private readonly string uwpDumperPath;
        private readonly System.Windows.Forms.Timer checkDumperTimer;
        private int gameProcessId;
        private Process? uwpInjectorProcess = new();
        public State CurrentState { get; private set; } = State.INAPP;

        public MainForm()
        {
            InitializeComponent();
            FormClosing += MainForm_FormClosing!;

            tempFolderPath = Path.Combine(Path.GetTempPath(), "HW2Dumper");
            _ = Directory.CreateDirectory(tempFolderPath);

            uwpInjectorPath = Path.Combine(tempFolderPath, "UWPInjector.exe");
            uwpDumperPath = Path.Combine(tempFolderPath, "UWPDumper.dll");

            ResourceHelper.ExtractResource(Properties.Resources.UWPInjector, uwpInjectorPath);
            ResourceHelper.ExtractResource(Properties.Resources.UWPDumper, uwpDumperPath);

            checkDumperTimer = new System.Windows.Forms.Timer { Interval = 100 };
            checkDumperTimer.Tick += CheckDumperTimer_Tick!;
            checkDumperTimer.Start();

            Console.WriteLine("MainForm initialized.");
        }

        private async void CheckDumperTimer_Tick(object sender, EventArgs e)
        {
            bool isDumperRunning = Process.GetProcessesByName("UwpInjector").Length != 0;
            if (isDumperRunning && CurrentState != State.DUMPING)
            {
                Console.WriteLine("Dumper process detected, changing state to DUMPING.");
                await SetStateAsync(State.DUMPING);
            }
            else if (!isDumperRunning && CurrentState == State.DUMPING)
            {
                Console.WriteLine("Dumper process not detected, changing state to FINISHED.");
                await SetStateAsync(State.FINISHED);
            }
        }

        private async void CheckGameOpen_Tick(object sender, EventArgs e)
        {
            if (CurrentState != State.DUMPING)
            {
                await UpdateGameStateAsync();
                UpdateStateUI();
            }
        }

        private async Task UpdateGameStateAsync()
        {
            await Task.Run(() =>
            {
                Process? gameProcess = Process.GetProcessesByName("HaloWars2_WinAppDX12Final").FirstOrDefault();
                State newState = gameProcess != null ? State.INGAME : State.INAPP;
                if (CurrentState != newState)
                {
                    CurrentState = newState;
                    gameProcessId = gameProcess?.Id ?? 0;
                    Console.WriteLine($"Game state updated. CurrentState: {CurrentState}, gameProcessId: {gameProcessId}");
                }
            });
        }

        private void UpdateStateUI()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateStateUI));
                return;
            }

            switch (CurrentState)
            {
                case State.INGAME:
                    UIHelper.SetUIState(IndicatorState, TxtState, BtnDump, Color.Green, "The game is running, you can start dumping.", true);
                    break;
                case State.INAPP:
                    UIHelper.SetUIState(IndicatorState, TxtState, BtnDump, Color.Red, "The game is not running, please start the game.", false);
                    break;
                case State.DUMPING:
                    UIHelper.SetUIState(IndicatorState, TxtState, BtnDump, Color.LightBlue, "The game is currently dumping, this can take a long time.", false);
                    break;
                case State.FINISHED:
                    UIHelper.SetUIState(IndicatorState, TxtState, BtnDump, Color.Green, "The game finished dumping.", true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task SetStateAsync(State newState)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"SetState called. New state: {newState}");
                CurrentState = newState;
                Invoke(new Action(UpdateStateUI));
            });
        }

        private async Task RunUWPInjectorAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    Console.WriteLine("RunUWPInjector called.");

                    if (!File.Exists(uwpInjectorPath))
                    {
                        Console.WriteLine($"File not found: {uwpInjectorPath}");
                        return;
                    }

                    ProcessStartInfo startInfo = new()
                    {
                        FileName = uwpInjectorPath,
                        Arguments = $"-p {gameProcessId} -c -d {TxBoxDumpLocation.Text}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    uwpInjectorProcess = Process.Start(startInfo);
                    if (uwpInjectorProcess != null)
                    {
                        Console.WriteLine($"UWPInjector started with PID: {uwpInjectorProcess.Id}");
                    }
                    else
                    {
                        Console.WriteLine("Failed to start UWPInjector process.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"InvalidOperationException: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            });
        }


        private async Task BackgroundWorker_DoWorkAsync()
        {
            Console.WriteLine("BackgroundWorker_DoWork called.");
            await RunUWPInjectorAsync();
        }

        private async void BtnDump_Click(object sender, EventArgs e)
        {
            Console.WriteLine("BtnDump_Click called.");
            if (CurrentState == State.INGAME)
            {
                await SetStateAsync(State.DUMPING);
                await BackgroundWorker_DoWorkAsync();
                Console.WriteLine("Background worker completed successfully.");
            }
        }

        private async void CleanUpTemporaryFiles()
        {
            await Task.Run(() =>
            {
                Console.WriteLine("CleanUpTemporaryFiles called.");
                try
                {
                    if (File.Exists(uwpInjectorPath)) File.Delete(uwpInjectorPath);
                    if (File.Exists(uwpDumperPath)) File.Delete(uwpDumperPath);
                    if (Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath);
                    Console.WriteLine("Temporary files cleaned up.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cleaning up temporary files: {ex.Message}");
                }
            });
        }

        private async void StopUWPInjectorProcess()
        {
            await Task.Run(() =>
            {
                Console.WriteLine("StopUWPInjectorProcess called.");
                ProcessHelper.TerminateProcessesByName("UwpInjector");
                ProcessHelper.TerminateProcessesByName("UwpDumper");
                ProcessHelper.TerminateProcessesByName("HaloWars2_WinAppDX12Final");

                if (uwpInjectorProcess != null && !uwpInjectorProcess.HasExited)
                {
                    try
                    {
                        uwpInjectorProcess.Kill();
                        uwpInjectorProcess.Dispose();
                        Console.WriteLine("UWPInjector process stopped.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error stopping UWPInjector process: {ex.Message}");
                    }
                }
            });
        }

        private void BtnChangeDumpFolder_Click(object sender, EventArgs e)
        {
            Console.WriteLine("BtnChangeDumpFolder_Click called.");
            using VistaFolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TxBoxDumpLocation.Text = dialog.SelectedPath;
                Console.WriteLine($"Dump location changed to: {TxBoxDumpLocation.Text}");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("MainForm_FormClosing called.");
            CleanUpTemporaryFiles();
            StopUWPInjectorProcess();
            Console.WriteLine("MainForm closed.");
        }
    }
}
