using System;
using System.Drawing;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;
using HighwaysEngland.Util;
using System.Collections.Generic;
using System.Windows.Forms;
using static HighwaysEngland.Util.Common;

namespace HighwaysEngland.Callouts
{
    [CalloutInfo("RTC", CalloutProbability.Always)]
    public class RTC : Callout
    {
        private Ped driver1;
        private Ped driver2;
        private Vehicle vehicle1;
        private Vehicle vehicle2;
        private Blip blip;
        private Vector3 spawnPoint;
        private CalloutState state;


        public override bool OnBeforeCalloutDisplayed()
        {
            spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(500f));
            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 30f);
            AddMinimumDistanceCheck(250f, spawnPoint);

            CalloutMessage = "~o~Road Traffic Collision";
            CalloutPosition = spawnPoint;
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            blip = new Blip(spawnPoint, 30f);
            blip.Color = Color.Yellow;
            blip.EnableRoute(Color.Yellow);

            state = CalloutState.EnRoute;

            vehicle1 = new Vehicle(vehicles[rand.Next(vehicles.Length)], spawnPoint, getTrafficHeading(spawnPoint));
            setUpVeh(vehicle1, 55f, 40f, true, false); 
            vehicle1.Health = 40;
            openVehilceDoor(vehicle1, 4, false);

            vehicle2 = new Vehicle(vehicles[rand.Next(vehicles.Length)], vehicle1.GetOffsetPositionFront(5f), vehicle1.Heading);
            setUpVeh(vehicle2, 200f, 100f, true, false);

            driver1 = vehicle1.CreateRandomDriver();
            setupPed(driver1, vehicle1);

            driver2 = vehicle2.CreateRandomDriver();
            setupPed(driver2, vehicle2);

            World.AddSpeedZone(spawnPoint, 100f, 15f);

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (state == CalloutState.EnRoute && Game.LocalPlayer.Character.Position.DistanceTo(spawnPoint) <= 15)
            {
                state = CalloutState.Arrived;
                startIncedent();
            }


            if (!Functions.IsCalloutRunning()) End();
        }

        public override void End()
        {
            base.End();
            if (driver1.Exists()) { driver1.Dismiss(); }
            if (driver2.Exists()) { driver2.Dismiss(); }
            if (vehicle1.Exists()) { vehicle1.Dismiss(); }
            if (vehicle2.Exists()) { vehicle1.Dismiss(); }
            if (blip.Exists()) { blip.Delete(); }
        }

        private void setUpVeh(Vehicle vehicle, float engineHealth, float fuelTankHealth, bool isPersistant, bool isDirveable)
        {
            vehicle.IsPersistent = isPersistant;
            vehicle.IsDriveable = isDirveable;
            vehicle.EngineHealth = engineHealth;
            vehicle.FuelTankHealth = fuelTankHealth;
            vehicle.IsEngineOn = false;
        }

        private void setupPed(Ped ped, Vehicle vehicle)
        {
            linkPedToVehicle(ped, vehicle);
            ped.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(6000);
            ped.IsPersistent = true;
            ped.BlockPermanentEvents = true;
        }

        private void startIncedent()
        {
            GameFiber.StartNew(delegate
            {
                driver1.PlayAmbientSpeech("GENERIC_FUCK_YOU", true);
                GameFiber.Sleep(4500);
                driver2.PlayAmbientSpeech("GENERIC_CURSE_HIGH", true);

                if (rand.Next(5) >= 4)
                {
                    driver2.Tasks.FightAgainst(driver1);
                    driver1.Tasks.FightAgainst(driver2);
                    Functions.RequestBackup(vehicle1.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                }
            });
        }
    }
}
