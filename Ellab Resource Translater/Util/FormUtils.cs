using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Util
{
    internal class FormUtils
    {
        public static void SaveCheckBoxListLocalised(List<string> list, CheckedListBox checkedListBox, Dictionary<string, string> localiser)
        {
            var sindex = checkedListBox.SelectedIndex;
            var sitem = checkedListBox.SelectedItem;

            // Rare case of no selected item
            if (sitem == null)
                return;

            // Since this happens BEFORE the check is made, the check state is opposite of what it's gonna be
            if (!checkedListBox.GetItemChecked(sindex))
                list.Add(localiser.FirstOrDefault(predicate: x => sitem.Equals(x.Value)).Key);
            else
                list.RemoveAll(item => item.Equals(localiser.FirstOrDefault(predicate: x => sitem.Equals(x.Value)).Key));
        }

        public static void LoadCheckboxListLocalised(List<string> list, CheckedListBox checkedListBox, Dictionary<string, string> localiser)
        {
            var checkitems = list.Select(x => localiser[x]).ToList();
            checkedListBox.Items.Clear();
            localiser.Values.ToList().ForEach(x => checkedListBox.Items.Add(x));
            int height = 0;
            for (int i = 0; i < checkedListBox.Items.Count; i++)
            {
                height += checkedListBox.GetItemHeight(i) + 1;
                if (checkitems.Contains(checkedListBox.Items[i]))
                    checkedListBox.SetItemChecked(i, true);
            }

            // Changing the height as there's no AutoSize on 
            var width = checkedListBox.Width * 
                (checkedListBox.MaximumSize.Height <= 0 ? 1 : (height / checkedListBox.MaximumSize.Height));
            checkedListBox.Size = new System.Drawing.Size(width, height);
        }
        public static void HandleProcess(Action update, ListView listView, string resourceName, Action process)
        {
            HandleProcess((s) => s, update, listView, resourceName, process);
        }

        public static void HandleProcess(int pathLength, Action update, ListView listView, string resourceName, Action process)
        {
            HandleProcess(pathLength, update, listView, resourceName, () => { process(); return string.Empty; });
        }

        public static void HandleProcess(Func<string, string> getResource, Action update, ListView listView, string resourceName, Action process)
        {
            HandleProcess(getResource, update, listView, resourceName, () => { process(); return string.Empty; });
        }

        public static TResult HandleProcess<TResult>(int pathLength, Action update, ListView listView, string resourceName, Func<TResult> process)
        {
            return HandleProcess((s) => s[(pathLength+1)..], update, listView, resourceName, process);
        }

        public static TResult HandleProcess<TResult>(Func<string, string> getResource, Action update, ListView listView, string resourceName, Func<TResult> process)
        {
            string shortenedPath = getResource(resourceName); // Remove the root path
            ListViewItem listViewItem = listView.Invoke(() => listView.Items.Add(shortenedPath));

            var output = process();

            listView.Invoke(() => listView.Items.Remove(listViewItem));
            update.Invoke();

            return output;
        }

        public static void LabelTextUpdater(Label label, params object[] texts)
        {
            label.Invoke(() => label.Text = string.Concat(texts));
        }
    }
}
