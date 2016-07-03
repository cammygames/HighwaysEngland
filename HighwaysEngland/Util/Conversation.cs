using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighwaysEngland.Util
{
    public static class Conversation
    {
        public static Dictionary<string, List<string>> conversations = new Dictionary<string, List<string>>()
        {
            {"RTC_1_1", new List<string>()
                {
                    "~b~Officer: ~w~Hello Sir/Madam I just need to check if you are injured before we can sort this mess out.",
                    "~o~Driver: ~w~I feel fine thank you officer i dont feel any pain anywhere.",
                    "~b~Officer: ~w~Ok Sir/Madam do you know what happend here to cause this RTC?",
                    "~o~Driver: ~w~Nope.exe",
                }
            },
            {"RTC_1_2", new List<string>()
                {
                    "~b~Officer: ~w~Hello Sir/Madam I just need to check if you are injured before we can sort this mess out.",
                    "~o~Driver: ~w~I feel fine thank you officer i dont feel any pain anywhere.",
                    "~b~Officer: ~w~Ok Sir/Madam do you know what happend here to cause this RTC?",
                    "~o~Driver: ~w~Nope.dexe",
                }
            }
        };
    }
}
