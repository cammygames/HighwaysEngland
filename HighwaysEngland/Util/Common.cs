using System;
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

    }
}
