namespace Cloud.Governance.Samples.CustomAction
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    #endregion using directives

    public class BlockURLService
    {
        public List<String> GetBlockedURLs()
        {
            var result = new List<String>();
            var doc = new XmlDocument();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DomainService", "BlockedURLs.xml");
            doc.Load(path);
            var nodeList = doc.SelectNodes("//url");
            if (nodeList != null)
            {
                foreach (XmlNode item in nodeList)
                {
                    result.Add(item.Attributes["value"].Value.ToLower());
                }
            }
            return result;
        }
    }
}