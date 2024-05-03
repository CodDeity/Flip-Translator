using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace Flip
{
    public static class Utility
    {
        public async static Task<bool> CheckProxy(string proxyHost,int port)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Proxy = new WebProxy(proxyHost, port);
            HttpClient client = new HttpClient(handler);
            HttpResponseMessage rsp = await client.GetAsync("https://google.com");
            if(rsp.IsSuccessStatusCode == true)
            {
                return true;
            }
            return false;
        }
    }
}
