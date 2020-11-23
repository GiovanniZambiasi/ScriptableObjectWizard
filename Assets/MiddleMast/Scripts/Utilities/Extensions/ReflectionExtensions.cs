using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiddleMast.Utilities.Extensions
{
    public static class ReflectionExtensions
    {
        public static FieldInfo GetFieldInParent(this System.Type type, string name, BindingFlags flags)
        {
            System.Type __currentType = type;
            FieldInfo __field;
            var __sanity = 0;
            do
            {
                __field = __currentType.GetField(name, flags);
                __currentType = __currentType.BaseType;
                __sanity++;
                if(__sanity > 16)
                {
                    break;
                    throw new System.Exception($"Couldn't find field: {name} from type: {type}");
                }
            } while (__field == null);

            return __field;
        }

        public static bool HasAttribute(this MemberInfo member, Type attribute)
        {
            var __attributes = member.CustomAttributes;
            for (int i = 0; i < __attributes.Count(); i++)
            {
                var __attribute = __attributes.ElementAt(i);
                if (__attribute.AttributeType == attribute)
                    return true;
            }
            return false;
        }

        public static bool HasAttribute<TAttribute>(this MemberInfo member) where TAttribute : Attribute
        {
            return member.HasAttribute(typeof(TAttribute));
        }

        public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
        {
            var __attributes = member.CustomAttributes;
            var __type = typeof(T);
            var __customAttribute = Attribute.GetCustomAttribute(member, __type);
            return __customAttribute != null ? __customAttribute as T : null;
            // for (int i = 0; i < __attributes.Count(); i++)
            // {
            //     var __attribute = __attributes.ElementAt(i);
            //     if (__attribute.AttributeType == __type)
            //         return __attribute. as T;
            // }

            return null;
        }

        public static void GetAllConcreteInheritors(this System.Type type, IEnumerable<Assembly> assembliesToSweep, List<System.Type> inheritors)
        {
            for (var __i = 0; __i < assembliesToSweep.Count(); __i++)
            {
                var __assembly = assembliesToSweep.ElementAt(__i);
                type.GetAllConcreteInheritors(__assembly, inheritors);
            }
        }
        
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