namespace Cloud.Governance.Samples.Sdk
{
    #region using directives
    using AvePoint.GA.WebAPI;
    using AvePoint.GA.WebAPI.Models;
    using System;
    #endregion

    /// <summary>
    /// The sample for list/library provision
    /// </summary>
    public class CreateListSample
    {
        private ICommonService commonService;
        private IRequestService requestService;

        /// <summary>
        /// Submit a request to create list or library
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
        /// Get request template to specified Create List/Library service
        /// </summary>
        /// <returns>Request template</returns>
        private APIRequestCreateList GetRequestTemplate()
        {
            //The ID of Create List/Library service
            var serviceId = new Guid("");
            var serviceInfo = this.commonService.Get(serviceId);
            return serviceInfo.APIRequest as APIRequestCreateList;
        }

        /// <summary>
        /// Set the request information
        /// </summary>
        /// <param name="template">Request template</param>
        /// <returns>Request information</returns>
        private APIRequestCreateList SetValue(APIRequestCreateList template)
        {
            var requestInfo = template;

            #region Required

            //Request Summary
            requestInfo.RequestSummary = "Create List or Library Sample";
            //Parent Site URL
            requestInfo.SiteUrl = "";
            //List or Library
            requestInfo.IsLibrary = true;
            //URL
            requestInfo.ListUrl = "Sample";
            //Title
            requestInfo.ListTitle = "Sample";
            
            #endregion

            #region Not Required

            //Request Description
            requestInfo.Description = "Sample";
            //List Description
            requestInfo.ListDescription = "Sample";

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
        /// Save and submit Create List/Library request
        /// </summary>
        /// <param name="requestInfo">Request information</param>
        /// <returns>The result of submitting request</returns>
        private Boolean SaveAndSubmit(APIRequestCreateList requestInfo)
        {
            var requestId = this.requestService.Save(requestInfo);
            return this.requestService.Submit(requestId);
        }
    }
}
