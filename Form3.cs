using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.Statistics;
using System.Windows.Forms.DataVisualization.Charting;

namespace StockInformationV2
{
    public partial class Form3 : Form
    {
        double RangeAmplitude;
        List<AgregatedData> AggregatedData;
        List<double> CrudeData;
        int IntervalCount, TotalDataCount;
        double Mean, Mode, Median, Q1,Q3;
        bool VisibleCharts = true, Chart1Visible = true;
        public Form3(double rangeAmplitude,
            List<AgregatedData> aggregatedData,
            int intervalCount,
            int totalDataCount,
            List<double> crudeData)
        {
            InitializeComponent();

            IntervalCount = intervalCount;
            RangeAmplitude = rangeAmplitude;
            AggregatedData = aggregatedData;
            TotalDataCount = totalDataCount;
            CrudeData = crudeData;
            //Construct the datagrid
            dataGridView1.ColumnCount = 6;
            dataGridView1.Columns[0].Name = "Rango";
            dataGridView1.Columns[1].Name = "Frecuencia";
            dataGridView1.Columns[2].Name = "FrecuenciaRelativa";
            dataGridView1.Columns[3].Name = "Porcentaje";
            dataGridView1.Columns[4].Name = "FrecuenciaAcumulada";
            dataGridView1.Columns[5].Name = "FrecuenciaRelativaAcumulada";

            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            MathStuff();
            CreateBoxPlot();
            CreateGraphData();
            CalculateTableData();
            SetItemsSize();
        }

        private void MathStuff()
        {
            Mean = Math.Round((AggregatedData.Sum(x => x.Frequency * x.ClassMark)) / TotalDataCount, 3);

            CalculateMode();
            CalculateMedian();
            
        }

        private void CreateBoxPlot()
        {
            Q1 = CrudeData.Where(x => x < Median).Median();
            Q3 = CrudeData.Where(x => x > Median).Median();
            chart2.Series[0].Points.AddXY(0,
                        CrudeData.Min(),
                        CrudeData.Max(),
                        Q1,
                        Q3,
                        CrudeData.Average(),
                        Median);
            SetItemsList();
        }

        private void SetItemsList()
        {
            listView1.Items.Clear();
            listView1.Items.Add($"Promedio: {Mean}");
            listView1.Items.Add($"Moda: {Mode}");
            listView1.Items.Add($"Mediana: {Median}");
            listView1.Items.Add($"Q1: {Q1}");
            listView1.Items.Add($"Q3: {Q3}");
        }

        private void CalculateMode()
        {
            var mostCommonRange = AggregatedData.FirstOrDefault(x => x.Frequency == AggregatedData.Max(y => y.Frequency));
            var indexMCR = AggregatedData.IndexOf(mostCommonRange);
            var belowMCR = indexMCR > 0 ? AggregatedData[indexMCR - 1] : null;
            var aboveMCR = indexMCR < AggregatedData.Count ? AggregatedData[indexMCR + 1] : null;
            Mode = Math.Round(mostCommonRange.Min + (calculateNumerator() / calculateDenominator()) * RangeAmplitude, 3);
            double calculateNumerator()
            {
                return mostCommonRange.Frequency - belowMCR?.Frequency ?? 0;
            }
            double calculateDenominator()
            {
                var below = (mostCommonRange.Frequency - belowMCR?.Frequency ?? 0);
                var above = (mostCommonRange.Frequency - aboveMCR?.Frequency ?? 0);
                return below + above;
            }
        }

        private void CalculateMedian()
        {
            var count = 0;
            AgregatedData medianRange = null;
            foreach (var item in AggregatedData)
            {
                count += item.Frequency;
                if (count > (TotalDataCount / 2))
                {
                    medianRange = item;
                    break;
                }
            }
            var indexMDR = AggregatedData.IndexOf(medianRange);
            var belowMDR = indexMDR > 0 ? AggregatedData[indexMDR - 1] : null;
            Median = Math.Round(medianRange.Min + (CalculateNumerator() / medianRange.Frequency) * RangeAmplitude, 3);
            double CalculateNumerator()
            {
                return (TotalDataCount / 2) - belowMDR?.AccumulatedFrecuency ?? 0;
            }
        }

        private void CreateGraphData()
        {
            foreach (var item in AggregatedData)
            {
                chart1.Series[0].Points.AddXY(item.RangeLabel, item.Frequency);

            }
            chart1.Series[0].ToolTip = "Rango: #VALX, Frecuencia: #VAL";
        }

        private void CalculateTableData()
        {
            foreach (var item in AggregatedData.OrderBy(x => x.Min))
            {
                item.AccumulatedFrecuency = AggregatedData.Where(x => x.Min <= item.Min).Sum(x => x.Frequency);
                item.AccumulatedRelativeFrecuency = AggregatedData.Where(x => x.Min <= item.Min).Sum(x => x.RelativeFrequency);
                dataGridView1.Rows.Add(item.RangeLabel,
                    item.Frequency,
                    item.RelativeFrequency,
                    item.Percentage,
                    item.AccumulatedFrecuency,
                    item.AccumulatedRelativeFrecuency);
            }

        }

        private void SetItemsSize()
        {
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            int w = Convert.ToInt32(screen.Width * 0.85);
            int h = Convert.ToInt32(screen.Height * 0.9);
            chart1.Size = new Size(w, h);
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.RangeColumn;
            chart2.Size = new Size(w, h);
            chart2.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.BoxPlot;
            button1.Location = new Point(w + 20, 42);
            listView1.Size = new Size(screen.Width - w, h);
            listView1.Location = new Point(w, 112);
            dataGridView1.Size = new Size(w, h);
            this.WindowState = FormWindowState.Maximized;
            UpdateFont();
        }
        private void UpdateFont()
        {
            //Change cell font
            foreach (DataGridViewColumn c in dataGridView1.Columns)
            {
                c.DefaultCellStyle.Font = new Font("Arial", 16F, GraphicsUnit.Pixel);
            }
        }

        private void ChangeViews(object sender, EventArgs e)
        {
            VisibleCharts = !VisibleCharts;
            UpdateInterfaces();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Chart1Visible = !Chart1Visible;
            UpdateInterfaces();
        }

        private void UpdateInterfaces()
        {
            this.chart1.Visible = VisibleCharts && Chart1Visible;
            this.chart2.Visible = VisibleCharts && !Chart1Visible;
            this.button2.Visible = VisibleCharts;
            this.dataGridView1.Visible = !VisibleCharts;
            if (VisibleCharts)
            {
                this.button1.Text = "Ver tabla";
                if (Chart1Visible)
                {
                    this.button2.Text = "D. Caja";
                }
                else
                {
                    this.button2.Text = "D. Barras";
                }
            }
            else
            {
                this.button1.Text = "Ver gráfica";
            }
        }
    }
}
