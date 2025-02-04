using Azure;
using Ellab_Resource_Translater.Forms;
using Ellab_Resource_Translater.Objects;
using Ellab_Resource_Translater.Util;
using Newtonsoft.Json;

namespace Ellab_Resource_Translater
{
    public partial class MainForm : Form
    {
        private Settings? activeSetting;
        private int setup = 0;
        private bool batching = false;
        private DbConnectionExtension? dbConnection;
        private TranslationService? translationService;
        internal const string CONNECTION_SECRET = "EllabResourceTranslator:dbConnection";
        internal const string AZURE_SECRET = "EllabResourceTranslator:azure";
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
            TryConnectAzure();
            setup--;
        }

        public Task TryConnectDB()
        {
            return Task.Run(async () =>
            {
                TryCloseDBConnection();

                string? dbConn = SecretManager.GetUserSecret(CONNECTION_SECRET);

                // Debugging
                //RefreshConnectionButton.Invoke(() => MessageBox.Show(dbConn.Replace(";", ";\n")));

                // Avoid trying to refresh while still connecting.
                RefreshConnectionButton.Invoke(() => RefreshConnectionButton.Enabled = false);

                if (dbConn != null)
                {
                    dbConnection = new(DBStringHandler.CreateDbConnection(dbConn), dbConn);
                    connectionStatus.Invoke(() => connectionStatus.Text = "DB Connecting...");
                    try
                    {
                        await dbConnection.connection.OpenAsync();
                    }
                    catch (Exception ex)
                    {
                        connectionStatus.Invoke(() => connectionStatus.Text = ex.Message);
                        return;
                    }
                    if (dbConnection.connection.State == System.Data.ConnectionState.Open)
                    {
                        connectionStatus.Invoke(() => connectionStatus.Text = "DB Connected");
                    }
                    return;
                }
                else
                {
                    connectionStatus.Invoke(() =>
                    {
                        connectionStatus.Text = "DB, Need Connection String - Try Setup";
                    });
                }

                // Reenabling the refresh
                RefreshConnectionButton.Invoke(() => RefreshConnectionButton.Enabled = true);
            });
        }

        public Task TryConnectAzure()
        {
            return Task.Run(() =>
            {
                AzureCredentials? azureCreds;

                try
                {
                    string? jsonCreds = SecretManager.GetUserSecret(AZURE_SECRET);
                    if (jsonCreds != null)
                        azureCreds = JsonConvert.DeserializeObject<AzureCredentials>(jsonCreds);
                    else
                    {
                        AzureConnectionStatus.Invoke(() => {
                            AzureConnectionStatus.Text = "Azure, Need Credentials";
                            OpenAzureSetup();
                        });
                        return;
                    }
                }
                catch
                {
                    AzureConnectionStatus.Invoke(() => {
                        AzureConnectionStatus.Text = "Azure, Error with stored Credentials";
                        OpenAzureSetup();
                    });
                    return;
                }
                

                // Avoid trying to refresh while still connecting.
                RefreshAzureButton.Invoke(() => RefreshAzureButton.Enabled = false);

                if (azureCreds != null)
                {
                    AzureConnectionStatus.Invoke(() => AzureConnectionStatus.Text = "Azure Connecting...");
                    try
                    {
                        AzureKeyCredential credentials = new(azureCreds.Key);
                        translationService = new(creds: credentials, uri: new Uri(azureCreds.URI), region: azureCreds.Region);
                        AzureConnectionStatus.Invoke(() => AzureConnectionStatus.Text = "Azure Connected");
                    }
                    catch (Exception ex)
                    {
                        AzureConnectionStatus.Invoke(() => AzureConnectionStatus.Text = ex.Message);
                        return;
                    }
                    return;
                }
                else
                {
                    AzureConnectionStatus.Invoke(() =>
                    {
                        AzureConnectionStatus.Text = "Azure, Need Credentials";
                        OpenAzureSetup();
                    });
                }

                // Reenabling the refresh
                RefreshAzureButton.Invoke(() => RefreshAzureButton.Enabled = true);
            });
        }

        private void TryCloseDBConnection()
        {
            if (dbConnection != null && dbConnection.connection.State != System.Data.ConnectionState.Closed)
            {
                dbConnection.connection.Close();
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

        private void TranslationCheckedListBox_CheckChanged(object sender, EventArgs e)
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

            if(Config.Get().languagesToAiTranslate.Count == 0 || translationService != null)
            {
                await Task.Run(() => EMSuite_Init(translationService));

                progressTitle.Invoke(() => progressTitle.Text = "Done");
            }
            else
            {
                MessageBox.Show(@"Azure not connected, you can either:
    1) Setup Azure in the Azure button at the buttom.
    2) Disable AI Translation for all groups in Settings");
            }

            ValSuiteButton.Enabled = true;
            EMSuiteButton.Enabled = true;
            EMandValButton.Enabled = true;
        }

        private void EMSuite_Init(TranslationService? transServ)
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
            
            batching = true;

            if (batching)
                await Task.Run(() => EMSuite_Init(translationService));
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

        private void AzureSettingsSetup_Click(object sender, EventArgs e)
        {
            OpenAzureSetup();
        }

        private void OpenAzureSetup()
        {
            AzureForm azureform = new(this);
            azureform.ShowDialog();
        }

        private void RefreshConnectionButton_Click(object sender, EventArgs e)
        {
            TryConnectDB();
        }

        private void RefreshAzureButton_Click(object sender, EventArgs e)
        {
            TryConnectAzure();
        }
    }
}
