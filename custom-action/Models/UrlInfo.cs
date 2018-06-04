namespace Cloud.Governance.Samples.CustomAction
{
    #region using directives

    using System;

    #endregion using directives

    public class UrlInfo
    {
        public String DomainName { get; set; }
        public String Prefix { get; set; }
        public String Url { get; set; }
        public String ManagedPath { get; set; }
        public Boolean IsMySite { get; set; }
        public String MysiteUrl { get; set; }
        public String Property { get; set; } // My site Department

        public override String ToString()
        {
            if (this.IsMySite)
                return this.MysiteUrl;
            if (this.ManagedPath.Equals("/"))
                return this.Prefix.TrimEnd('/');
            return this.Prefix.TrimEnd('/') + this.ManagedPath + this.Url;
        }
    }
}