using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirQualityRecorder.Untils
{
    public class FilePollService : IDisposable
    {
        private Thread _filePollThread;
        private Dictionary<FileIO, List<FileInfo>> _filesDictionary;
        private static FilePollService _filePollService;

        private FilePollService()
        {
            _filesDictionary = new Dictionary<FileIO, List<FileInfo>>();
        }
        
        public static FilePollService Instance {
            get {
                if (_filePollService == null)
                    _filePollService = new FilePollService();
                return _filePollService;
            }
        }

        public delegate void FileUpdateHandler(object recevier, object sender);

        public event FileUpdateHandler OnFileUpdate;
        
        public void AddPollFilePath(FileIO file)
        {
            if (!_filesDictionary.ContainsKey(file))
                _filesDictionary.Add(file, GetFileInfos(file.FilePath));

            if (_filePollThread == null)
                StartContinuousCheckUpdates();
        }

        private List<FileInfo> GetFileInfos(string filePath)
        {
            var fileInfos = new List<FileInfo>();
            if (IsDirectory(filePath))
            {
                foreach (var path in Directory.GetFiles(filePath).OrderBy(x => x))
                {
                    fileInfos.Add(new FileInfo(path));
                }
            }
            else
            {
                fileInfos.Add(new FileInfo(filePath));
            }

            return fileInfos;
        }

        public void RemovePollFilePath(FileIO file)
        {
            if (_filesDictionary.ContainsKey(file))
                _filesDictionary.Remove(file);

            if (_filesDictionary.Count == 0)
                StopContinuousCheckUpdates();
        }

        public void CheckUpdates()
        {
            var filesRequireUpdate = new List<FileIO>();

            foreach (var file in _filesDictionary)
            {
                if (!IsUpdated(file)) continue;
                filesRequireUpdate.Add(file.Key);
            }

            foreach (var file in filesRequireUpdate)
            {
                _filesDictionary.Remove(file);
                _filesDictionary.Add(file, GetFileInfos(file.FilePath));
                OnFileUpdate(file, this);
            }
        }

        private bool IsUpdated(KeyValuePair<FileIO, List<FileInfo>> file)
        {
            if (IsDirectory(file.Key.FilePath))
                return IsFolderUpdated(file.Key.FilePath, file.Value);
            return IsFileUpdated(file.Key.FilePath, file.Value.First());
        }

        private bool IsFileUpdated(string filePath, FileInfo fileInfo)
        {
            var checkfileInfo = new FileInfo(filePath);

            if (fileInfo.Exists != checkfileInfo.Exists || fileInfo.LastWriteTime != checkfileInfo.LastWriteTime)
            {
                return true;
            }
            return false;
        }

        private bool IsFolderUpdated(string folderPath, List<FileInfo> fileInfos)
        {
            int index = 0;
            foreach (var path in Directory.GetFiles(folderPath).OrderBy(x => x))
            {
                if (IsFileUpdated(path, fileInfos[index])) return true;
                index++;
            }
            return false;
        }

        private bool IsDirectory(string filePath)
        {
            if(!File.Exists(filePath)) return false;
            FileAttributes fileAttributes = File.GetAttributes(filePath);
            if ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                return true;
            return false;
        }

        private void StartContinuousCheckUpdates()
        {
            _filePollThread = new Thread(() => {
                while (true)
                {
                    CheckUpdates();
                    Thread.Sleep(1000);
                }
            });

            _filePollThread.Start();
        }

        private void StopContinuousCheckUpdates()
        {
            if (_filePollThread != null)
            {
                _filePollThread.Abort();
                _filePollThread = null;
            }
        }

        public void Dispose()
        {
            StopContinuousCheckUpdates();
        }
    }
}
