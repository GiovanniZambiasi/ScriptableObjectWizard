using System.Collections.Generic;
using System.Reflection;

namespace MiddleMast.Utilities.Extensions
{
    public static class ReflectionExtensions
    {
        public static void GetAllConcreteInheritors(this System.Type type, Assembly assembly, List<System.Type> inheritors)
        {
            var __types = assembly.GetTypes();
                
            for (int __j = 0; __j < __types.Length; __j++)
            {
                var __type = __types[__j];
                if (__type.IsClass && !__type.IsAbstract && __type.IsSubclassOf(type))
                    inheritors.Add(__type);
            }
        }
    }
}