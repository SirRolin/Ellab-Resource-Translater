using Ellab_Resource_Translater.Enums;
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
            string connectionString = "";
            switch (connectionStringChoice.SelectedIndex)
            {
                case 0:
                    //MySQLPanel.Show();
                    break;
                case 1:
                    //SqlServerPanel.Show();
                    break;
                case 2:
                    //PostgreSqlPanel.Show();
                    break;
                case 3:
                    connectionString = DBStringHandler.JsonExtractIfNeeded(ManuelStringText.Text);
                    break;
                default:
                    break;
            }
            MessageBox.Show("Type: " + DBStringHandler.DetectType(connectionString));
            if (DBStringHandler.DetectType(connectionString) != ConnType.None)
            {
                SecretManager.SetUserSecret(MainForm.CONNECTION_SECRET, connectionString);
                mainFormParent.TryConnectDB();
                //this.Close();
            }
        }
    }
}
