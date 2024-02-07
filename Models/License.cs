using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndexInfo.Models
{
    [Table("License")]
    public class License
    {
        [Key]
        public int Id { get; set; }
        public string ProductID { get; set; }
        public string TenantId { get; set; }
        public int ValidDays { get; set; }
        public int Utilization { get; set; }
        public string LicenseType { get; set; }
        public string Module { get; set; }
        public DateTime GenarationOn { get; set; }
        public DateTime ExpiredOn { get; set; }
        //public bool IsActive { get; set; }
        public string? Domain { get; set; }
    }
}
    