using Flip.Translation.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;

namespace Flip.Translation
{
    public static class OfflineTranslations
    {
        public readonly static string TranslationsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "OfflineTranslations");
        private static StreamWriter? StreamWriter = null;
        private static StreamReader? StreamReader = null;
        private static FileStream? FileStream = null;
        private static List<string> Translations = new List<string> ();
        public static void Load()
        {
            FileStream = File.Open(TranslationsFilePath, FileMode.OpenOrCreate);
            StreamReader = new StreamReader(FileStream);
            StreamWriter = new StreamWriter(FileStream);
            Translations = ReadAllLines();
        }
        private static string Parse(TranslationModel model)
        {
            return $"{model.Text.ToLower()} ({model.from}): {model.Translations.First()} ({model.to})";
        }
        private static List<string> ReadAllLines()
        {
            List<string> lines = new List<string>();
            while (!StreamReader.EndOfStream)
            {
                lines.Add(StreamReader.ReadLine());
            }
            return lines;
        }
        private static (bool, string) ContainTranslation(string text, string src)
        {
            try
            {
                text = text.Replace(" ","").ToLower();
                bool Contained = false;
                string? ParsedLine = "";
                Parallel.ForEach(Translations, (line, state, LNum) =>
                {
                    Match match = Regex.Match(line, "(.*?) \\((.*?)\\):");
                    if (match.Success)
                    {
                        if (match.Groups[1].Value.Replace(" ","") == text && match.Groups[2].Value == src)
                        {
                            Contained = true;
                            ParsedLine = line;
                            state.Stop();
                        }
                    }
                });
                return (Contained, ParsedLine);
            }
            catch
            {
                return (false, "");
            }
        }
        public static void Add(TranslationModel? translationModel)
        {
            if (!ContainTranslation(translationModel.Text,translationModel.from).Item1)
            {
                string parsed = Parse(translationModel);
                Translations.Add(parsed);
                StreamWriter.WriteLine(parsed);
                StreamWriter.Flush();
            }
        }
        private static TranslationModel? ParseDown(string parsedText)
        {
            if (string.IsNullOrEmpty(parsedText))
                return null;
            Match match = Regex.Match(parsedText, "(.*) \\((.*?)\\): (.*) \\((.*)\\)");
            if (match.Success)
            {
                TranslationModel translationModel = new TranslationModel(true)
                {
                    Text = match.Groups[1].Value,
                    from = match.Groups[2].Value,
                    Translations = new List<string>() { match.Groups[3].Value },
                    to = match.Groups[4].Value
                };
                return translationModel;
            }
            return null;
        }
        public static TranslationModel? Get(string text,string src ,string dst)
        {
            (bool ,string ) ContainedStruct = ContainTranslation(text,src);
            TranslationModel? translation = ParseDown(ContainedStruct.Item2);
            if (translation == null)
                return null;
            if (ContainedStruct.Item1 && translation.to == dst)
            {
                return translation;
            }
            return null;
        }
    }
}
