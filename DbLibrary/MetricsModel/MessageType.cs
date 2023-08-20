using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLibrary.MetricsModel
{
    public enum MessageType : ushort
    {
        None,
        OnlineStat,
        BestOf20Stat,
        MembersTrackedByRole
    }
}
