using StockTrader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using MathNet.Numerics.Statistics;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StockInformationV2
{
    public partial class Form2 : Form
    {
        readonly StockTrending TrendingData;
        double Mean, RangeAmplitude;
        string StockName;
        List<AgregatedData> AggregatedData;
        int IntervalCount;
        List<double> NumericData;
        public Form2(StockTrending trendingData, string stockName)
        {
            InitializeComponent();
            TrendingData = trendingData;
            StockName = stockName;
            MathStuff();
            CreateGraph();
        }

        private void NavigateToFrecuencyData(object sender, EventArgs e)
        {
            Form3 form3 = new Form3(RangeAmplitude, AggregatedData, IntervalCount, TrendingData.OrderedData.Count, NumericData);
            form3.ShowDialog();
        }

        private void CreateGraph()
        {
            SetItemsSize();
            foreach (var item in TrendingData.OrderedData)
            {
                chart1.Series[0].Points.AddXY(item.Key, item.Value);
                var point = chart1.Series[0].Points.LastOrDefault();
                if (item.Value > Mean)
                {
                    point.Color = Color.Green;
                }
                else
                {
                    point.Color = Color.Red;
                }
            }


            chart1.Series.Add("TrendLine");
            chart1.Series["TrendLine"].ChartType = SeriesChartType.Line;
            chart1.Series["TrendLine"].BorderWidth = 3;
            chart1.Series["TrendLine"].Color = Color.Red;
            // Line of best fit is linear
            string typeRegression = "Linear";//"Exponential";//
                                             // The number of days for Forecasting
            string forecasting = "1";
            // Show Error as a range chart.
            string error = "false";
            // Show Forecasting Error as a range chart.
            string forecastingError = "false";
            // Formula parameters
            string parameters = typeRegression + ',' + forecasting + ',' + error + ',' + forecastingError;
            chart1.Series[0].Sort(PointSortOrder.Ascending, "X");

            StripLine stripline = new StripLine();
            stripline.Interval = 0;
            stripline.IntervalOffset = Mean;
            stripline.StripWidth = 1;
            stripline.BackColor = Color.Blue;
            chart1.ChartAreas[0].AxisY.StripLines.Add(stripline);
            //chart1.Series["Media"].Points.AddY(Mean);
            //chart1.Series[0].Sort(PointSortOrder.Ascending, "X");
            // Create Forecasting Series.
            //chart1.DataManipulator.FinancialFormula(FinancialFormula.MedianPrice, parameters, chart1.Series[0], chart1.Series["Median"]);
            chart1.DataManipulator.FinancialFormula(FinancialFormula.Forecasting, parameters, chart1.Series[0], chart1.Series["TrendLine"]);


            this.WindowState = FormWindowState.Maximized;
        }

        private void SetItemsSize()
        {
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            int w = Convert.ToInt32(screen.Width * 0.8);
            int h = Convert.ToInt32(screen.Height * 0.9);
            chart1.Size = new Size(w, h);
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            listView1.Size = new Size(screen.Width - w, h);
            listView1.Location = new Point(w, 42);
        }

        private void MathStuff()
        {
            AggregatedData = AggregateDataSet();
            var statistics = new DescriptiveStatistics(TrendingData.OrderedData.Select(x => x.Value));

            var largestElement = statistics.Maximum;
            var smallestElement = statistics.Minimum;
            var median = TrendingData.OrderedData.Select(x => x.Value).ToArray().Median();

            Mean = Math.Round(statistics.Mean, 4);
            var variance = Math.Round(statistics.Variance, 4);
            var stdDev = Math.Round(statistics.StandardDeviation, 4);

            var kurtosis = Math.Round(statistics.Kurtosis, 4);
            var skewness = Math.Round(statistics.Skewness, 4);

            var mode = GetMode(TrendingData.OrderedData.Select(x => x.Value).ToArray());

            listView1.Items.Clear();
            listView1.Items.Add($"Stock: {StockName}");
            listView1.Items.Add($"Mínimo: {smallestElement}");
            listView1.Items.Add($"Máximo: {largestElement}");
            listView1.Items.Add($"Mediana: {median}");
            listView1.Items.Add($"Media: {Mean}");
            listView1.Items.Add($"Moda: {mode.FirstOrDefault().Key} ({mode.FirstOrDefault().Value})");
            listView1.Items.Add($"Varianza: {variance}");
            listView1.Items.Add($"Desviacion estándar: {stdDev}");
            listView1.Items.Add($"Curtosis: {kurtosis}");
            listView1.Items.Add($"Sesgo: {skewness}");
            var lastRecord = TrendingData.OrderedData.FirstOrDefault();
            listView1.Items.Add($"Último valor: {lastRecord.Value}");
            listView1.Items.Add($"Último registro: {lastRecord.Key.ToString("dd/MMM/yyyy")}");
            listView1.Items.Add($"Pendiente: {TrendingData.PricingTrend.Slope * -1}");
            listView1.Items.Add($"Rango superior (RL): {Math.Round(TrendingData.PricingTrend.Start, 4)}");
            listView1.Items.Add($"Rango inferior (RL): {Math.Round(TrendingData.PricingTrend.End, 4)}");
        }

        private List<AgregatedData> AggregateDataSet()
        {
            IntervalCount = Convert.ToInt32(Math.Round(1 + (3.35 * Math.Log(TrendingData.OrderedData.Count))));
            List<double> data = TrendingData.OrderedData.Select(x => x.Value).ToList();
            var max = data.Max();
            var min = data.Min();
            RangeAmplitude = Math.Round((max - min) / IntervalCount, 3) + 0.1;
            double currentMin = data.Min();
            double currentMax = data.Min() + RangeAmplitude;
            List<AgregatedData> dataOutput = new List<AgregatedData>();
            for (double i = 1; i <= IntervalCount; i++)
            {
                var relevantData = data.Where(x => x >= currentMin && x < currentMax).ToList();
                dataOutput.Add(new AgregatedData(data.Count, relevantData) { Min = relevantData.Count > 0 ? relevantData.Min() : currentMin, Max = relevantData.Count > 0 ? relevantData.Max() : currentMin });
                currentMin = currentMax;
                currentMax = currentMin + RangeAmplitude;
            }
            NumericData = data;
            return dataOutput.OrderBy(x => x.Min).ToList();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        public static Dictionary<double, int> GetMode(double[] arrSource)
        {
            // Check if the array has values        
            var most = arrSource.GroupBy(i => i).OrderByDescending(grp => grp.Count())
                .Select(grp =>
                {
                    return new Dictionary<double, int>() { { grp.Key, grp.Count() } };
                }).First();

            return most;
        }
    }
}
