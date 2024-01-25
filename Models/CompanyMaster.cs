using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndexInfo.Models
{
    [Table("CustomerMaster")]
    public class CompanyMaster
    {
        [Key]
        public int Id { get; set; }
        public String CustomerName { get; set; }
        public String CustomerMailId { get; set; }
        public DateTime GenarationOn { get; set; }
    }
}
