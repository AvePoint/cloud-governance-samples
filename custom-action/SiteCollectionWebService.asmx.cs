namespace Cloud.Governance.Samples.CustomAction
{
    #region using directives

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.Services;

    #endregion using directives

    /// <summary>
    /// Summary description for SiteCollectionWebService
    /// </summary>
    [WebService(Namespace = "http://www.avepoint.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the
    // following line. [System.Web.Script.Services.ScriptService]
    public class SiteCollectionWebService : WebService
    {
        [WebMethod]
        public void IsUrlValid(
            String requestId,
            String securityToken)
        {
            try
            {
                Trace.TraceInformation(
                    "Start to execute checking if site collection url is valid.requestId:{0}, securityToken:{1}",
                    requestId, securityToken);
                var info = CustomHttpHelper.GetSiteCollectionUrlInfo(securityToken, requestId);
                var requestURL = (info.ManagedPath + info.Url).TrimStart('/');
                Trace.TraceInformation("request url:{0}", requestURL);
                var service = new BlockURLService();
                var blockedURLs = service.GetBlockedURLs();
                if (blockedURLs.Contains(requestURL.ToLower()))
                {
                    Trace.TraceInformation("The request url {0} is blocked", requestURL);
                    throw new Exception("The request site collection URL is not valid");
                }
                Trace.TraceInformation("The request url {0} is valid", requestURL);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Some error happened while checking if site collection url is valid.{0}", ex);
                throw;
            }
        }
    }
}