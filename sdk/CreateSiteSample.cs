namespace Cloud.Governance.Samples.Sdk
{
    #region using directives
    using AvePoint.GA.WebAPI;
    using AvePoint.GA.WebAPI.Models;
    using System;
    #endregion

    /// <summary>
    /// The sample for site provision
    /// </summary>
    public class CreateSiteSample
    {
        private ICommonService commonService;
        private IRequestService requestService;

        /// <summary>
        /// Submit a request to create site
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
        /// Get request template to specified Create Site service
        /// </summary>
        /// <returns>Request template</returns>
        private APIRequestProvWeb GetRequestTemplate()
        {
            //The ID of Create Site service
            var serviceId = new Guid("");
            var serviceInfo = this.commonService.Get(serviceId);
            return serviceInfo.APIRequest as APIRequestProvWeb;
        }

        /// <summary>
        /// Set the request information
        /// </summary>
        /// <param name="template">Request template</param>
        /// <returns>Request information</returns>
        private APIRequestProvWeb SetValue(APIRequestProvWeb template)
        {
            var requestInfo = template;

            #region Required

            //Request Summary
            requestInfo.RequestSummary = "Create Site Sample";
            //Parent Site Collection or Site URL
            requestInfo.ParentSiteUrl = "";
            //URL
            requestInfo.Url = "Sample";
            //Title
            requestInfo.WebTitle = "Sample";
            //Primary Contact
            requestInfo.PrimaryContact = "";
            //Secondary Contact
            requestInfo.SecondaryContact = "";

            #endregion

            #region Not Required

            //Request Description
            requestInfo.Description = "Sample";
            //Site Description
            requestInfo.WebDescription = "Sample";

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
        /// Save and submit Create Site request
        /// </summary>
        /// <param name="requestInfo">Request information</param>
        /// <returns>The result of submitting request</returns>
        private Boolean SaveAndSubmit(APIRequestProvWeb requestInfo)
        {
            var requestId = this.requestService.Save(requestInfo);
            return this.requestService.Submit(requestId);
        }
    }
}
