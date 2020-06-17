using QI4A.ZIP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace AutoUpdaterDotNET
{
    public class UpdateDownload
    {
        private int percent = 0;

        public void Download(List<FileModel> files, string downloadUrl)
        {
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Safari/537.36";

            foreach (var updateFile in files)
            {
                var uri = new Uri(downloadUrl + updateFile.Name);
                var _tempFile = Path.Combine(AutoUpdater.DownloadPath, $"{Guid.NewGuid().ToString()}.tmp");

                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                webClient.QueryString.Add("fileName", updateFile.Name);
                webClient.QueryString.Add("tmpFileName", updateFile.Name);
                webClient.Headers[HttpRequestHeader.UserAgent] = userAgent;
                webClient.DownloadFileAsync(uri, _tempFile);


            

                    //이름 바꾸고 옮기고 삭제
            }

        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
           var fileName = ((System.Net.WebClient)(sender)).QueryString["fileName"];
           var tmpFileName = ((System.Net.WebClient)(sender)).QueryString["tmpFileName"];
        }

    }
}
