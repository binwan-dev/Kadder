using FM.ConsulInterop.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer
{
    public class ConsulSetting
    {
        public ConsulLocalServiceConfig Local { get; set; }

        public ConsulRemoteServiceConfig[] Remotes { get; set; }

        public ConsulRemoteServiceConfig Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("Cannot get consul remote config!");
            if (Remotes == null || Remotes.Length == 0) throw new NullReferenceException("The consul remote config is null!");
            var remoteSetting = Remotes.FirstOrDefault(p => p.Name == name);
            if (remoteSetting == null) throw new KeyNotFoundException($"Cannot found consul remote config ({name});");
            return remoteSetting;
        }
    }
}
