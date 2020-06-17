using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace QI4A.ZIP
{
    /// <summary>
    /// 업데이트 파일 수집
    /// </summary>
    public class Collection
    {
        public List<FileModel> FileCompair(List<FileInfo> localList, List<FileModel> serverList)
        {
            Collection collection = new Collection();
            List<FileModel> updateList = new List<FileModel>();
            

            for (int i = 0; i < serverList.Count; i++)
            {
                bool newFileCheck = true;
                var serverItem = serverList[i];

                for (int j = 0; j < localList.Count; j++)
                {
                    var localItem = localList[j];
                    string localFileName = collection.DirFileName(localItem.FullName);

                    if (localFileName.Equals(serverItem.Name))
                    {
                        newFileCheck = false;
                        bool dateCheck = localItem.LastWriteTime.ToString() == serverItem.Date;
                        bool sizeCheck = localItem.Length.ToString() == serverItem.Size;

                        if (!dateCheck || !sizeCheck)
                        {
                            updateList.Add(serverItem);
                        }

                        break;
                    }
                }

                if (newFileCheck)
                {
                    updateList.Add(serverItem);
                }
            }

            return updateList;
        }
        public List<FileInfo> GetUpdateFileList(string path)
        {
            List<FileInfo> fileList = new List<FileInfo>();
            GetFileList(path, ref fileList);
            WriteFileInfo(fileList);
            return fileList;
        }

        private void GetFileList(string path, ref List<FileInfo> fileList)
        {
            System.IO.DirectoryInfo dirInfo = new DirectoryInfo(path);
            foreach (var file in dirInfo.GetFiles())
            {
                string filePath = string.Format(@"{0}\{1}", path, file);
                fileList.Add(new FileInfo(filePath));
            }

            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                GetFileList(dir, ref fileList);
            }
        }

        private void WriteFileInfo(List<FileInfo> fileList)
        {
            XmlDocument xml = new XmlDocument();
            XmlNode root = xml.CreateElement("UpdateList");
            xml.AppendChild(root);

            foreach(var file in fileList)
            {
                XmlNode fileNode = xml.CreateElement("File");
                XmlAttribute nameAttr = xml.CreateAttribute("Name");
                nameAttr.Value = DirFileName(file.FullName);
                fileNode.Attributes.Append(nameAttr);

                XmlNode dateAttr = xml.CreateElement("Date");
                dateAttr.InnerText = file.LastWriteTime.ToString();
                fileNode.AppendChild(dateAttr);

                XmlNode sizeAttr = xml.CreateElement("Size");
                sizeAttr.InnerText = file.Length.ToString();
                fileNode.AppendChild(sizeAttr);

                root.AppendChild(fileNode);
            }

            xml.Save(AppDomain.CurrentDomain.BaseDirectory + @"\UpdateList.xml");
        }

        public string DirFileName(string filePath)
        {
            string buildMode = string.Empty;
#if DEBUG
            buildMode = "Debug\\";
#else
            buildMode = "Release\\";
#endif
            int fileIndex = filePath.IndexOf(buildMode) + buildMode.Length;
            int fileLastIndex = filePath.Length - fileIndex;

            string dirFilePath = filePath.Substring(fileIndex, fileLastIndex);
            return dirFilePath;
        }

        public List<FileModel> ReadUpdateFile(string urlPath)
        {
            List<FileModel> fileModels = new List<FileModel>();
            XmlDocument xml = new XmlDocument();
            xml.Load(urlPath);
            XmlNodeList nodes = xml.ChildNodes;
            nodes = nodes[0].ChildNodes;
            
            foreach(XmlNode node in nodes)
            {
                string name = node.Attributes["Name"].Value;
                string date = node["Date"].InnerText;
                string size = node["Size"].InnerText;

                FileModel model = new FileModel
                {
                    Name = name,
                    Date = date,
                    Size = size
                };
                fileModels.Add(model);
            }

            return fileModels;
        }
    }
}
