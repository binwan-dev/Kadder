using System;
using System.Reflection;

namespace Atlantis.Grpc.Utilies
{
    public interface IObjectContainer
    {
        void Register<TInterface, TService>(LifeScope lifeScope = LifeScope.Single);

        void Register<TService>(LifeScope lifeScope = LifeScope.Single);

        void Register(Type interfaceType, Type serviceType, LifeScope lifeScope = LifeScope.Single);

        void Register(Type serviceType, LifeScope lifeScope = LifeScope.Single);

        void RegisterFromAssemblysForInterface(params Assembly[] assemblys);

        void Register<TService>(TService instance, Type aliasType,LifeScope lifeScope= LifeScope.Single) where TService : class;

        T Resolve<T>();

        object Resolve(Type @type);
    }
}
