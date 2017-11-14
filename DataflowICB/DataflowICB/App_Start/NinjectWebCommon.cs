[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(DataflowICB.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(DataflowICB.App_Start.NinjectWebCommon), "Stop")]

namespace DataflowICB.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Microsoft.AspNet.Identity.Owin;
    using DataflowICB.Database;
    using Dataflow.DataServices.Contracts;
    using Dataflow.DataServices;
    using System.Net.Http;
    using Dataflow.Services.Contracts;
    using Dataflow.Services;
    using DataflowICB.App_Start.Contracts;

    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
                kernel.Bind<IUserServices>().To<CustomUserServicesICB>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ApplicationUserManager>()
                .ToMethod(_ =>
                    HttpContext.Current
                    .GetOwinContext()
                    .GetUserManager<ApplicationUserManager>()
                );

            kernel.Bind<ApplicationDbContext>()
                .ToMethod(_ =>
                    HttpContext.Current
                    .GetOwinContext()
                    .Get<ApplicationDbContext>()
                )
                .InRequestScope();

            kernel.Bind<ISensorService>()
                .To<SensorService>()
                .InRequestScope();

            kernel.Bind<IHttpClientProvider>()
                .To<HttpClientProvider>()
                .InRequestScope();

            kernel.Bind<IEmailService>()
                .To<GmailEmailService>()
                .InRequestScope();
        }
    }
}
