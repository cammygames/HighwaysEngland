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
        private Ped driver1, driver2;
        private Vehicle vehicle1, vehicle2;
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

            int rand1 = rand.Next(vehicles.Length);
            vehicle1 = new Vehicle(vehicles[rand1], spawnPoint, getTrafficHeading(spawnPoint));
            setUpVeh(vehicle1, 55f, 40f, true, false); 
            vehicle1.Health = 40;
            openVehilceDoor(vehicle1, 4, false);

            int rand2 = rand.Next(vehicles.Length);
            vehicle2 = new Vehicle(vehicles[rand2], vehicle1.GetOffsetPositionFront(5f), vehicle1.Heading);
            setUpVeh(vehicle2, 200f, 100f, true, false);
            openVehilceDoor(vehicle2, 5, false);

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

            if (state == CalloutState.EnRoute && Game.LocalPlayer.Character.Position.DistanceTo(spawnPoint) <= 15 && Functions.IsCalloutRunning())
            {
                state = CalloutState.Arrived;
                startIncedent();
                if (blip.Exists()) { blip.Delete(); }
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
                callTowTruck(vehicle2, vehicle2.Position);
                callTowTruck(vehicle1, vehicle1.Position);
            }, "RTC.startIncedent");
        }

        private void callTowTruck(Vehicle vehicle, Vector3 position)
        {
            GameFiber.StartNew(delegate
            {
                Blip vehicleBlip;
                Vector3 truckSpawn = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(200f, 400f));
                Game.LogTrivial("Towtruck & Driver Spawnpoint: ~r~" + truckSpawn);

                Vehicle towTruck = new Vehicle("towtruck", truckSpawn);
                vehicleBlip = towTruck.AttachBlip();
                vehicleBlip.Color = Color.HotPink;

                Ped truckDriver = towTruck.CreateRandomDriver();
                Game.LogTrivial("Towtruck & Driver Created");

                truckDriver.Tasks.DriveToPosition(position, 30f, VehicleDrivingFlags.Normal, 15f).WaitForCompletion(25000);
                Game.LogTrivial("Driver tasked to drive to: " + vehicle.Position);

                while (true)
                {
                    GameFiber.Yield();
                    if (towTruck.DistanceTo(position) <= 15)
                    {
                        towTruck.TowVehicle(vehicle, true);
                        GameFiber.Sleep(2000);
                        Game.LogTrivial("Driver tasked to drive away to: " + truckSpawn);
                        truckDriver.Tasks.DriveToPosition(truckSpawn, 30f, VehicleDrivingFlags.Normal, 15f).WaitForCompletion(25000);
                        towTruck.Dismiss();
                        vehicleBlip.Delete();
                        break;
                    }
                }

                GameFiber.Hibernate();
            }, "RTC.callTowTruck");
        }
    }
}
