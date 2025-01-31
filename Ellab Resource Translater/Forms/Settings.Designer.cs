using System.Windows.Forms;

namespace Ellab_Resource_Translater
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            EMsuiteFBDialog = new FolderBrowserDialog();
            NotEmFBDialog = new FolderBrowserDialog();
            LocationTablePanel = new TableLayoutPanel();
            EMSuiteText = new Label();
            EMsuitePath = new TextBox();
            EMsuiteFBButton = new Button();
            NotEmText = new Label();
            NotEmPath = new TextBox();
            NotEmFBButton = new Button();
            translationPanel = new Panel();
            translationLabel = new Label();
            translationCheckedListBox = new CheckedListBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            LocationTablePanel.SuspendLayout();
            translationPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // NotEmFBDialog
            // 
            NotEmFBDialog.AddToRecent = false;
            // 
            // LocationTablePanel
            // 
            LocationTablePanel.AutoSize = true;
            LocationTablePanel.ColumnCount = 3;
            LocationTablePanel.ColumnStyles.Add(new ColumnStyle());
            LocationTablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            LocationTablePanel.ColumnStyles.Add(new ColumnStyle());
            LocationTablePanel.Controls.Add(EMSuiteText, 0, 0);
            LocationTablePanel.Controls.Add(EMsuitePath, 1, 0);
            LocationTablePanel.Controls.Add(EMsuiteFBButton, 2, 0);
            LocationTablePanel.Controls.Add(NotEmText, 0, 1);
            LocationTablePanel.Controls.Add(NotEmPath, 1, 1);
            LocationTablePanel.Controls.Add(NotEmFBButton, 2, 1);
            LocationTablePanel.Dock = DockStyle.Top;
            LocationTablePanel.Location = new Point(0, 0);
            LocationTablePanel.Margin = new Padding(15);
            LocationTablePanel.Name = "LocationTablePanel";
            LocationTablePanel.Padding = new Padding(15);
            LocationTablePanel.RowCount = 2;
            LocationTablePanel.RowStyles.Add(new RowStyle());
            LocationTablePanel.RowStyles.Add(new RowStyle());
            LocationTablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            LocationTablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            LocationTablePanel.Size = new Size(624, 88);
            LocationTablePanel.TabIndex = 4;
            // 
            // EMSuiteText
            // 
            EMSuiteText.AutoSize = true;
            EMSuiteText.Dock = DockStyle.Fill;
            EMSuiteText.Location = new Point(18, 15);
            EMSuiteText.Name = "EMSuiteText";
            EMSuiteText.Size = new Size(96, 29);
            EMSuiteText.TabIndex = 0;
            EMSuiteText.Text = "EMSuite location";
            EMSuiteText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // EMsuitePath
            // 
            EMsuitePath.Dock = DockStyle.Fill;
            EMsuitePath.Location = new Point(120, 18);
            EMsuitePath.Name = "EMsuitePath";
            EMsuitePath.Size = new Size(415, 23);
            EMsuitePath.TabIndex = 0;
            EMsuitePath.TextChanged += EMsuitePath_TextChanged;
            // 
            // EMsuiteFBButton
            // 
            EMsuiteFBButton.Dock = DockStyle.Right;
            EMsuiteFBButton.Location = new Point(541, 18);
            EMsuiteFBButton.Name = "EMsuiteFBButton";
            EMsuiteFBButton.Size = new Size(65, 23);
            EMsuiteFBButton.TabIndex = 2;
            EMsuiteFBButton.Text = "...";
            EMsuiteFBButton.UseVisualStyleBackColor = true;
            EMsuiteFBButton.Click += EMsuiteBrowse_Click;
            // 
            // NotEmText
            // 
            NotEmText.AutoSize = true;
            NotEmText.Dock = DockStyle.Fill;
            NotEmText.Location = new Point(18, 44);
            NotEmText.Name = "NotEmText";
            NotEmText.Size = new Size(96, 29);
            NotEmText.TabIndex = 1;
            NotEmText.Text = "ValSuite location";
            NotEmText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // NotEmPath
            // 
            NotEmPath.Dock = DockStyle.Fill;
            NotEmPath.Location = new Point(120, 47);
            NotEmPath.Name = "NotEmPath";
            NotEmPath.Size = new Size(415, 23);
            NotEmPath.TabIndex = 3;
            NotEmPath.TextChanged += NotEmPath_TextChanged;
            // 
            // NotEmFBButton
            // 
            NotEmFBButton.Dock = DockStyle.Right;
            NotEmFBButton.Location = new Point(541, 47);
            NotEmFBButton.Name = "NotEmFBButton";
            NotEmFBButton.Size = new Size(65, 23);
            NotEmFBButton.TabIndex = 4;
            NotEmFBButton.Text = "...";
            NotEmFBButton.UseVisualStyleBackColor = true;
            NotEmFBButton.Click += NotEmBrowse_Click;
            // 
            // translationPanel
            // 
            translationPanel.AutoSize = true;
            translationPanel.Controls.Add(translationLabel);
            translationPanel.Controls.Add(translationCheckedListBox);
            translationPanel.Location = new Point(15, 15);
            translationPanel.Margin = new Padding(15);
            translationPanel.MinimumSize = new Size(100, 0);
            translationPanel.Name = "translationPanel";
            translationPanel.Size = new Size(103, 223);
            translationPanel.TabIndex = 0;
            // 
            // translationLabel
            // 
            translationLabel.Dock = DockStyle.Top;
            translationLabel.Location = new Point(0, 0);
            translationLabel.Margin = new Padding(15, 15, 15, 0);
            translationLabel.Name = "translationLabel";
            translationLabel.Size = new Size(103, 15);
            translationLabel.TabIndex = 3;
            translationLabel.Text = "Ai Translation";
            translationLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // translationCheckedListBox
            // 
            translationCheckedListBox.CheckOnClick = true;
            translationCheckedListBox.Cursor = Cursors.Hand;
            translationCheckedListBox.FormattingEnabled = true;
            translationCheckedListBox.Items.AddRange(new object[] { "DE", "ES", "FR", "IT", "JA", "KO", "NL", "PL", "PT", "TR", "ZH" });
            translationCheckedListBox.Location = new Point(0, 18);
            translationCheckedListBox.MultiColumn = true;
            translationCheckedListBox.Name = "translationCheckedListBox";
            translationCheckedListBox.RightToLeft = RightToLeft.No;
            translationCheckedListBox.Size = new Size(100, 202);
            translationCheckedListBox.Sorted = true;
            translationCheckedListBox.TabIndex = 5;
            translationCheckedListBox.ItemCheck += translationCheckedListBox_SelectedIndexChanged;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(translationPanel);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 88);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(624, 465);
            flowLayoutPanel1.TabIndex = 5;
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(624, 553);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(LocationTablePanel);
            Name = "Settings";
            Text = "Settings";
            Load += Settings_Load;
            LocationTablePanel.ResumeLayout(false);
            LocationTablePanel.PerformLayout();
            translationPanel.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TableLayoutPanel LocationTablePanel;
        private Label EMSuiteText;
        private TextBox EMsuitePath;
        private Button EMsuiteFBButton;
        private Label NotEmText;
        private TextBox NotEmPath;
        private Button NotEmFBButton;
        private FolderBrowserDialog EMsuiteFBDialog;
        private FolderBrowserDialog NotEmFBDialog;
        private Panel translationPanel;
        private Label translationLabel;
        private CheckedListBox translationCheckedListBox;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}