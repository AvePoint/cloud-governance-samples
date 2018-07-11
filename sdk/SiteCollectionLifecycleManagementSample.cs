namespace Cloud.Governance.Samples.Sdk
{
    #region using directives
    using AvePoint.GA.WebAPI;
    using AvePoint.GA.WebAPI.Models;
    using System;
    #endregion

    /// <summary>
    /// The sample for site collection lifecycle management
    /// </summary>
    public class SiteCollectionLifecycleManagementSample
    {
        private ICommonService commonService;
        private IRequestService requestService;

        /// <summary>
        /// Submit a request for site collection lifecycle management
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
        /// Get request template to specified Site Collection Lifecycle Management service
        /// </summary>
        /// <returns>Request template</returns>
        private APIRequestSiteColLifeMgmt GetRequestTemplate()
        {
            //The ID of Site Collection Lifecycle Management service
            var serviceId = new Guid("");
            var serviceInfo = this.commonService.Get(serviceId);
            return serviceInfo.APIRequest as APIRequestSiteColLifeMgmt;
        }

        /// <summary>
        /// Set the request information
        /// </summary>
        /// <param name="template">Request template</param>
        /// <returns>Request information</returns>
        private APIRequestSiteColLifeMgmt SetValue(APIRequestSiteColLifeMgmt template)
        {
            var requestInfo = template;

            #region Required

            //Request Summary
            requestInfo.RequestSummary = "Site Collection Lifecycle Management Sample";
            //URL
            requestInfo.SiteCollectionUrl = "";

            if (requestInfo.LifecycleMgmtAction == APILifecycleManagementAction.ChangeSCQuota)
            {
                //Quota Value
                requestInfo.ChangeSCQuotaToValue = 1;
            }
            else if (requestInfo.LifecycleMgmtAction == APILifecycleManagementAction.ChangePolicy)
            {
                if (requestInfo.ChangePolicyAction != null)
                {
                    //Target Policy ID
                    requestInfo.ChangePolicyAction.TargetPolicy = "";
                }
            }

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
        /// Save and submit Site Collection Lifecycle Management request
        /// </summary>
        /// <param name="requestInfo">Request information</param>
        /// <returns>The result of submitting request</returns>
        private Boolean SaveAndSubmit(APIRequestSiteColLifeMgmt requestInfo)
        {
            var requestId = this.requestService.Save(requestInfo);
            return this.requestService.Submit(requestId);
        }
    }
}
