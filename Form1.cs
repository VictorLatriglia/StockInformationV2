using StockTrader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockInformationV2
{
    public partial class Form1 : Form
    {
        readonly FormInterface StockRetriever;
        bool YearlyData = false;
        public Form1()
        {
            InitializeComponent();
            StockRetriever = new FormInterface();
        }
        private void CheckEnter(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                button1_Click(sender, e);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(textBox1.Text))
            {
                textBox1.Text = "IBM";
            }
            textBox1.Text = textBox1.Text.ToUpper();
            var info = Task.Run(() => StockRetriever.FetchStockData(textBox1.Text));
            ShowProgress();
            info.Wait();
            HideProgress();
            if (YearlyData)
            {
                info.Result.OrderedData = info.Result.OrderedDataYearly;
                info.Result.PricingTrend = info.Result.PricingTrendYearly;
            }
            Form2 form2 = new Form2(info.Result, textBox1.Text);
            textBox1.Text = "";
            form2.ShowDialog();
        }

        private async void HideProgress()
        {
            progressBar1.Value = 0;
            progressBar1.Visible = false; // runs on UI thread
        }
        private async void ShowProgress()
        {
            progressBar1.Visible = true;
            for (int i = 0; i < 100; i++)
            {
                progressBar1.Value = i;
                Thread.Sleep(6);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            YearlyData = checkBox1.Checked;
        }
    }
}
