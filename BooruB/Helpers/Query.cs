﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Streams;

namespace BooruB.Helpers
{
    public class Query
    {
        public static HttpClient client = null;

        public virtual async Task<string> Get(string url)
        {
            string content = null;
            try
            {
                if (client == null) {
                    client = new HttpClient();
                }
                using (HttpResponseMessage response = await client.GetAsync(new Uri(url)))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        content = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception)
            {

            }

            return content;
        }

        public virtual async Task<IBuffer> GetBuffer(string url)
        {
            if (client == null)
            {
                client = new HttpClient();
            }

            Byte[] bytes = await client.GetByteArrayAsync(url);
            return bytes.AsBuffer();
        }

        /*
        public static async Task<string> GetRedirect(string proxy, string url, string postParams)
        {
            string redirectedUrl = null;
            try
            {
                var cookieContainer = new CookieContainer();
                var handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    CookieContainer = cookieContainer
                };

                using (HttpClient client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 Mobile Safari/537.36");
                    client.DefaultRequestHeaders.Add("Referer", "http://webproxy.to/");
                    await client.GetAsync(new Uri("http://webproxy.to/"));

                    using (HttpResponseMessage response = await client.PostAsync(proxy, new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("u", url),
                        new KeyValuePair<string, string>("encodeURL", "on"),
                        new KeyValuePair<string, string>("allowCookies", "on"),
                    })))
                    using (HttpContent content = response.Content)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Found)
                        {
                            HttpResponseHeaders headers = response.Headers;
                            if (headers != null && headers.Location != null)
                            {
                                redirectedUrl = headers.Location.AbsoluteUri;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return redirectedUrl;
        }
        */
    }
}
