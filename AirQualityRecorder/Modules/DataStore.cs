using AirQualityRecorder.Untils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirQualityRecorder.Modules
{
    public class DataStore
    {
        private const string DATASTORE_BASEPATH = "Data/";

        public void WriteData(double value, DateTime datetime, string area, string typeName)
        {
            var fileName = $"{datetime.ToString("yyyy-MM-dd")}.csv";
            var datatime = datetime.ToString("yyyy-MM-dd HH:mm");
            var fileAccess = new FileIO(Path.Combine(AppPath, DATASTORE_BASEPATH, area, typeName, fileName));

            fileAccess.WriteLine($"{datatime},{value}");
        }

        public List<(DateTime, double)> ReadData(DateTime datetimeBegin, DateTime datetimeEnd, string area, string typeName)
        {
            var result = new List<(DateTime, double)>();
            var directiory = Path.Combine(AppPath, DATASTORE_BASEPATH, area, typeName);

            if (!Directory.Exists(directiory)) return result;

            var files = Directory.GetFiles(Path.Combine(AppPath, DATASTORE_BASEPATH, area, typeName));

            foreach (var file in files)
            {
                DateTime fileDatetime = DateTime.ParseExact(Path.GetFileNameWithoutExtension(file), "yyyy-MM-dd", 
                    CultureInfo.InvariantCulture);
                TimeSpan fileTimeSpan = fileDatetime.Subtract(new DateTime());
                TimeSpan beginTimeSpan = datetimeBegin.Subtract(new DateTime());
                TimeSpan endTimeSpan = datetimeEnd.Subtract(new DateTime());

                if (fileTimeSpan.TotalDays >= beginTimeSpan.TotalDays && fileTimeSpan.TotalDays <= endTimeSpan.TotalDays)
                {
                    var lines = File.ReadAllLines(file);
                    var fileData = lines.Select(x => x.Split(','))
                        .Select(x => (DateTime.ParseExact(x[0], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),double.Parse(x[1].Trim())))
                        .Where(x => x.Item1 >= datetimeBegin && x.Item1 <= datetimeEnd)
                        .OrderBy(x => x.Item1).ToList();

                    result.AddRange(fileData);
                }
            }

            result.OrderBy(x => x.Item1);
            return result;
        }
        
        public string AppPath
        {
            get => AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
