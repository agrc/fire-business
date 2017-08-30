namespace fire_business_soe.Models
{
    public class CriteriaFilter
    {
        public CriteriaFilter(int layerId, string whereClause)
        {
            LayerId = layerId;
            WhereClause = whereClause;
        }

        public int LayerId { get; set; }
        public string WhereClause { get; set; }
    }
}