using System;
using System.Linq;
using System.Diagnostics;

namespace ProcessLock
{
    public class ProcessIdentifier
    {
        String name;
        string[] aliases;

        public ProcessIdentifier(String name, string[] aliases)
        {
            this.name = name;
            this.aliases = aliases;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return name.Equals(((ProcessIdentifier)obj).name);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public bool IdentifiesProcess(Process p)
        {
            return p.ProcessName.Equals(name);
        }

        public bool IsAlias(string maybeAlias)
        {
            return aliases.Contains(maybeAlias);
        }

        public static ProcessIdentifier FromProcess(Process p,
                                                    string[] aliases = null)
        {
            string[] alii = aliases ?? new string[0];
            return new ProcessIdentifier(p.ProcessName, alii);
        }

        public override string ToString()
        {
            return $"ProcessIdentifier<{name}>";
        }
    }
}
