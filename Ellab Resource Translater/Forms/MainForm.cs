using Azure;
using Ellab_Resource_Translater.Forms;
using Ellab_Resource_Translater.Objects;
using Ellab_Resource_Translater.Translators;
using Ellab_Resource_Translater.Util;
using Newtonsoft.Json;
using System.Data.Common;

namespace Ellab_Resource_Translater
{
    public partial class MainForm : Form
    {
        private Settings? activeSetting;
        private int setup = 0;
        private bool batching = false;
        private ConnectionProvider? connProv;
        private TranslationService? translationService;
        internal const string CONNECTION_SECRET = "EllabResourceTranslator:dbConnection";
        internal const string AZURE_SECRET = "EllabResourceTranslator:azure";
        private CancellationTokenSource? cancelTSource;

        private string connectionStringState = "";
        private readonly Task connectionStringUpdater;

        public MainForm()
        {
            InitializeComponent();
            connectionStringUpdater = new Task(() => {
                while (!IsDisposed)
                {
                    connectionStatus.Invoke(() => connectionStatus.Text = string.Concat("DB ", connectionStringState));
                    Task.Delay(500).Wait();
                }
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var config = Config.Get();
            var languagePairs = Config.DefaultLanguages();
            var checkitems = config.languagesToTranslate;

            setup++;
            FormUtils.LoadCheckboxListLocalised(
                list: checkitems,
                checkedListBox: translationCheckedListBox,
                localiser: languagePairs
                );

            TryConnectDB();
            TryConnectAzure();
            Config.AssignSizeSetting(this, (s) => config.MainWindowSize = s, config.MainWindowSize);
            setup--;

            connectionStringUpdater.Start();
        }

        public Task TryConnectDB()
        {
            return Task.Run(async () =>
            {
                // Avoid trying to refresh while still connecting.
                RefreshConnectionButton.Invoke(() => RefreshConnectionButton.Enabled = false);

                // cleanup
                connProv?.Dispose();

                string? connString = SecretManager.GetUserSecret(CONNECTION_SECRET);

                // Debugging
                //RefreshConnectionButton.Invoke(() => MessageBox.Show(dbConn.Replace(";", ";\n")));


                if (connString != null)
                {
                    connProv = new(connString);
                    connectionStringState = "Test...";
                    try
                    {
                        using DbConnection conn = connProv.Get();
                        await conn.OpenAsync();
                        if (conn.State == System.Data.ConnectionState.Open)
                        {
                            connectionStringState = "Can Connect";
                        }
                        await conn.CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        connectionStringState = ex.Message;
                        return;
                    }
                    return;
                }
                else
                {
                    connectionStringState = "Need Setup:";
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
                        AzureConnectionStatus.Invoke(() =>
                        {
                            AzureConnectionStatus.Text = "Azure, Need Credentials";
                            OpenAzureSetup();
                        });
                        return;
                    }
                }
                catch
                {
                    AzureConnectionStatus.Invoke(() =>
                    {
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

        private void MainForm_Closed(object sender, EventArgs e)
        {
            connProv?.Dispose();
            Config.Save();
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

            FormUtils.SaveCheckBoxListLocalised(
                list: config.languagesToTranslate,
                checkedListBox: translationCheckedListBox,
                localiser: languagePairs);

        }

        private async void ValSuite_Initiation(object sender, EventArgs e)
        {
            AbleControls(false);

            if (Config.Get().languagesToAiTranslate.Count == 0 || translationService != null)
            {
                cancelTSource = new CancellationTokenSource();

                await Task.Run(() => ValSuite_Init(translationService, cancelTSource));

                progressTitle.Invoke(() => progressTitle.Text = cancelTSource.IsCancellationRequested ? "Request Cancelled" : "Request Done");

                if (!cancelTSource.IsCancellationRequested && Config.Get().closeOnceDone)
                    Close();

                cancelTSource.Dispose();
            }
            else
            {
                MessageBox.Show(@"Azure not connected, you can either:
    1) Setup Azure in the Azure button at the buttom.
    2) Disable AI Translation for all groups in Settings");
            }
            AbleControls(true);
        }

        private void ValSuite_Init(TranslationService? transServ, CancellationTokenSource source)
        {
            progressTitle.Invoke(() => progressTitle.Text = "Val Suite");
            var config = Config.Get();
            if (config.ValPath != null && config.ValPath != "" && !source.IsCancellationRequested)
            {
                try
                {
                    ValSuite val = new(transServ, connProv, source);
                    val.Run(config.ValPath, progressListView, progressTracker);
                }
                catch (OperationCanceledException){}

            } else if (batching == true)
            {
                DialogResult shouldWeContinue = MessageBox.Show("Check ValSuite path in Settings.\nShould we continue with the rest?", "Val suite Path Missing!", MessageBoxButtons.YesNo);
                if (shouldWeContinue != DialogResult.Yes) source.Cancel();
            }
            else
            {
                MessageBox.Show("Check ValSuite path in Settings", "Val suite path Missing", MessageBoxButtons.OK);
            }
        }

        private async void EMSuite_Initiation(object sender, EventArgs e)
        {
            AbleControls(false);

            if (Config.Get().languagesToAiTranslate.Count == 0 || translationService != null)
            {
                cancelTSource = new CancellationTokenSource();

                await Task.Run(() => EMSuite_Init(translationService, cancelTSource));

                progressTitle.Invoke(() => progressTitle.Text = "Done");

                if (!cancelTSource.IsCancellationRequested && Config.Get().closeOnceDone)
                    Close();

                cancelTSource.Dispose();
            }
            else
            {
                MessageBox.Show(@"Azure not connected, you can either:
    1) Setup Azure in the Azure button at the buttom.
    2) Disable AI Translation for all groups in Settings");
            }

            AbleControls(true);
        }

        /// <summary>
        /// Disables the Buttons so that we don't Instantiate multiple tranlations at once
        /// </summary>
        /// <param name="enable">enabled or not - reversed for Cancel button.</param>
        private void AbleControls(bool enable)
        {
            ValSuiteButton.Enabled = enable;
            EMSuiteButton.Enabled = enable;
            EMandValButton.Enabled = enable;
            SettingsButton.Enabled = enable;
            DBConnectionSetup.Enabled = enable;
            AzureSettingsSetup.Enabled = enable;
            RefreshAzureButton.Enabled = enable;
            RefreshConnectionButton.Enabled = enable;
            translationCheckedListBox.Enabled = enable;
            CancellationButton.Enabled = !enable;
        }

        private void EMSuite_Init(TranslationService? transServ, CancellationTokenSource source)
        {
            progressTitle.Invoke(() => progressTitle.Text = "EM Suite");
            var config = Config.Get();
            if (config.EMPath != null && config.EMPath != "" && !source.IsCancellationRequested)
            {
                try
                {
                    EMSuite emsuite = new(transServ, connProv, source);
                    emsuite.Run(config.EMPath, progressListView, progressTracker);
                }
                catch (OperationCanceledException){}
            } else if (batching == true)
            {
                DialogResult shouldWeContinue = MessageBox.Show("Check EMSuite path in Settings.\nShould we continue with the rest?", "EM suite Path Missing!", MessageBoxButtons.YesNo);
                if (shouldWeContinue != DialogResult.Yes) source.Cancel();
            }
            else
            {
                MessageBox.Show("Check EMSuite path in Settings", "EM suite path Missing", MessageBoxButtons.OK);
            }
        }

        private async void EMandValButton_Click(object sender, EventArgs e)
        {
            AbleControls(false);

            batching = true;

            if (Config.Get().languagesToAiTranslate.Count == 0 || translationService != null)
            {
                cancelTSource = new CancellationTokenSource();
                if (!cancelTSource.IsCancellationRequested)
                    await Task.Run(() => EMSuite_Init(translationService, cancelTSource));
                if (!cancelTSource.IsCancellationRequested) // in case we want to cancel after finding out EMsuite didn't have a value
                    await Task.Run(() => ValSuite_Init(translationService, cancelTSource));

                if (!cancelTSource.IsCancellationRequested && Config.Get().closeOnceDone)
                    Close();

                cancelTSource.Dispose();
            }
            else
            {
                MessageBox.Show(@"Azure not connected, you can either:
    1) Setup Azure in the Azure button at the buttom.
    2) Disable AI Translation for all groups in Settings");
            }
            progressTitle.Invoke(() => progressTitle.Text = "Done");
            batching = false;

            AbleControls(true);
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

        private void CancellationButton_Click(object sender, EventArgs e)
        {
            cancelTSource?.Cancel();
        }
    }
}
