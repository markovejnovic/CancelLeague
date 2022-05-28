using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;

namespace ProcessLock
{
    class GameDataProvider
    {
        public Dictionary<ProcessIdentifier, int> SecondsLeft = new Dictionary<ProcessIdentifier, int>();
        public ProcessMatcher ProcessMatcher = new ProcessMatcher();

        public void TrackProcess(ProcessIdentifier p)
        {
            ProcessMatcher.ProcessIdentifiers.Add(p);
            SecondsLeft.Add(p, 0);
        }

        public IEnumerable<Process> GetOffendingProcesses()
        {
            return ProcessMatcher.FilterRunning().Where(p => !CanRunProcess(p));
        }

        /// <summary>
        /// Returns whether a specific process can be run.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool CanRunProcessIdentifier(ProcessIdentifier p)
        {
            return SecondsLeft.ContainsKey(p) && SecondsLeft[p] > 0;
        }

        public bool CanRunProcess(Process p)
        {
            return CanRunProcessIdentifier(ProcessIdentifier.FromProcess(p));
        }

        /// <summary>
        /// Buys time for a process.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="seconds"></param>
        public void AddSeconds(ProcessIdentifier p, int seconds)
        {
            SecondsLeft[p] += seconds;
        }

        /// <summary>
        /// Decrements the total time for each process.
        /// </summary>
        public void DecrementSecond()
        {
            foreach (ProcessIdentifier p in SecondsLeft.Keys.ToArray())
            {
                if (SecondsLeft[p] <= 0) { continue; }

                SecondsLeft[p]--;
            }
        }

        public String StateToString()
        {
            return String.Join(" | ",
                SecondsLeft.Select(pair => $"{pair.Key} {pair.Value}"));
        }
    }
}