using System.Collections.Generic;
using System.Configuration;
using BOC.SynchronousService.Framework.Config;
using System.Xml.Linq;
using System;
using System.Diagnostics;
using System.IO;
using BOC.SynchronousService.Framework.Common;
using System.Linq;
using System.Windows.Forms;
using BOC.SynchronousService.Framework;
using BOC.SynchronousService.Framework.Assembler;

namespace BOC.SynchronousService.Config
{
    /// <summary>
    /// 配置文件解析类
    /// </summary>
    public class AppConfig : ConfigBase
    {
        #region Fields

        public static readonly string CONFIG_NODE_NODESETTING = "NodeSettings";

        public static readonly string CONFIG_ENGINETYPE = "EngineType";
        public static readonly string CONFIG_ADAPTERTYPE = "AdapterType";

        public static readonly string CONFIG_UNIT_UNITSETTING = "UnitSettings";
        public static readonly string CONFIG_UNTI_UNIT = "Unit";
        public static readonly string CONFIG_UNIT_FILETYPE = "FileType";
        public static readonly string CONFIG_UNIT_UNITTYPE = "UnitType";

        public static readonly string CONFIG_SCHEDULE_SETTING = "ScheduleSettings";
        public static readonly string CONFIG_SCHEDULE_TRIGGERTYPE = "TriggerType";
        public static readonly string CONFIG_SCHEDULE_CONFIGDETAIL = "ConfigDetail";


        public static readonly string CONFIG_ADAPTER_ADAPTERSETTING = "AdapterSettings";
        public static readonly string CONFIG_ADAPTER_REMOTEADDRESS = "RemoteAddress";
        public static readonly string CONFIG_ADAPTER_REMOTEPORT = "RemotePort";
        public static readonly string CONFIG_ADAPTER_USERNAME = "Username";
        public static readonly string CONFIG_ADAPTER_PASSWORD = "Password";

        public static readonly string CONFIG_FOLDER_FOLDERSETTINGS = "FolderSettings";
        public static readonly string CONFIG_FOLDER_REMOTEDOWNLOADDIRECTORY = "RemoteDownloadDirectory";
        public static readonly string CONFIG_FOLDER_LOCALDOWNLOADDIRECTORY = "LocalDownloadDirectory";
        public static readonly string CONFIG_FOLDER_REMOTEUPLOADDIRECTORY = "RemoteUploadDirectory";
        public static readonly string CONFIG_FOLDER_LOCALUPLOADDIRECTORY = "LocalUploadDirectory";
        public static readonly string CONFIG_FOLDER_TEMPFILESDIRECTORY = "LocalTempFilesDirectory";

        public static readonly string CONFIG_DB_DATASETTING = "DataSettings";
        public static readonly string CONFIG_DB_PATH = "Path";
        public static readonly string CONFIG_DB_NODEID = "NodeID";

        public static readonly string CONFIG_ASSEMBLE_ASSEMBLERSETTING = "AssemblerSettings";
        public static readonly string CONFIG_ASSEMBLE_ENCODING = "Encoding";
        public static readonly string CONFIG_ASSEMBLE_SCHEMADIRECTORY = "SchemaDirectory";


        private static readonly string CONFIG_CONFIGTYPE = "ConfigType";
        private static readonly string CONFIG_CONFIGDIR = "ConfigDir";
        private static readonly string CONFIG_CONFIGSETTING = "ConfigSettings";

        public static readonly string CONFIG_FILE_SETTING = "FileSettings";
        public static readonly string CONFIG_FILE_FILE = "File";
        public static readonly string CONFIG_FILE_NAME = "Name";
        public static readonly string CONFIG_FILE_SCHEMANAME = "SchemaName";
        public static readonly string CONFIG_FILE_SCHEMADIRECTORY = "SchemaDirectory";
        public static readonly string CONFIG_FILE_SYSTEMCODE = "SystemCode";


        public static readonly string CONFIG_ASSEMBLY_SETTING = "AssemblySettings";
        public static readonly string CONFIG_ASSEMBLY_ASSEMBLY = "Assembly";
        public static readonly string CONFIG_ASSEMBLY_NAME = "Name";
        public static readonly string CONFIG_ASSEMBLY_TYPE = "Type";
        public static readonly string CONFIG_ASSEMBLY_ENCODING = "Encoding";

        public static readonly string CONFIG_COMPONENT_SETTING = "ComponentSettings";
        public static readonly string CONFIG_COMPONENT_COMPONENT = "Component";
        public static readonly string CONFIG_COMPONENT_FILENAME = "FileName";
        public static readonly string CONFIG_COMPONENT_ASSEMBLYNAME = "AssemblyName";

        public static Dictionary<string, ConfigBase> ConfigList { get; set; }


        #endregion


        #region IConfiguration Members

        #region Methods

        static AppConfig()
        {
            ConfigList = new Dictionary<string, ConfigBase>();
            string configDir = Path.Combine(Application.StartupPath, ConfigurationManager.AppSettings[CONFIG_CONFIGDIR]);
            List<KeyValuePair<string, string>> configSettings = (List<KeyValuePair<string, string>>)ConfigurationManager.GetSection(CONFIG_CONFIGSETTING);
            for (int i = 0; i < configSettings.Count; i++)
            {
                KeyValuePair<string, string> pair = configSettings[i];
                string serviceType = pair.Key;
                string configFile = pair.Value;
                ConfigBase config = ObjectFactory.CreateObject<ConfigBase>(ConfigurationManager.AppSettings[CONFIG_CONFIGTYPE]);
                config.ServerType = serviceType;
                config.Load(Path.Combine(configDir, configFile));
                InitMessageComponents(config);
                ConfigList.Add(serviceType, config);
            }
        }
        /// <summary>
        /// for Message,Components
        /// </summary>
        /// <param name="config"></param>
        private static void InitMessageComponents(ConfigBase config)
        {
            foreach (var f in config.FileSettings)
            {
                if (!_Messages.Any(m => m.ID.Equals(f.Key)))
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        Path.Combine(f.Value.Item2, f.Value.Item1));
                    XElement element = XElement.Load(path);

                    var message = new Framework.Message();
                    message.ID = f.Key;

                    foreach (var e in element.Element("HEAD").Elements())
                    {
                        string id = e.Attribute("ID").Value;
                        string format = e.Attribute("Format").Value;
                        var filedSchema = new FieldSchema(id, format)
                        {
                            Value = e.Value
                        };
                        message.Head.Add(filedSchema);
                    }
                    foreach (var e in element.Element("BODY").Elements())
                    {
                        string id = e.Attribute("ID").Value;
                        string format = e.Attribute("Format").Value;

                        var filedSchema = new FieldSchema(id, format)
                        {
                            Value = e.Value
                        };
                        message.Body.Add(filedSchema);

                    }
                    _Messages.Add(message);
                }
            }

            foreach (var a in config.AssemblySettings)
            {
                if (!_AssemblyBases.Any(ass => ass.ID.Equals(a.Key)))
                {
                    var obj = ObjectFactory.
                        CreateObject<AssemblerBase>(
                                            a.Value.Item1,
                                            new Dictionary<string, string> { { "Encoding", a.Value.Item2 }, { "ID", a.Key } }
                                                   );
                    _AssemblyBases.Add(obj);
                }
            }

            foreach (var c in config.ComponentSettings)
            {
                if (!_Messages.Any(m => m.ID.Equals(c.Key)))
                {
                    throw new Exception($"Schema集合中并不包含此类型.{c.Key}");
                }
                if (!_AssemblyBases.Any(a => a.ID.Equals(c.Value)))
                {
                    throw new Exception($"处理集合中并不包含此类型.{c.Value}");
                }
                _Components.Add(_Messages.First(m => m.ID.Equals(c.Key)), _AssemblyBases.First(a => a.ID.Equals(c.Value)));
            }
        }
        public override void Load(string path)
        {
            XElement rootElement = XElement.Load(path);


            #region 解析组件类型

            _EngineType = rootElement.Element(CONFIG_ENGINETYPE).Value;
            _AdapterType = rootElement.Element(CONFIG_ADAPTERTYPE).Value;

            #endregion

            #region 解析服务运行时间

            XElement scheduleSettingsElement = rootElement.Element(CONFIG_SCHEDULE_SETTING);
            _ScheduleTriggerType = (ScheduleType)Enum.Parse(typeof(ScheduleType), scheduleSettingsElement.Element(CONFIG_SCHEDULE_TRIGGERTYPE).Value);
            _ScheduleConfigDetail = scheduleSettingsElement.Element(CONFIG_SCHEDULE_CONFIGDETAIL).Value;

            #endregion

            #region 解析adapter

            XElement adapterSettingsElement = rootElement.Element(CONFIG_ADAPTER_ADAPTERSETTING);
            _AdapterSettings[CONFIG_ADAPTER_REMOTEADDRESS] = adapterSettingsElement.Element(CONFIG_ADAPTER_REMOTEADDRESS).Value;
            _AdapterSettings[CONFIG_ADAPTER_REMOTEPORT] = adapterSettingsElement.Element(CONFIG_ADAPTER_REMOTEPORT).Value;
            _AdapterSettings[CONFIG_ADAPTER_USERNAME] = adapterSettingsElement.Element(CONFIG_ADAPTER_USERNAME).Value;
            _AdapterSettings[CONFIG_ADAPTER_PASSWORD] = adapterSettingsElement.Element(CONFIG_ADAPTER_PASSWORD).Value;

            #endregion

            #region 解析ftp文件目录

            XElement folderSettingsElement = rootElement.Element(CONFIG_FOLDER_FOLDERSETTINGS);
            _RemoteDownloadDirectory = folderSettingsElement.Element(CONFIG_FOLDER_REMOTEDOWNLOADDIRECTORY).Value;
            _LocalDownloadDirectory = folderSettingsElement.Element(CONFIG_FOLDER_LOCALDOWNLOADDIRECTORY).Value;
            _RemoteUploadDirectory = folderSettingsElement.Element(CONFIG_FOLDER_REMOTEUPLOADDIRECTORY).Value;
            _LocalUploadDirectory = folderSettingsElement.Element(CONFIG_FOLDER_LOCALUPLOADDIRECTORY).Value;
            _TempFilesDirectory = folderSettingsElement.Element(CONFIG_FOLDER_TEMPFILESDIRECTORY).Value;

            if (!Path.IsPathRooted(_LocalDownloadDirectory))
                _LocalDownloadDirectory = Path.Combine(Application.StartupPath, _LocalDownloadDirectory);

            if (!Path.IsPathRooted(_LocalUploadDirectory))
                _LocalUploadDirectory = Path.Combine(Application.StartupPath, _LocalUploadDirectory);

            if (!Path.IsPathRooted(_TempFilesDirectory))
                _TempFilesDirectory = Path.Combine(Application.StartupPath, _TempFilesDirectory);

            if (!Directory.Exists(_TempFilesDirectory))
                Directory.CreateDirectory(_TempFilesDirectory);

            if (!Directory.Exists(Path.Combine(_TempFilesDirectory, "Success")))
                Directory.CreateDirectory(Path.Combine(_TempFilesDirectory, "Success"));

            if (!Directory.Exists(_LocalUploadDirectory))
                Directory.CreateDirectory(_LocalUploadDirectory);


            #endregion

            #region 解析DBServer连接字符串

            XElement DataSettings = rootElement.Element(CONFIG_DB_DATASETTING);
            foreach (XElement pathElement in DataSettings.Elements(CONFIG_DB_PATH))
            {
                XAttribute nodeIDAttr = pathElement.Attribute(CONFIG_DB_NODEID);

                _DataPathSettings.Add(nodeIDAttr.Value, pathElement.Value);
            }

            #endregion

            #region 解析处理单元组件

            XElement unitSettingsElement = rootElement.Element(CONFIG_UNIT_UNITSETTING);
            foreach (XElement unitElement in unitSettingsElement.Elements(CONFIG_UNTI_UNIT))
            {
                string fileType = unitElement.Element(CONFIG_UNIT_FILETYPE).Value;
                string unitType = unitElement.Element(CONFIG_UNIT_UNITTYPE).Value;
                KeyValuePair<string, string> pair = new KeyValuePair<string, string>(fileType, unitType);
                _UnitList.Add(pair);
            }

            XElement fileSettingsElement = rootElement.Element(CONFIG_FILE_SETTING);
            foreach (XElement fileElement in fileSettingsElement.Elements(CONFIG_FILE_FILE))
            {
                string name = fileElement.Attribute(CONFIG_FILE_NAME).Value;
                string schemaName = fileElement.Attribute(CONFIG_FILE_SCHEMANAME).Value;
                string schemaDirectory = fileElement.Attribute(CONFIG_FILE_SCHEMADIRECTORY).Value;
                string systemCode = fileElement.Attribute(CONFIG_FILE_SYSTEMCODE).Value;

                if (_FileSettings.ContainsKey(name))
                {
                    throw new Exception($"FileSettings配置中存在重复的Name名称.{name}");
                }
                if (_FileSettings.Values.Any(t => t.Item1.Equals(schemaName)
                && t.Item2.Equals(schemaDirectory) && t.Item3.Equals(systemCode)))
                {
                    throw new Exception($"FileSettings的Name名称为{name}的属性存在重复项.{schemaName}.{schemaDirectory}");
                }
                _FileSettings.Add(name, Tuple.Create(schemaName, schemaDirectory, systemCode));

            }
            XElement assemblySettingsElement = rootElement.Element(CONFIG_ASSEMBLY_SETTING);
            foreach (XElement assemblyElement in assemblySettingsElement.Elements(CONFIG_ASSEMBLY_ASSEMBLY))
            {
                string name = assemblyElement.Attribute(CONFIG_ASSEMBLY_NAME).Value;
                string typeName = assemblyElement.Attribute(CONFIG_ASSEMBLY_TYPE).Value;
                string encodingName = assemblyElement.Attribute(CONFIG_ASSEMBLY_ENCODING).Value;

                if (_AssemblySettings.ContainsKey(name))
                {
                    throw new Exception($"AssemblySettings配置中存在重复的Name名称.{name}");
                }
                if (_AssemblySettings.Values.Any(t => t.Item1.Equals(typeName) && t.Item2.Equals(encodingName)))
                {
                    throw new Exception($"AssemblySettings配置中存在重复的Name名称{name}的属性存在重复项.{typeName}.{encodingName}");
                }
                _AssemblySettings.Add(name, Tuple.Create(typeName, encodingName));
            }

            XElement componentSettingsElement = rootElement.Element(CONFIG_COMPONENT_SETTING);
            foreach (XElement componentElement in componentSettingsElement.Elements(CONFIG_COMPONENT_COMPONENT))
            {
                string fileName = componentElement.Attribute(CONFIG_COMPONENT_FILENAME).Value;
                string assemblyName = componentElement.Attribute(CONFIG_COMPONENT_ASSEMBLYNAME).Value;

                if (_ComponentSettings.ContainsKey(fileName))
                {
                    throw new Exception($"ComponentSettings配置中存在重复.{fileName}.{assemblyName}");
                }
                _ComponentSettings.Add(fileName, assemblyName);
            }
            #endregion


        }

        #endregion

        #endregion
    }
}