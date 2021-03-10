namespace BusinessApp.Domain.UnitTest
{
    using Xunit;
    using BusinessApp.Domain;
    using FakeItEasy;
    using System.Linq;

    public class EventStreamTests
    {
        public class Constructor : EventStreamTests
        {
            [Fact]
            public void NoArgs_EmptyStream()
            {
                /* Act */
                var stream = new EventStream();

                /* Assert */
                Assert.Empty(stream);
            }

            [Fact]
            public void MissingEvents_ExceptionThrown()
            {
                /* Act */
                var ex = Record.Exception(() => new EventStream(null));

                /* Assert */
                Assert.NotNull(ex);
            }
        }

        public class IEnumerableImpl : EventStreamTests
        {
            [Fact]
            public void DrainsTheQueue()
            {
                /* Arrange */
                var events = A.CollectionOfDummy<IDomainEvent>(4);
                var stream = new EventStream(events);

                /* Act */
                var _ = stream.Select(s => s).ToList();

                /* Assert */
                Assert.Empty(stream);
            }

            [Fact]
            public void ResetTheQueue()
            {
                /* Arrange */
                var events = A.CollectionOfDummy<IDomainEvent>(4);
                var stream = new EventStream(events);
                var _ = stream.Select(s => s).ToList();

                /* Act */
                stream.GetEnumerator().Reset();

                /* Assert */
                Assert.Equal(4, stream.Count());
            }
        }
    }
}