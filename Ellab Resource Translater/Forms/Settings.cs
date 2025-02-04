using Ellab_Resource_Translater.objects;
using Ellab_Resource_Translater.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            var dialogResult = NotEmFBDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                NotEmPath.Text = NotEmFBDialog.SelectedPath;
            }
        }

        private void TranslationCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // While Loading I don't want this to run
            if (setup > 0)
                return;

            var config = Config.Get();
            var languagePairs = Config.DefaultLanguages();

            RolinsFormUtils.SaveCheckBoxListLocalised(
                list: config.languagesToAiTranslate,
                checkedListBox: translationCheckedListBox,
                localiser: languagePairs);
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            var config = Config.Get();
            var languagePairs = Config.DefaultLanguages();

            setup++;
            RolinsFormUtils.LoadCheckboxListLocalised(
                list: config.languagesToAiTranslate,
                checkedListBox: translationCheckedListBox,
                localiser: languagePairs
                );

            EMsuitePath.Text = config.EMPath;
            NotEmPath.Text = config.NotEMPath;
            coresNumeric.Value = config.threadsToUse;
            setup--;
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

            Config.Get().NotEMPath = NotEmPath.Text;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            // While Loading I don't want this to run
            if (setup > 0)
                return;

            Config.Get().threadsToUse = (int) coresNumeric.Value;
        }
    }
}
