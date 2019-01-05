using BOC.SynchronousService.Framework;
using BOC.SynchronousService.Framework.Assembler;
using BOC.SynchronousService.Framework.Common;
using System;
using System.Collections.Generic;

namespace BOC.SynchronousService.Framework.Config
{
    public abstract class ConfigBase
    {
        #region Fields

        protected string _ServerType;

        protected string _NodeID;

        protected string _EngineType;
        protected string _AdapterType;

        protected Dictionary<string, string> _DataPathSettings;

        protected string _RemoteDownloadDirectory;
        protected string _LocalDownloadDirectory;
        protected string _RemoteUploadDirectory;
        protected string _LocalUploadDirectory;
        protected string _TempFilesDirectory;

        protected Dictionary<string, string> _AdapterSettings;

        protected List<KeyValuePair<string, string>> _UnitList;

        protected ScheduleType _ScheduleTriggerType;
        protected string _ScheduleConfigDetail;

        protected Dictionary<string, Tuple<string, string, string>> _FileSettings;
        protected Dictionary<string, Tuple<string, string>> _AssemblySettings;
        protected Dictionary<string, string> _ComponentSettings;

        protected static IList<Message> _Messages = new List<Message>();
        protected static IList<AssemblerBase> _AssemblyBases = new List<AssemblerBase>();
        protected static Dictionary<Message, AssemblerBase> _Components = new Dictionary<Message, AssemblerBase>();

        #endregion

        #region Constructors

        protected ConfigBase()
        {
            _DataPathSettings = new Dictionary<string, string>();

            _AdapterSettings = new Dictionary<string, string>();

            _FileSettings = new Dictionary<string, Tuple<string, string, string>>();

            _AssemblySettings = new Dictionary<string, Tuple<string, string>>();

            _ComponentSettings = new Dictionary<string, string>();

            _UnitList = new List<KeyValuePair<string, string>>();
        }

        #endregion

        #region Public Properties

        public string ServerType
        {
            get { return _ServerType; }
            set { _ServerType = value; }
        }

        public string NodeID
        {
            get { return _NodeID; }
        }

        public string AdapterType
        {
            get { return _AdapterType; }
        }

        public string EngineType
        {
            get { return _EngineType; }
        }

        public Dictionary<string, string> DataPathSettings
        {
            get { return _DataPathSettings; }
        }

        public string RemoteDownloadDirectory
        {
            get { return _RemoteDownloadDirectory; }
        }

        public string LocalDownloadDirectory
        {
            get { return _LocalDownloadDirectory; }
        }

        public string RemoteUploadDirectory
        {
            get { return _RemoteUploadDirectory; }
        }

        public string LocalUploadDirectory
        {
            get { return _LocalUploadDirectory; }
        }

        public Dictionary<Message, AssemblerBase> Components
        {
            get { return _Components; }
        }
        public string TempFilesDirectory
        {
            get { return _TempFilesDirectory; }
        }

        public Dictionary<string, string> AdapterSettings
        {
            get { return _AdapterSettings; }
        }

        public List<KeyValuePair<string, string>> UnitList
        {
            get { return _UnitList; }
        }


        public ScheduleType ScheduleTriggerType
        {
            get { return _ScheduleTriggerType; }
        }
        public string ScheduleConfigDetail
        {
            get { return _ScheduleConfigDetail; }
        }

        public Dictionary<string, Tuple<string, string, string>> FileSettings
        {
            get
            {
                return _FileSettings;
            }
        }
        public Dictionary<string, Tuple<string, string>> AssemblySettings
        {
            get
            {
                return _AssemblySettings;
            }
        }
        public Dictionary<string, string> ComponentSettings
        {
            get
            {
                return _ComponentSettings;
            }
        }

        #endregion

        #region Methods

        public abstract void Load(string path);

        #endregion       
    }
}