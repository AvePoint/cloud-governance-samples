namespace Cloud.Governance.Samples.CustomAction
{
    #region using directives

    using System;
    using System.Text;

    #endregion using directives

    [Serializable]
    public class CustomAPIRequest
    {
        public String ServiceId { get; set; }

        public String Title { get; set; }

        public String PrimarySiteAdmin { get; set; }

        public String SecondarySiteAdmin { get; set; }

        public String Url { get; set; }

        public String ChangedToPrimaryContact { get; set; }

        public String ChangedToSecondaryContact { get; set; }

        public String Result { get; set; }

        public override String ToString()
        {
            var request = new StringBuilder();
            request.Append($"ServiceId : {this.ServiceId}");
            request.Append($"Title : {this.Title}");
            request.Append($"PrimarySiteAdmin : {this.PrimarySiteAdmin}");
            request.Append($"SecondarySiteAdmin : {this.SecondarySiteAdmin}");
            request.Append($"Url : {this.Url}");
            request.Append($"ChangedToPrimaryContact : {this.ChangedToPrimaryContact}");
            request.Append($"ChangedToSecondaryContact : {this.ChangedToSecondaryContact}");
            request.Append($"ExternalTickedId : {this.ExternalTicketId}");
            request.Append($"LineOfBusiness : {this.LineOfBusiness}");
            request.Append($"SitePurpose : {this.SitePurpose}");
            request.Append($"ApprovingVP : {this.ApprovingVP}");
            request.Append($"Result : {this.Result}");
            return request.ToString();
        }

        #region metadata

        public String ExternalTicketId { get; set; }

        public String LineOfBusiness { get; set; }

        public String SitePurpose { get; set; }

        public String ApprovingVP { get; set; }

        #endregion metadata
    }
}