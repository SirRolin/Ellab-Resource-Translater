using Ellab_Resource_Translater.Objects;
using Ellab_Resource_Translater.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Translators
{
    internal class ValSuite(TranslationService? translationService, DbConnectionExtension? DBCon) : DBProcessorBase(translationService, DBCon, 0, Config.Get().threadsToUse)
    {
        internal void Run(string path, ListView view, Label progresText)
        {
            Run(path, view, progresText, new(".*(?<!\\...)\\.resx"));
        }
    }
}
