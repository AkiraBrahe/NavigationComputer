using BattleTech;
using BTSimpleMechAssembly;
using System;

namespace NavigationComputer.Utils
{
    public static class CustomExtensions
    {
        /// <summary>
        /// Gets the vehicle's base chassis name, handling special cases for Carrier variants.
        /// </summary>
        /// <remarks>
        /// Intended for use with the Expansion Pack; determines the base chassis name from the assembly variant for a cleaner name, e.g. "Behemoth" instead of "Behemoth Heavy Tank".
        /// </remarks>
        public static string GetBaseChassis(this VehicleDef vehicle)
        {
            try
            {
                string baseChassis = CCIntegration.GetCCVehicleAssemblyVariant(vehicle.Chassis)?.PrefabID;
                if (!string.IsNullOrEmpty(baseChassis))
                {
                    // Remove parenthical suffixes
                    int parenIndex = baseChassis.IndexOf('(');
                    if (parenIndex > 0)
                    {
                        baseChassis = baseChassis.Substring(0, parenIndex).Trim();
                    }

                    // Handle Carrier variants
                    if (baseChassis.Contains("Carrier"))
                    {
                        string prefix = baseChassis.StartsWith("Heavy") ? "Heavy " : baseChassis.StartsWith("Light") ? "Light " : "";
                        baseChassis = $"{prefix}Carrier";
                    }

                    return baseChassis;
                }

                // Fallback to the vehicle's name if no assembly variant is found
                return vehicle.Description.Name;
            }
            catch (Exception ex)
            {
                Main.Log.LogError($"Error getting base chassis for {vehicle.Description.Id}: {ex.Message}");
                return vehicle.Description.Name;
            }
        }
    }
}