namespace BusinessApp.Data.IntegrationTest
{
    using Xunit;
    using System;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using FakeItEasy;
    using System.Security.Principal;
    using BusinessApp.Test.Shared;
    using BusinessApp.App;
    using System.Threading;
    using System.Threading.Tasks;

    [Collection(nameof(DatabaseCollection))]
    public class EFCommandStoreRequestDecoratorTests
    {
        private readonly EFCommandStoreRequestDecorator<RequestStub, ResponseStub> sut;
        private readonly IRequestHandler<RequestStub, ResponseStub> inner;
        private readonly BusinessAppDbContext db;
        private readonly IPrincipal user;
        private readonly IEntityIdFactory<MetadataId> idFactory;
        private readonly CancellationToken cancelToken;

        public EFCommandStoreRequestDecoratorTests(DatabaseFixture fixture)
        {
            user = A.Fake<IPrincipal>();
            inner = A.Fake<IRequestHandler<RequestStub, ResponseStub>>();
            db = A.Fake<BusinessAppDbContext>();
            idFactory = A.Fake<IEntityIdFactory<MetadataId>>();
            cancelToken = A.Dummy<CancellationToken>();

            sut = new EFCommandStoreRequestDecorator<RequestStub, ResponseStub>(inner, user,
                db, idFactory);

            A.CallTo(() => user.Identity.Name).Returns("f");
        }

        public class Constructor : EFCommandStoreRequestDecoratorTests
        {
            public Constructor(DatabaseFixture f) : base(f)
            {}

            public static IEnumerable<object[]> InvalidCtorArgs
            {
                get
                {
                    return new []
                    {
                        new object[]
                        {
                            null,
                            A.Dummy<IPrincipal>(),
                            A.Dummy<BusinessAppDbContext>(),
                            A.Dummy<IEntityIdFactory<MetadataId>>(),
                        },
                        new object[]
                        {
                            A.Dummy<IRequestHandler<RequestStub, ResponseStub>>(),
                            null,
                            A.Dummy<BusinessAppDbContext>(),
                            A.Dummy<IEntityIdFactory<MetadataId>>(),
                        },
                        new object[]
                        {
                            A.Dummy<IRequestHandler<RequestStub, ResponseStub>>(),
                            A.Dummy<IPrincipal>(),
                            null,
                            A.Dummy<IEntityIdFactory<MetadataId>>(),
                        },
                        new object[]
                        {
                            A.Dummy<IRequestHandler<RequestStub, ResponseStub>>(),
                            A.Dummy<IPrincipal>(),
                            A.Dummy<BusinessAppDbContext>(),
                            null,
                        },
                    };
                }
            }

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IRequestHandler<RequestStub, ResponseStub> i,
                IPrincipal p, BusinessAppDbContext db, IEntityIdFactory<MetadataId> d)
            {
                /* Arrange */
                void shouldThrow() => new EFCommandStoreRequestDecorator<RequestStub, ResponseStub>(
                    i, p, db, d);

                /* Act */
                var ex = Record.Exception((Action)shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : EFCommandStoreRequestDecoratorTests
        {
            private readonly DatabaseFixture fixture;

            public HandleAsync(DatabaseFixture f) : base(f)
            {
                fixture = f;
            }

            [Fact]
            public void SetsMetadataIdProperty()
            {
                /* Arrange */
                Metadata<RequestStub> metadata = null;
                var id = A.Dummy<MetadataId>();
                A.CallTo(() => db.Add(A<Metadata<RequestStub>>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<Metadata<RequestStub>>(0));
                A.CallTo(() => idFactory.Create()).Returns(id);

                /* Act */
                var _ = sut.HandleAsync(A.Dummy<RequestStub>(), cancelToken);

                /* Assert */
                Assert.Same(id, metadata.Id);
            }

            [Fact]
            public void SetsMetadataUsernameProperty()
            {
                /* Arrange */
                Metadata<RequestStub> metadata = null;
                A.CallTo(() => db.Add(A<Metadata<RequestStub>>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<Metadata<RequestStub>>(0));
                A.CallTo(() => user.Identity.Name).Returns("foo");

                /* Act */
                var _ = sut.HandleAsync(A.Dummy<RequestStub>(), cancelToken);

                /* Assert */
                Assert.Equal("foo", metadata.Username);
            }

            [Fact]
            public void SetsMetadataTypeNameProperty()
            {
                /* Arrange */
                Metadata<RequestStub> metadata = null;
                A.CallTo(() => db.Add(A<Metadata<RequestStub>>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<Metadata<RequestStub>>(0));

                /* Act */
                var _ = sut.HandleAsync(A.Dummy<RequestStub>(), cancelToken);

                /* Assert */
                Assert.Equal(MetadataType.Request.ToString(), metadata.TypeName);
            }

            [Fact]
            public void SetsMetadataDataProperty()
            {
                /* Arrange */
                var request = A.Dummy<RequestStub>();
                Metadata<RequestStub> metadata = null;
                A.CallTo(() => db.Add(A<Metadata<RequestStub>>._))
                    .Invokes(ctx => metadata = ctx.GetArgument<Metadata<RequestStub>>(0));

                /* Act */
                var _ = sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Same(request, metadata.Data);
            }

            [Fact]
            public void InnerHandle_CalledAfterDbAdd()
            {
                /* Arrange */
                var request = A.Dummy<RequestStub>();

                /* Act */
                var _ = sut.HandleAsync(request, cancelToken);

                /* Assert */
                A.CallTo(() => db.Add(A<Metadata<RequestStub>>._)).MustHaveHappenedOnceExactly()
                    .Then(A.CallTo(() => inner.HandleAsync(request, cancelToken))
                        .MustHaveHappenedOnceExactly());
            }

            [Fact]
            public async Task ReturnsInnerHandleResult()
            {
                /* Arrange */
                var request = A.Dummy<RequestStub>();
                var expectedResult = Result.Ok(A.Dummy<ResponseStub>());
                A.CallTo(() => inner.HandleAsync(request, cancelToken))
                    .Returns(expectedResult);

                /* Act */
                var actualResult = await sut.HandleAsync(request, cancelToken);

                /* Assert */
                Assert.Equal(expectedResult, actualResult);
            }
        }
    }
}
