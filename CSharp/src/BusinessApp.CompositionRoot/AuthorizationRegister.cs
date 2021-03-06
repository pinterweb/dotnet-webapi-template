using System;
using System.Reflection;
using System.Security.Principal;
using BusinessApp.Infrastructure;
using SimpleInjector;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Registers authorization services
    /// </summary>
    public class AuthorizationRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public AuthorizationRegister(IBootstrapRegister inner) => this.inner = inner;

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            context.Container.RegisterDecorator<IPrincipal, AnonymousUser>();

            container.RegisterConditional(
                typeof(IAuthorizer<>),
                typeof(AuthorizeAttributeHandler<>),
                IsAuthCommand);

            inner.Register(context);

            container.RegisterConditional(
                typeof(IAuthorizer<>),
                typeof(NullAuthorizer<>),
                c => !c.Handled);
        }

        private static bool IsAuthCommand(PredicateContext ctx)
        {
            var requestType = ctx.ServiceType.GetGenericArguments()[0];

            static bool HasAuthAttribute(Type targetType)
                => targetType.GetCustomAttribute<AuthorizeAttribute>() != null;

            while (!HasAuthAttribute(requestType) && requestType.IsConstructedGenericType)
            {
                requestType = requestType.GetGenericArguments()[0];
            }

            return HasAuthAttribute(requestType);
        }

        /// <summary>
        /// Null object pattern. When no authorization is used on a request
        /// </summary>
        private sealed class NullAuthorizer<T> : IAuthorizer<T>
            where T : notnull
        {
            public bool AuthorizeObject(T instance) => true;
        }
    }
}
