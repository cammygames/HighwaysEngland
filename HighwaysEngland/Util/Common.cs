using System;
using System.Drawing;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace HighwaysEngland.Util
{
    public class Common
    {
        public static Random rand = new Random();

        public enum CalloutState
        {
            Created,
            EnRoute,
            Arrived,
            Complete
        }

        public static String[] vehicles = { "utillitruck2", "voodoo2", "baller2", "baller", "bison2", "bison3", "burrito", "pony2", "rapidgt2", "scrap", "minivan", "ninef2", "surfer", "taco" };

        /// <summary>Returns the heading of nearby traffic</summary>
        /// <param name="position"> Position to check</param>    
        public static float getTrafficHeading(Vector3 position)
        {
            float trafficHeading;
            Vector3 closestVehicleNodeCoords;

            NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING(position.X, position.Y, position.Z, out closestVehicleNodeCoords, out trafficHeading, 1, 3, 0);
            if (trafficHeading.Equals(null)) { trafficHeading = 90f; }
            return trafficHeading;
        }

        /// <summary>Sets a vehicles owner name to match peds full name</summary>   
        public static void linkPedToVehicle(Ped ped, Vehicle vehicle)
        {
            Persona persona = Functions.GetPersonaForPed(ped);
            Functions.SetVehicleOwnerName(vehicle, persona.FullName);
        }

        public static void openVehilceDoor(Vehicle vehicle, int index, bool instant)
        {
            if (vehicle.Doors[index].IsValid()) vehicle.Doors[index].Open(instant);
        }

        public static bool callTowTruck(Vehicle vehicle, Vector3 position)
        {
            GameFiber.StartNew(delegate
            {
                Blip towBlip;
                Vector3 truckSpawn = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(200f, 400f));
                Game.LogTrivial("Towtruck & Driver Spawnpoint: ~r~" + truckSpawn);

                Vehicle towTruck = new Vehicle("towtruck", truckSpawn);
                towBlip = towTruck.AttachBlip();
                towBlip.Color = Color.HotPink;

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
                        towBlip.Delete();
                        break;
                    }
                }         
                GameFiber.Hibernate();
            }, "Common.callTowTruck");

            return true;
        }

    }
}
