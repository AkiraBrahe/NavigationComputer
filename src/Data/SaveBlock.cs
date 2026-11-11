using System;
using System.Collections.Generic;

namespace NavigationComputer.Data
{
    public class SaveBlock
    {
        public List<string> PreviousSearchQueries = [];
        public Dictionary<string, DateTime> SkippedFlashpoints = [];
    }
}