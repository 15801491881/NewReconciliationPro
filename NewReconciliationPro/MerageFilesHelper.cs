using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

namespace ReconciliatlnPro
{
    class MerageFilesHelper
    {
        private  ObservableCollection<DataFileInfo> fileResultData = new ObservableCollection<DataFileInfo>();
        private const string FILE_PRIX = "保利国际影城";
        public Task<ObservableCollection<DataFileInfo>> MerageFiles(string templateDir, string dataDir, string targetDir)
        {
            return Task.Run(() =>
            {
                Console.WriteLine("MerageFiles......." + Thread.CurrentThread);

                // SetprogressBar(progressBar1.Value + 1);
                //1 模板文件
                Dictionary<string, FileInfo> tempFilesMapping = ReadFileList(templateDir, ".xlsx");
                //2 读取数据文件
                Dictionary<string, FileInfo> dataFilesMapping = ReadFileList(dataDir, "_");

                //3 比较文件
                Dictionary<FileInfo, FileInfo> merageMapping = merageDataMapping(tempFilesMapping, dataFilesMapping);

                //4 清理目标文件夹
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();//实例化Excel对象
                object missing = System.Reflection.Missing.Value;//获取缺少的object类型值
                DirectoryInfo dir = new DirectoryInfo(targetDir);
                if (dir.Exists)
                {
                    FileInfo[] fileInfos = dir.GetFiles();
                    DirectoryInfo[] childs = dir.GetDirectories();
                    foreach (FileInfo child in fileInfos)
                    {
                        if (!(child.Name.StartsWith(".") || child.Name.StartsWith("~$")))
                        {
                            Console.WriteLine(child.FullName);
                            child.Delete();
                        }
                    }
                }

                //5 合并文件
                foreach (var item in merageMapping)
                {
                    //SetprogressBar(progressBar1.Value + 1);
                    FileInfo tempateData = item.Key;
                    FileInfo dataData = item.Value;
                    Microsoft.Office.Interop.Excel.Workbook templateWorkbook = excel.Application.Workbooks.Open(tempateData.FullName, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing);
                    Microsoft.Office.Interop.Excel.Workbook dataWorkbook = excel.Application.Workbooks.Open(dataData.FullName, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing);
                    Console.WriteLine(dataData.FullName);
                    Console.WriteLine(templateWorkbook.Worksheets.Count);
                    Console.WriteLine(templateWorkbook.Worksheets[1]);
                    Microsoft.Office.Interop.Excel.Worksheet templateSheet = templateWorkbook.Worksheets[1];
                    Microsoft.Office.Interop.Excel.Worksheet orderSheet = dataWorkbook.Worksheets[1];
                    Microsoft.Office.Interop.Excel.Workbook workbook = excel.Application.Workbooks.Add(true);
                    //更新 日期
                    updateDataDuration(templateSheet);
                    templateSheet.Copy(workbook.Worksheets[1], Type.Missing);
                    orderSheet.Copy(workbook.Worksheets[2], Type.Missing);
                    //写入订单数据--
                   // wirteOrdertoDB(orderSheet);
                    if (dataWorkbook.Worksheets.Count >= 2)
                    {
                        Microsoft.Office.Interop.Excel.Worksheet goodsSheet = dataWorkbook.Worksheets[2];
                        goodsSheet.Copy(workbook.Worksheets[3], Type.Missing);
                    }

                    Console.WriteLine(templateSheet.Name);

                    //  workbook.Worksheets.;
                    workbook.Worksheets[1].Select();
                    workbook.Worksheets[workbook.Worksheets.Count].Delete();
                    string merageDataFile = targetDir + "\\M-" + tempateData.Name;
                    workbook.SaveAs(merageDataFile);
                    workbook.Close(false, missing, missing);
                    dataWorkbook.Close(false, missing, missing);
                    templateWorkbook.Close(false, missing, missing);
                    addRecord(merageDataFile, "OK");
                    //更新模板日期

                    //  this.label1.Content = this.progressBar1.Value + "%";
                    // SetprogressBar(progressBar1.Value + 1);
                }

                //6 确认提示
                return fileResultData;
            });
           



        }

        private void updateDataDuration(Worksheet templateSheet)
        {
            if (templateSheet != null) {
                templateSheet.Cells[4, 2].value = "2017.11.01 - 2017.11.30";
                templateSheet.Cells[6, 2].value = "2017年11月内通过自有渠道购票用户";
            }
        }

        private Dictionary<FileInfo, FileInfo> merageDataMapping(Dictionary<string, FileInfo> tempFilesMapping, Dictionary<string, FileInfo> dataFilesMapping)
        {
            Dictionary<FileInfo, FileInfo> merageMapping = new Dictionary<FileInfo, FileInfo>();

            foreach (var item in tempFilesMapping)
            {
                Console.WriteLine(item.Key + item.Value);
                if (dataFilesMapping.ContainsKey(item.Key))
                {
                    Console.WriteLine("存在：" + item.Key);
                    merageMapping.Add(item.Value, dataFilesMapping[item.Key]);
                }
                else
                {
                    Console.WriteLine("不存在：" + item.Key);
                    addRecord(item.Value.FullName, "不存在：" + item.Key);

                }
            }
        //    this.label1.Content = this.progressBar1.Value + "%";
            return merageMapping;
        }

        private void addRecord(string fileName, string result)
        {
            getFileResultData().Add(new DataFileInfo()
                {
                    Name = fileName,
                    Result = result

                });
        }
      
        public ObservableCollection<DataFileInfo> getFileResultData()
        {
            lock (this) { 
                  return fileResultData;
            }
        }

        private Dictionary<string, FileInfo> ReadFileList(string path, string splitChar)
        {
            Dictionary<string, FileInfo> fileMapping = new Dictionary<string, FileInfo>();
            string folderFullName = path;
            DirectoryInfo TheFolder = new DirectoryInfo(folderFullName);
            FileInfo[] files = TheFolder.GetFiles("*.xlsx");
            foreach (FileInfo info in files)
            {
                if (!(info.Name.StartsWith(".") || info.Name.StartsWith("~$")))
                {
                    if (info.Name.IndexOf(FILE_PRIX) >= 0)
                    {
                        int start = info.Name.IndexOf(FILE_PRIX) + FILE_PRIX.Length;
                        int end = info.Name.IndexOf(splitChar);
                        string shortName = info.Name.Substring(start, end - start);
                        Console.WriteLine("-short name-:" + shortName.Trim());
                        Console.WriteLine(info.FullName);
                        fileMapping.Add(shortName.Trim(), info);
                    }
                    else if (info.Name.Substring(0, 6).IndexOf("-") >= 0)//判断比较愚蠢需要修改
                    {
                        Console.WriteLine(info.FullName);
                        int start = info.Name.IndexOf("-") + "-".Length;
                        int end = info.Name.IndexOf(splitChar);
                        string shortName = info.Name.Substring(start, end - start);
                        Console.WriteLine("-short name-:" + shortName.Trim());

                        fileMapping.Add(shortName.Trim(), info);
                    }

                    else
                    {
                        int start = 0;
                        int end = info.Name.IndexOf(splitChar);
                        string shortName = info.Name.Substring(start, end - start);
                        Console.WriteLine("-short name-:" + shortName.Trim());
                        Console.WriteLine(info.FullName);
                        fileMapping.Add(shortName.Trim(), info);
                    }
                }
            }
            return fileMapping;
        }

    }
}
