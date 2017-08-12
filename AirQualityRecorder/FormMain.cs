using AirQualityRecorder.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace AirQualityRecorder
{
    public partial class FormMain : Form
    {
        private ConfigurationLoader _configurationLoader;
        
        private DataStore _dataStore;

        public FormMain()
        {
            InitializeComponent();

            Initialize();
        }

        public void Initialize()
        {
            _configurationLoader = new ConfigurationLoader();
            _dataStore = new DataStore();

            _configurationLoader.OnAreasUpdated += InvokeUpdateAreas;
            _configurationLoader.OnTypesUpdated += InvokeUpdateTypes;
            _configurationLoader.OnLogoUpdated += InvokeUpdateLogo;
            _configurationLoader.OnBaudsUpdated += InvokeUpdateBauds;
            _configurationLoader.OnPortsUpdated += InvokeUpdatePorts;

            InvokeUpdateAreas(_configurationLoader);
            InvokeUpdateTypes(_configurationLoader);
            InvokeUpdateLogo(_configurationLoader);
            InvokeUpdateBauds(_configurationLoader);
            InvokeUpdatePorts(_configurationLoader);

            _configurationLoader.StartAutoReload();
        }
        
        public void InvokeUpdateAreas(ConfigurationLoader configLoader)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { UpdateAreas(configLoader); });
            }
            else
            {
                UpdateAreas(configLoader);
            }
        }

        public void UpdateAreas(ConfigurationLoader configLoader)
        {
            comboBoxInputAreas.Items.Clear();
            comboBoxDataCollectionAreas.Items.Clear();
            comboBoxChartAreas.Items.Clear();

            var areas = configLoader.Areas.ToArray();
            
            comboBoxInputAreas.Items.AddRange(areas);
            comboBoxDataCollectionAreas.Items.AddRange(areas);
            comboBoxChartAreas.Items.AddRange(areas);

            if (areas.Length > 0)
            {
                comboBoxInputAreas.SelectedItem = comboBoxInputAreas.Items[0];
                comboBoxDataCollectionAreas.SelectedItem = comboBoxDataCollectionAreas.Items[0];
                comboBoxChartAreas.SelectedItem = comboBoxChartAreas.Items[0];
            }
        }

        public void InvokeUpdateTypes(ConfigurationLoader configLoader)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { UpdateTypes(configLoader); });
            }
            else
            {
                UpdateTypes(configLoader);
            }
        }

        public void UpdateTypes(ConfigurationLoader configLoader)
        {
            tableLayoutPanelInputTypes.RowCount = 0;
            tableLayoutPanelInputTypes.RowStyles.Clear();
            tableLayoutPanelInputTypes.Controls.Clear();

            var types = configLoader.Types.ToArray();

            foreach (var type in types)
            {
                var row = tableLayoutPanelInputTypes.RowCount;

                tableLayoutPanelInputTypes.RowCount++;
                tableLayoutPanelInputTypes.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

                var inputControl = CreateTypeInput(type.Key, type.Value);

                tableLayoutPanelInputTypes.Controls.Add(inputControl.Item1, 1, row);
                tableLayoutPanelInputTypes.Controls.Add(inputControl.Item2, 2, row);
                tableLayoutPanelInputTypes.Controls.Add(inputControl.Item3, 3, row);
                tableLayoutPanelInputTypes.Controls.Add(inputControl.Item4, 4, row);
            }

            tableLayoutPanelInputTypes.RowCount++;
            tableLayoutPanelInputTypes.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        }

        public void InvokeUpdateBauds(ConfigurationLoader configLoader)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { UpdateBauds(configLoader); });
            }
            else
            {
                UpdateBauds(configLoader);
            }
        }

        public void UpdateBauds(ConfigurationLoader configLoader)
        {
            comboBoxBauds.Items.Clear();

            var bauds = configLoader.Bauds.ToArray();

            comboBoxBauds.Items.AddRange(bauds);

            if (bauds.Length > 0)
            {
                comboBoxBauds.SelectedItem = comboBoxBauds.Items[0];
            }
        }

        public void InvokeUpdatePorts(ConfigurationLoader configLoader)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { UpdatePorts(configLoader); });
            }
            else
            {
                UpdatePorts(configLoader);
            }
        }

        public void UpdatePorts(ConfigurationLoader configLoader)
        {
            comboBoxPorts.Items.Clear();

            var ports = configLoader.Ports.ToArray();

            comboBoxPorts.Items.AddRange(ports);

            if (ports.Length > 0)
            {
                comboBoxPorts.SelectedItem = comboBoxPorts.Items[0];
            }
        }

        public void InvokeUpdateLogo(ConfigurationLoader configLoader)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { UpdateLogo(configLoader); });
            }
            else
            {
                UpdateLogo(configLoader);
            }
        }

        public void UpdateLogo(ConfigurationLoader configLoader)
        {
            pictureBoxLogo.ImageLocation = _configurationLoader.Logo;
        }

        public (Label, Label, TextBox, Button) CreateTypeInput(string typeName, string typeUnit)
        {
            var labelName = new Label()
            {
                Text = typeName,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var labelUnit = new Label()
            {
                Text = typeUnit,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var textBox = new TextBox()
            {
                Dock = DockStyle.Fill,
                TextAlign = HorizontalAlignment.Right,
                Text = "0"
            };

            var button = new Button()
            {
                Text = "保存",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            button.Click += (sender, e) => { SaveTypeInput(comboBoxInputAreas, typeName, textBox); };

            textBox.TextChanged += (sender, e) => {
                double value;
                if (textBox.Text == "" || double.TryParse(textBox.Text, out value))
                {
                    if (textBox.Text == "") textBox.Text = "0";
                    textBox.Tag = textBox.Text;
                }
                else
                {
                    textBox.Text = (string)textBox.Tag;
                }
            };

            return (labelName, labelUnit, textBox, button);
        }

        public void SaveTypeInput(ComboBox areaComboBox, string typeName, TextBox textBox)
        {
            _dataStore.WriteData(double.Parse(textBox.Text), dateTimePickerInput.Value, (string)areaComboBox.SelectedItem, typeName);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            _configurationLoader.StopAutoReload();
        }
        
        private void buttonChartRefresh_Click(object sender, EventArgs e)
        {
            RefreshChart();
        }

        private void RefreshChart()
        {
            chartMain.ChartAreas.Clear();
            chartMain.Series.Clear();

            foreach(var chart in _configurationLoader.Charts)
            {
                AddChartArea(chart, (string)comboBoxChartAreas.SelectedItem);
            }
        }

        private void AddChartArea(ChartAreaConfig chartConfig, string area)
        {
            var chartAreaLoader = new ChartAreaLoader(chartConfig, area);

            var chart = chartAreaLoader.GenerateChartArea(dateTimePickerChartBegin.Value, dateTimePickerChartEnd.Value);

            chartMain.ChartAreas.Add(chart.Item1);

            foreach (var series in chart.Item2)
            {
                chartMain.Series.Add(series);
            }
        }
    }
}
