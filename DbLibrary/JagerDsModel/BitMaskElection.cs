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


        NotificationBefore_1Mounth = 9007199254740992,
        NotificationBefore_2Week = 18014398509481984,
        NotificationBefore_1Week = 36028797018963968,
        NotificationBefore_48Hour = 72057594037927936,
        NotificationBefore_24Hour = 144115188075855872,
        NotificationBefore_12Hour = 288230376151711744,
        NotificationBefore_6Hour = 576460752303423488,
        NotificationBefore_2Hour = 1152921504606846976,
        NotificationBefore_1Hour = 2305843009213693952,
        NotificationBefore_15Minutes = 4611686018427387904,
    }
}
