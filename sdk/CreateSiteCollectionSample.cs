namespace Cloud.Governance.Samples.Sdk
{
    #region using directives
    using AvePoint.GA.WebAPI;
    using AvePoint.GA.WebAPI.Models;
    using System;
    #endregion

    /// <summary>
    /// The sample for site collection provision
    /// </summary>
    public class CreateSiteCollectionSample
    {
        private ICommonService commonService;
        private IRequestService requestService;

        /// <summary>
        /// Submit a request to create site collection
        /// </summary>
        /// <returns>The result of submitting request</returns>
        public Boolean SubmitRequest()
        {
            this.Initialize();
            var template = this.GetRequestTemplate();
            var requestInfo = this.SetValue(template);
            return this.SaveAndSubmit(requestInfo);
        }

        /// <summary>
        /// Initialize the API client
        /// </summary>
        private void Initialize()
        {
            //Your Cloud Governance user name
            var username = "";
            //Your Cloud Governance password
            var password = "";
            GaoApi.Init(Region.EastUS, username, password);
            this.commonService = GaoApi.Create<ICommonService>();
            this.requestService = GaoApi.Create<IRequestService>();
        }

        /// <summary>
        /// Get request template to specified Create Site Collection service
        /// </summary>
        /// <returns>Request template</returns>
        private APIRequestProvSite GetRequestTemplate()
        {
            //The ID of Create Site Collection service
            var serviceId = new Guid("33f84b08-2e5e-4a53-bf6d-6f5229453465");
            var serviceInfo = this.commonService.Get(serviceId);
            return serviceInfo.APIRequest as APIRequestProvSite;
        }

        /// <summary>
        /// Set the request information
        /// </summary>
        /// <param name="template">Request template</param>
        /// <returns>Request information</returns>
        private APIRequestProvSite SetValue(APIRequestProvSite template)
        {
            var requestInfo = template;

            #region Required

            //Request Summary
            requestInfo.RequestSummary = "Create Site Collection Sample";
            //Title
            requestInfo.SiteTitle = "Sample";
            //URL
            requestInfo.Url.ManagedPath = "/sites/";
            requestInfo.Url.Url = "sample";
            //Primary Administrator
            requestInfo.PrimaryAdministrator = "alicel@tenant.onmicrosoft.com";
            //Primary Contact
            requestInfo.PrimaryContact = "alicel@tenant.onmicrosoft.com";
            //Secondary Contact
            requestInfo.SecondaryContact = "brianj@tenant.onmicrosoft.com";

            #endregion

            #region Not Required

            //Request Description
            requestInfo.Description = "Sample";
            //Site Collection Description
            requestInfo.SiteDescription = "Sample";

            this.SetMetadataValue(requestInfo);

            #endregion

            return requestInfo;
        }

        /// <summary>
        /// Set request metadata value
        /// </summary>
        /// <param name="requestInfo">Request information</param>
        private void SetMetadataValue(APIRequest requestInfo)
        {
            //Metadata Name
            var metadataName = "";
            var metadata = requestInfo.MetadataList.Find(m => m.Name.Equals(metadataName));
            if (metadata != null)
            {
                //Metadata Value
                metadata.Value = "";
            }
        }

        /// <summary>
        /// Save and submit Create Site Collection request
        /// </summary>
        /// <param name="requestInfo">Request information</param>
        /// <returns>The result of submitting request</returns>
        private Boolean SaveAndSubmit(APIRequestProvSite requestInfo)
        {
            var requestId = this.requestService.Save(requestInfo);
            return this.requestService.Submit(requestId);
        }
    }
}
