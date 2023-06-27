using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.DbContexts
{
    [Table("servergroup")]
    public class ServerGroup
    {
        [Key]
        [Column("ServerGroupID")]
        public ushort ID { get; set; }

        [Column("GroupName")]
        public string? ServerGroupName { get; set; }

        public virtual IEnumerable<Server> Servers { get; set; }
    }


}
