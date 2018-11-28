namespace Cloud.Governance.Samples.Sdk
{
    #region using directives
    using AvePoint.GA.WebAPI;
    using AvePoint.GA.WebAPI.Models;
    using System;
    #endregion

    /// <summary>
    /// The sample for content move
    /// </summary>
    public class ContentMoveSample
    {
        private ICommonService commonService;
        private IRequestService requestService;
        
        /// <summary>
        /// Submit a request for content move
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
        /// Get request template to specified Content Move service
        /// </summary>
        /// <returns>Request template</returns>
        private APIRequestContentMove GetRequestTemplate()
        {
            //The ID of Content Move service
            var serviceId = new Guid("d61f7ef5-79cc-446e-9e17-bcaf7c5b0b91");
            var serviceInfo = this.commonService.Get(serviceId);
            return serviceInfo.APIRequest as APIRequestContentMove;
        }

        /// <summary>
        /// Set the request information
        /// </summary>
        /// <param name="template">Request template</param>
        /// <returns>Request information</returns>
        private APIRequestContentMove SetValue(APIRequestContentMove template)
        {
            var requestInfo = template;

            #region Required

            //Request Summary
            requestInfo.RequestSummary = "Content Move Sample";

            #endregion

            #region Not Required

            //Request Description
            requestInfo.Description = "";

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
        /// Save and submit Content Move request
        /// </summary>
        /// <param name="requestInfo">Request information</param>
        /// <returns>The result of submitting request</returns>
        private Boolean SaveAndSubmit(APIRequestContentMove requestInfo)
        {
            var requestId = this.requestService.Save(requestInfo);
            return this.requestService.Submit(requestId);
        }
    }
}
