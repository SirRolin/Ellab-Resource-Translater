﻿using Ellab_Resource_Translater.Util;

namespace Ellab_Resource_Translater.Translators
{
    internal class EMSuite(TranslationService? translationService, ConnectionProvider? connProv, CancellationTokenSource source) : DBProcessorBase(translationService, connProv, source, 1, Config.Get().threadsToUse)
    {
        internal void Run(string path, ListView view, Label progresText)
        {
            Run(path, view, progresText, new(@".*\\Resources\\.*(?<!\..{0,5})\.resx"));
        }
    }
}
