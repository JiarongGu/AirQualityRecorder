using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace AirQualityRecorder.Modules
{
    public class ChartAreaConfig
    {
        public string Title { get; set; }

        public SeriesChartType ChartType { get; set; }
        
        public DateTimeIntervalType IntervalType { get; set; }

        public Dictionary<string, string> DataTypes { get; set; }
    }
}
