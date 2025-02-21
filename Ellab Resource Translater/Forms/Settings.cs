using Ellab_Resource_Translater.Objects;
using Ellab_Resource_Translater.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Config = Ellab_Resource_Translater.Util.Config;

namespace Ellab_Resource_Translater
{
    public partial class Settings : Form
    {
        private int setup = 0;
        public Settings()
        {
            InitializeComponent();
            this.FormClosed += Settings_Exit;
        }

        private void EMsuiteBrowse_Click(object sender, EventArgs e)
        {
            /*var folderPaths = EMsuitePath.Text.Reverse<char>().ToString().Split('\\', 2);
            if(folderPaths.Length > 1)
            {
                folderBrowserDialogEMsuite.InitialDirectory = folderPaths[1].Reverse<char>().ToString();
            }*/
            var dialogResult = EMsuiteFBDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                EMsuitePath.Text = EMsuiteFBDialog.SelectedPath;
            }
        }

        private void NotEmBrowse_Click(object sender, EventArgs e)
        {
            var dialogResult = ValFBDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                ValPath.Text = ValFBDialog.SelectedPath;
            }
        }

        private void TranslationCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // While Loading I don't want this to run
            if (setup > 0)
                return;

            var config = Config.Get();
            var languagePairs = Config.DefaultLanguages();

            FormUtils.SaveCheckBoxListLocalised(
                list: config.languagesToAiTranslate,
                checkedListBox: TranslationCheckedListBox,
                localiser: languagePairs);
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            var config = Config.Get();
            var languagePairs = Config.DefaultLanguages();

            setup++;
            FormUtils.LoadCheckboxListLocalised(
                list: config.languagesToAiTranslate,
                checkedListBox: TranslationCheckedListBox,
                localiser: languagePairs
                );

            EMsuitePath.Text = config.EMPath;
            ValPath.Text = config.ValPath;
            ReaderNumeric.Value = config.threadsToUse;
            InserterNumeric.Value = config.insertersToUse;
            CloseOnSuccess.Checked = config.closeOnceDone;
            Config.AssignSizeSetting(this, (s) => config.SettingWindowSize = s, this.Size);
            setup--;

            TooltipNormal.SetToolTip(ReaderLabel, "Amount of threads to use when reading/writing from/to the disk");
            TooltipNormal.SetToolTip(InserterLabel, "Amount of threads to use when writing to the database");
            TooltipNormal.SetToolTip(DelayLabel, "");
        }

        private void Settings_Exit(object? sender, EventArgs e)
        {
            Config.Save();
        }

        private void EMsuitePath_TextChanged(object sender, EventArgs e)
        {
            // While Loading I don't want this to run
            if (setup > 0)
                return;

            Config.Get().EMPath = EMsuitePath.Text;
        }

        private void NotEmPath_TextChanged(object sender, EventArgs e)
        {
            // While Loading I don't want this to run
            if (setup > 0)
                return;

            Config.Get().ValPath = ValPath.Text;
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            // While Loading I don't want this to run
            if (setup > 0)
                return;

            Config.Get().threadsToUse = (int)ReaderNumeric.Value;
        }

        private void CloseOnSuccess_CheckedChanged(object sender, EventArgs e)
        {
            // While Loading I don't want this to run
            if (setup > 0)
                return;

            Config.Get().closeOnceDone = CloseOnSuccess.Checked;
        }

        private void InserterNumeric_ValueChanged(object sender, EventArgs e)
        {
            // While Loading I don't want this to run
            if (setup > 0)
                return;

            Config.Get().insertersToUse = (int)InserterNumeric.Value;
        }

        private void DelayNumeric_ValueChanged(object sender, EventArgs e)
        {
            Config.Get().checkDelay = Convert.ToInt32(DelayNumeric.Value);
        }
    }
}
