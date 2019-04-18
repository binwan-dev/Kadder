using System;
using System.Reflection;
using Autofac;
using Autofac.Builder;

namespace Atlantis.Grpc.Utilies
{
    public class AutofacObjectContainer:IObjectContainer
    {
        private IContainer _container;
        private ContainerBuilder _builder;

        public AutofacObjectContainer(ContainerBuilder builder=null)
        {
            _builder = builder?? new ContainerBuilder();
        }

        public IContainer Build()
        {
            if(_container==null)
            {
                _container=_builder.Build();
            }
            return _container;
        }

        public void Register<TInterface, TService>(LifeScope lifeScope = LifeScope.Single)
        {
            _builder.RegisterType<TService>().As<TInterface>().SetLifeScope(lifeScope);
        }

        public void Register<TService>(LifeScope lifeScope = LifeScope.Single)
        {
            _builder.RegisterType<TService>().SetLifeScope(lifeScope);
        }

        public void Register(Type interfaceType, Type serviceType, LifeScope lifeScope = LifeScope.Single)
        {
            _builder.RegisterType(serviceType).As(interfaceType).SetLifeScope(lifeScope);
        }

        public void Register(Type serviceType, LifeScope lifeScope = LifeScope.Single)
        {
            _builder.RegisterType(serviceType).SetLifeScope(lifeScope);
        }

        public void Register<TService>(TService instance, Type aliasType, LifeScope lifeScope = LifeScope.Single) where TService : class
        {
            var registerBuilder = _builder.RegisterInstance(instance).SetLifeScope(lifeScope);
            if (aliasType != null) registerBuilder.As(aliasType);
        }

        public void RegisterFromAssemblysForInterface(params Assembly[] assemblys)
        {
            _builder.RegisterAssemblyTypes(assemblys).AsImplementedInterfaces();//.SetLifeScope(LifeScope.Transient);

        }

        public void Release()
        {
            throw new NotImplementedException();
        }

        public T Resolve<T>()
        {
            if (_container == null) _container = _builder.Build();
            return _container.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            if (_container == null) _container = _builder.Build();
            return _container.Resolve(type);
        }

        public T ResolveWithLifeScope<T>()
        {
            if(_container==null)
            {
                _container=_builder.Build();
            }
            
            using(var scope=_container.BeginLifetimeScope())
            {
                return scope.Resolve<T>();
            }
        }
    }

    internal static class AutofacExetension
    {
        internal static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> SetLifeScope<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registerInfo, LifeScope lifeScope)
        {
            if (lifeScope == LifeScope.Single) registerInfo.SingleInstance();
            return registerInfo;
        }
    }

}
