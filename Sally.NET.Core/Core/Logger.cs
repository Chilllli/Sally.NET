using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sally.NET.Core
{
    public class Logger
    {
        private string filePath = String.Empty;
        public Logger(string path)
        {
            filePath = path;
        }

        public void Log(string text)
        {
            if(File.Exists(filePath))
            {
                File.AppendAllText(filePath, String.Format("[{0}] {1}{2}", DateTime.Now, text, Environment.NewLine));
            }
            else
            {
                directoryBuilder(filePath);
                File.Create(filePath).Dispose();
                File.AppendAllText(filePath, String.Format("[{0}] {1}{2}", DateTime.Now, text, Environment.NewLine));
            }
        }

        private void directoryBuilder(string path)
        {
            //create start for path
            string subpath = ".\\";
            //get path without the ".\"
            string substring = path.Substring(2, path.Length - 2);
            //split complete path to a list
            List<string> directories = substring.Split("\\").ToList();
            //remove the last element because this is the actual file and no directory
            directories.RemoveAt(directories.Count - 1);
            //loop through every directory and check if its exist or not, create if doesn't
            foreach (string directory in directories)
            {
                subpath += (directory+"\\");
                if (!Directory.Exists(subpath))
                {
                    Directory.CreateDirectory(subpath);
                }
            }
        }
    }
}
