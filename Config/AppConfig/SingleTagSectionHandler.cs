using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace BOC.SynchronousService.Config
{
    public class SingleTagSectionHandler : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler Members

        public object Create(object parent, object configContext, XmlNode section)
        {
            Dictionary<string, string> config = new Dictionary<string, string>();

            foreach (XmlAttribute attr in section.Attributes)
            {
                if (config.ContainsKey(attr.Name))
                {
                    throw new Exception(string.Format("配置节点中存在重复项: {0}.", attr.Name));
                }

                config.Add(attr.Name, attr.Value);
            }

            return config;
        }

        #endregion
    }
}