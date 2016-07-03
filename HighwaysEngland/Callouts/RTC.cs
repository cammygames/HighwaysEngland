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
        private Blip blip, towBlip1, towBlip2;
        private Vector3 spawnPoint;
        private CalloutState state;
        private Ped player;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(500f));
            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 30f);
            AddMinimumDistanceCheck(250f, spawnPoint);

            CalloutMessage = "~o~Road Traffic Collision";
            CalloutPosition = spawnPoint;
            Functions.PlayScannerAudioUsingPosition("CITIZENS_REPORT_03 CIVILIAN_NEEDING_ASSISTANCE IN_OR_ON_POSITION", this.spawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            blip = new Blip(spawnPoint, 30f);
            blip.Color = Color.Yellow;
            blip.EnableRoute(Color.Yellow);

            state = CalloutState.EnRoute;
            player = Game.LocalPlayer.Character;

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

            if (state == CalloutState.EnRoute && player.Position.DistanceTo(spawnPoint) <= 15 && Functions.IsCalloutRunning())
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
            if (vehicle1.Exists()) { vehicle1.Delete(); }
            if (vehicle2.Exists()) { vehicle2.Delete(); }
            if (blip.Exists()) { blip.Delete(); }
            if (towBlip1.Exists()) { towBlip1.Delete(); }
            if (towBlip2.Exists()) { towBlip2.Delete(); }
        }

        private void setUpVeh(Vehicle vehicle, float engineHealth, float fuelTankHealth, bool isPersistant, bool isDirveable)
        {
            Game.LogTrivial("Called setupVehicle.");
            vehicle.IsPersistent = isPersistant;
            vehicle.IsDriveable = isDirveable;
            vehicle.EngineHealth = engineHealth;
            vehicle.FuelTankHealth = fuelTankHealth;
            vehicle.IsEngineOn = false;
            Game.LogTrivial("Finished setupVehicle.");
        }

        private void setupPed(Ped ped, Vehicle vehicle)
        {
            Game.LogTrivial("Called setupPed.");
            linkPedToVehicle(ped, vehicle);
            ped.IsPersistent = true;
            ped.BlockPermanentEvents = true;
            ped.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(6000);
            Game.LogTrivial("Finished setupPed.");
        }

        private void startIncedent()
        {
            Game.LogTrivial("Called startIncedent Starting new Fiber.");
            GameFiber.StartNew(delegate
            {
                bool talkedToD1 = false, talkedToD2 = false, recoverdVehicle1 = false, recoverdVehicle2 = false;
                List<String>.Enumerator currentConv1, currentConv2;

                while (true && Functions.IsCalloutRunning())
                {
                    GameFiber.Yield();
                    if (player.Position.DistanceTo(driver1) <= 4 && !talkedToD1)
                    {
                        currentConv1 = Conversation.conversations["RTC_1_1"].GetEnumerator();
                        Game.DisplaySubtitle("Hold ~b~T ~w~to talk to the driver.");
                        while (Game.IsKeyDownRightNow(Keys.T))
                        {
                            Game.LogTrivial("RTC.startIncedent Fiber: Player is talking to NPC.");
                            GameFiber.Yield();
                            if (currentConv1.MoveNext())
                            {
                                Game.DisplaySubtitle(currentConv1.Current, 2000);
                                GameFiber.Sleep(3000);
                            } 
                            else
                            {
                                Game.DisplaySubtitle("~b~Officer: ~w~Thanks for your cooperation, Ill get a tow truck down here to take your vehicle.");
                                talkedToD1 = true;
                                Game.LogTrivial("RTC.startIncedent Fiber: Player finished conversation, calling tow truck.");
                                towBlip1 = callTowTruck(vehicle1, vehicle1.Position, true);
                            }
                        }
                    }
                    else if (player.Position.DistanceTo(driver2) <= 4 && !talkedToD2)
                    {
                        currentConv2 = Conversation.conversations["RTC_1_2"].GetEnumerator();
                        Game.DisplaySubtitle("Hold ~b~T ~w~to talk to the driver.");
                        while (Game.IsKeyDownRightNow(Keys.T))
                        {
                            Game.LogTrivial("RTC.startIncedent Fiber: Player is talking to NPC.");
                            GameFiber.Yield();
                            if (currentConv2.MoveNext())
                            {
                                Game.DisplaySubtitle(currentConv2.Current, 2000);
                                GameFiber.Sleep(3000);
                            }
                            else
                            {
                                Game.DisplaySubtitle("~b~Officer: ~w~Thanks for your cooperation, Ill get a tow truck down here to take your vehicle.");
                                talkedToD2 = true;
                                Game.LogTrivial("RTC.startIncedent Fiber: Player finished conversation, calling tow truck.");
                                towBlip2 = callTowTruck(vehicle2, vehicle2.Position, true);
                            }
                        }
                    }
                    else if (talkedToD1 & talkedToD2 && vehicle1.Exists() && vehicle2.Exists())
                    {
                        if (vehicle1.FindTowTruck() != null) recoverdVehicle1 = true;
                        if (vehicle2.FindTowTruck() != null) recoverdVehicle2 = true;
                    }
                    else if (talkedToD1 && recoverdVehicle1 && talkedToD2 && recoverdVehicle2)
                    {
                        Game.LogTrivial("RTC.startIncedent Fiber: Both vehicles and drivers sorted, finish & cleanup.");
                        Game.DisplaySubtitle("~b~Officer: ~w~Thank you both for your cooperation, you can be on your way now.");
                        GameFiber.Sleep(5000);
                        End();
                        GameFiber.Hibernate();
                    }
                }

            }, "RTC.startIncedent");
        }
    }
}
