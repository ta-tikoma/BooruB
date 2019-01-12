using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Streams;

namespace BooruB.Helpers
{
    public class Query
    {
        /*
        public virtual async Task<IBuffer> GetFileStatic(string url, Pages.MainPage mainPage)
        {
            try
            {
                Progress<Windows.Web.Http.HttpProgress> progressCallback = new Progress<Windows.Web.Http.HttpProgress>((Windows.Web.Http.HttpProgress obj) => {
                    if (obj.TotalBytesToReceive == null)
                    {
                        return;
                    }
                    mainPage.SetProgressValue((int)(obj.BytesReceived / (obj.TotalBytesToReceive / 100)));
                });

                Windows.Web.Http.Filters.HttpBaseProtocolFilter RootFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
                RootFilter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.MostRecent;
                RootFilter.CacheControl.WriteBehavior = Windows.Web.Http.Filters.HttpCacheWriteBehavior.NoCache;
                Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient(RootFilter);
                Windows.Web.Http.HttpResponseMessage response = await client.GetAsync(new Uri(url)).AsTask(mainPage.cts.Token, progressCallback);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsBufferAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("IsSuccessStatusCode:" + response.StatusCode);
                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("OperationCanceledException:");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ex:" + ex.Message);
                return null;
            }
        }
        */

        public virtual async Task<string> Get(string url)
        {
            string content = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(new Uri(url)))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            content = await response.Content.ReadAsStringAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return content;
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
