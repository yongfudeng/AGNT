using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace BOC.SynchronousService.Config
{
    public class MultiTagSectionHandler : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler Members

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            List<KeyValuePair<string, string>> config = new List<KeyValuePair<string, string>>();

            foreach (XmlNode node in section.ChildNodes)
            {
                if (node.Name != "Config")
                {
                    throw new Exception(string.Format("不可识别的配置节点: {0}.", node.Name));
                }

                config.Add(new KeyValuePair<string, string>(node.Attributes["JobName"].Value, node.Attributes["ConfigPath"].Value));
            }

            return config;
        }

        #endregion
    }
}