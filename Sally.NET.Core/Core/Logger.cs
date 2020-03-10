using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Core
{
    public class Logger : IDisposable
    {
        private string filePath = String.Empty;
        private FileStream fileStream;
        private StreamWriter writer;
        private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        private Task writeTask;

        public Logger(string path)
        {
            if (!File.Exists(path))
            {
                directoryBuilder(path);
            }
            this.fileStream = File.OpenWrite(path);
            this.writer = new StreamWriter(fileStream);
            this.writeTask = new Task(writeLoop);
            this.writeTask.Start();
        }

        private async void writeLoop()
        {
            while (true)
            {
                if (queue.TryDequeue(out string result))
                {
                    this.writer.WriteLine(result);
                    this.writer.Flush();
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        public void Dispose()
        {
            this.writeTask.Dispose();
            this.writer.Dispose();
            this.fileStream.Close();
            this.fileStream.Dispose();
        }

        public void Log(string text)
        {
            string result = String.Format("[{0}] {1}{2}", DateTime.Now, text, Environment.NewLine);
            queue.Enqueue(result);
        }

        private void directoryBuilder(string path)
        {
            //create start for path
            string subpath = "./";
            //get path without the "./"
            string substring = path.Substring(2, path.Length - 2);
            //split complete path to a list
            List<string> directories = substring.Split("/").ToList();
            //remove the last element because this is the actual file and no directory
            directories.RemoveAt(directories.Count - 1);
            //loop through every directory and check if its exist or not, create if doesn't
            foreach (string directory in directories)
            {
                subpath += (directory + "/");
                if (!Directory.Exists(subpath))
                {
                    Directory.CreateDirectory(subpath);
                }
            }
        }
    }
}
