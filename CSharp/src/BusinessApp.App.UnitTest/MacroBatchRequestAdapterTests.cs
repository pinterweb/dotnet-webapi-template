namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;
    using FakeItEasy;
    using Xunit;

    public class MacroBatchRequestAdapterTests
    {
        private readonly CancellationToken cancelToken;
        private readonly MacroBatchRequestAdapter<CommandMacro, CommandStub, IEnumerable<CommandStub>> sut;
        private readonly ICommandHandler<IEnumerable<CommandStub>> inner;
        private readonly IBatchMacro<CommandMacro, CommandStub> expander;

        public MacroBatchRequestAdapterTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            inner = A.Fake<ICommandHandler<IEnumerable<CommandStub>>>();
            expander = A.Fake<IBatchMacro<CommandMacro, CommandStub>>();

            sut = new MacroBatchRequestAdapter<CommandMacro, CommandStub, IEnumerable<CommandStub>>(
                expander, inner);
        }

        public class Constructor : MacroBatchRequestAdapterTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<ICommandHandler<IEnumerable<CommandStub>>>(),
                },
                new object[]
                {
                    A.Fake<IBatchMacro<CommandMacro, CommandStub>>(),
                    null,
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(
                IBatchMacro<CommandMacro, CommandStub> g,
                ICommandHandler<IEnumerable<CommandStub>> i)
            {
                /* Arrange */
                void shouldThrow() => new MacroBatchRequestAdapter<CommandMacro, CommandStub, IEnumerable<CommandStub>>(g, i);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class HandleAsync : MacroBatchRequestAdapterTests
        {
            [Fact]
            public async Task WithoutCommandArg_ExceptionThrown()
            {
                /* Arrange */
                Task shouldthrow() => sut.HandleAsync(null, A.Dummy<CancellationToken>());

                /* Act */
                var ex = await Record.ExceptionAsync(shouldthrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public async Task NoReturnedPayloadsFromMacro_ExceptionThrown()
            {
                /* Arrange */
                var macro = A.Dummy<CommandMacro>();
                A.CallTo(() => expander.ExpandAsync(macro, cancelToken)).Returns(new CommandStub[0]);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(macro, cancelToken));

                /* Assert */
                Assert.IsType<BusinessAppAppException>(ex);
                Assert.Equal(
                    "The macro you ran expected to find records to change, but none were " +
                    "found",
                    ex.Message
                );
            }

            [Fact]
            public async Task ReturnedPayloadsFromMacro_PassedToInnerHandler()
            {
                /* Arrange */
                var macro = A.Dummy<CommandMacro>();
                var commands = new[] { A.Dummy<CommandStub>() };
                A.CallTo(() => expander.ExpandAsync(macro, cancelToken)).Returns(commands);

                /* Act */
                var ex = await Record.ExceptionAsync(() => sut.HandleAsync(macro, cancelToken));

                /* Assert */
                A.CallTo(() => inner.HandleAsync(commands, cancelToken)).MustHaveHappenedOnceExactly();
            }

            [Fact]
            public async Task ErrorFromInnerHandler_ReturnedInNewResult()
            {
                /* Arrange */
                var macro = A.Dummy<CommandMacro>();
                var error = new Exception();
                var innerResult = Result.Error<IEnumerable<CommandStub>>(error);
                var commands = new[] { A.Dummy<CommandStub>() };
                A.CallTo(() => expander.ExpandAsync(macro, cancelToken)).Returns(commands);
                A.CallTo(() => inner.HandleAsync(commands, cancelToken)).Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(macro, cancelToken);

                /* Assert */
                Assert.Equal(innerResult, result);
            }

            [Fact]
            public async Task OkResultFromInnerHandler_MacroReturned()
            {
                /* Arrange */
                var macro = A.Dummy<CommandMacro>();
                var innerResult = Result.Ok<IEnumerable<CommandStub>>(new CommandStub[0]);
                var commands = new[] { A.Dummy<CommandStub>() };
                A.CallTo(() => expander.ExpandAsync(macro, cancelToken)).Returns(commands);
                A.CallTo(() => inner.HandleAsync(commands, cancelToken)).Returns(innerResult);

                /* Act */
                var result = await sut.HandleAsync(macro, cancelToken);

                /* Assert */
                Assert.Equal(innerResult, result);
            }
        }

        public sealed class CommandMacro : IMacro<CommandStub>
        {}
    }
}