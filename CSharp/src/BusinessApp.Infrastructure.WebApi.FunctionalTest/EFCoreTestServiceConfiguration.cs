using BusinessApp.Infrastructure.EntityFramework;
using BusinessApp.Test.Shared;
using SimpleInjector;

namespace BusinessApp.Infrastructure.WebApi.FunctionalTest
 {
    internal class EFCoreTestServiceConfiguration : IServiceConfiguration
    {
        public void Configure(Container container)
        {
            container.RegisterDecorator(
                typeof(BusinessAppDbContext),
                typeof(BusinessAppTestDbContext));
        }
    }
}