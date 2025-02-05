using Ellab_Resource_Translater.Util;
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
            ValFBDialog = new FolderBrowserDialog();
            LocationTablePanel = new TableLayoutPanel();
            EMSuiteText = new Label();
            EMsuitePath = new TextBox();
            EMsuiteFBButton = new Button();
            ValText = new Label();
            ValPath = new TextBox();
            ValFBButton = new Button();
            translationPanel = new Panel();
            translationLabel = new Label();
            translationCheckedListBox = new CheckedListBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel1 = new Panel();
            coresNumeric = new NumericUpDown();
            label1 = new Label();
            LocationTablePanel.SuspendLayout();
            translationPanel.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)coresNumeric).BeginInit();
            SuspendLayout();
            // 
            // ValFBDialog
            // 
            ValFBDialog.AddToRecent = false;
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
            LocationTablePanel.Controls.Add(ValText, 0, 1);
            LocationTablePanel.Controls.Add(ValPath, 1, 1);
            LocationTablePanel.Controls.Add(ValFBButton, 2, 1);
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
            LocationTablePanel.Size = new Size(584, 88);
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
            EMsuitePath.Size = new Size(375, 23);
            EMsuitePath.TabIndex = 0;
            EMsuitePath.TextChanged += EMsuitePath_TextChanged;
            // 
            // EMsuiteFBButton
            // 
            EMsuiteFBButton.Dock = DockStyle.Right;
            EMsuiteFBButton.Location = new Point(501, 18);
            EMsuiteFBButton.Name = "EMsuiteFBButton";
            EMsuiteFBButton.Size = new Size(65, 23);
            EMsuiteFBButton.TabIndex = 2;
            EMsuiteFBButton.Text = "...";
            EMsuiteFBButton.UseVisualStyleBackColor = true;
            EMsuiteFBButton.Click += EMsuiteBrowse_Click;
            // 
            // ValText
            // 
            ValText.AutoSize = true;
            ValText.Dock = DockStyle.Fill;
            ValText.Location = new Point(18, 44);
            ValText.Name = "ValText";
            ValText.Size = new Size(96, 29);
            ValText.TabIndex = 1;
            ValText.Text = "ValSuite location";
            ValText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ValPath
            // 
            ValPath.Dock = DockStyle.Fill;
            ValPath.Location = new Point(120, 47);
            ValPath.Name = "ValPath";
            ValPath.Size = new Size(375, 23);
            ValPath.TabIndex = 3;
            ValPath.TextChanged += NotEmPath_TextChanged;
            // 
            // ValFBButton
            // 
            ValFBButton.Dock = DockStyle.Right;
            ValFBButton.Location = new Point(501, 47);
            ValFBButton.Name = "ValFBButton";
            ValFBButton.Size = new Size(65, 23);
            ValFBButton.TabIndex = 4;
            ValFBButton.Text = "...";
            ValFBButton.UseVisualStyleBackColor = true;
            ValFBButton.Click += NotEmBrowse_Click;
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
            translationCheckedListBox.ItemCheck += TranslationCheckedListBox_SelectedIndexChanged;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(translationPanel);
            flowLayoutPanel1.Controls.Add(panel1);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(0, 88);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(584, 273);
            flowLayoutPanel1.TabIndex = 5;
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.Controls.Add(coresNumeric);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(148, 15);
            panel1.Margin = new Padding(15);
            panel1.Name = "panel1";
            panel1.Size = new Size(178, 52);
            panel1.TabIndex = 1;
            // 
            // coresNumeric
            // 
            coresNumeric.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            coresNumeric.Location = new Point(3, 26);
            coresNumeric.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
            coresNumeric.Name = "coresNumeric";
            coresNumeric.Size = new Size(172, 23);
            coresNumeric.TabIndex = 1;
            coresNumeric.Value = new decimal(new int[] { 32, 0, 0, 0 });
            coresNumeric.ValueChanged += numericUpDown1_ValueChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(3, 3);
            label1.Margin = new Padding(3);
            label1.Name = "label1";
            label1.Padding = new Padding(3);
            label1.Size = new Size(172, 21);
            label1.TabIndex = 0;
            label1.Text = "Workers (0 = sync, norm 8-32)";
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 361);
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
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)coresNumeric).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TableLayoutPanel LocationTablePanel;
        private Label EMSuiteText;
        private TextBox EMsuitePath;
        private Button EMsuiteFBButton;
        private Label ValText;
        private TextBox ValPath;
        private Button ValFBButton;
        private FolderBrowserDialog EMsuiteFBDialog;
        private FolderBrowserDialog ValFBDialog;
        private Panel translationPanel;
        private Label translationLabel;
        private CheckedListBox translationCheckedListBox;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel panel1;
        private NumericUpDown coresNumeric;
        private Label label1;
    }
}