using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNFileSizeReportGenerator
{
    class Log
    {
        static public string FolderPath { get; set; }
        static public string StartTime { get; set; }
        static Log()
        {
            FolderPath = "";
            StartTime = DateTime.Now.ToString("yyyyMMdd_HH_mm_ss");
            
    }
        static public void LogRunEvent(string logText)
        {
            string timeStamp = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
            File.AppendAllText(FolderPath + "/log" + StartTime + ".txt",  timeStamp + " : " + logText + Environment.NewLine);
            Console.WriteLine(timeStamp + ": " + logText);
        }


        static public void LogErrorEvent(string logText, string errorText)
        {
            string timeStamp = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
            File.AppendAllText(FolderPath + "/errorlog" + StartTime + ".txt", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss") + " : " + logText + Environment.NewLine);
            Console.WriteLine(timeStamp + ": " + logText);
        }

        static public bool CheckErrorLogExists()
        {
            if(File.Exists(FolderPath + "/errorlog" + StartTime + ".txt"))
            {
                return true;
            }
            return false;
        }

        static public string  ReturnErrorLogs()
        {
            string errors = string.Empty;
            using (StreamReader sr = new StreamReader(FolderPath + "/errorlog" + StartTime + ".txt"))
            { 
                errors = sr.ReadToEnd();
            }

            return errors;
        }
                
    }
}
