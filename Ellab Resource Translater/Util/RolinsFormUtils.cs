using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Util
{
    internal class RolinsFormUtils
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
    }
}
