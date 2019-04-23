using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Windows.Web.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading;

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

        public virtual async Task<string> Comment(string id, string text)
        {
            string content = null;

            try
            {
                if (client == null)
                {
                    client = new HttpClient();
                }

                string url = App.Settings.current_site + "index.php?page=comment&id=" + id + "&s=save";
                HttpMultipartFormDataContent httpContents = new HttpMultipartFormDataContent();
                httpContents.Add(new HttpStringContent(text), "comment");
                httpContents.Add(new HttpStringContent("Post comment"), "submit");
                httpContents.Add(new HttpStringContent("1"), "conf");

                using (HttpResponseMessage response = await client.PostAsync(new Uri(url), httpContents))
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

        public virtual async Task DownloadFile(string url, StorageFile file, EventHandler<int> handler = null)
        {
            if (client == null)
            {
                client = new HttpClient();
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

            Progress<HttpProgress> progressCallback = new Progress<HttpProgress>((obj) =>
            {
                if (handler != null)
                {
                    if (obj.TotalBytesToReceive == null)
                    {
                        handler(null, 0);
                    }
                    else
                    {
                       handler(null, (int)((100 / (double)obj.TotalBytesToReceive) * (double)obj.BytesReceived));
                    }
                }
            });
            var tokenSource = new CancellationTokenSource();
            HttpResponseMessage response = await client.SendRequestAsync(request).AsTask(tokenSource.Token, progressCallback);

            IInputStream inputStream = await response.Content.ReadAsInputStreamAsync();

            IOutputStream outputStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            await RandomAccessStream.CopyAndCloseAsync(inputStream, outputStream);
        }
    }
}
