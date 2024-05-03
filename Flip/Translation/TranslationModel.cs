using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flip.Translation
{
    public class TranslationModel
    {
        public string? Text { get; set; }
        public List<string>? Translations { get; set; }
        public Language from { get; set; }
        public Language to { get; set; }
        public bool Success { get; set; }
        public string? ExceptionMessage { get; set; }
        public TranslateApi TranslateApi { get; set; }
        public TranslationModel(bool success)
        {
            Success = success;
        }
    }
}
