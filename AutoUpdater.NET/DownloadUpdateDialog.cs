using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using AutoUpdaterDotNET.Properties;
using QI4A.ZIP;

namespace AutoUpdaterDotNET
{
    internal partial class DownloadUpdateDialog : Form
    {
        private readonly UpdateInfoEventArgs _args;
        private int downloadCount = 0;
        private int downloadMaxCount = 0;
        private MyWebClient _webClient;
        private List<FileModel> _updateList;
        private DateTime _startedAt;
        private List<string> updateList = new List<string>();
        delegate void ProgVarCall(int var);

        public DownloadUpdateDialog(UpdateInfoEventArgs args, List<FileModel> updateList)
        {
            InitializeComponent();

            _args = args;
            _updateList = updateList;

            if (AutoUpdater.Mandatory && AutoUpdater.UpdateMode == Mode.ForcedDownload)
            {
                ControlBox = false;
            }
        }

        private void DownloadSet()
        {
            downloadCount = 0;
            downloadMaxCount = 0;
            updateList.Clear();
        }

        public void Download(object sender, EventArgs e)
        {
            DownloadSet();

            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Safari/537.36";
            downloadMaxCount = _updateList.Count;

            foreach (var updateFile in _updateList)
            {
                string fileFullName = updateFile.Name;
                string fileName = fileFullName;
                string fileDir = string.Empty;

                var fileSplit = fileFullName.Split('\\');
                if (fileSplit.Length != 1)
                {
                    fileName = fileSplit[fileSplit.Length - 1];
                    fileDir = fileFullName.Substring(0, fileFullName.Length - fileName.Length);
                    
                    string tmpDirPath = Path.GetTempPath() + fileDir;
                    DirectoryInfo di = new DirectoryInfo(tmpDirPath);
                    if (!di.Exists)
                    {
                        di.Create();
                    }

                }

                var uri = new Uri(_args.DownloadURL + updateFile.Name);
                var _tempFile = Path.Combine(Path.GetTempPath(), fileFullName);

                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                webClient.DownloadProgressChanged += OnDownloadProgressChanged;
                webClient.QueryString.Add("fileName", fileName);
                webClient.QueryString.Add("fileDir", fileDir);
                webClient.QueryString.Add("tmpFileName", _tempFile);
                webClient.Headers[HttpRequestHeader.UserAgent] = userAgent;
                webClient.DownloadFileAsync(uri, _tempFile);
            }

        }


        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            var fileName = ((System.Net.WebClient)(sender)).QueryString["fileName"];
            var fileDir = ((System.Net.WebClient)(sender)).QueryString["fileDir"];
            var tmpFileName = ((System.Net.WebClient)(sender)).QueryString["tmpFileName"];
            var filePath = Environment.CurrentDirectory;
            var fileFullPath = Path.Combine(filePath, fileName);
     
            if(fileDir.Length != 0)
            {
                fileFullPath = string.Format(@"{0}\{1}{2}", filePath, fileDir, fileName);
                DirectoryInfo di = new DirectoryInfo(Path.Combine(filePath, fileDir));
                if (di.Exists == false)
                {
                    di.Create();
                }
            }

            if(File.Exists(fileFullPath))
            {
                string deleteFile = fileFullPath + ".tmp";
                if(File.Exists(deleteFile))
                {
                    File.SetAttributes(deleteFile, FileAttributes.Normal);
                    File.Delete(deleteFile);
                }

                File.Move(fileFullPath, deleteFile);
                File.Copy(tmpFileName, fileFullPath, true);
                File.SetAttributes(deleteFile, FileAttributes.Normal);
            }
            else
            {
                File.Move(tmpFileName, fileFullPath);
            }

            downloadCount++;
            updateList.Add(fileFullPath);

            if(downloadCount == downloadMaxCount)
            {
                MakeCompleteUpdateListFile();
                this.Close();
                Application.Restart();
            }
        }

        private void MakeCompleteUpdateListFile()
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "update.txt")))
            {
                foreach(var name in updateList)
                {
                    sw.WriteLine(name);
                }
            }
        }


        private void ProgValueSetting(int var)
        {
            progressBar.Value = var;
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            int per = (int)(((double)downloadCount / (double)downloadMaxCount) * 100);
            progressBar.Invoke(new ProgVarCall(ProgValueSetting), per);
        }

        private static string BytesToString(long byteCount)
        {
            string[] suf = {"B", "KB", "MB", "GB", "TB", "PB", "EB"};
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{(Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture)} {suf[place]}";
        }

        private static void CompareChecksum(string fileName, CheckSum checksum)
        {
            using (var hashAlgorithm =
                HashAlgorithm.Create(
                    string.IsNullOrEmpty(checksum.HashingAlgorithm) ? "MD5" : checksum.HashingAlgorithm))
            {
                using (var stream = File.OpenRead(fileName))
                {
                    if (hashAlgorithm != null)
                    {
                        var hash = hashAlgorithm.ComputeHash(stream);
                        var fileChecksum = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

                        if (fileChecksum == checksum.Value.ToLower()) return;

                        throw new Exception(Resources.FileIntegrityCheckFailedMessage);
                    }

                    throw new Exception(Resources.HashAlgorithmNotSupportedMessage);
                }
            }
        }

        private void DownloadUpdateDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_webClient == null)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (_webClient.IsBusy)
            {
                _webClient.CancelAsync();
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}