using System;
using System.Collections.Generic;

namespace BOC.SynchronousService.Framework.Adapter
{
    public interface IAdapter
    {
        #region Methods

        List<string> ListDirectory(string remotePath);

        void DeleteFile(string remotePath, string remoteFileName);

        void RenameFile(string remotePath, string oriName, string newName);

        void Receive(string remotePath, string localPath, string remoteFileName, string localFileName);

        void Send(string localPath, string remotePath, string localFileName, string remoteFileName);

        #endregion
    }
}