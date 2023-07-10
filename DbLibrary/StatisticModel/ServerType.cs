using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.StatisticModel
{
    [Table("servertype")]
    public class ServerType
    {
        [Key]
        [Column("ServerTypeID")]
        public byte ID { get; set; }

        [Column("Type")]
        public string? Type { get; set; }

        public virtual ICollection<Server> Servers { get; set; }
    }
}
