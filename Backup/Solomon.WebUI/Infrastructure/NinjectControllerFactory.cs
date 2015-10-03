using Solomon.Domain.Abstract;
using Solomon.Domain.Concrete;
using Ninject;
using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Generic;
using Ninject.Syntax;
using Ninject.Modules;
using Solomon.WebUI.Mailers;

namespace Solomon.WebUI.Infrastructure
{
    //public class NinjectControllerFactory : DefaultControllerFactory
    //{
    //    private IKernel ninjectKernel;

    //    public NinjectControllerFactory()
    //    {
    //        ninjectKernel = new StandardKernel();



    //        AddBindings();
    //    }

    //    protected override IController GetControllerInstance(RequestContext requestContext,
    //        Type controllerType)
    //    {

    //        return controllerType == null
    //            ? base.GetControllerInstance(requestContext, controllerType)
    //            : (IController)ninjectKernel.Get(controllerType);
    //    }

    //    private void AddBindings()
    //    {
    //        ninjectKernel.Bind<IRepository>().To<EFRepository>();
    //    }
    //}

    public class FullNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository>().To<EFRepository>();
            Bind<IUserMailer>().To<UserMailer>();
        }
    }

    public class NinjectDependencyResolver : IDependencyResolver
    {
        private IKernel kernel;
        public NinjectDependencyResolver()
        {
            kernel = new StandardKernel(new FullNinjectModule());
        }

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        public IBindingToSyntax<T> Bind<T>()
        {
            return kernel.Bind<T>();
        }

        public IKernel Kernel
        {
            get { return kernel; }
        }
    }
}