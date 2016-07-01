using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;

namespace HighwaysEngland
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedEventHandler;
            Game.LogTrivial("Plguin Highways England Callouts " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " by Maurice Moss has been initialised.");
        }
        public override void Finally()
        {
            Game.LogTrivial("Plguin Highways England Callouts " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " plugin cleaned up.");
        }

        private static void OnOnDutyStateChangedEventHandler(bool OnDuty)
        {
            if (OnDuty)
            {  
                RegisterCallouts();
            }
        }

        private static void RegisterCallouts()
        {
            Functions.RegisterCallout(typeof(Callouts.RTC));
            Game.DisplayNotification("~o~Highways England Callouts ~g~V" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " ~w~by ~b~Maurice Moss ~w~has been initialised.");
        }
    }
}
