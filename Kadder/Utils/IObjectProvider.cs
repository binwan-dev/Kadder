using System;

namespace Kadder.Utils
{
    public interface IObjectProvider
    {
        IObjectScope CreateScope();

        T GetObject<T>();

        object GetObject(Type type);
    }
}
