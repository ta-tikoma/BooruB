using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace BooruB.Helpers
{
    class QueryProxy : Query
    {
        /*
        public override async Task<IBuffer> GetFileStatic(string url, Pages.MainPage mainPage)
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

        public HttpClient client = null;

        public override async Task<string> Get(string url)
        {
            System.Diagnostics.Debug.WriteLine("Get");

            string content = null;
            try
            {
                if (client == null)
                {
                    client = new HttpClient();
                }
                System.Diagnostics.Debug.WriteLine("HttpClient");

                Uri uri = new Uri(url);

                client.DefaultRequestHeaders.Host = uri.Host;
                using (HttpResponseMessage response = await client.GetAsync(new Uri("http://180.244.40.2:8080")))
                {
                System.Diagnostics.Debug.WriteLine("response");
                    if (response.IsSuccessStatusCode)
                    {
                System.Diagnostics.Debug.WriteLine("IsSuccessStatusCode");
                        content = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception:" + ex.Message);
            }

            return content;
        }
    }
}
