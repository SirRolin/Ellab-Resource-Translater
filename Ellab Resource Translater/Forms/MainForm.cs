using Azure;
using Azure.AI.Translation.Text;
using Azure.Core;
using Ellab_Resource_Translater.Forms;
using Ellab_Resource_Translater.Util;
using System.Data.Common;
using System.Drawing;
using System.Net;
using System.Runtime.CompilerServices;

namespace Ellab_Resource_Translater
{
    public partial class MainForm : Form
    {
        private Settings? activeSetting;
        private int setup = 0;
        private bool batching = false;
        private DbConnection? dbConnection;
        internal const string CONNECTION_SECRET = "EllabResourceTranslator:dbConnection";
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var config = Config.Get();
            var languagePairs = Config.DefaultLanguages();
            var checkitems = config.languagesToTranslate;
            setup++;
            RolinsFormUtils.LoadCheckboxListLocalised(
                list: checkitems,
                checkedListBox: translationCheckedListBox,
                localiser: languagePairs
                );

            TryConnectDB();

            setup--;
        }

        public Task TryConnectDB()
        {
            return Task.Run(async () =>
            {
                TryCloseDBConnection();

                string? dbConn = SecretManager.GetUserSecret(CONNECTION_SECRET);
                RefreshConnectionButton.Invoke(() => RefreshConnectionButton.Enabled = false);
                if (dbConn != null)
                {
                    dbConnection = DBStringHandler.CreateDbConnection(dbConn);
                    connectionStatus.Invoke(() => connectionStatus.Text = "Connecting...");
                    try
                    {
                        await dbConnection.OpenAsync();
                    }
                    catch (Exception ex)
                    {
                        connectionStatus.Invoke(() => connectionStatus.Text = ex.Message);
                        return;
                    }
                    if (dbConnection.State == System.Data.ConnectionState.Open)
                    {
                        connectionStatus.Invoke(() => connectionStatus.Text = "Connected");
                    }
                }
                else
                {
                    connectionStatus.Invoke(() =>
                    {
                        connectionStatus.Text = "Need Connection String - Try Setup";
                    });
                }
            });
        }

        private void TryCloseDBConnection()
        {
            if (dbConnection != null && dbConnection.State != System.Data.ConnectionState.Closed)
            {
                dbConnection.Close();
                connectionStatus.Invoke(() => connectionStatus.Text = "Closed");
            }
        }

        private void Form1_Closed(object sender, EventArgs e)
        {
            Config.Save();
            TryCloseDBConnection();
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            if (activeSetting == null || activeSetting.IsDisposed)
            {
                activeSetting = new Settings();
                activeSetting.FormClosed += (s, e) => activeSetting = null;
                activeSetting.ShowDialog();
            }
            else
            {
                activeSetting.BringToFront();
            }
        }

        private void translationCheckedListBox_CheckChanged(object sender, EventArgs e)
        {
            // While Loading I don't want this to run
            if (setup > 0)
                return;

            var config = Config.Get();
            var languagePairs = Config.DefaultLanguages();

            RolinsFormUtils.SaveCheckBoxListLocalised(
                list: config.languagesToTranslate,
                checkedListBox: translationCheckedListBox,
                localiser: languagePairs);

        }

        private async void ValSuite_Initiation(object sender, EventArgs e)
        {
            // Disables the Buttons so that we don't Instantiate multiple tranlations at once
            ValSuiteButton.Enabled = false;
            EMSuiteButton.Enabled = false;
            EMandValButton.Enabled = false;

            await Task.Run(() => ValSuite_Init());

            progressTitle.Invoke(() => progressTitle.Text = "Done");
            ValSuiteButton.Enabled = true;
            EMSuiteButton.Enabled = true;
            EMandValButton.Enabled = true;
        }

        private void ValSuite_Init()
        {
            progressTitle.Invoke(() => progressTitle.Text = "Val Suite");
            var config = Config.Get();
            if (config.NotEMPath != "")
            {
                Translators.ValSuite.Run(config.NotEMPath, config.languagesToTranslate, config.languagesToAiTranslate, progressListView, progressTracker);
            }
            else if (batching == true)
            {
                DialogResult shouldWeContinue = MessageBox.Show("Check ValSuite path in Settings.\nShould we continue with the rest?", "Val suite Path Missing!", MessageBoxButtons.YesNo);
                batching = shouldWeContinue == DialogResult.Yes;
            }
            else
            {
                MessageBox.Show("Check ValSuite path in Settings", "Val suite path Missing", MessageBoxButtons.OK);
            }
        }

        private async void EMSuite_Initiation(object sender, EventArgs e)
        {
            // Disables the Buttons so that we don't Instantiate multiple tranlations at once
            ValSuiteButton.Enabled = false;
            EMSuiteButton.Enabled = false;
            EMandValButton.Enabled = false;

            //TextTranslationClient client = new(new AzureKeyCredential(""), new Uri(""), ""); // TODO test without this AND after database setup with this
            TranslationService transServ = null;// new(client);

            await Task.Run(() => EMSuite_Init(transServ));

            progressTitle.Invoke(() => progressTitle.Text = "Done");
            ValSuiteButton.Enabled = true;
            EMSuiteButton.Enabled = true;
            EMandValButton.Enabled = true;
        }

        private void EMSuite_Init(TranslationService transServ)
        {
            progressTitle.Invoke(() => progressTitle.Text = "EM Suite");
            var config = Config.Get();
            if (config.EMPath != "")
            {
                Translators.EMSuite emsuite = new(transServ, dbConnection);
                emsuite.Run(config.EMPath, progressListView, progressTracker);
            }
            else if (batching == true)
            {
                DialogResult shouldWeContinue = MessageBox.Show("Check EMSuite path in Settings.\nShould we continue with the rest?", "EM suite Path Missing!", MessageBoxButtons.YesNo);
                batching = shouldWeContinue == DialogResult.Yes;
            }
            else
            {
                MessageBox.Show("Check EMSuite path in Settings", "EM suite path Missing", MessageBoxButtons.OK);
            }
        }

        private async void EMandValButton_Click(object sender, EventArgs e)
        {
            // Disables the Buttons so that we don't Instantiate multiple tranlations at once
            ValSuiteButton.Enabled = false;
            EMSuiteButton.Enabled = false;
            EMandValButton.Enabled = false;

            TextTranslationClient client = new(new AzureKeyCredential(""), new Uri(""), "");
            TranslationService transServ = new(client);

            batching = true;

            if (batching)
                await Task.Run(() => EMSuite_Init(transServ));
            if (batching) // in case we want to cancel after finding out EMsuite didn't have a value
                await Task.Run(() => ValSuite_Init());

            progressTitle.Invoke(() => progressTitle.Text = "Done");
            batching = false;

            ValSuiteButton.Enabled = true;
            EMSuiteButton.Enabled = true;
            EMandValButton.Enabled = true;
        }

        private void DBConnectionSetup_Click(object sender, EventArgs e)
        {
            DatabaseSelecterForm form = new(this);
            form.ShowDialog();
        }
    }
}
