using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Atlantis.Grpc.Utilies
{
    public class RefelectionHelper
    {
        public static IList<Type> GetImplInterfaceTypes(
            Type type, bool interfaceFilter = false, params Assembly[] assemblys)
        {
            var types = new List<Type>();
            foreach (var assembly in assemblys)
            {
                if (!interfaceFilter)
                {
                    types.AddRange(
                        assembly.GetModules()[0].GetTypes()
                        .Where(p => p.GetInterface(type.Name) != null));
                }
                else
                {
                    types.AddRange(
                        assembly.GetModules()[0].GetTypes()
                        .Where(p =>
                                   p.GetInterface(type.Name) != null &&
                                   p.IsInterface));
                }
            }
            return types;
        }
    }
}
