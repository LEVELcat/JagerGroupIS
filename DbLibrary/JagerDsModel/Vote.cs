using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.JagerDsModel
{
    [Table("vote")]
    public class Vote
    {
        [Key]
        [Column("VoteID")]
        public uint ID { get; set; }

        [Column("ElectionID")]
        public uint ElectionID { get; set; }
        public virtual Election? Election { get; set; }

        [Column("VoteDateTime")]
        public DateTime VoteDateTime { get; set; }

        [Column("MemberID")]
        public ulong MemberID { get; set; }

        [Column("VoteValue")]
        public bool? VoteValue { get; set; }
    }

}
