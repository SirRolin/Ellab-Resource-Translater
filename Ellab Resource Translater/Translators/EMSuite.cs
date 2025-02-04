using Ellab_Resource_Translater.objects;
using Ellab_Resource_Translater.Util;
using Microsoft.Data.SqlClient;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Resources;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using Ellab_Resource_Translater.Objects;
using System.Linq;

namespace Ellab_Resource_Translater.Translators
{
    internal class EMSuite(TranslationService? translationService, DbConnectionExtension? DBCon) : DBProcessorBase(translationService, DBCon, 1, Config.Get().threadsToUse)
    {
        internal void Run(string path, ListView view, Label progresText)
        {
            Run(path, view, progresText, new(@".*\\Resources\\.*(?<!\..{0,5})\.resx"));
        }
    }
}
