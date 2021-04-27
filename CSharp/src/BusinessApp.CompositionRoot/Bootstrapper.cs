using System.Linq;
using BusinessApp.Infrastructure;
using Microsoft.Extensions.Logging;
using SimpleInjector;

namespace BusinessApp.CompositionRoot
{
    public static class Bootstrapper
    {
        public static void RegisterServices(Container container,
            RegistrationOptions options, ILoggerFactory loggerFactory)
        {
            var bootstrapContainer = SetupBootstrapContainer(options, loggerFactory);

            RegisterBootstrapDecorators(bootstrapContainer, options);

            bootstrapContainer.Verify();

            var bootstrap = bootstrapContainer.GetInstance<IBootstrapRegister>();
            var regContext = new RegistrationContext(container);

            bootstrap.Register(regContext);

            // run this once on the outer most context
            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(RequestExceptionDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>)));
        }

        private static Container SetupBootstrapContainer(RegistrationOptions options,
            ILoggerFactory loggerFactory)
        {
            var bootstrapContainer = new Container();

            bootstrapContainer.Register<IBootstrapRegister, MainRegister>();
            bootstrapContainer.RegisterInstance(options);
            bootstrapContainer.RegisterInstance(loggerFactory);

            return bootstrapContainer;
        }

        private static void RegisterBootstrapDecorators(Container container,
            RegistrationOptions options)
        {
            var bootstrapDecorators = container
                .GetTypesToRegister(typeof(IBootstrapRegister),
                    options.RegistrationAssemblies.Concat(new[] { typeof(Bootstrapper).Assembly }),
                    new TypesToRegisterOptions
                    {
                        IncludeDecorators = true
                    })
                .Where(type => type != typeof(MainRegister));

            foreach (var type in bootstrapDecorators)
            {
                container.RegisterDecorator(typeof(IBootstrapRegister), type);
            }
        }
    }
}
