using System.ComponentModel.DataAnnotations.Schema;

namespace IndexInfo.Models
{
    [Table("TenantCompanyMapping")]
    public class TenantCompanyMapping
    {
        public int id { get; set; }
        public string TenantId { get; set; }
        public int companyid { get; set; }
        public int? Active { get; set; }
    }
}
