using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;

namespace HighwaysEngland.Callouts
{
    [CalloutInfo("deadAnimals", CalloutProbability.Always)]
    public class deadAnimals : Callout
    {
        public override bool OnBeforeCalloutDisplayed()
        {
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
        }

        public override void End()
        {
            base.End();
        }
    }
}
