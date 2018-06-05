namespace Cloud.Governance.Samples.CustomAction
{
    #region using directives

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net;
    using System.Web.Services;
    using AvePoint.GA.WebAPI;
    using AvePoint.GA.WebAPI.Models;

    #endregion using directives

    /// <summary>
    /// A demo web service named SiteCollectionWebService to describe a customized service which can
    /// be invoked by the Cloud Governance service cluster. As a matter of fact, the cloud governance
    /// service cluster only consider the exception result as a Custom Action has a invalid result,
    /// that means, if you want to let the cloud governance knows the invalid result, you should use
    /// a exception message with non 2-serials http status code instead of return a status by the web method.
    /// </summary>
    [WebService(Namespace = "http://www.avepoint.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
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
                    $"Start checking if site collection.requestId:{requestId}, " +
                    "securityToken:{securityToken}");

                //Cloud Governance Client Sdk need a Region url and security token
                //to callback the Cloud Governance web api.
                GaoApi.Init(Region.EastUS, securityToken);

                var requestService = GaoApi.Create<IRequestService>();

                //In Cloud Governance Client Sdk, everty request id link to one request type,
                //In this case, the request is a ProvSite request.
                var request = requestService.Get(new Guid(requestId)) as APIRequestProvSite;

                var url = (request?.Url.ManagedPath + request?.Url.Url).TrimStart('/');

                //A url repository which hold the block urls

                var blockedUrLs = UrlStore.Get();

                if (blockedUrLs.Contains(url.ToLower()))
                {
                    Trace.TraceInformation("The request url {0} is blocked", url);
                    throw new WebException("The request site collection url is not valid");
                }
                Trace.TraceInformation("The request url {0} is valid", url);
            }
            catch (Exception ex)
            {
                Trace.TraceError("An error occurred while validating the " +
                                 $"site collection url, reason {ex}");
                throw;
            }
        }
    }
}