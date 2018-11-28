namespace Cloud.Governance.Samples.Sdk
{
    #region using directives
    using AvePoint.GA.WebAPI;
    using AvePoint.GA.WebAPI.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// The sample to change site collection contact or administrator
    /// </summary>
    public class ChangeSiteCollectionContactSample
    {
        private ICommonService commonService;
        private IRequestService requestService;

        /// <summary>
        /// Submit a request to change site collection contact or administrator
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
        /// Get request template to specified Change Site Collection Contact or Administrator service
        /// </summary>
        /// <returns>Request template</returns>
        private APIRequestChangeSCContactAdmin GetRequestTemplate()
        {
            //The ID of Change Site Collection Contact or Administrator service
            var serviceId = new Guid("4a580529-52ec-463f-aeb6-c9aded52e181");
            var serviceInfo = this.commonService.Get(serviceId);
            return serviceInfo.APIRequest as APIRequestChangeSCContactAdmin;
        }

        /// <summary>
        /// Set the request information
        /// </summary>
        /// <param name="template">Request template</param>
        /// <returns>Request information</returns>
        private APIRequestChangeSCContactAdmin SetValue(APIRequestChangeSCContactAdmin template)
        {
            var requestInfo = template;

            #region Required

            //Request Summary
            requestInfo.RequestSummary = "Change Site Collection Contact or Administrator Sample";

            requestInfo.Department = "Marketing";

            if (requestInfo.Settings.ChangeByUser)
            {
                #region Change Contact by User

                //Original Contact or Administrator
                requestInfo.Settings.CurrentContactOrAdmin = "alexw@tenant.onmicrosoft.com";
                //New Contact or Administrator
                requestInfo.Settings.NewContactOrAdmin = "brianj@tenant.onmicrosoft.com";
                //Alternate new primary site collection contact/secondary site collection contact
                requestInfo.Settings.AlternateNewContact = "candyd@tenant.onmicrosoft.com";

                #endregion
            }
            else
            {
                #region Change Contact by URL

                var changeContactInfo = new ChangeContactAction
                {
                    //Site Collection URL
                    SiteUrl = "https://tenant.sharepoint.com/sites/Sample"
                };
                //Primary Administrator
                changeContactInfo.PrimaryAdmin = new ContactAdminInfo
                {
                    ChangeToLoginName = "alexw@tenant.onmicrosoft.com"
                };
                //Additional Administrators
                changeContactInfo.AdditionalAdmin = new ContactAdminInfo
                {
                    ChangeToLoginName = "candyd@tenant.onmicrosoft.com"
                };
                //Primary Contact
                changeContactInfo.PrimaryContact = new ContactAdminInfo
                {
                    ChangeToLoginName = "brianj@tenant.onmicrosoft.com"
                };
                //Secondary Contact
                changeContactInfo.SecondaryContact = new ContactAdminInfo
                {
                    ChangeToLoginName = "debrab@tenant.onmicrosoft.com"
                };
                var changeContactInfoList = new List<ChangeContactAction> { changeContactInfo };
                requestInfo.Settings.ChangeContact = JsonConvert.SerializeObject(changeContactInfoList);
                #endregion
            }

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
        /// Save and submit Change Site Collection Contact or Administrator request
        /// </summary>
        /// <param name="requestInfo">Request information</param>
        /// <returns>The result of submitting request</returns>
        private Boolean SaveAndSubmit(APIRequestChangeSCContactAdmin requestInfo)
        {
            var requestId = this.requestService.Save(requestInfo);
            return this.requestService.Submit(requestId);
        }
    }
}
