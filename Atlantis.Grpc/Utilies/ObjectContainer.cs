using System;
using System.Reflection;

namespace Atlantis.Grpc.Utilies
{
    public class ObjectContainer
    {
        private static IObjectContainer _container;

        internal static IObjectContainer Container
        {
            get
            {
                if (_container == null) throw new ArgumentNullException("Object container value is null! please call SetContainer method to set container value first!");
                return _container;
            }
        }

        public static void SetContainer(IObjectContainer container)
        {
            _container = container;
        }

        public static void Register<TInterface, TService>(LifeScope lifeScope = LifeScope.Single)
        {
            _container.Register<TInterface, TService>(lifeScope);
        }
        
        public static void Register(Type interfaceType, Type serviceType, LifeScope lifeScope = LifeScope.Single)
        {
            _container.Register(interfaceType, serviceType, lifeScope);
        }
        
        public static void RegisterFromAssemblysForInterface(params Assembly[] assemblys)
        {
            _container.RegisterFromAssemblysForInterface(assemblys);
        }

        public static void RegisterInstance<TService>(TService instance, Type aliasType=null,LifeScope lifeScope= LifeScope.Single) where TService : class
        {
            _container.Register(instance,aliasType,lifeScope);
        }

        public static T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public static object Resolve(Type type)
        {
            return _container.Resolve(type);
        }
    }
}
