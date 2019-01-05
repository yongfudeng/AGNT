using System.Collections.Generic;
using System.IO;
using BOC.SynchronousService.Framework.Adapter;
using BOC.SynchronousService.Framework.Assembler;
using BOC.SynchronousService.Framework.Common;
using BOC.SynchronousService.Framework.Config;
using System;
using System.Linq;
using System.Text;
using BOC.Services.LogService;
using System.Diagnostics;
using System.Collections;

namespace BOC.SynchronousService.Framework.Runtime
{
    public class RealEngine : EngineBase, IEngine
    {
        private IAdapter _Adapter = null;
        private Dictionary<Message, AssemblerBase> _Components = new Dictionary<Message, AssemblerBase>();
        /// <summary>
        /// 存在缓存的问题，有待解决,其实也不应该放在这
        /// </summary>
        /// <param name="config"></param>
        public void Initialize(ConfigBase config)
        {
            if (_Config == null)
            {
                _Config = config;
            }
            _Adapter = ObjectFactory.CreateObject<IAdapter>(_Config.AdapterType, _Config.AdapterSettings);
            _Components = _Config.Components;
        }

        public void ProcessForSynch()
        {
            var downloadSuccessFiles = new List<string>();
            #region 先FTP下载以前处理的结果文件，扩展名为RET,到本地临时文件目录，然后删除已下载的远程文件，ERR错误文件可以保留
            var downLoadFiles = _Adapter.ListDirectory(_Config.RemoteDownloadDirectory);            
            foreach (var file in downLoadFiles)
            {
                try
                {
                    _Adapter.Receive(_Config.RemoteDownloadDirectory, _Config.TempFilesDirectory, file, file);
                    EventLoggerHelper.WriteEventLogEntry
                       (
                           Utility.EventSource
                           , Utility.EventID
                           , $"下载文件[{file}]成功."
                           , Utility.Category
                           , EventLogEntryType.Information
                       );
                    //只处理扩展名为RET的文件
                    if (file.EndsWith(FileStag.RET.ToString()))
                        downloadSuccessFiles.Add(file);
                    else
                    {
                        try
                        {
                            //扩展名不为RET的文件，下载完成后，直接删除FTP Server上的文件
                            _Adapter.DeleteFile(_Config.RemoteDownloadDirectory, file);
                        }
                        catch (Exception ex)
                        {
                            //记录错误日志，不用抛出
                            EventLoggerHelper.WriteEventLogEntry
                             (
                                 Utility.EventSource
                                 , Utility.EventID
                                 , $"删除远程文件[{file}]出错：{ex.ToString()}"
                                 , Utility.Category
                                 , EventLogEntryType.Error
                             );
                        }

                    }
                }
                catch (Exception ex)
                {
                    //记录错误日志，不用抛出
                    EventLoggerHelper.WriteEventLogEntry
                     (
                         Utility.EventSource
                         , Utility.EventID
                         , $"下传文件出错：{ex.ToString()}"
                         , Utility.Category
                         , EventLogEntryType.Error
                     );
                }
            }
            #endregion

            #region.处理每一个RET文件

            var handlerSuccessFiles = new List<string>();
            foreach (var df in downloadSuccessFiles)
            {
                try
                {
                    var fileType = df.Substring(3, 4);
                    if (!_Components.Any(c => c.Key.ID == fileType))
                        throw new Exception($"无法找到此文件对应的处理组件.{df}");
                    var currentComponent = _Components.FirstOrDefault(c => c.Key.ID == fileType);
                    var currentDataSettings = _Config.DataPathSettings.FirstOrDefault(d => d.Key == fileType);
                    if (currentDataSettings.Key == null)
                        throw new Exception($"无法找到处理此文件对应的数据库连接字符串信息.{df}");
                    Type unitType = Type.GetType(_Config.UnitList.FirstOrDefault(u => u.Key == fileType).Value);
                    IUnit currentUnit = Activator.CreateInstance(unitType) as IUnit;
                    var idList = currentUnit.ProcessDownloadFile(currentComponent, _Config.TempFilesDirectory, df);
                    //对于一个文件，所有的记录一起处理，如果出错，那么本省全部出错,
                    //一块处理的目的是一个信息出错了，那么认为这个文件处理就出错了，需要分析错误原因
                    if (!string.IsNullOrEmpty(idList))
                        currentUnit.UpdateDataToDatabase(currentDataSettings.Value, idList);
                    EventLoggerHelper.WriteEventLogEntry
                     (
                         Utility.EventSource
                         , Utility.EventID
                         , $"处理下传文件[{df}]成功."
                         , Utility.Category
                         , EventLogEntryType.Information
                     );
                    handlerSuccessFiles.Add(df);
                }
                catch (Exception ex)
                {
                    //记录错误日志，并不抛出
                    EventLoggerHelper.WriteEventLogEntry
                      (
                          Utility.EventSource
                          , Utility.EventID
                          , $"处理下传文件出错：{ex.ToString()}"
                          , Utility.Category
                          , EventLogEntryType.Error
                      );
                }

            }
            #endregion

            #region 把处理成功的文件move到Success目录下,处理错误的文件继续放在临时目录下，用于以后定位排查问题
            foreach (var f in handlerSuccessFiles)
            {
                var sourceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    Path.Combine(_Config.TempFilesDirectory, f));
                var targetFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    Path.Combine(Path.Combine(_Config.TempFilesDirectory, "Success"), f));
                try
                {
                    File.Move(sourceFile, targetFile);
                    //处理成功后才删除FTP服务器上的文件，没有成功就继续获取下次再处理
                    _Adapter.DeleteFile(_Config.RemoteDownloadDirectory, f);
                }
                catch (Exception ex)
                {
                    //记录错误日志，不抛出                      
                    EventLoggerHelper.WriteEventLogEntry
                      (
                          Utility.EventSource
                          , Utility.EventID
                          , $"移动文件[{sourceFile}]到[{targetFile}]目录出错：{ ex.ToString()}"
                          , Utility.Category
                          , EventLogEntryType.Error
                      );
                }
            }
            #endregion

            #region 然后从数据中查询IsProcessed = 0的记录，按照每一个省行号生成一个扩展名为TMP的文件（格式按照XX.AGNT.xml定义）
            foreach (var f in _Config.FileSettings)
            {
                Type unitType = Type.GetType(_Config.UnitList.FirstOrDefault(u => u.Key == f.Key).Value);
                IUnit currentUnit = Activator.CreateInstance(unitType) as IUnit;
                var currentDataSettings = _Config.DataPathSettings.FirstOrDefault(d => d.Key == f.Key);
                if (currentDataSettings.Key == null)
                    throw new Exception($"无法找到处理此类文件对应的数据库连接字符串信息.{f.Key}");
                var dataTable = new Dictionary<string, IList<Hashtable>>();
                try
                {
                    currentUnit.GetDataFromDatabase(currentDataSettings.Value, dataTable);
                    EventLoggerHelper.WriteEventLogEntry
                       (
                           Utility.EventSource
                           , Utility.EventID
                           , $"从数据库中获取数据[{f.Key}]成功."
                           , Utility.Category
                           , EventLogEntryType.Information
                       );
                }
                catch (Exception ex)
                {
                    //记录错误日志，不抛出                      
                    EventLoggerHelper.WriteEventLogEntry
                      (
                          Utility.EventSource
                          , Utility.EventID
                          , $"从数据库中获取[{f.Key}]数据出错：{ ex.ToString()}"
                          , Utility.Category
                          , EventLogEntryType.Error
                      );
                }
                AssembleFiles(f, dataTable, currentUnit);
            }
            #endregion
        }
        private void AssembleFiles(KeyValuePair<string, Tuple<string, string, string>> fileSetting,
            Dictionary<string, IList<Hashtable>> dataTable, IUnit currentUnit)
        {
            //开始生成文件
            var currentComponent = _Components.FirstOrDefault(c => c.Key.ID == fileSetting.Key);
            var uploadFiles = currentUnit.AssembleFiles(fileSetting, currentComponent, dataTable, _Config.LocalUploadDirectory);
            UploadFiles(uploadFiles);
        }

        #region 上传所有的文件到远程目录，上传完成后，修改文件扩展名为DAT；上传完成后，删除本地临时目录所有文件
        private void UploadFiles(IList<string> uploadFiles)
        {
            foreach (var f in uploadFiles)
            {
                try
                {
                    _Adapter.Send(
                                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _Config.LocalUploadDirectory),
                                   _Config.RemoteUploadDirectory,
                                    $"{f}.TMP",
                                    $"{f}.TMP"
                                 );

                    _Adapter.RenameFile(_Config.RemoteUploadDirectory, $"{f}.TMP", $"{f}.DAT");
                    EventLoggerHelper.WriteEventLogEntry
                        (
                            Utility.EventSource
                            , Utility.EventID
                            , $"上传文件[{f}.DAT]成功."
                            , Utility.Category
                            , EventLogEntryType.Information
                        );
                    //File.Delete(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _Config.LocalUploadDirectory), $"{f}.TMP"));
                }
                catch (Exception ex)
                {
                    //记录错误日志，不抛出
                    EventLoggerHelper.WriteEventLogEntry
                     (
                         Utility.EventSource
                         , Utility.EventID
                         , $"上传文件[{f}.TMP]出错：{ ex.ToString()}"
                         , Utility.Category
                         , EventLogEntryType.Error
                     );
                }
            }
            #endregion
        }
    }
}