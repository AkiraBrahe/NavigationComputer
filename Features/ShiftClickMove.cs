using BattleTech;
using HBS.Nav;
using System.Collections.Generic;
using System.Linq;

namespace NavigationComputer.Features
{
    /// <summary>
    /// Handles shift-click movement in the starmap to extend the current planned path.
    /// </summary>
    public static class ShiftClickMove
    {
        public static bool NextSelectIsShiftClick { get; set; }

        public static bool HandleClickSystem(Starmap starmap, StarSystemNode system)
        {
            if (!NextSelectIsShiftClick) return true;
            NextSelectIsShiftClick = false;

            var plannedPath = starmap.Screen.plannedPath;
            if (starmap.PotentialPath == null || starmap.PotentialPath.Count == 0 || plannedPath == null ||
                plannedPath.positionCount == 0)
            {
                Main.Log.LogDebug("Shift clicked system but had no previous route");
                return true;
            }

            // Setting CurSelected here prevents the star map from clearing the planned path
            starmap.CurSelected = system;

            var prevPath = new List<INavNode>([.. starmap.PotentialPath]);
            var prevPathLast = prevPath.Last();
            var starmapPathfinder = starmap.starmapPathfinder;
            starmapPathfinder.InitFindPath(prevPathLast, system, 1, 1E-06f, result =>
            {
                if (result.status != PathStatus.Complete)
                {
                    Main.Log.LogError("Something went wrong with pathfinding!");
                    return;
                }

                result.path.Remove(prevPathLast);
                result.path.InsertRange(0, prevPath);

                Main.Log.LogDebug($"Created new hybrid route of size {result.path.Count}");
                starmap.OnPathfindingComplete(result);
            });

            return false;
        }
    }
}
