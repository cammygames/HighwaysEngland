using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighwaysEngland.Util
{
    class Common
    {
        public Random rand = new Random();

        public enum CalloutState
        {
            Created,
            EnRoute,
            Arrived,
            Complete
        }
    }
}
