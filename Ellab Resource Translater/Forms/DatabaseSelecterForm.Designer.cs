namespace Ellab_Resource_Translater.Forms
{
    partial class DatabaseSelecterForm
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
            MySQLPanel = new TableLayoutPanel();
            EMSuiteText = new Label();
            EMsuitePath = new TextBox();
            NotEmText = new Label();
            NotEmPath = new TextBox();
            HeaderPanel = new Panel();
            SaveButton = new Button();
            connectionStringChoice = new ComboBox();
            ServerTypeLabel = new Label();
            SqlServerPanel = new TableLayoutPanel();
            label1 = new Label();
            textBox1 = new TextBox();
            label2 = new Label();
            textBox2 = new TextBox();
            PostgreSqlPanel = new TableLayoutPanel();
            label3 = new Label();
            textBox3 = new TextBox();
            label4 = new Label();
            textBox4 = new TextBox();
            ManualPanel = new TableLayoutPanel();
            JsonStringLabel = new Label();
            ManuelStringText = new TextBox();
            MySQLPanel.SuspendLayout();
            HeaderPanel.SuspendLayout();
            SqlServerPanel.SuspendLayout();
            PostgreSqlPanel.SuspendLayout();
            ManualPanel.SuspendLayout();
            SuspendLayout();
            // 
            // MySQLPanel
            // 
            MySQLPanel.AutoSize = true;
            MySQLPanel.ColumnCount = 2;
            MySQLPanel.ColumnStyles.Add(new ColumnStyle());
            MySQLPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            MySQLPanel.ColumnStyles.Add(new ColumnStyle());
            MySQLPanel.Controls.Add(EMSuiteText, 0, 0);
            MySQLPanel.Controls.Add(EMsuitePath, 1, 0);
            MySQLPanel.Controls.Add(NotEmText, 0, 1);
            MySQLPanel.Controls.Add(NotEmPath, 1, 1);
            MySQLPanel.Dock = DockStyle.Top;
            MySQLPanel.Location = new Point(0, 31);
            MySQLPanel.Margin = new Padding(15);
            MySQLPanel.Name = "MySQLPanel";
            MySQLPanel.Padding = new Padding(15);
            MySQLPanel.RowCount = 2;
            MySQLPanel.RowStyles.Add(new RowStyle());
            MySQLPanel.RowStyles.Add(new RowStyle());
            MySQLPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            MySQLPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            MySQLPanel.Size = new Size(478, 88);
            MySQLPanel.TabIndex = 5;
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
            EMsuitePath.Size = new Size(340, 23);
            EMsuitePath.TabIndex = 0;
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
            NotEmPath.Size = new Size(340, 23);
            NotEmPath.TabIndex = 3;
            // 
            // HeaderPanel
            // 
            HeaderPanel.AutoSize = true;
            HeaderPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            HeaderPanel.Controls.Add(SaveButton);
            HeaderPanel.Controls.Add(connectionStringChoice);
            HeaderPanel.Controls.Add(ServerTypeLabel);
            HeaderPanel.Dock = DockStyle.Top;
            HeaderPanel.Location = new Point(0, 0);
            HeaderPanel.Name = "HeaderPanel";
            HeaderPanel.Size = new Size(478, 31);
            HeaderPanel.TabIndex = 6;
            // 
            // SaveButton
            // 
            SaveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SaveButton.AutoSize = true;
            SaveButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SaveButton.Location = new Point(385, 3);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(90, 25);
            SaveButton.TabIndex = 2;
            SaveButton.Text = "Save Changes";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // connectionStringChoice
            // 
            connectionStringChoice.FormattingEnabled = true;
            connectionStringChoice.Items.AddRange(new object[] { "MySQL", "SQL Server", "POSTgresSQL", "Manual (or Json)" });
            connectionStringChoice.Location = new Point(77, 3);
            connectionStringChoice.Name = "connectionStringChoice";
            connectionStringChoice.Size = new Size(200, 23);
            connectionStringChoice.TabIndex = 0;
            connectionStringChoice.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            // 
            // ServerTypeLabel
            // 
            ServerTypeLabel.AutoSize = true;
            ServerTypeLabel.Location = new Point(7, 7);
            ServerTypeLabel.Margin = new Padding(7);
            ServerTypeLabel.Name = "ServerTypeLabel";
            ServerTypeLabel.Size = new Size(63, 15);
            ServerTypeLabel.TabIndex = 1;
            ServerTypeLabel.Text = "ServerType";
            ServerTypeLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // SqlServerPanel
            // 
            SqlServerPanel.AutoSize = true;
            SqlServerPanel.ColumnCount = 2;
            SqlServerPanel.ColumnStyles.Add(new ColumnStyle());
            SqlServerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            SqlServerPanel.ColumnStyles.Add(new ColumnStyle());
            SqlServerPanel.Controls.Add(label1, 0, 0);
            SqlServerPanel.Controls.Add(textBox1, 1, 0);
            SqlServerPanel.Controls.Add(label2, 0, 1);
            SqlServerPanel.Controls.Add(textBox2, 1, 1);
            SqlServerPanel.Dock = DockStyle.Top;
            SqlServerPanel.Location = new Point(0, 119);
            SqlServerPanel.Margin = new Padding(15);
            SqlServerPanel.Name = "SqlServerPanel";
            SqlServerPanel.Padding = new Padding(15);
            SqlServerPanel.RowCount = 2;
            SqlServerPanel.RowStyles.Add(new RowStyle());
            SqlServerPanel.RowStyles.Add(new RowStyle());
            SqlServerPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            SqlServerPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            SqlServerPanel.Size = new Size(478, 88);
            SqlServerPanel.TabIndex = 7;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(18, 15);
            label1.Name = "label1";
            label1.Size = new Size(96, 29);
            label1.TabIndex = 0;
            label1.Text = "EMSuite location";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Fill;
            textBox1.Location = new Point(120, 18);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(340, 23);
            textBox1.TabIndex = 0;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(18, 44);
            label2.Name = "label2";
            label2.Size = new Size(96, 29);
            label2.TabIndex = 1;
            label2.Text = "ValSuite location";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // textBox2
            // 
            textBox2.Dock = DockStyle.Fill;
            textBox2.Location = new Point(120, 47);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(340, 23);
            textBox2.TabIndex = 3;
            // 
            // PostgreSqlPanel
            // 
            PostgreSqlPanel.AutoSize = true;
            PostgreSqlPanel.ColumnCount = 2;
            PostgreSqlPanel.ColumnStyles.Add(new ColumnStyle());
            PostgreSqlPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            PostgreSqlPanel.ColumnStyles.Add(new ColumnStyle());
            PostgreSqlPanel.Controls.Add(label3, 0, 0);
            PostgreSqlPanel.Controls.Add(textBox3, 1, 0);
            PostgreSqlPanel.Controls.Add(label4, 0, 1);
            PostgreSqlPanel.Controls.Add(textBox4, 1, 1);
            PostgreSqlPanel.Dock = DockStyle.Top;
            PostgreSqlPanel.Location = new Point(0, 207);
            PostgreSqlPanel.Margin = new Padding(15);
            PostgreSqlPanel.Name = "PostgreSqlPanel";
            PostgreSqlPanel.Padding = new Padding(15);
            PostgreSqlPanel.RowCount = 2;
            PostgreSqlPanel.RowStyles.Add(new RowStyle());
            PostgreSqlPanel.RowStyles.Add(new RowStyle());
            PostgreSqlPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            PostgreSqlPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            PostgreSqlPanel.Size = new Size(478, 88);
            PostgreSqlPanel.TabIndex = 8;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Location = new Point(18, 15);
            label3.Name = "label3";
            label3.Size = new Size(96, 29);
            label3.TabIndex = 0;
            label3.Text = "EMSuite location";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // textBox3
            // 
            textBox3.Dock = DockStyle.Fill;
            textBox3.Location = new Point(120, 18);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(340, 23);
            textBox3.TabIndex = 0;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(18, 44);
            label4.Name = "label4";
            label4.Size = new Size(96, 29);
            label4.TabIndex = 1;
            label4.Text = "ValSuite location";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // textBox4
            // 
            textBox4.Dock = DockStyle.Fill;
            textBox4.Location = new Point(120, 47);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(340, 23);
            textBox4.TabIndex = 3;
            // 
            // ManualPanel
            // 
            ManualPanel.AutoSize = true;
            ManualPanel.ColumnCount = 2;
            ManualPanel.ColumnStyles.Add(new ColumnStyle());
            ManualPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            ManualPanel.ColumnStyles.Add(new ColumnStyle());
            ManualPanel.Controls.Add(JsonStringLabel, 0, 0);
            ManualPanel.Controls.Add(ManuelStringText, 1, 0);
            ManualPanel.Dock = DockStyle.Top;
            ManualPanel.Location = new Point(0, 295);
            ManualPanel.Margin = new Padding(15);
            ManualPanel.Name = "ManualPanel";
            ManualPanel.Padding = new Padding(15);
            ManualPanel.RowCount = 1;
            ManualPanel.RowStyles.Add(new RowStyle());
            ManualPanel.RowStyles.Add(new RowStyle());
            ManualPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            ManualPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            ManualPanel.Size = new Size(478, 59);
            ManualPanel.TabIndex = 9;
            // 
            // JsonStringLabel
            // 
            JsonStringLabel.AutoSize = true;
            JsonStringLabel.Dock = DockStyle.Fill;
            JsonStringLabel.Location = new Point(18, 15);
            JsonStringLabel.Name = "JsonStringLabel";
            JsonStringLabel.Size = new Size(38, 29);
            JsonStringLabel.TabIndex = 0;
            JsonStringLabel.Text = "String";
            JsonStringLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ManuelStringText
            // 
            ManuelStringText.Dock = DockStyle.Fill;
            ManuelStringText.Location = new Point(62, 18);
            ManuelStringText.Name = "ManuelStringText";
            ManuelStringText.Size = new Size(398, 23);
            ManuelStringText.TabIndex = 0;
            // 
            // DatabaseSelecterForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(478, 450);
            Controls.Add(ManualPanel);
            Controls.Add(PostgreSqlPanel);
            Controls.Add(SqlServerPanel);
            Controls.Add(MySQLPanel);
            Controls.Add(HeaderPanel);
            Name = "DatabaseSelecterForm";
            Text = "DatabaseSelecterForm";
            Load += DatabaseSelecterForm_Load;
            MySQLPanel.ResumeLayout(false);
            MySQLPanel.PerformLayout();
            HeaderPanel.ResumeLayout(false);
            HeaderPanel.PerformLayout();
            SqlServerPanel.ResumeLayout(false);
            SqlServerPanel.PerformLayout();
            PostgreSqlPanel.ResumeLayout(false);
            PostgreSqlPanel.PerformLayout();
            ManualPanel.ResumeLayout(false);
            ManualPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel MySQLPanel;
        private Label EMSuiteText;
        private TextBox EMsuitePath;
        private Label NotEmText;
        private TextBox NotEmPath;
        private Panel HeaderPanel;
        private ComboBox connectionStringChoice;
        private Label ServerTypeLabel;
        private Button SaveButton;
        private TableLayoutPanel SqlServerPanel;
        private Label label1;
        private TextBox textBox1;
        private Label label2;
        private TextBox textBox2;
        private TableLayoutPanel PostgreSqlPanel;
        private Label label3;
        private TextBox textBox3;
        private Label label4;
        private TextBox textBox4;
        private TableLayoutPanel ManualPanel;
        private Label JsonStringLabel;
        private TextBox ManuelStringText;
    }
}