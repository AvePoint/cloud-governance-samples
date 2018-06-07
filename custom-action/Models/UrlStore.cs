namespace Cloud.Governance.Samples.CustomAction
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    #endregion using directives

    public static class UrlStore
    {
        private static readonly List<String> cachedUrls = new List<String>();

        public static List<String> Get()
        {
            if (cachedUrls.Count == 0)
            {
                lock (cachedUrls)
                {
                    if (cachedUrls.Count == 0)
                    {
                        var doc = new XmlDocument();
                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DomainService",
                            "UrlStore.xml");
                        doc.Load(path);
                        var nodeList = doc.SelectNodes("//url");
                        if (nodeList != null)
                        {
                            foreach (XmlNode item in nodeList)
                            {
                                cachedUrls.Add(item.Attributes["value"].Value.ToLower());
                            }
                        }
                    }
                }
            }

            return cachedUrls;
        }
    }
}