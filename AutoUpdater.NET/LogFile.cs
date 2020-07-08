using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoUpdaterDotNET
{
    static class LogFile
    {
        static public void Log(String msg)
        {
            string specialFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\QI4A";
            string FilePath = specialFolder + @"\UpdateLogs\" + DateTime.Today.ToString("yyyyMMdd") + ".log";
            string DirPath = specialFolder + @"\UpdateLogs";
            string temp;

            DirectoryInfo di = new DirectoryInfo(DirPath);
            FileInfo fi = new FileInfo(FilePath);

            try
            {

                if (di.Exists != true) Directory.CreateDirectory(DirPath);
                if (fi.Exists != true)
                {
                    using (StreamWriter sw = new StreamWriter(FilePath))
                    {
                        temp = string.Format("[{0}] {1}", GetDateTime(), msg);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        temp = string.Format("[{0}] : {1}", GetDateTime(), msg);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private static object GetDateTime()
        {
            DateTime NowDate = DateTime.Now;

            return NowDate.ToString("yyyy-MM-dd HH:mm:ss") + ":" + NowDate.Millisecond.ToString("000");

        }
    }   
}
