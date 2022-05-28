using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace ProcessLock
{
    public class ProcessMatcher
    {
        public List<ProcessIdentifier> ProcessIdentifiers = new List<ProcessIdentifier>();

        /// <summary>
        /// Filters out processes according to the given specification.
        /// </summary>
        /// <param name="processes">The input processes</param>
        /// <returns>Filtered processes.</returns>
        public IEnumerable<Process> OrFilter(Process[] processes) {
            return processes.Where(p => ProcessIdentifiers.Any(i => i.IdentifiesProcess(p)));
        }

        /// <summary>
        /// Fetches running processes, automatically filtering them.
        /// </summary>
        /// <returns>Running processes filtered.</returns>
        public IEnumerable<Process> FilterRunning() {
            return OrFilter(Process.GetProcesses());
        }
    }
}