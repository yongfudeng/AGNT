using BOC.SynchronousService.Framework.Config;

namespace BOC.SynchronousService.Framework.Runtime
{
    public interface IEngine
    {
        #region Methods

        void Initialize(ConfigBase config);     

        void ProcessForSynch();

        #endregion
    }
}