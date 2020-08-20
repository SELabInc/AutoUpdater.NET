using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace QI4A.ZIP
{
    /// <summary>
    /// 업데이트 파일 수집
    /// </summary>
    public class Collection
    {
        private string[] updateExceptList = new string[] { "x86", "x64" };


        /// <summary>
        /// 업데이트 할 파일들의 비교
        /// </summary>
        /// <param name="localList"></param>
        /// <param name="serverList"></param>
        /// <returns></returns>
        public List<FileModel> FileCompair(List<FileModel> localList, List<FileModel> serverList, string mode = "HASH")
        {
            Collection collection = new Collection();
            List<FileModel> updateList = new List<FileModel>();
            
            for (int i = 0; i < serverList.Count; i++)
            {
                bool newFileCheck = true;
                var serverItem = serverList[i];

                bool exceptCheck = UpdateExceptFileCheck(serverItem.Name);
                if(!exceptCheck)
                {
                    continue;
                }

                for (int j = 0; j < localList.Count; j++)
                {
                    var localItem = localList[j];
                    string localFileName = localItem.Name;

                    if (localFileName.Equals(serverItem.Name))
                    {
                        newFileCheck = false;

                        if(mode == "HASH")
                        {
                            bool hashCheck = localItem.Hash == serverItem.Hash;
                            bool sizeCheck = localItem.Size == serverItem.Size;

                            if (!hashCheck || !sizeCheck)
                            {
                                updateList.Add(serverItem);
                                break;
                            }

                            if (localItem.LocalFile != null && !(localItem.LocalFile.Size == serverItem.Size))
                            {
                                updateList.Add(serverItem);
                                break;
                            }
                        }
                        else if(mode == "SIZE")
                        {
                            bool sizeCheck = localItem.Size == serverItem.Size;
                            if (!sizeCheck)
                            {
                                updateList.Add(serverItem);
                            }

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
        
        public List<FileModel> setLocalFileList(List<FileModel> xmlLocalList)
        {
            string path = Environment.CurrentDirectory;
            var localList = GetUpdateFileList(path);

            for(int i = 0; i < xmlLocalList.Count; i++)
            {
                for(int j = 0; j < localList.Count; j++)
                {
                    if(xmlLocalList[i].Name == localList[j].Name)
                    {
                        xmlLocalList[i].LocalFile = localList[j];
                        break;
                    }
                }
            }

            return xmlLocalList;
        }

        /// <summary>
        /// 예외 파일 처리
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool UpdateExceptFileCheck(string fileName)
        {
            var fileSplit = fileName.Split('\\');

            if(fileSplit.Length == 1)
            {
                return true;
            }

            for(int i = 0; i < updateExceptList.Length; i++)
            {
                if(fileSplit[0] == updateExceptList[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 업데이트 할 파일들의 리스트 수집
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<FileModel> GetUpdateFileList(string path)
        {
            List<FileModel> fileList = new List<FileModel>();
            GetFileList(path, ref fileList);
            //WriteFileInfo(fileList);
            return fileList;
        }

        private void GetFileList(string path, ref List<FileModel> fileList)
        {
            System.IO.DirectoryInfo dirInfo = new DirectoryInfo(path);
            foreach (var file in dirInfo.GetFiles())
            {
                string filePath = string.Format(@"{0}\{1}", path, file);
                var fileInfo = new FileInfo(filePath);
                FileModel fileModel = new FileModel() {
                    Size = fileInfo.Length.ToString(),
                    Date = fileInfo.LastWriteTime.ToString(),
                    Name = fileInfo.Name,
                    Hash = ""                    
                };

                fileList.Add(fileModel);
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

        /// <summary>
        /// 빌드 모드에 따른 폴더 구분
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
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

        /// <summary>
        /// xml의 데이터 읽기
        /// </summary>
        /// <param name="urlPath"></param>
        /// <returns></returns>
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
                string hash = node["Hash"].InnerText;

                FileModel model = new FileModel
                {
                    Name = name,
                    Date = date,
                    Size = size,
                    Hash = hash
                };
                fileModels.Add(model);
            }


            return fileModels;
        }
    }
}
