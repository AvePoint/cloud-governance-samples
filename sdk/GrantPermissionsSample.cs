namespace Cloud.Governance.Samples.Sdk
{
    #region using directives
    using AvePoint.GA.WebAPI;
    using AvePoint.GA.WebAPI.Models;
    using System;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The sample for granting permissions
    /// </summary>
    public class GrantPermissionsSample
    {
        private ICommonService commonService;
        private IRequestService requestService;

        /// <summary>
        /// Submit a request for granting permissions
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
        /// Get request template to specified Grant Permissions service
        /// </summary>
        /// <returns>Request template</returns>
        private APIRequestGRUserPermission GetRequestTemplate()
        {
            //The ID of Grant Permissions service
            var serviceId = new Guid("22f7c9bc-1c1f-44a5-8ba6-c01060d7f942");
            var serviceInfo = this.commonService.Get(serviceId);
            return serviceInfo.APIRequest as APIRequestGRUserPermission;
        }

        /// <summary>
        /// Set the request information
        /// </summary>
        /// <param name="template">Request template</param>
        /// <returns>Request information</returns>
        private APIRequestGRUserPermission SetValue(APIRequestGRUserPermission template)
        {
            var requestInfo = template;

            #region Required

            //Request Summary
            requestInfo.RequestSummary = "Grant Permissions Sample";
            //URL
            requestInfo.ScopeUrl = "https://m365x398150.sharepoint.com/sites/Apple2";
            //Users
            requestInfo.SelectUsers = new APIPeopleValidationInfo { Content = "debrab@m365x398150.onmicrosoft.com" };
            //Grant Type
            requestInfo.GrantPermissionFrom = APIGrantPermissionType.GrantDirectly;
            //Permanent or Temporary
            requestInfo.IsGrantTemporary = false;
            //Permission Level
            requestInfo.SelectPermGroup = new APIGRFarmPermGroup
            {
                Levels = new List<APIGRUserPermSPLevel> { new APIGRUserPermSPLevel { Name = "Full Control" } }
            };

            #endregion

            #region Not Required

            //Request Description
            requestInfo.Description = "Sample";

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
            var metadataName = "Sample";
            var metadata = requestInfo.MetadataList.Find(m => m.Name.Equals(metadataName));
            if (metadata != null)
            {
                //Metadata Value
                metadata.Value = "Sample";
            }
        }

        /// <summary>
        /// Save and submit Grant Permissions request
        /// </summary>
        /// <param name="requestInfo">Request information</param>
        /// <returns>The result of submitting request</returns>
        private Boolean SaveAndSubmit(APIRequestGRUserPermission requestInfo)
        {
            var requestId = this.requestService.Save(requestInfo);
            return this.requestService.Submit(requestId);
        }
    }
}
