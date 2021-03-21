﻿namespace BusinessApp.WebApi.IntegrationTest
{
    using Xunit;
    using SimpleInjector;
    using Microsoft.AspNetCore.Hosting;
    using FakeItEasy;
    using System.Linq;
    using BusinessApp.App;
    using BusinessApp.Test.Shared;
    using System.Threading.Tasks;
    using System.Threading;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using System;
    using SimpleInjector.Lifestyles;
    using BusinessApp.Domain;
    using Microsoft.Extensions.Configuration;
    using BusinessApp.Data;
    using Microsoft.Extensions.Logging;
    using BusinessApp.WebApi.Json;

    public partial class ServiceRegistrationsTests : IDisposable
    {
        private readonly Container container;
        private readonly Scope scope;

        public ServiceRegistrationsTests()
        {
            container = new Container();
            new Startup(A.Dummy<IConfiguration>(), container);
            scope = AsyncScopedLifestyle.BeginScope(container);
        }

        public void Dispose() => scope.Dispose();

        public void CreateRegistrations(Container container, IWebHostEnvironment env = null)
        {
            var bootstrapOptions = new BootstrapOptions
            {
                DbConnectionString = "Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar",
                RegistrationAssemblies = new[]
                {
                    typeof(ServiceRegistrationsTests).Assembly,
                    typeof(IQuery).Assembly,
                    typeof(IQueryVisitor<>).Assembly
                }
            };
            container.RegisterInstance(A.Fake<IHttpContextAccessor>());
            Bootstrapper.RegisterServices(container,
                bootstrapOptions,
                (env ?? A.Dummy<IWebHostEnvironment>()),
                A.Dummy<ILoggerFactory>());
        }


        public class RequestHandlers : ServiceRegistrationsTests
        {
            [Fact]
            public void NoCommandRequestHandler_HasAllDefaultHandlers()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<NoHandlerCommandStub, NoHandlerCommandStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(serviceType);

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(RequestExceptionDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(TransactionRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EFMetadataStoreRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(NoBusinessLogicRequestHandler<NoHandlerCommandStub>),
                        implType)
                );
            }

            [Fact]
            public void NotABatchCommand_WithEventStreamResponse_NoBatchDecoratorsInHandlers()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<CommandStub, EventStreamStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(serviceType);

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(RequestExceptionDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(TransactionRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EventConsumingRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(CommandHandlerStreamStub),
                        implType)
                );
            }

            [Fact]
            public void NotABatchCommand_WithoutEventStreamResponse_NoBatchDecoratorsInHandlers()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<CommandStub, CommandStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(serviceType);

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(RequestExceptionDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(TransactionRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EFMetadataStoreRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(CommandHandlerStub),
                        implType)
                );
            }

            [Fact]
            public void IsABatchCommand_WithEventStreamResponse_BatchDecoratorsInHandlers()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(
                    typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                    typeof(IRequestHandler<CommandStub, EventStreamStub>));

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(RequestExceptionDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(GroupedBatchRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ScopedBatchRequestProxy<CommandStub, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(TransactionRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(BatchRequestAdapter<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EventConsumingRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(BatchProxyRequestHandler<CommandHandlerStreamStub, CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(CommandHandlerStreamStub),
                        implType)
                );
            }

            [Fact]
            public void IsABatchCommand_WithoutEventStreamResponse_BatchDecoratorsInHandlers()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(
                    typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                    typeof(IRequestHandler<CommandStub, CommandStub>));

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(RequestExceptionDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(GroupedBatchRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ScopedBatchRequestProxy<CommandStub, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(TransactionRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(BatchRequestAdapter<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EFMetadataStoreRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(BatchProxyRequestHandler<CommandHandlerStub, CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(CommandHandlerStub),
                        implType)
                );
            }

            [Fact]
            public void IsABatchMacroCommand_WithStreamResponse_BatchMacroDecoratorsInHandlers()
            {
                /* Arrange */
                container.RegisterInstance(A.Fake<IBatchMacro<MacroStub, CommandStub>>());
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<MacroStub, IEnumerable<EventStreamStub>>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(
                    typeof(IRequestHandler<MacroStub, IEnumerable<EventStreamStub>>),
                    typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                    typeof(IRequestHandler<CommandStub, EventStreamStub>));

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(RequestExceptionDecorator<MacroStub, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<MacroStub, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<MacroStub, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EFMetadataStoreRequestDecorator<MacroStub, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(MacroBatchRequestAdapter<MacroStub, CommandStub, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(GroupedBatchRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ScopedBatchRequestProxy<CommandStub, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(TransactionRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(MacroBatchProxyRequestHandler<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(BatchRequestAdapter<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EventConsumingRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(BatchProxyRequestHandler<CommandHandlerStreamStub, CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(CommandHandlerStreamStub),
                        implType)
                );
            }

            [Fact]
            public void IsABatchMacroCommand_WithOutStreamResponse_BatchMacroDecoratorsInHandlers()
            {
                /* Arrange */
                container.RegisterInstance(A.Fake<IBatchMacro<MacroStub, CommandStub>>());
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<MacroStub, IEnumerable<CommandStub>>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(
                    typeof(IRequestHandler<MacroStub, IEnumerable<CommandStub>>),
                    typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                    typeof(IRequestHandler<CommandStub, CommandStub>));

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(RequestExceptionDecorator<MacroStub, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<MacroStub, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<MacroStub, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EFMetadataStoreRequestDecorator<MacroStub, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(MacroBatchRequestAdapter<MacroStub, CommandStub, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(GroupedBatchRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ScopedBatchRequestProxy<CommandStub, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(TransactionRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(MacroBatchProxyRequestHandler<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(BatchRequestAdapter<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EFMetadataStoreRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(BatchProxyRequestHandler<CommandHandlerStub, CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(CommandHandlerStub),
                        implType)
                );
            }

            [Fact]
            public void QueryRequest_QueryDecoratorsAdded()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<QueryStub, ResponseStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(typeof(IRequestHandler<QueryStub, ResponseStub>));

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(RequestExceptionDecorator<QueryStub, ResponseStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EFTrackingQueryDecorator<QueryStub, ResponseStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<QueryStub, ResponseStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(InstanceCacheQueryDecorator<QueryStub, ResponseStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<QueryStub, ResponseStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EntityNotFoundQueryDecorator<QueryStub, ResponseStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<QueryStub, ResponseStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(QueryHandlerStub),
                        implType)
                );
            }

            [Fact]
            public void NoExplicityQueryRequestRegistration_SingleQueryHandlerDecoratorsAdded()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<UnregisteredQuery, UnregisteredQuery>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(
                    typeof(IRequestHandler<UnregisteredQuery, UnregisteredQuery>),
                    typeof(IRequestHandler<UnregisteredQuery, IEnumerable<UnregisteredQuery>>));

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(RequestExceptionDecorator<UnregisteredQuery, UnregisteredQuery>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EFTrackingQueryDecorator<UnregisteredQuery, UnregisteredQuery>),
                        implType),
                    implType => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<UnregisteredQuery, UnregisteredQuery>),
                        implType),
                    implType => Assert.Equal(
                        typeof(InstanceCacheQueryDecorator<UnregisteredQuery, UnregisteredQuery>),
                        implType),
                    implType => Assert.Equal(
                        typeof(ValidationRequestDecorator<UnregisteredQuery, UnregisteredQuery>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EntityNotFoundQueryDecorator<UnregisteredQuery, UnregisteredQuery>),
                        implType),
                    implType => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<UnregisteredQuery, UnregisteredQuery>),
                        implType),
                    implType => Assert.Equal(
                        typeof(SingleQueryRequestAdapter<EFQueryStrategyHandler<UnregisteredQuery, UnregisteredQuery>, UnregisteredQuery, UnregisteredQuery>),
                        implType),
                    implType => Assert.Equal(
                        typeof(EFQueryStrategyHandler<UnregisteredQuery, UnregisteredQuery>),
                        implType)
                );
            }
        }

        public class HttpRequestHandlers : ServiceRegistrationsTests
        {
            [Fact]
            public void HasCorrectOrder()
            {
                /* Arrange */
                var linkFactory = A.Fake<Func<CommandStub, string>>();
                container.Collection.Register<HateoasLink<CommandStub>>(new[]
                {
                    new HateoasLink<CommandStub>(linkFactory, "foo", "bar")
                });
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IHttpRequestHandler<CommandStub, CommandStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(serviceType);

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(HttpResponseDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(JsonHttpDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(HttpRequestLoggingDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(WeblinkingHeaderRequestDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(NewtonsoftJsonExceptionDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(SystemJsonExceptionDecorator<CommandStub, CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(HttpRequestHandler<CommandStub, CommandStub>),
                        implType)
                );
            }
        }

        public class Validators : ServiceRegistrationsTests
        {
            [Fact]
            public void RegistersAllValidators()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IValidator<CommandStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var firstType = container.GetRegistration(serviceType);
                var validators = firstType
                    .GetDependencies()
                    .Prepend(firstType)
                    .Select(ip => ip.Registration.ImplementationType);

                Assert.Collection(validators,
                    implType => Assert.Equal(
                        typeof(CompositeValidator<CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(IEnumerable<IValidator<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(DataAnnotationsValidator<CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(FirstValidatorStub),
                        implType),
                    implType => Assert.Equal(
                        typeof(SecondValidatorStub),
                        implType),
                    implType => Assert.Equal(
                        typeof(FluentValidationValidator<CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(IEnumerable<FluentValidation.IValidator<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(FirstFluentValidatorStub),
                        implType),
                    implType => Assert.Equal(
                        typeof(SecondFluentValidatorStub),
                        implType)
                );
            }

            private class FirstFluentValidatorStub : FluentValidation.AbstractValidator<CommandStub>
            {}

            private class SecondFluentValidatorStub : FluentValidation.AbstractValidator<CommandStub>
            {}

            private class FirstValidatorStub : IValidator<CommandStub>
            {
                public Task<Result<Unit, Exception>> ValidateAsync(CommandStub instance, CancellationToken cancelToken)
                {
                    return Task.FromResult(Result.OK);
                }
            }

            private class SecondValidatorStub : IValidator<CommandStub>
            {
                public Task<Result<Unit, Exception>> ValidateAsync(CommandStub instance, CancellationToken cancelToken)
                {
                    return Task.FromResult(Result.OK);
                }
            }
        }

        public class Loggers : ServiceRegistrationsTests
        {
            [Fact]
            public void UsesStringLogEntryFormatterInDevMode()
            {
                /* Arrange */
                var env = A.Fake<IWebHostEnvironment>();
                A.CallTo(() => env.EnvironmentName).Returns("Development");
                CreateRegistrations(container, env);
                container.Verify();
                var serviceType = typeof(ILogEntryFormatter);

                /* Act */
                var instance = container.GetInstance(serviceType);

                /* Assert */
                Assert.IsType<StringLogEntryFormatter>(instance);
            }

            [Fact]
            public void UsesSerializedLogEntryFormatterNotInDevMode()
            {
                /* Arrange */
                var env = A.Fake<IWebHostEnvironment>();
                A.CallTo(() => env.EnvironmentName).Returns("Production");
                CreateRegistrations(container, env);
                container.Verify();
                var serviceType = typeof(ILogEntryFormatter);

                /* Act */
                var instance = container.GetInstance(serviceType);

                /* Assert */
                Assert.IsType<SerializedLogEntryFormatter>(instance);
            }
        }

        private IEnumerable<Type> GetServiceGraph(params Type[] serviceTypes)
        {
            var firstType = container.GetRegistration(serviceTypes.First());

            return firstType
                .GetDependencies()
                .Where(i => serviceTypes.Any(st => st.IsAssignableFrom(i.ServiceType)))
                .Prepend(firstType)
                .Select(ip => ip.Registration.ImplementationType);
        }

        private sealed class CommandHandlerStreamStub : IRequestHandler<CommandStub, EventStreamStub>
        {
            public Task<Result<EventStreamStub, Exception>> HandleAsync(CommandStub request,
                CancellationToken cancelToken)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class CommandHandlerStub : IRequestHandler<CommandStub, CommandStub>
        {
            public Task<Result<CommandStub, Exception>> HandleAsync(CommandStub request,
                CancellationToken cancelToken)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class QueryHandlerStub : IQueryHandler<QueryStub, ResponseStub>
        {
            public Task<Result<ResponseStub, Exception>> HandleAsync(QueryStub request, CancellationToken cancelToken)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class NoHandlerCommandStub { }

        public sealed class CommandStub { }

        public sealed class EventStreamStub : IEventStream
        {
            public IEnumerable<IDomainEvent> Events { get; set; }
        }

        public sealed class MacroStub : IMacro<CommandStub> { }

        public sealed class UnregisteredQuery : Query
        {
            public override IEnumerable<string> Sort { get; set; }
        }
    }
}