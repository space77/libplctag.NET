using System;
using Xunit;

namespace libplctag.Tests
{
    public class UnitTest1
    {

        [Fact]
        public void Status_ok_when_first_created()
        {
            var tag = new MockTag();

            var status = tag.GetStatus();

            Assert.Equal(Status.Ok, status);
        }

        [Fact]
        public void Can_not_use_if_already_disposed()
        {
            var tag = new MockTag();

            tag.Dispose();

            Assert.Throws<ObjectDisposedException>(() => tag.GetStatus());
        }

        [Fact]
        public void AttributeStringFormatted()
        {

            var nativeTag = new MockNativeTag();

            var tag = new MockTag(nativeTag)
            {
                ElementSize = 4,
                ElementCount = 10,
                PlcType = PlcType.Slc500,
                Name = "TagName",
                Protocol = Protocol.ab_eip
            };

            tag.Initialize();

            Assert.Equal("protocol=ab_eip&plc=slc500&elem_size=4&elem_count=10&name=TagName", nativeTag.AttributeString);

        }
    }
}
