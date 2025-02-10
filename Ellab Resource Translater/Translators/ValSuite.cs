using Ellab_Resource_Translater.Objects;
using Ellab_Resource_Translater.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellab_Resource_Translater.Translators
{
    internal class ValSuite(TranslationService? translationService, ConnectionProvider? DBCon, CancellationTokenSource source) : DBProcessorBase(translationService, DBCon, source, 0, Config.Get().threadsToUse)
    {
        internal void Run(string path, ListView view, Label progresText)
        {
            Run(path, view, progresText, new(@".*(?<!\..{0,5})\.resx"));
        }
    }
}
