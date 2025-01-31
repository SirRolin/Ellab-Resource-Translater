namespace Ellab_Resource_Translater
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
            SettingsButton = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            translationPanel = new Panel();
            translationLabel = new Label();
            translationCheckedListBox = new CheckedListBox();
            progresPanel = new Panel();
            progressListView = new ListView();
            progressTracker = new Label();
            progressTitle = new Label();
            splitContainer1 = new SplitContainer();
            DBConnectionPanel = new Panel();
            DBConnectionSetup = new Button();
            RefreshConnectionButton = new Button();
            connectionStatus = new Label();
            ButtonPanel = new Panel();
            EMandValButton = new Button();
            EMSuiteButton = new Button();
            ValSuiteButton = new Button();
            sqlCommand1 = new Microsoft.Data.SqlClient.SqlCommand();
            flowLayoutPanel1.SuspendLayout();
            translationPanel.SuspendLayout();
            progresPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            DBConnectionPanel.SuspendLayout();
            ButtonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // SettingsButton
            // 
            SettingsButton.Dock = DockStyle.Right;
            SettingsButton.Location = new Point(222, 5);
            SettingsButton.MinimumSize = new Size(0, 30);
            SettingsButton.Name = "SettingsButton";
            SettingsButton.Size = new Size(75, 30);
            SettingsButton.TabIndex = 4;
            SettingsButton.Text = "Settings";
            SettingsButton.UseVisualStyleBackColor = true;
            SettingsButton.Click += SettingsButton_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(translationPanel);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 40);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(15);
            flowLayoutPanel1.Size = new Size(302, 579);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // translationPanel
            // 
            translationPanel.AutoSize = true;
            translationPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            translationPanel.Controls.Add(translationLabel);
            translationPanel.Controls.Add(translationCheckedListBox);
            translationPanel.Location = new Point(18, 18);
            translationPanel.MinimumSize = new Size(100, 0);
            translationPanel.Name = "translationPanel";
            translationPanel.Size = new Size(100, 217);
            translationPanel.TabIndex = 5;
            // 
            // translationLabel
            // 
            translationLabel.Dock = DockStyle.Top;
            translationLabel.Location = new Point(0, 0);
            translationLabel.Name = "translationLabel";
            translationLabel.Size = new Size(100, 15);
            translationLabel.TabIndex = 5;
            translationLabel.Text = "Translation";
            translationLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // translationCheckedListBox
            // 
            translationCheckedListBox.CheckOnClick = true;
            translationCheckedListBox.FormattingEnabled = true;
            translationCheckedListBox.Items.AddRange(new object[] { "DE", "ES", "FR", "IT", "JA", "KO", "NL", "PL", "PT", "TR", "ZH" });
            translationCheckedListBox.Location = new Point(0, 15);
            translationCheckedListBox.Margin = new Padding(0);
            translationCheckedListBox.MinimumSize = new Size(100, 40);
            translationCheckedListBox.Name = "translationCheckedListBox";
            translationCheckedListBox.Size = new Size(100, 202);
            translationCheckedListBox.Sorted = true;
            translationCheckedListBox.TabIndex = 5;
            translationCheckedListBox.ItemCheck += translationCheckedListBox_CheckChanged;
            // 
            // progresPanel
            // 
            progresPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            progresPanel.Controls.Add(progressListView);
            progresPanel.Controls.Add(progressTracker);
            progresPanel.Controls.Add(progressTitle);
            progresPanel.Dock = DockStyle.Fill;
            progresPanel.Location = new Point(0, 0);
            progresPanel.Margin = new Padding(0);
            progresPanel.MinimumSize = new Size(100, 100);
            progresPanel.Name = "progresPanel";
            progresPanel.Padding = new Padding(15);
            progresPanel.Size = new Size(576, 619);
            progresPanel.TabIndex = 0;
            // 
            // progressListView
            // 
            progressListView.AccessibleRole = AccessibleRole.None;
            progressListView.Dock = DockStyle.Fill;
            progressListView.HeaderStyle = ColumnHeaderStyle.None;
            progressListView.Location = new Point(15, 45);
            progressListView.MultiSelect = false;
            progressListView.Name = "progressListView";
            progressListView.RightToLeft = RightToLeft.Yes;
            progressListView.ShowGroups = false;
            progressListView.Size = new Size(546, 559);
            progressListView.TabIndex = 0;
            progressListView.TabStop = false;
            progressListView.UseCompatibleStateImageBehavior = false;
            progressListView.View = View.List;
            // 
            // progressTracker
            // 
            progressTracker.Dock = DockStyle.Top;
            progressTracker.Location = new Point(15, 30);
            progressTracker.Margin = new Padding(0);
            progressTracker.Name = "progressTracker";
            progressTracker.Size = new Size(546, 15);
            progressTracker.TabIndex = 0;
            progressTracker.Text = "x out of y";
            progressTracker.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressTitle
            // 
            progressTitle.Dock = DockStyle.Top;
            progressTitle.Location = new Point(15, 15);
            progressTitle.Margin = new Padding(0);
            progressTitle.Name = "progressTitle";
            progressTitle.Size = new Size(546, 15);
            progressTitle.TabIndex = 0;
            progressTitle.Text = "nothing running";
            progressTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = Color.Gold;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel2;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.BackColor = Color.Brown;
            splitContainer1.Panel1.Controls.Add(progresPanel);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(DBConnectionPanel);
            splitContainer1.Panel2.Controls.Add(flowLayoutPanel1);
            splitContainer1.Panel2.Controls.Add(ButtonPanel);
            splitContainer1.Size = new Size(879, 619);
            splitContainer1.SplitterDistance = 576;
            splitContainer1.SplitterWidth = 1;
            splitContainer1.TabIndex = 0;
            splitContainer1.TabStop = false;
            // 
            // DBConnectionPanel
            // 
            DBConnectionPanel.Controls.Add(DBConnectionSetup);
            DBConnectionPanel.Controls.Add(RefreshConnectionButton);
            DBConnectionPanel.Controls.Add(connectionStatus);
            DBConnectionPanel.Dock = DockStyle.Bottom;
            DBConnectionPanel.Location = new Point(0, 544);
            DBConnectionPanel.Name = "DBConnectionPanel";
            DBConnectionPanel.Size = new Size(302, 75);
            DBConnectionPanel.TabIndex = 6;
            // 
            // DBConnectionSetup
            // 
            DBConnectionSetup.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            DBConnectionSetup.Location = new Point(227, 37);
            DBConnectionSetup.Name = "DBConnectionSetup";
            DBConnectionSetup.Size = new Size(60, 23);
            DBConnectionSetup.TabIndex = 2;
            DBConnectionSetup.Text = "Setup";
            DBConnectionSetup.UseVisualStyleBackColor = true;
            DBConnectionSetup.Click += DBConnectionSetup_Click;
            // 
            // RefreshConnectionButton
            // 
            RefreshConnectionButton.Location = new Point(15, 37);
            RefreshConnectionButton.Name = "RefreshConnectionButton";
            RefreshConnectionButton.Size = new Size(60, 23);
            RefreshConnectionButton.TabIndex = 1;
            RefreshConnectionButton.Text = "Refresh";
            RefreshConnectionButton.UseVisualStyleBackColor = true;
            // 
            // connectionStatus
            // 
            connectionStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            connectionStatus.Location = new Point(15, 15);
            connectionStatus.Name = "connectionStatus";
            connectionStatus.Size = new Size(272, 19);
            connectionStatus.TabIndex = 0;
            connectionStatus.Text = "Connection Status";
            connectionStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ButtonPanel
            // 
            ButtonPanel.AutoSize = true;
            ButtonPanel.Controls.Add(EMandValButton);
            ButtonPanel.Controls.Add(EMSuiteButton);
            ButtonPanel.Controls.Add(ValSuiteButton);
            ButtonPanel.Controls.Add(SettingsButton);
            ButtonPanel.Dock = DockStyle.Top;
            ButtonPanel.Location = new Point(0, 0);
            ButtonPanel.Margin = new Padding(0);
            ButtonPanel.MinimumSize = new Size(0, 40);
            ButtonPanel.Name = "ButtonPanel";
            ButtonPanel.Padding = new Padding(5);
            ButtonPanel.Size = new Size(302, 40);
            ButtonPanel.TabIndex = 0;
            // 
            // EMandValButton
            // 
            EMandValButton.BackColor = SystemColors.Control;
            EMandValButton.Dock = DockStyle.Right;
            EMandValButton.Location = new Point(-3, 5);
            EMandValButton.Margin = new Padding(0);
            EMandValButton.MinimumSize = new Size(0, 30);
            EMandValButton.Name = "EMandValButton";
            EMandValButton.Size = new Size(75, 30);
            EMandValButton.TabIndex = 1;
            EMandValButton.Text = "EM && Val";
            EMandValButton.UseVisualStyleBackColor = true;
            EMandValButton.Click += EMandValButton_Click;
            // 
            // EMSuiteButton
            // 
            EMSuiteButton.BackColor = SystemColors.Control;
            EMSuiteButton.Dock = DockStyle.Right;
            EMSuiteButton.Location = new Point(72, 5);
            EMSuiteButton.Margin = new Padding(0);
            EMSuiteButton.MinimumSize = new Size(0, 30);
            EMSuiteButton.Name = "EMSuiteButton";
            EMSuiteButton.Size = new Size(75, 30);
            EMSuiteButton.TabIndex = 2;
            EMSuiteButton.Text = "EMSuite";
            EMSuiteButton.UseVisualStyleBackColor = true;
            EMSuiteButton.Click += EMSuite_Initiation;
            // 
            // ValSuiteButton
            // 
            ValSuiteButton.BackColor = SystemColors.Control;
            ValSuiteButton.Dock = DockStyle.Right;
            ValSuiteButton.Location = new Point(147, 5);
            ValSuiteButton.MinimumSize = new Size(0, 30);
            ValSuiteButton.Name = "ValSuiteButton";
            ValSuiteButton.Size = new Size(75, 30);
            ValSuiteButton.TabIndex = 3;
            ValSuiteButton.Text = "ValSuite";
            ValSuiteButton.UseVisualStyleBackColor = true;
            ValSuiteButton.Click += ValSuite_Initiation;
            // 
            // sqlCommand1
            // 
            sqlCommand1.CommandTimeout = 30;
            sqlCommand1.EnableOptimizedParameterBinding = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(879, 619);
            Controls.Add(splitContainer1);
            Name = "MainForm";
            Text = "Ellab Resource Tranlator";
            FormClosed += Form1_Closed;
            Load += Form1_Load;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            translationPanel.ResumeLayout(false);
            progresPanel.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            DBConnectionPanel.ResumeLayout(false);
            ButtonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button SettingsButton;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel translationPanel;
        private Label translationLabel;
        private CheckedListBox translationCheckedListBox;
        private Panel progresPanel;
        private ListView progressListView;
        private Label progressTracker;
        private SplitContainer splitContainer1;
        private Panel ButtonPanel;
        private Button EMSuiteButton;
        private Button ValSuiteButton;
        private Button EMandValButton;
        private Label progressTitle;
        private Panel DBConnectionPanel;
        private Label connectionStatus;
        private Button DBConnectionSetup;
        private Button RefreshConnectionButton;
        private Microsoft.Data.SqlClient.SqlCommand sqlCommand1;
    }
}
