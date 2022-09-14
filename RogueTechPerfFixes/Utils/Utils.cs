﻿using RogueTechPerfFixes.Models;

namespace RogueTechPerfFixes.Utils
{
    public static class Utils
    {
        public static void CheckExitCounter(string message, int counter)
        {
            var exitCounter = VisibilityCacheGate.GetCounter;
            if (exitCounter > counter)
            {
                RTPFLogger.Error?.Write(message);
                VisibilityCacheGate.ExitAll();
            }
        }
    }
}
