using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockInformationV2
{
    public class AgregatedData
    {
        private readonly int TotalDataCount;
        public double Min { get; set; }
        public double Max { get; set; }

        public List<double> Data { get; set; }

        public string RangeLabel =>
            Min + " - " + Max;

        public double ClassMark =>
            Data.Count > 0 ? Data.Average() : 0;
        public int Frequency => Data.Count();
        public double RelativeFrequency =>
            Math.Round(Convert.ToDouble(Frequency) / Convert.ToDouble(TotalDataCount), 2);
        public string Percentage =>
            (RelativeFrequency * 100) + "%";

        public double AccumulatedFrecuency { get; set; }
        public double AccumulatedRelativeFrecuency { get; set; }
        public AgregatedData(int totalDataCount,
            List<double> data)
        {
            TotalDataCount = totalDataCount;
            Data = data;
        }
    }

    public class AgregatedDataCollection : ICollection<AgregatedData>
    {
        public int Count => this.Count;

        public bool IsReadOnly => this.IsReadOnly;

        public void Add(AgregatedData item)
        {
            this.Add(item);
        }

        public void Clear()
        {
            this.Clear();
        }

        public bool Contains(AgregatedData item)
        {
            return this.Contains(item);
        }

        public void CopyTo(AgregatedData[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<AgregatedData> GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool Remove(AgregatedData item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
