namespace Cloud.Governance.Samples.CustomAction
{
    #region using directives

    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    #endregion using directives

    public class CustomHttpHelper
    {
        public static String ScriptPath =
            String.Format("{0}/Scripts", ConfigurationManager.AppSettings["GAOWebServiceAddress"]);

        public static String WebApiAddress = ConfigurationManager.AppSettings["GAOAPIAddress"];

        public static String SendRequest(String method, String url, String content, String token)
        {
            Trace.TraceInformation("api request url: {0}", url);
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.ContentType = "application/json";
            var reply = String.Empty;
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;
            if (!String.IsNullOrEmpty(token))
                httpRequest.Headers.Set("X_GovernanceAutomation_Access_Token", token);
            method = method.ToUpper();
            httpRequest.Method = method;
            httpRequest.Timeout = 60 * 60 * 1000; //1 hour
            httpRequest.AllowWriteStreamBuffering = false;
            if (!"GET".Equals(method))
            {
                var encoding = new UTF8Encoding();
                var data = encoding.GetBytes(content);
                httpRequest.ContentLength = data.Length;
                using (var requestStream = httpRequest.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                }
            }
            try
            {
                var response = (HttpWebResponse)httpRequest.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    using (var responseReader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        reply = responseReader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                try
                {
                    using (var responseStream = ex.Response.GetResponseStream())
                    {
                        using (var responseReader = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            var webErrorMsg = responseReader.ReadToEnd();
                            Trace.TraceError("An error occurred while invoking GAO API " + webErrorMsg);
                        }
                    }
                }
                catch
                {
                }
                throw;
            }
            return reply;
        }

        public static UrlInfo GetSiteCollectionUrlInfo(String securityToken, String requestId)
        {
            var replay = SendRequest(
                "GET",
                String.Format("{0}/api/requests/get/{1}", WebApiAddress, requestId),
                String.Empty,
                securityToken);
            var request = JsonConvert.DeserializeObject(replay) as JObject;
            return JsonConvert.DeserializeObject<UrlInfo>(request.GetValue("Url").ToString());
        }
    }
}