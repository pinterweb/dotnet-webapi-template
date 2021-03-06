using System;
using Xunit;

namespace BusinessApp.Kernel.UnitTest
{
    public class BadStateExceptionTests
    {
        public class Constructor : BadStateExceptionTests
        {
            [Fact]
            public void WithMessage_MappedToProperty()
            {
                /* Act */
                var ex = new BadStateException("foo");

                /* Assert */
                Assert.Equal("foo", ex.Message);
            }

            [Fact]
            public void InnerException_MappedToProp()
            {
                /* Act */
                var inner = new Exception();
                var ex = new BadStateException("foo", inner);

                /* Assert */
                Assert.Same(inner, ex.InnerException);
            }
        }
    }
}
