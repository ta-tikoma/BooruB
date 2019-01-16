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
