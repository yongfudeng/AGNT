using System;
using System.Collections.Generic;
using BOC.SynchronousService.Framework;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using BOC.SynchronousService.Framework.Assembler;
using System.Linq;
using System.IO;
using BOC.SynchronousService.Framework.Common;
using System.Text;
using System.Diagnostics;
using BOC.Services.LogService;
using System.Configuration;

namespace BOC.SynchronousService.Unit
{
    public class PlkfUnit : IUnit
    {
        private static readonly string GetNoProcessAgntData = "GetNoProcessAgnt";
        private static readonly string UpdateProcessStatus = "UpdateProcessStatus";


        #region Public Methods       
        public void GetDataFromDatabase(string dataPath, Dictionary<string, IList<Hashtable>> dataTable)
        {
            using (SqlConnection conn = new SqlConnection(dataPath))
            {
                conn.Open();
                using (var command = new SqlCommand(GetNoProcessAgntData, conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var hashTable = new Hashtable();
                            var key = reader["ProvinceBranchNo"].ToString();
                            hashTable.Add("ID", reader["ID"].ToString());
                            hashTable.Add("ProvinceBranchNo", key);
                            hashTable.Add("BranchNo", reader["BranchNo"].ToString());
                            hashTable.Add("CustNum", reader["CustNum"].ToString());
                            hashTable.Add("CardNum", reader["CardNum"].ToString());
                            hashTable.Add("Amount", reader["Amount"].ToString());
                            hashTable.Add("IDType", reader["IDType"].ToString());
                            hashTable.Add("IDNum", reader["IDNum"].ToString());
                            hashTable.Add("CustName", reader["CustName"].ToString());
                            hashTable.Add("ChargeType", reader["ChargeType"].ToString());
                            if (!dataTable.ContainsKey(key))
                            {
                                var hashTableList = new List<Hashtable>();
                                hashTableList.Add(hashTable);
                                dataTable.Add(key, hashTableList);
                            }
                            else
                                dataTable[key].Add(hashTable);
                        }

                    }

                }
                conn.Close();
            }
        }

        public void UpdateDataToDatabase(string dataPath, string idList)
        {
            using (SqlConnection conn = new SqlConnection(dataPath))
            {
                conn.Open();
                using (var command = new SqlCommand(UpdateProcessStatus, conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@IDList", SqlDbType.VarChar, -1).Value = idList;
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }


        public IList<string> AssembleFiles(KeyValuePair<string, Tuple<string, string, string>> fileSetting,
            KeyValuePair<Message, AssemblerBase> component, Dictionary<string, IList<Hashtable>> dataTable, string localUploadDirectory)
        {
            var currentAssembly = component.Value;
            var uploadFiles = new List<string>();
            foreach (var province in dataTable.Keys)//分省
            {
                var fileName = string.Empty;
                try
                {
                    //分省的消息集合
                    var messages = new List<Message>();
                    var headerMessage = component.Key.Clone();
                    messages.Add(headerMessage);
                    var hashTableList = dataTable[province];
                    var totalNumber = hashTableList.Count();
                    decimal totalMoney = 0;
                    var realDate = DateTime.Now.AddDays(int.Parse(ConfigurationManager.AppSettings["AddDays"]));
                    fileName = $"{fileSetting.Value.Item3}.{fileSetting.Key}.{province.PadLeft(5, '0')}.{string.Empty.PadRight(10, '0')}.001.E{realDate.ToString("MMdd")}";
                    int seqNum = 1;
                    foreach (var rows in hashTableList)
                    {
                        totalMoney += decimal.Parse(rows["Amount"].ToString());
                        var message = component.Key.Clone();
                        message.Body.FirstOrDefault(b => b.ID == "SeqNum").Value = (++seqNum).ToString();
                        message.Body.FirstOrDefault(b => b.ID == "BranchNum").Value = rows["BranchNo"].ToString();
                        message.Body.FirstOrDefault(b => b.ID == "CustNum").Value = rows["CustNum"].ToString();
                        message.Body.FirstOrDefault(b => b.ID == "OutCardNum").Value = rows["CardNum"].ToString();
                        message.Body.FirstOrDefault(b => b.ID == "Amount").Value = rows["Amount"].ToString();
                        //优化:以后把收费类型/摘要做成可配置的参数放到数据库或者配置文件中
                        var chargeType = rows["ChargeType"].ToString() == "01" ? "al" : "bh";
                        message.Body.FirstOrDefault(b => b.ID == "PromoCode").Value = chargeType;
                        message.Body.FirstOrDefault(b => b.ID == "IDType").Value = rows["IDType"].ToString();
                        message.Body.FirstOrDefault(b => b.ID == "IDCard").Value = rows["IDNum"].ToString();
                        message.Body.FirstOrDefault(b => b.ID == "CustName").Value = rows["CustName"].ToString();                        
                        var summary = chargeType == "al" ? "卡工本费|智能柜台" : "认证工具|智能柜台";
                        message.Body.FirstOrDefault(b => b.ID == "Summary").Value = summary;
                        message.Body.FirstOrDefault(b => b.ID == "ID").Value = rows["ID"].ToString();
                        currentAssembly.Assemble(message);
                        messages.Add(message);

                    }
                    headerMessage.Head.FirstOrDefault(b => b.ID == "FileName").Value = $"{fileName}.DAT";
                    headerMessage.Head.FirstOrDefault(b => b.ID == "BranchNum").Value = province;
                    headerMessage.Head.FirstOrDefault(b => b.ID == "TotalNumber").Value = totalNumber.ToString();
                    headerMessage.Head.FirstOrDefault(b => b.ID == "TotalMoney").Value = totalMoney.ToString();
                    headerMessage.Head.FirstOrDefault(b => b.ID == "EffectTime").Value = realDate.ToString("yyyyMMdd");
                    currentAssembly.Assemble(headerMessage, true);
                    //生成文件                   
                    string tempFileName = $"{fileName}.TMP";
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, localUploadDirectory);

                    using (FileStream fs = new FileStream(Path.Combine(path, tempFileName), FileMode.Create))
                    {
                        bool isFirst = true;
                        foreach (var mes in messages)
                        {
                            var fileValue = new StringBuilder();
                            if (isFirst)
                            {
                                foreach (var mh in mes.Head)
                                {
                                    fileValue.Append(mh.Value);
                                }
                                isFirst = false;
                            }
                            else
                                foreach (var mb in mes.Body)
                                {
                                    fileValue.Append(mb.Value);
                                }

                            byte[] data = Encoding.GetEncoding("gb2312").GetBytes(fileValue.ToString());
                            fs.Write(data, 0, data.Length);
                            fs.WriteByte(13);
                            fs.WriteByte(10);
                        }
                        fs.Flush();
                        fs.Close();
                    }
                    EventLoggerHelper.WriteEventLogEntry
                         (
                             Utility.EventSource
                             , Utility.EventID
                             , $"生成文件[{tempFileName}]成功."
                             , Utility.Category
                             , EventLogEntryType.Information
                         );
                    uploadFiles.Add(fileName);
                }
                catch (Exception ex)
                {
                    //记录错误日志，不抛出
                    EventLoggerHelper.WriteEventLogEntry
                     (
                         Utility.EventSource
                         , Utility.EventID
                         , $"写入文件[{ fileName}.TMP]出错：{ ex.ToString()}"
                         , Utility.Category
                         , EventLogEntryType.Error
                     );
                }
            }
            return uploadFiles;
        }


        public string ProcessDownloadFile(KeyValuePair<Message, AssemblerBase> component, string tempFilesDirectory, string fileName)
        {
            var currentAssembly = component.Value;
            string idList = string.Empty;
            using (StreamReader reader = new StreamReader(Path.Combine(tempFilesDirectory, fileName), Encoding.GetEncoding("gb2312")))
            {
                //先读取header
                string line = reader.ReadLine();
                line = reader.ReadLine();
                while (line != null)
                {
                    byte[] bytes = currentAssembly.Encode.GetBytes(line);
                    var message = component.Key.Clone();
                    currentAssembly.Dissemble(bytes, message);
                    var agntId = message.Body.FirstOrDefault(b => b.ID == "ID").Value.Trim();
                    var resultCode = message.Body.FirstOrDefault(b => b.ID == "ResultCode").Value.Trim();
                    if (!string.IsNullOrEmpty(agntId) && (resultCode == "0" || resultCode == "0000"))
                    {
                        idList += agntId + ",";
                    }
                    line = reader.ReadLine();
                }
            }
            return idList;
        }


        #endregion
    }
}
