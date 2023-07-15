using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLibrary.JagerDsModel
{
    [Flags]
    public enum BitMaskElection : ulong
    {
        None = 0,
        AgreeList = 1,
        RejectList = 2,
        NotVotedList = 4,
        NotificationForAgree = 8,
        NotificationForNotVoted = 16,
    }
}
