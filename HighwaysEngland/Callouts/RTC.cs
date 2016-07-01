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
        private int veh1;
        private int veh2;
        private CalloutState state;
        private Random random;

        private String[] vehicles = { "utillitruck2", "voodoo2", "baller2", "baller", "bison2", "bison3", "burrito", "pony2", "rapidgt2", "scrap", "minivan", "ninef2", "surfer", "taco" };

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(500f));
            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 30f);
            AddMinimumDistanceCheck(250f, spawnPoint);

            random = new Random();
            veh1 = random.Next(vehicles.Length);
            veh2 = random.Next(vehicles.Length);

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

            Vector3 closestVehicleNodeCoords;
            float roadheading;

            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(spawnPoint.X, spawnPoint.Y, spawnPoint.Z, out closestVehicleNodeCoords, out roadheading, 1, 3, 0);
            if (roadheading.Equals(null)) { roadheading = 180f; }
            vehicle1 = new Vehicle(vehicles[veh1], spawnPoint, roadheading);
            setUpVeh(vehicle1, 55f, 40f, true, false); 
            vehicle1.Health = 40;
            if (vehicle1.Doors[4].IsValid()) vehicle1.Doors[4].Open(false);


            vehicle2 = new Vehicle(vehicles[veh2], vehicle1.GetOffsetPositionFront(5f), vehicle1.Heading);
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


            if (!Functions.IsCalloutRunning() && state == CalloutState.Complete) End();
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
            Persona persona = Functions.GetPersonaForPed(ped);
            Functions.SetVehicleOwnerName(vehicle, persona.FullName);
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

                if (random.Next(5) >= 4)
                {
                    driver2.Tasks.FightAgainst(driver1);
                    driver1.Tasks.FightAgainst(driver2);
                    Functions.RequestBackup(vehicle1.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                }
            });
        }
    }
}
