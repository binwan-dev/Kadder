using System;

namespace Kadder.Utils
{
    public interface IObjectScope : IDisposable
    {
        IObjectProvider Provider { get; }
    }
}
