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
    /// ftp�ϴ��ӿ�
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
        /// Զ��ftp��������ַ
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
                    throw new Exception(string.Format("���� RemoteAddress {0} ʧ��.", value), e);
                }
            }
        }

        /// <summary>
        /// Զ��ftp�������˿�
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
                    throw new Exception(string.Format("���� RemotePort {0} ʧ��.", value), e);
                }
            }
        }

        /// <summary>
        /// Զ��ftp��������¼�û�
        /// </summary>
        public string Username
        {
            get { return _Username; }
            set { _Username = value; }
        }

        /// <summary>
        /// Զ��ftp��������¼�û�����
        /// </summary>
        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }

        #endregion

        #region IAdapter Members

        /// <summary>
        /// �г�Զ��ftp������·���µ������ļ�
        /// </summary>
        /// <param name="remotePath">Զ�̷�����·��</param>
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
                TraceHelper.TraceInformation("FTP ����ʧ�ܻ����ļ��б�Ϊ��");    
                //throw new Exception("��ѯ�ļ��б�ʧ��.", e);               
            }
            return fileList;
        }

        /// <summary>
        /// ����Զ��ftp�������ϵ��ļ���
        /// </summary>
        /// <param name="remotePath">Զ��ftp�������ļ�·��</param>
        /// <param name="oriName">�ļ�ԭ����</param>
        /// <param name="newName">�ļ�������</param>
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
                throw new Exception(string.Format("������ {0} �� {1} ʧ��.", oriName, newName), e);
            }
        }

        /// <summary>
        /// ɾ��Զ��ftp�ļ��������ϵ��ļ�
        /// </summary>
        /// <param name="remotePath">�ļ�·��</param>
        /// <param name="remoteFileName">�ļ�����</param>
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
                throw new Exception(string.Format("ɾ���ļ� {0}  ʧ��.", Path.Combine(remotePath, remoteFileName)), e);
            }

        }

        /// <summary>
        /// ��Զ��ftp�����������ļ�
        /// </summary>
        /// <param name="remotePath">Զ���ļ�·��</param>
        /// <param name="localPath">�����ļ�·��</param>
        /// <param name="remoteFileName">Զ���ļ�����</param>
        /// <param name="localFileName">�����ļ�����</param>
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
                throw new Exception(string.Format("���� {0} �� {1} ʧ��.", remoteFileName, localFileName), e);
            }
        }

        /// <summary>
        /// �ϴ��ļ���Զ�̷�����
        /// </summary>
        /// <param name="localPath">�����ļ�·��</param>
        /// <param name="remotePath">Զ���ļ�·��</param>
        /// <param name="localFileName">�����ļ�����</param>
        /// <param name="remoteFileName">Զ���ļ�����</param>
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
                throw new Exception(string.Format("�ϴ� {0} �� {1} ʧ��.", localFileName, remoteFileName), e);
            }
        }

        #endregion
    }
}