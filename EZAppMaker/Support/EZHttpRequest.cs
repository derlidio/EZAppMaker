/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Text;
using System.Net.Http.Headers;
using System.Net.Security;

using Newtonsoft.Json;

using EZAppMaker.Attributes;
using EZAppMaker.Components;

namespace EZAppMaker.Support
{
    public class HttpRequest
    {
        public string Url { get; set; } = "";
        public string Body { get; set; } = "";
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public bool UseCache { get; set; } = false;
        public bool IgnoreCertificateErrors { get; set; } = false;
        public bool LogExceptions { get; set; } = false;
        public int TimeoutInSeconds { get; set; } = 35;
    }

    public class ExceptionLog
    {
        public DateTime DateTime { get; set; }

        public HttpRequest request { get; set; }
        public string View { get; set; }
        public string Exception { get; set; }
    }

    public static class EZHttpRequest
    {
        public static async Task<HttpResponseMessage> Get(HttpRequest request)
        {
            HttpResponseMessage response = await Service("get", request);

            return response;
        }

        public static async Task<HttpResponseMessage> Post(HttpRequest request)
        {
            HttpResponseMessage response = await Service("post", request);

            return response;
        }

        private static async Task<HttpResponseMessage> Service(string method, HttpRequest request)
        {
            HttpResponseMessage response = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.RequestTimeout
            };

            if (string.IsNullOrWhiteSpace(request.Url)) return response;

            try
            {
                StringContent content;

                HttpClient client = BuildHttpClient(request);
                string url = BuildRequestUrl(request);

                switch (method)
                {
                    case "post":

                        content = new StringContent(request.Body, Encoding.UTF8, "application/json");
                        response = await client.PostAsync(url, content);
                        break;

                    case "get":

                        response = await client.GetAsync(url);
                        break;
                }
            }
            catch (Exception exception)
            {
                if (request.LogExceptions)
                {
                    LogHttpClientException(exception, request);
                }
            }

            return response;
        }

        public static async Task<string> GetContent(HttpResponseMessage response)
        {
            string content = "";

            if ((response != null) && (response.Content != null))
            {
                content = await response.Content.ReadAsStringAsync();
            }

            return content;
        }

        private static HttpClient BuildHttpClient(HttpRequest request)
        {
            HttpClient client = new HttpClient
            (
                new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                    {
                        return ShouldAcceptCertificate(sslPolicyErrors, request);
                    }
                }
            )
            {
                Timeout = TimeSpan.FromSeconds(request.TimeoutInSeconds)
            };

            if (!request.UseCache)
            {
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            }

            if (request.Headers != null)
            {
                foreach (KeyValuePair<string, string> pair in request.Headers)
                {
                    client.DefaultRequestHeaders.Add(pair.Key, pair.Value);
                }
            }

            return client;
        }

        private static string BuildRequestUrl(HttpRequest request)
        {
            string url = request.Url.Trim();

            if (request.Parameters == null) return url;

            bool first = url.IndexOf("?") == -1;

            foreach (KeyValuePair<string, string> pair in request.Parameters)
            {
                if (first)
                {
                    url += $"?{pair.Key}={pair.Value}";
                    first = false;
                    continue;
                }

                url += $"&{pair.Key}={pair.Value}";
            }

            return url;
        }

        private static bool ShouldAcceptCertificate(SslPolicyErrors Errors, HttpRequest request)
        {
            if (!request.IgnoreCertificateErrors)
            {
                return Errors == SslPolicyErrors.None;
            }

            return true;
        }

        [AsyncVoidOnPurpose]
        private static async void LogHttpClientException(Exception exception, HttpRequest request)
        {
            ExceptionLog report = new ExceptionLog() { DateTime = DateTime.Now };

            EZContentView view = await EZApp.Container.GetTopPage();

            report.View = (view == null) ? "Undefined" : view.ItemId;
            report.request = request;
            report.Exception = exception.ToString();

            string json = JsonConvert.SerializeObject(report);

            EZLocalAppData.SaveFile("http_exception.json", json);
        }
    }
}