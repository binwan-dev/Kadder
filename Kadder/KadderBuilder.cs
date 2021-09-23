using System;
using System.Collections.Generic;
using System.Reflection;
using Kadder.Utils;

namespace Kadder
{
    public class KadderBuilder
    {
        private readonly List<Assembly> _assembles;

        public KadderBuilder()
        {
            _assembles = new List<Assembly>();
        }

        public List<Assembly> Assemblies => _assembles;
    }
}
