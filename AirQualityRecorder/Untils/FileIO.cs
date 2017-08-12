using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirQualityRecorder.Untils
{
    public class FileIO :IDisposable
    {
        private FileInfo _fileInfo;
        private string _filePath;

        public delegate void FileUpdateHandler(object sender);

        public FileIO(string filePath)
        {
            _filePath = filePath;
            _fileInfo = new FileInfo(filePath);

            _fileInfo.Directory.Create();
        }

        public void WriteLine(string line)
        {
            using (var sw = new StreamWriter(_filePath))
            {
                sw.WriteLine(line);
                sw.Close();
            }
        }

        public List<string> ReadAllLines()
        {
            if (_fileInfo.Exists == false) return new List<string>();

            return File.ReadAllLines(_fileInfo.FullName).ToList();
        }

        public void StartContinuousCheckUpdates()
        {
            FilePollService.Instance.AddPollFilePath(this);
            FilePollService.Instance.OnFileUpdate += (recevier, sender) =>
            {
                if (recevier == this) OnFileUpdated(this);
            };
        }

        public void StopContinuousCheckUpdates()
        {
            FilePollService.Instance.RemovePollFilePath(this);
        }
        
        public string FilePath { get => _filePath; set => _filePath = value; }

        public void Dispose()
        {
            FilePollService.Instance.RemovePollFilePath(this);
        }

        public event FileUpdateHandler OnFileUpdated;
    }
}
