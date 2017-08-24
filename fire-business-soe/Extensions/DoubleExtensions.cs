using System;
using System.Globalization;

namespace fire_business_soe.Extensions
{
    public static class DoubleExtensions
    {
        public static string InAcres(this double value)
        {
            try
            {
                return string.Format("{0:#,###0.####} ac", Math.Round(value * 0.00024710538187021526, 4));
            }
            catch (Exception)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        } 
    }
}