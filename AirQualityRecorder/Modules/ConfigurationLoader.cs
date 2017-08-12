using AirQualityRecorder.Untils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace AirQualityRecorder.Modules
{
    public class ConfigurationLoader
    {
        private const string AREA_CONFIGURATION_PATH = "Config/Areas.txt";
        private const string TYPE_CONFIGURATION_PATH = "Config/Types.txt";
        private const string BAUD_CONFIGURATION_PATH = "Config/Bauds.txt";
        private const string PORT_CONFIGURATION_PATH = "Config/Ports.txt";
        private const string Chart_CONFIGURATION_PATH = "Config/Charts.txt";

        private const string LOGO_IMAGE_PATH = "Resource/Logo.";
        private const string LOGO_IMAGE_TYPES = "jpg,png,jpeg,bmp";

        private FileIO _areasAccess;
        private FileIO _typesAccess;
        private FileIO _logoAccess;
        private FileIO _baudsAccess;
        private FileIO _portsAccess;
        private FileIO _chartsAccess;

        public delegate void AreasUpdateHandler(ConfigurationLoader configLoader);
        public delegate void TypesUpdateHandler(ConfigurationLoader configLoader);
        public delegate void LogoUpdateHandler(ConfigurationLoader configLoader);
        public delegate void BaudsUpdateHandler(ConfigurationLoader configLoader);
        public delegate void PortsUpdateHandler(ConfigurationLoader configLoader);

        public ConfigurationLoader()
        {
            _areasAccess = new FileIO(Path.Combine(AppPath, AREA_CONFIGURATION_PATH));
            _areasAccess.OnFileUpdated += AreasAccessUpdated;

            _typesAccess = new FileIO(Path.Combine(AppPath, TYPE_CONFIGURATION_PATH));
            _typesAccess.OnFileUpdated += TypesAccessUpdated;

            _logoAccess = new FileIO(GetLogoLocation());
            _logoAccess.OnFileUpdated += LogoAccessUpdated;

            _baudsAccess = new FileIO(Path.Combine(AppPath, BAUD_CONFIGURATION_PATH));
            _baudsAccess.OnFileUpdated += BaudsAccessUpdated;

            _portsAccess = new FileIO(Path.Combine(AppPath, PORT_CONFIGURATION_PATH));
            _portsAccess.OnFileUpdated += PortsAccessUpdated;

            _chartsAccess = new FileIO(Path.Combine(AppPath, Chart_CONFIGURATION_PATH));
        }

        public List<string> Areas
        {
            get => _areasAccess.ReadAllLines().Select(x => x.Trim()).ToList();
        }

        public Dictionary<string, string> Types
        {
            get => _typesAccess.ReadAllLines()
                .Select(x => x.Split(',').Concat(new List<string>() { "" }))
                .Select(y => new { Type = y.ElementAt(0).Trim(), Unit = y.ElementAt(1).Trim()})
                .ToDictionary(x => x.Type, x => x.Unit);
        }

        public List<string> Bauds
        {
            get => _baudsAccess.ReadAllLines().Select(x => x.Trim()).ToList();
        }

        public List<string> Ports
        {
            get => _portsAccess.ReadAllLines().Select(x => x.Trim()).ToList();
        }

        public List<ChartAreaConfig> Charts
        {
            get {
                var chartAreaConfigs = new List<ChartAreaConfig>();
                var chartConfigLines = _chartsAccess.ReadAllLines();
                var types = Types;

                foreach (var configLine in chartConfigLines)
                {
                    string[] partsDelimilator = { "=>" };
                    var configParts = configLine.Split(partsDelimilator, StringSplitOptions.None);
                    var chartConfigParts = configParts[0].Trim().Split(',').Select(x => x.Trim()).ToList();
                    var typeConfigParts = configParts[1].Trim().Split(',').Select(x => x.Trim()).ToList();

                    var chartAreaConfig = new ChartAreaConfig();
                    chartAreaConfig.Title = chartConfigParts[0];
                    chartAreaConfig.ChartType = (SeriesChartType)Enum.Parse(typeof(SeriesChartType), chartConfigParts[1]);
                    chartAreaConfig.IntervalType = (DateTimeIntervalType)Enum.Parse(typeof(DateTimeIntervalType), chartConfigParts[2]);
                    chartAreaConfig.DataTypes = types.Where(x => typeConfigParts.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

                    chartAreaConfigs.Add(chartAreaConfig);
                }

                return chartAreaConfigs;
            }
        }

        public string Logo
        {
            get {
                var filePath = GetLogoLocation();
                return new FileInfo(filePath).Exists ? filePath : "";
            }
        }

        private string GetLogoLocation()
        {
            return Path.Combine(AppPath, LOGO_IMAGE_PATH + LOGO_IMAGE_TYPES.Split(',')
                    .Where(x => new FileInfo(Path.Combine(AppPath, LOGO_IMAGE_PATH + x)).Exists)
                    .FirstOrDefault()??"");
        }

        public void StartAutoReload()
        {
            _areasAccess.StartContinuousCheckUpdates();
            _typesAccess.StartContinuousCheckUpdates();
            _logoAccess.StartContinuousCheckUpdates();
            _baudsAccess.StartContinuousCheckUpdates();
            _portsAccess.StartContinuousCheckUpdates();
        }

        public void StopAutoReload()
        {
            _areasAccess.StopContinuousCheckUpdates();
            _typesAccess.StopContinuousCheckUpdates();
            _logoAccess.StopContinuousCheckUpdates();
            _baudsAccess.StopContinuousCheckUpdates();
            _portsAccess.StopContinuousCheckUpdates();
        }

        public void AreasAccessUpdated(object areasAccess)
        {
            OnAreasUpdated(this);
        }

        public void TypesAccessUpdated(object typesAccess)
        {
            _logoAccess.FilePath = GetLogoLocation();
            OnTypesUpdated(this);
        }

        public void LogoAccessUpdated(object logoAccess)
        {
            OnLogoUpdated(this);
        }

        public void BaudsAccessUpdated(object baudsAccess)
        {
            OnBaudsUpdated(this);
        }

        public void PortsAccessUpdated(object portsAccess)
        {
            OnPortsUpdated(this);
        }

        public string AppPath {
            get => AppDomain.CurrentDomain.BaseDirectory;
        }

        public event AreasUpdateHandler OnAreasUpdated;

        public event TypesUpdateHandler OnTypesUpdated;

        public event LogoUpdateHandler OnLogoUpdated;

        public event BaudsUpdateHandler OnBaudsUpdated;

        public event PortsUpdateHandler OnPortsUpdated;
    }
}
