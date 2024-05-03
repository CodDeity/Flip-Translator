using Flip.Settings;
using Flip.Translation.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Flip.Translation
{
    public static class Translator
    {
        private static GoogleApi googleApi = new GoogleApi();
        private static DuckDuckGoApi duckDuckGoApi = new DuckDuckGoApi();
        
        public async static Task<TranslationModel?> Translate(string text, string from, string to,bool fromClipboard = false)
        {
            TranslateApi api = GlobalSettings.Settings.TranslateApi;
            return await Translate(text, from, to, api,fromClipboard);
        }
        public async static Task<TranslationModel?> Translate(string Text, string from,string to,TranslateApi TranslateApi,bool fromClipboard = false)
        {
            if (GlobalSettings.Settings.TryOfflineUse)
            {
                TranslationModel? offlineModel = OfflineTranslations.Get(Text, from, to);
                if (offlineModel != null)
                {
                    offlineModel.TranslateApi = TranslateApi.OfflineDataBase;
                    return offlineModel;
                }
            }
            ITranslateApi? api = GetApi(TranslateApi);
            if (api == null)
                throw new ArgumentException("no suitable api found!");
            TranslationModel? model = await api.GetTranslate(Text, from, to);
            if (model != null && ((fromClipboard && GlobalSettings.Settings.StoreClipData) || !fromClipboard))
            {
                model.TranslateApi = TranslateApi;
                if (model.Success && GlobalSettings.Settings.StoreSingleWord && isSingleWord(model.Text))
                {
                    OfflineTranslations.Add(model);
                }
            }
            return model;

        }
        public static bool isSingleWord(string text)
        {
            if(text.Count(c => c == ' ') <= 1)
                return true;
            else
                return false;
        }
        private static ITranslateApi? GetApi(TranslateApi api)
        {
            switch(api)
            {
                case TranslateApi.Google:
                    return googleApi;
                case TranslateApi.DuckDuckgo:
                    return duckDuckGoApi;
                    break;
            }
            return null;
        }
    }

}
