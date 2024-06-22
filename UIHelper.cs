namespace HW2Dumper
{
    public static class UIHelper
    {
        public static void SetUIState(Control indicatorState, Label txtState, Button btnDump, Color indicatorColor, string stateText, bool isDumpButtonEnabled)
        {
            Console.WriteLine($"SetUIState called. State: {stateText}, Dump Button Enabled: {isDumpButtonEnabled}");
            indicatorState.BackColor = indicatorColor;
            txtState.Text = stateText;
            btnDump.Enabled = isDumpButtonEnabled;
        }
    }
}
