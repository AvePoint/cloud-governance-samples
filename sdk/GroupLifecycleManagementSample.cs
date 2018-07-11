namespace Cloud.Governance.Samples.Sdk
{
    #region using directives
    using AvePoint.GA.WebAPI;
    using AvePoint.GA.WebAPI.Models;
    using System;
    #endregion

    /// <summary>
    /// The sample for group lifecycle management
    /// </summary>
    public class GroupLifecycleManagementSample
    {
        private ICommonService commonService;
        private IRequestService requestService;

        /// <summary>
        /// Submit a request for group lifecycle management
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
        /// Get request template to specified Group Lifecycle Management service
        /// </summary>
        /// <returns>Request template</returns>
        private APIRequestGroupLifecycle GetRequestTemplate()
        {
            //The ID of Group Lifecycle Management service
            var serviceId = new Guid("");
            var serviceInfo = this.commonService.Get(serviceId);
            return serviceInfo.APIRequest as APIRequestGroupLifecycle;
        }

        /// <summary>
        /// Set the request information
        /// </summary>
        /// <param name="template">Request template</param>
        /// <returns>Request information</returns>
        private APIRequestGroupLifecycle SetValue(APIRequestGroupLifecycle template)
        {
            var requestInfo = template;

            #region Required

            //Request Summary
            requestInfo.RequestSummary = "Group Lifecycle Management Sample";

            if (requestInfo.LifecycleType == APIServiceType.DeleteGroup)
            {
                #region Delete

                var deleteGroupRequest = requestInfo as APIRequestDeleteGroup;
                //Name
                deleteGroupRequest.GroupName = "Sample";
                //Email
                deleteGroupRequest.GroupEmail = "";

                #endregion
            }
            else if (requestInfo.LifecycleType == APIServiceType.ExtendGroup)
            {
                #region Extend

                var extendGroupRequest = requestInfo as APIRequestExtendGroup;
                //Name
                extendGroupRequest.GroupName = "Sample";
                //Email
                extendGroupRequest.GroupEmail = "";
                //Extend Duration Type
                extendGroupRequest.ExtendDurationType = (Int32)ExtendDurationType.Day;
                //Extend Duration
                extendGroupRequest.ExtendDuration = 5;

                #endregion
            }
            else if (requestInfo.LifecycleType == APIServiceType.ChangeGroupPolicy)
            {
                #region Change Policy

                var changePolicyRequest = requestInfo as APIRequestChangeGroupPolicy;
                //Name
                changePolicyRequest.GroupName = "Sample";
                //Email
                changePolicyRequest.GroupEmail = "";
                //Policy Name
                changePolicyRequest.PolicyName = "";

                #endregion
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
        /// Save and submit Group Lifecycle Management request
        /// </summary>
        /// <param name="requestInfo">Request information</param>
        /// <returns>The result of submitting request</returns>
        private Boolean SaveAndSubmit(APIRequestGroupLifecycle requestInfo)
        {
            var requestId = this.requestService.Save(requestInfo);
            return this.requestService.Submit(requestId);
        }
    }
}
