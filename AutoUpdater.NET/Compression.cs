using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace QI4A.ZIP
{
    /// <summary>
    /// 수집 파일 압축
    /// </summary>
    public class Compression
    {
        public List<FileInfo> FileCompair(List<FileInfo> localList, List<FileModel> serverList)
        {
            Collection collection = new Collection();
            List<FileInfo> updateList = new List<FileInfo>();

            for (int i = 0; i < localList.Count; i++)
            {
                bool newFileCheck = true;

                for (int j = 0; j < serverList.Count; j++)
                {
                    string localFileName = collection.DirFileName(localList[i].FullName);

                    if (localFileName.Equals(serverList[j].Name))
                    {
                        newFileCheck = false;
                        bool dateCheck = localList[i].LastWriteTime.ToString() == serverList[j].Date;
                        bool sizeCheck = localList[i].Length.ToString() == serverList[j].Size;

                        if (!dateCheck || !sizeCheck)
                        {
                            updateList.Add(localList[i]);
                        }

                        break;
                    }
                }

                if (newFileCheck)
                {
                    updateList.Add(localList[i]);
                }
            }

            return updateList;
        }

        public void MakeZip(List<FileInfo> fileList)
        {
            Collection collection = new Collection();

            string startPath = AppDomain.CurrentDomain.BaseDirectory;
            using (FileStream fs = new FileStream(startPath + @"\Update.zip", FileMode.Create, FileAccess.ReadWrite))
            {
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    foreach (var file in fileList)
                    {
                        string fileFullPath = file.FullName;
                        string dirFileName = collection.DirFileName(file.FullName);
                        try
                        {
                            zip.CreateEntryFromFile(fileFullPath, dirFileName);
                        }
                        catch
                        {

                        }

                    }
                }
            }

        }
    }
}
