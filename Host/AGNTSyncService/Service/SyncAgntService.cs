using BOC.SynchronousService.Framework.Common;
using BOC.SynchronousService.Framework.Config;
using BOC.SynchronousService.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BOC.SynchronousService.Host.AGNTSyncService
{
    public class SyncAgntService
    {
        private static IEngine engine = null;
        public void Process(ConfigBase config)
        {
            if (engine == null)
            {
                engine = ObjectFactory.CreateObject<IEngine>(config.EngineType, null);
            }
            engine.Initialize(config);
            engine.ProcessForSynch();
        }

    }
}
