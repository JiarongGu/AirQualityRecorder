using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace AirQualityRecorder.Modules
{
    public class ChartSeriesLoader
    {
        private SeriesChartType _seriesType;
        private DataStore _dataStore;
        private Dictionary<string, string> _dataTypes;
        private string _area;

        private string _title;
        
        public ChartSeriesLoader(DataStore dataStore, SeriesChartType seriesType,
            string area, Dictionary<string,string> dataTypes, string title)
            : this(dataStore, seriesType, area, dataTypes)
        {
            _title = title;
        }

        public ChartSeriesLoader(DataStore dataStore, SeriesChartType seriesType, string area, Dictionary<string, string> dataTypes)
            :this(seriesType)
        {
            _title = area;
            _area = area;
            _dataStore = dataStore;
            _dataTypes = dataTypes;
        }

        private ChartSeriesLoader(SeriesChartType seriesType)
        {
            _seriesType = seriesType;
        }
        
        public List<Series> GenerateSeries(DateTime datetimeBegin, DateTime datetimeEnd)
        {
            var series = new List<Series>();

            foreach (var dataType in _dataTypes)
            {
                series.Add(GenerateSeries(dataType, datetimeBegin, datetimeEnd));
            }

            series.ForEach(x => x.ChartArea = _title);

            return series;
        }

        public Series GenerateSeries(KeyValuePair<string, string> dataType, DateTime datetimeBegin, DateTime datetimeEnd)
        {
            Series series = new Series($"{_title}_{_area}_{dataType.Key}");
            series.ChartType = _seriesType;
            series.XValueType = ChartValueType.DateTime;

            foreach (var data in _dataStore.ReadData(datetimeBegin, datetimeEnd, _area, dataType.Key))
            {
                series.Points.AddXY(data.Item1, data.Item2);
            }
            return series;
        }
    }
}
