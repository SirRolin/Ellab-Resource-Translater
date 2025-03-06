using Ellab_Resource_Translater.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Structs
{
    public readonly struct ChangeTranslationColumns(DataColumn resource, DataColumn key, DataColumn value, DataColumn comment, DataColumn language)
    {
        public DataColumn Resource { get; } = resource;
        public DataColumn Key { get; } = key;
        public DataColumn Value { get; } = value;
        public DataColumn Comment { get; } = comment;
        public DataColumn Language { get; } = language;
        public static bool TryExtract(Indexed<DataTable> dt, Action myUpdate, ConcurrentQueue<TableCollectionRow> dataRows, out ChangeTranslationColumns ctc)
        {
            if (dt.item.Columns["ResourceName"] is DataColumn resourceColumn
                && dt.item.Columns["Key"] is DataColumn keyColumn
                && dt.item.Columns["ChangedText"] is DataColumn textColumn
                && dt.item.Columns["Comment"] is DataColumn commentValue
                && dt.item.Columns["LanguageCode"] is DataColumn languageValue)
            {
                ctc = new(resourceColumn, keyColumn, textColumn, commentValue, languageValue);
                foreach (DataRow row in dt.item.Rows)
                {
                    dataRows.Enqueue(new TableCollectionRow(dt.index, row));
                    myUpdate();
                }
                return true;
            }
            ctc = default;
            return false;
        }
    }
}
