using Flip.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flip.Translation.Api
{
    public class DuckDuckGoApi : ITranslateApi
    {
        private Uri BaseUrl = new Uri("https://duckduckgo.com");
        private string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36";
        private HttpClient HttpClient = null;
        private Random rnd = new Random();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private string proxyHost = "";
        private int proxyPort = 0;
        public DuckDuckGoApi()
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
            if (string.IsNullOrEmpty(this.proxyHost) && this.proxyPort > 0 && this.proxyPort <= 65535)
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
            return (await this.GetTranslate("Examinate", Language.En, Language.Fa))?.Success;
        }

        public long GetPingToApi()
        {
            try
            {
                Ping ping = new Ping();
                var rsp = ping.Send(BaseUrl.Host);
                return rsp.RoundtripTime;
            }
            catch (PingException)
            {
                return -1;
            }
        }
        private TranslationModel? ParseResponse(string response)
        {
            try
            {
                Match match = Regex.Match(response, "\"translated\":\"(.*?)\"}");
                if (match.Success)
                {
                    string a = match.Groups[1].Value;
                    string translation = Regex.Unescape(a);
                    return new TranslationModel(true) { Translations = new List<string>() { translation } };
                }
                return new TranslationModel(false);
            }
            catch
            {
                return new TranslationModel(false);
            }
        }

        public async Task<TranslationModel?> GetTranslate(string text, Language from, Language to)
        {
            try
            {
                string PostUrl = $"https://duckduckgo.com/translation.js?vqd=4-82492802282990933213578594351879680983&query=translate&from={from.ToString().ToLower()}&to={to.ToString().ToLower()}";
                HttpResponseMessage rsp = await this.HttpClient.PostAsync(PostUrl, new StringContent(text, new MediaTypeHeaderValue("text/plain")), cancellationTokenSource.Token);
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
            catch (Exception ex)
            {
                return new TranslationModel(false) { ExceptionMessage = ex.Message };
            }
        }
        public void CancelOperation()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
