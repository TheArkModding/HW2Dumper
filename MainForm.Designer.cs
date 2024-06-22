namespace HW2Dumper
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            CheckGameOpen = new System.Windows.Forms.Timer(components);
            TxtState = new Label();
            IndicatorState = new Panel();
            TxBoxDumpLocation = new TextBox();
            BtnChangeDumpFolder = new Button();
            label2 = new Label();
            BtnDump = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // CheckGameOpen
            // 
            CheckGameOpen.Enabled = true;
            CheckGameOpen.Tick += CheckGameOpen_Tick;
            // 
            // TxtState
            // 
            TxtState.FlatStyle = FlatStyle.Flat;
            TxtState.Location = new Point(29, 12);
            TxtState.Name = "TxtState";
            TxtState.Size = new Size(535, 16);
            TxtState.TabIndex = 0;
            TxtState.Text = "STATE";
            TxtState.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // IndicatorState
            // 
            IndicatorState.BackColor = Color.Red;
            IndicatorState.Location = new Point(12, 12);
            IndicatorState.Name = "IndicatorState";
            IndicatorState.Size = new Size(16, 16);
            IndicatorState.TabIndex = 1;
            // 
            // TxBoxDumpLocation
            // 
            TxBoxDumpLocation.Location = new Point(12, 66);
            TxBoxDumpLocation.Name = "TxBoxDumpLocation";
            TxBoxDumpLocation.Size = new Size(470, 23);
            TxBoxDumpLocation.TabIndex = 2;
            TxBoxDumpLocation.WordWrap = false;
            // 
            // BtnChangeDumpFolder
            // 
            BtnChangeDumpFolder.Location = new Point(475, 66);
            BtnChangeDumpFolder.Name = "BtnChangeDumpFolder";
            BtnChangeDumpFolder.Size = new Size(90, 23);
            BtnChangeDumpFolder.TabIndex = 3;
            BtnChangeDumpFolder.Text = "Change";
            BtnChangeDumpFolder.UseVisualStyleBackColor = true;
            BtnChangeDumpFolder.Click += BtnChangeDumpFolder_Click;
            // 
            // label2
            // 
            label2.FlatStyle = FlatStyle.Flat;
            label2.Location = new Point(12, 47);
            label2.Name = "label2";
            label2.Size = new Size(314, 16);
            label2.TabIndex = 5;
            label2.Text = "Dump to folder...";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // BtnDump
            // 
            BtnDump.Location = new Point(12, 116);
            BtnDump.Name = "BtnDump";
            BtnDump.Size = new Size(153, 33);
            BtnDump.TabIndex = 6;
            BtnDump.Text = "DUMP";
            BtnDump.UseVisualStyleBackColor = true;
            BtnDump.Click += BtnDump_Click;
            // 
            // label1
            // 
            label1.FlatStyle = FlatStyle.Flat;
            label1.Location = new Point(425, 133);
            label1.Name = "label1";
            label1.Size = new Size(139, 16);
            label1.TabIndex = 7;
            label1.Text = "Made by CinderellaKuru";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.None;
            ClientSize = new Size(576, 172);
            Controls.Add(label1);
            Controls.Add(BtnDump);
            Controls.Add(label2);
            Controls.Add(BtnChangeDumpFolder);
            Controls.Add(TxBoxDumpLocation);
            Controls.Add(IndicatorState);
            Controls.Add(TxtState);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HW2Dumper";
            FormClosing += MainForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Timer CheckGameOpen;
        private Label TxtState;
        private Panel IndicatorState;
        private TextBox TxBoxDumpLocation;
        private Button BtnChangeDumpFolder;
        private Label label2;
        private Button BtnDump;
        private Label label1;
    }
}
