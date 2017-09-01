using System;

namespace fire_business_soe.Extensions
{
    public static class DoubleExtensions
    {
        public static double InAcres(this double value)
        {
            try
            {
                return Math.Round(value*0.00024710538187021526, 6);
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }
}