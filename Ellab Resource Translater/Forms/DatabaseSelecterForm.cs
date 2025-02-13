﻿using Ellab_Resource_Translater.Enums;
using Ellab_Resource_Translater.Util;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ellab_Resource_Translater.Forms
{
    public partial class DatabaseSelecterForm : Form
    {
        private readonly string[] choices =
        [
            "MySQL",
            "SQL Server",
            "POSTgresSQL",
            "Manual (or Json)"
        ];

        private readonly MainForm mainFormParent;

        public DatabaseSelecterForm(MainForm parent)
        {
            this.mainFormParent = parent;
            InitializeComponent();
        }

        private void DatabaseSelecterForm_Load(object sender, EventArgs e)
        {
            // To Continue from last setup
            string? dbconn = SecretManager.GetUserSecret(MainForm.CONNECTION_SECRET);
            connectionStringChoice.SelectedIndex = dbconn == null ? 3 :
                DBStringHandler.DetectType(dbconn) switch
                {
                    ConnType.MySql => 0,
                    ConnType.MSSql => 1,
                    ConnType.PostgreSql => 2,
                    _ => 3,
                };
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CollapseGroups();
            SwitchToMethod();
        }

        private void SwitchToMethod()
        {
            switch (connectionStringChoice.SelectedIndex)
            {
                case 0:
                    MySQLPanel.Show();
                    break;
                case 1:
                    SqlServerPanel.Show();
                    break;
                case 2:
                    PostgreSqlPanel.Show();
                    break;
                case 3:
                    ManualPanel.Show();
                    break;
                default:
                    break;
            }
        }

        private void CollapseGroups()
        {
            MySQLPanel.Hide();
            SqlServerPanel.Hide();
            PostgreSqlPanel.Hide();
            ManualPanel.Hide();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string connectionString = connectionStringChoice.SelectedIndex switch
            {
                0 => MySqlConnString(),
                1 => MSSqlConnString(),
                2 => PostgreSqlConnString(),
                3 => DBStringHandler.JsonExtractIfNeeded(ManuelStringText.Text),
                _ => "" // Default
            };

            // In Case someone somehow can input something wrong in a dropdownlist
            if (connectionString == "") return;

            if (DBStringHandler.DetectType(connectionString) != ConnType.None)
            {
                SecretManager.SetUserSecret(MainForm.CONNECTION_SECRET, connectionString);
                mainFormParent.TryConnectDB();
                this.Close();
            }
        }

        private string PostgreSqlConnString()
        {
            return $"Host={PostgresHostText.Text};Port={(PostgresPortText.Text == "" ? "5432" : PostgresPortText.Text)}; Database={PostgresDatabaseText.Text}; User ID={PostgresUserIDText.Text}; Password={PostgresPasswordText.Text}";
        }

        private string MSSqlConnString()
        {
            return $"Server={MSSqlServerText.Text}{(MSSqlPortText.Text != "" ? "," + MSSqlPortText.Text : "")};Database={MSSqlDatabaseText.Text};Persist Security Info=False;User ID={MSSqlUserIDText.Text};Password={MSSqlPasswordText};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        private string MySqlConnString()
        {
            StringBuilder sb = new();
            sb.Append($"Server ={ MySqlServerText.Text};");
            if(MySqlPortText.Text.Length > 0)
                sb.Append($"port={ MySqlPortText.Text};");
            sb.Append($"Database={MySqlDatabaseText.Text};Uid={MySqlUserIDText.Text};Pwd={MySqlPasswordText};Encrypt=True;SslMode=Required;default command timeout=30;");
            return sb.ToString();
        }

        private void MySqlIsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            MySqlUserIDText.Enabled = MySqlISCheckBox.Checked;
            MySqlPasswordText.Enabled = MySqlISCheckBox.Checked;
        }

        private void MSSQLISCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            MSSqlUserIDText.Enabled = MSSqlISCheckBox.Checked;
            MSSqlPasswordText.Enabled = MSSqlISCheckBox.Checked;
        }
    }
}
