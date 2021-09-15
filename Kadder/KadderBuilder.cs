using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kadder
{
    public class KadderBuilder
    {
        private readonly List<Assembly> _assembles;
        private readonly List<Type> _servicers;

        public KadderBuilder()
        {
            _assembles = new List<Assembly>();
        }

        public List<Assembly> Assemblies => _assembles;
    }
}
