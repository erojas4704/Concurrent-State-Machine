using System;
using System.Linq;

namespace CSM
{
    public static class ActorUtils
    {
        public static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(fullName))
                .FirstOrDefault(type => type != null);
        }
    }
}