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
            TranslationPanel = new Panel();
            TranslationLabel = new Label();
            TranslationCheckedListBox = new CheckedListBox();
            FlowLayoutPanel1 = new FlowLayoutPanel();
            panel1 = new Panel();
            CoresNumeric = new NumericUpDown();
            label1 = new Label();
            panel2 = new Panel();
            InserterNumeric = new NumericUpDown();
            label2 = new Label();
            CloseOnSuccess = new CheckBox();
            LocationTablePanel.SuspendLayout();
            TranslationPanel.SuspendLayout();
            FlowLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CoresNumeric).BeginInit();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)InserterNumeric).BeginInit();
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
            TranslationPanel.AutoSize = true;
            TranslationPanel.Controls.Add(TranslationLabel);
            TranslationPanel.Controls.Add(TranslationCheckedListBox);
            TranslationPanel.Location = new Point(15, 15);
            TranslationPanel.Margin = new Padding(15, 0, 15, 0);
            TranslationPanel.MinimumSize = new Size(100, 0);
            TranslationPanel.Name = "translationPanel";
            TranslationPanel.Size = new Size(103, 223);
            TranslationPanel.TabIndex = 0;
            // 
            // translationLabel
            // 
            TranslationLabel.Dock = DockStyle.Top;
            TranslationLabel.Location = new Point(0, 0);
            TranslationLabel.Margin = new Padding(15, 15, 15, 0);
            TranslationLabel.Name = "translationLabel";
            TranslationLabel.Size = new Size(103, 15);
            TranslationLabel.TabIndex = 3;
            TranslationLabel.Text = "Ai Translation";
            TranslationLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // translationCheckedListBox
            // 
            TranslationCheckedListBox.CheckOnClick = true;
            TranslationCheckedListBox.Cursor = Cursors.Hand;
            TranslationCheckedListBox.FormattingEnabled = true;
            TranslationCheckedListBox.Items.AddRange(new object[] { "DE", "ES", "FR", "IT", "JA", "KO", "NL", "PL", "PT", "TR", "ZH" });
            TranslationCheckedListBox.Location = new Point(0, 18);
            TranslationCheckedListBox.MultiColumn = true;
            TranslationCheckedListBox.Name = "translationCheckedListBox";
            TranslationCheckedListBox.RightToLeft = RightToLeft.No;
            TranslationCheckedListBox.Size = new Size(100, 202);
            TranslationCheckedListBox.Sorted = true;
            TranslationCheckedListBox.TabIndex = 5;
            TranslationCheckedListBox.ItemCheck += TranslationCheckedListBox_SelectedIndexChanged;
            // 
            // flowLayoutPanel1
            // 
            FlowLayoutPanel1.Controls.Add(TranslationPanel);
            FlowLayoutPanel1.Controls.Add(panel1);
            FlowLayoutPanel1.Controls.Add(panel2);
            FlowLayoutPanel1.Controls.Add(CloseOnSuccess);
            FlowLayoutPanel1.Dock = DockStyle.Fill;
            FlowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            FlowLayoutPanel1.Location = new Point(0, 88);
            FlowLayoutPanel1.Margin = new Padding(0);
            FlowLayoutPanel1.Name = "flowLayoutPanel1";
            FlowLayoutPanel1.Padding = new Padding(0, 15, 0, 15);
            FlowLayoutPanel1.Size = new Size(584, 273);
            FlowLayoutPanel1.TabIndex = 5;
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.Controls.Add(CoresNumeric);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(148, 15);
            panel1.Margin = new Padding(15, 0, 15, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(178, 52);
            panel1.TabIndex = 1;
            // 
            // CoresNumeric
            // 
            CoresNumeric.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CoresNumeric.Location = new Point(3, 26);
            CoresNumeric.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
            CoresNumeric.Name = "CoresNumeric";
            CoresNumeric.Size = new Size(172, 23);
            CoresNumeric.TabIndex = 1;
            CoresNumeric.Value = new decimal(new int[] { 32, 0, 0, 0 });
            CoresNumeric.ValueChanged += NumericUpDown1_ValueChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(3, 3);
            label1.Margin = new Padding(3);
            label1.Name = "label1";
            label1.Padding = new Padding(3);
            label1.Size = new Size(170, 21);
            label1.TabIndex = 0;
            label1.Text = "Readers (0 = sync, norm 8-32)";
            // 
            // panel2
            // 
            panel2.AutoSize = true;
            panel2.Controls.Add(InserterNumeric);
            panel2.Controls.Add(label2);
            panel2.Location = new Point(148, 67);
            panel2.Margin = new Padding(15, 0, 15, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(178, 52);
            panel2.TabIndex = 3;
            // 
            // InserterNumeric
            // 
            InserterNumeric.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            InserterNumeric.Location = new Point(3, 26);
            InserterNumeric.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
            InserterNumeric.Name = "InserterNumeric";
            InserterNumeric.Size = new Size(150, 23);
            InserterNumeric.TabIndex = 1;
            InserterNumeric.Value = new decimal(new int[] { 4, 0, 0, 0 });
            InserterNumeric.ValueChanged += InserterNumeric_ValueChanged;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(3, 3);
            label2.Margin = new Padding(3);
            label2.Name = "label2";
            label2.Padding = new Padding(3);
            label2.Size = new Size(135, 21);
            label2.TabIndex = 0;
            label2.Text = "Inserters (0 = sync, 1-8)";
            // 
            // CloseOnSuccess
            // 
            CloseOnSuccess.AutoSize = true;
            CloseOnSuccess.Location = new Point(151, 119);
            CloseOnSuccess.Margin = new Padding(18, 0, 15, 0);
            CloseOnSuccess.Name = "CloseOnSuccess";
            CloseOnSuccess.Size = new Size(172, 19);
            CloseOnSuccess.TabIndex = 2;
            CloseOnSuccess.Text = "Close Program if Successful";
            CloseOnSuccess.UseVisualStyleBackColor = true;
            CloseOnSuccess.CheckedChanged += CloseOnSuccess_CheckedChanged;
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 361);
            Controls.Add(FlowLayoutPanel1);
            Controls.Add(LocationTablePanel);
            Name = "Settings";
            Text = "Settings";
            Load += Settings_Load;
            LocationTablePanel.ResumeLayout(false);
            LocationTablePanel.PerformLayout();
            TranslationPanel.ResumeLayout(false);
            FlowLayoutPanel1.ResumeLayout(false);
            FlowLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)CoresNumeric).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)InserterNumeric).EndInit();
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
        private Panel TranslationPanel;
        private Label TranslationLabel;
        private CheckedListBox TranslationCheckedListBox;
        private FlowLayoutPanel FlowLayoutPanel1;
        private Panel panel1;
        private NumericUpDown CoresNumeric;
        private Label label1;
        private CheckBox CloseOnSuccess;
        private Panel panel2;
        private NumericUpDown InserterNumeric;
        private Label label2;
    }
}