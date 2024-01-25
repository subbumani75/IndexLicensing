using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndexInfo.Models
{
    [Table("ModuleMaster")]
    public class ModuleMaster
    {
        [Key]
        public int Id { get; set; }
        public string ModuleName { get; set; }

        [NotMapped]
        public string ModuleNameBind { get; set; }

        [NotMapped]
        public string CompanyNameBind { get; set; }

        [NotMapped]
        public string TenantBind { get; set; }
    }
}
