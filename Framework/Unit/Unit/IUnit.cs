using BOC.SynchronousService.Framework.Assembler;
using System;
using System.Collections;
using System.Collections.Generic;
namespace BOC.SynchronousService.Framework
{
    public interface IUnit
    {

        #region Methods

        IList<string> AssembleFiles(KeyValuePair<string, Tuple<string, string, string>> fileSetting,
            KeyValuePair<Message, AssemblerBase> component, Dictionary<string, IList<Hashtable>> dataTable,string localUploadDirectory);
        void UpdateDataToDatabase(string dataPath, string idList);
        void GetDataFromDatabase(string dataPath, Dictionary<string, IList<Hashtable>> dataTable);

        string ProcessDownloadFile(KeyValuePair<Message, AssemblerBase> component,string tempFilesDirectory,string fileName);
        #endregion
    }
}