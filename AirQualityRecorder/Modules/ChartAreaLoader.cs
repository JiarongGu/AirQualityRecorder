using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace AirQualityRecorder.Modules
{
    public class ChartAreaLoader
    {
        private ChartSeriesLoader _series;
        private ChartAreaConfig _areaConfig;
        private DataStore _dataStore;

        public ChartAreaLoader(ChartAreaConfig areaConfig, string area)
        {
            _dataStore = new DataStore();
            _areaConfig = areaConfig;
            _series = new ChartSeriesLoader(_dataStore, _areaConfig.ChartType, area, _areaConfig.DataTypes, _areaConfig.Title);
        }

        public ChartSeriesLoader Series
        {
            set { _series = value; }
            get { return _series; }
        }
        
        public (ChartArea, List<Series>) GenerateChartArea(DateTime timeBegin, DateTime timeEnd)
        {
            ChartArea chartArea = new ChartArea(_areaConfig.Title);

            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;

            chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chartArea.AxisX.IntervalType = _areaConfig.IntervalType;
            chartArea.AxisX.LabelStyle.IntervalType = _areaConfig.IntervalType;
            chartArea.AxisX.LabelStyle.Interval = 1;
            chartArea.AxisX.Title = $"{_areaConfig.Title} - {string.Join(", ", _areaConfig.DataTypes.Select(x => x.Key))}";

            return (chartArea, _series.GenerateSeries(timeBegin, timeEnd));
        }
    }
}
