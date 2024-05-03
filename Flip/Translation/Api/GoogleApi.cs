﻿using Flip.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flip.Translation.Api
{
    public class GoogleApi : ITranslateApi
    {
        private Uri BaseUrl = new Uri("https://www.google.com/async/translate?vet=12ahUKEwjpyuqH2473AhXOwKQKHWivAdgQqDh6BAgDECw..i&ei=eolVYumlA86BkwXo3obADQ&yv=3");
        private string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36";
        private HttpClient HttpClient = null;
        private Random rnd = new Random();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private string proxyHost = "";
        private int proxyPort = 0;
        public GoogleApi()
        {
            PrepareClient();
            this.proxyHost = GlobalSettings.Settings.ProxyHost;
            this.proxyPort = GlobalSettings.Settings.ProxyPort;
        }
        private async void PrepareClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            handler.UseCookies = true;
            handler.CookieContainer = new System.Net.CookieContainer();
            if(string.IsNullOrEmpty(this.proxyHost) && this.proxyPort > 0 && this.proxyPort <= 65535)
            {
                if (await Utility.CheckProxy(this.proxyHost, this.proxyPort))
                {
                    handler.Proxy = new WebProxy(proxyHost, proxyPort);
                }
            }
            HttpClient = new HttpClient(handler);
            HttpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            HttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        public async Task<bool?> CheckApi()
        {
            return (await this.GetTranslate("Examinate", Languages.LanguageList["English"], Languages.LanguageList["Persian"]))?.Success;
        }

        public long GetPingToApi()
        {
            try
            {
                Ping ping = new Ping();
                var rsp = ping.Send(BaseUrl.Host);
                return rsp.RoundtripTime;
            }
            catch(PingException)
            {
                return -1;
            }
        }
        private TranslationModel? ParseResponse(string response)
        {
            List<string> Translations = new List<string>();
            try
            {
                if (string.IsNullOrEmpty(response))
                    return new TranslationModel(false);
                MatchCollection match1 = Regex.Matches(response, "<span dir=\"rtl\" class=\".*?\" lang=\\\".*?\\\">(.*?)<\\/span>");
                MatchCollection match2 = Regex.Matches(response, "id=\"tw-answ-target-text\">(.*?)<\\/span>");
                foreach (Match match in match1)
                {
                    Translations.Add(match.Groups[1].Value);
                }
                foreach (Match match in match2)
                {
                    Translations.Add(match.Groups[1].Value);
                }
                return new TranslationModel(Translations.Count > 0)
                {
                    Translations = Translations,
                };
            }
            catch
            {
                return new TranslationModel(false);
            }
        }
        
        public async Task<TranslationModel?> GetTranslate(string text, string from, string to)
        {
            try
            {
                int id = rnd.Next(111111, 999999);
                string PostData = $"async=translate,sl:{from},tl:{to},st:{text},id:1647{id},qc:true,ac:true,_id:tw-async-translate,_pms:s,_fmt:pc";
                HttpResponseMessage rsp = await this.HttpClient.PostAsync(BaseUrl, new StringContent(PostData, new MediaTypeHeaderValue("application/x-www-form-urlencoded")), cancellationTokenSource.Token);
                string rspText = await rsp.Content.ReadAsStringAsync();
                if (rsp.StatusCode != System.Net.HttpStatusCode.OK)
                    return new TranslationModel(false);
                PrepareClient();
                TranslationModel? model = ParseResponse(rspText);
                if (model != null && model.Success)
                {
                    model.from = from;
                    model.to = to;
                    model.Text = text;
                }
                return model;
            }
            catch(Exception ex)
            { 
                return new TranslationModel(false) { ExceptionMessage = ex.Message}; 
            }
        }
        public void CancelOperation()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
