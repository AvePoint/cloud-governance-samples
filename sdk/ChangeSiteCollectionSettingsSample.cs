namespace Cloud.Governance.Samples.Sdk
{
    #region using directives
    using AvePoint.GA.WebAPI;
    using AvePoint.GA.WebAPI.Models;
    using System;
    #endregion

    /// <summary>
    /// The sample for changing site collection settings
    /// </summary>
    public class ChangeSiteCollectionSettingsSample
    {
        private ICommonService commonService;
        private IRequestService requestService;

        /// <summary>
        /// Submit a request to change site collection settings
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
        /// Get request template to specified Change Site Collection Settings service
        /// </summary>
        /// <returns>Request template</returns>
        private APIRequestChangeSCMetadata GetRequestTemplate()
        {
            //The ID of Change Site Collection Settings service
            var serviceId = new Guid("5a27fa51-1b55-49ca-b1aa-6b07b44b113e");
            var serviceInfo = this.commonService.Get(serviceId);
            return serviceInfo.APIRequest as APIRequestChangeSCMetadata;
        }

        /// <summary>
        /// Set the request information
        /// </summary>
        /// <param name="template">Request template</param>
        /// <returns>Request information</returns>
        private APIRequestChangeSCMetadata SetValue(APIRequestChangeSCMetadata template)
        {
            var requestInfo = template;

            #region Required

            //Request Summary
            requestInfo.RequestSummary = "Change Site Collection Settings Sample";
            //Department
            requestInfo.Department = "Sample";
            //Site Collection URL
            requestInfo.SiteCollectionUrl = "https://m365x752739.sharepoint.com/sites/sample";

            #region Change Title

            requestInfo.IsChangeSCTitle = true;
            //Modified Title
            requestInfo.ChangeSCTitle = "Change";

            #endregion

            #region Change Description

            requestInfo.IsChangeSCDescription = true;
            //Modified Description
            requestInfo.ChangeSCDescription = "Change";

            #endregion

            #region Change Metadata

            requestInfo.IsChangeSCMetadata = true;
            if (requestInfo.Settings != null)
            {
                //Modified Metadata Name
                var metadataName = "Sample";
                var metadata = requestInfo.Settings.Find(m => m.Name.Equals(metadataName));
                if (metadata != null)
                {
                    //Modified Metadata Value
                    metadata.Value = "Sample";
                }
            }

            #endregion

            #endregion

            #region Not Required

            requestInfo.Description = "Sample";

            #endregion

            this.SetMetadataValue(requestInfo);

            return requestInfo;
        }

        /// <summary>
        /// Set request metadata value
        /// </summary>
        /// <param name="requestInfo">Request information</param>
        private void SetMetadataValue(APIRequest requestInfo)
        {
            //Metadata Name
            var metadataName = "Sample";
            var metadata = requestInfo.MetadataList.Find(m => m.Name.Equals(metadataName));
            if (metadata != null)
            {
                //Metadata Value
                metadata.Value = "Sample";
            }
        }

        /// <summary>
        /// Save and submit Change Site Collection Settings request
        /// </summary>
        /// <param name="requestInfo">Request information</param>
        /// <returns>The result of submitting request</returns>
        private Boolean SaveAndSubmit(APIRequestChangeSCMetadata requestInfo)
        {
            var requestId = this.requestService.Save(requestInfo);
            return this.requestService.Submit(requestId);
        }
    }
}
