using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using BOC.SynchronousService.Framework.Adapter;
using System.Diagnostics;
using BOC.SynchronousService.Framework.Common;

namespace BOC.SynchronousService.Adapter
{
    /// <summary>
    /// ftp上传接口
    /// </summary>
    public class FtpAdapter : IAdapter
    {
        #region Fields

        private IPAddress _RemoteAddress;

        private int _RemotePort;

        private string _Username;

        private string _Password;

        #endregion

        #region Public Properties

        /// <summary>
        /// 远程ftp服务器地址
        /// </summary>
        public string RemoteAddress
        {
            get
            {
                return _RemoteAddress.ToString();
            }
            set
            {
                try
                {
                    _RemoteAddress = IPAddress.Parse(value);
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("设置 RemoteAddress {0} 失败.", value), e);
                }
            }
        }

        /// <summary>
        /// 远程ftp服务器端口
        /// </summary>
        public string RemotePort
        {
            get
            {
                return _RemotePort.ToString();
            }
            set
            {
                try
                {
                    _RemotePort = int.Parse(value);
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("设置 RemotePort {0} 失败.", value), e);
                }
            }
        }

        /// <summary>
        /// 远程ftp服务器登录用户
        /// </summary>
        public string Username
        {
            get { return _Username; }
            set { _Username = value; }
        }

        /// <summary>
        /// 远程ftp服务器登录用户密码
        /// </summary>
        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }

        #endregion

        #region IAdapter Members

        /// <summary>
        /// 列出远程ftp服务器路径下的所有文件
        /// </summary>
        /// <param name="remotePath">远程服务器路径</param>
        /// <returns></returns>
        public List<string> ListDirectory(string remotePath)
        {
            List<string> fileList = new List<string>();
            try
            {
                string uri = string.Format(@"ftp://{0}:{1}/{2}", RemoteAddress, RemotePort, remotePath);
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(uri);

                request.UseBinary = true;
                request.Credentials = new NetworkCredential(_Username, _Password);
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.ListDirectory;                

                using (FtpWebResponse respond = request.GetResponse() as FtpWebResponse)
                {
                    StreamReader reader = new StreamReader(respond.GetResponseStream());

                    string line = reader.ReadLine();
                    while (line != null)
                    {                        
                        fileList.Add(Path.GetFileName(line));
                        line = reader.ReadLine();                        
                    }
                }               
            }
            catch (Exception e)
            {
                TraceHelper.TraceInformation("FTP 连接失败或者文件列表为空");    
                //throw new Exception("查询文件列表失败.", e);               
            }
            return fileList;
        }

        /// <summary>
        /// 更改远程ftp服务器上的文件名
        /// </summary>
        /// <param name="remotePath">远程ftp服务器文件路径</param>
        /// <param name="oriName">文件原名称</param>
        /// <param name="newName">文件新名称</param>
        public void RenameFile(string remotePath, string oriName, string newName)
        {
            try
            {
                string oriUri = string.Format(@"ftp://{0}:{1}/{2}", RemoteAddress, RemotePort, Path.Combine(remotePath, oriName));
                string newUri = string.Format(@"ftp://{0}:{1}/{2}", RemoteAddress, RemotePort, Path.Combine(remotePath, newName));

                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(oriUri);

                request.UseBinary = true;
                request.Credentials = new NetworkCredential(_Username, _Password);
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.Rename;

                request.RenameTo = newName;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("重命名 {0} 至 {1} 失败.", oriName, newName), e);
            }
        }

        /// <summary>
        /// 删除远程ftp文件服务器上的文件
        /// </summary>
        /// <param name="remotePath">文件路径</param>
        /// <param name="remoteFileName">文件名称</param>
        public void DeleteFile(string remotePath, string remoteFileName)
        {
            try
            {
                string remoteUri = string.Format(@"ftp://{0}:{1}/{2}", RemoteAddress, RemotePort, Path.Combine(remotePath, remoteFileName));
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(remoteUri);

                request.UseBinary = true;
                request.Credentials = new NetworkCredential(_Username, _Password);
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("删除文件 {0}  失败.", Path.Combine(remotePath, remoteFileName)), e);
            }

        }

        /// <summary>
        /// 从远程ftp服务器下载文件
        /// </summary>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="localPath">本地文件路径</param>
        /// <param name="remoteFileName">远程文件名称</param>
        /// <param name="localFileName">本地文件名称</param>
        public void Receive(string remotePath, string localPath, string remoteFileName, string localFileName)
        {
            try
            {
                string remoteUri = string.Format(@"ftp://{0}:{1}/{2}", RemoteAddress, RemotePort, Path.Combine(remotePath, remoteFileName));
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(remoteUri);

                request.UseBinary = true;
                request.Credentials = new NetworkCredential(_Username, _Password);
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                string localUri = Path.Combine(localPath, localFileName);
                using (FileStream localStream = new FileStream(localUri, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    using (Stream remoteStream = response.GetResponseStream())
                    {
                        long tolLength = response.ContentLength;
                        int curLength = 0;

                        int bufferSize = 2048;
                        byte[] buffer = new byte[bufferSize];

                        curLength = remoteStream.Read(buffer, 0, bufferSize);
                        while (curLength > 0)
                        {
                            localStream.Write(buffer, 0, curLength);
                            curLength = remoteStream.Read(buffer, 0, bufferSize);
                        }
                    }
                    response.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("下载 {0} 至 {1} 失败.", remoteFileName, localFileName), e);
            }
        }

        /// <summary>
        /// 上传文件到远程服务器
        /// </summary>
        /// <param name="localPath">本地文件路径</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="localFileName">本地文件名称</param>
        /// <param name="remoteFileName">远程文件名称</param>
        public void Send(string localPath, string remotePath, string localFileName, string remoteFileName)
        {
            try
            {
                string remoteTmpUri = string.Format(@"ftp://{0}:{1}/{2}", RemoteAddress, RemotePort, Path.Combine(remotePath, remoteFileName));
                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(remoteTmpUri);

                request.UseBinary = true;
                request.Credentials = new NetworkCredential(_Username, _Password);
                request.KeepAlive = false;
                request.Method = WebRequestMethods.Ftp.UploadFile;

                using (Stream remoteStream = request.GetRequestStream())
                {
                    string localUri = Path.Combine(localPath, localFileName);
                    using (FileStream localStream = new FileStream(localUri, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        request.ContentLength = localStream.Length;
                        int curLength = 0;

                        int bufferSize = 2048;
                        byte[] buffer = new byte[bufferSize];

                        curLength = localStream.Read(buffer, 0, bufferSize);
                        while (curLength > 0)
                        {
                            remoteStream.Write(buffer, 0, curLength);
                            curLength = localStream.Read(buffer, 0, bufferSize);
                        }
                    }
                    remoteStream.Flush();
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("上传 {0} 至 {1} 失败.", localFileName, remoteFileName), e);
            }
        }

        #endregion
    }
}