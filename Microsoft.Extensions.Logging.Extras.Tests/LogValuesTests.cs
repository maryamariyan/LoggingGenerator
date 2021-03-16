// © Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Internal;
using Xunit;

namespace Microsoft.Extensions.Logging.Test
{
    public class LogValuesTests
    {
        [Fact]
        public void NoType()
        {
            var kvp = new[]
            {
                new KeyValuePair<string, object?>("{OriginalFormat}", "Foo"),
            };

            Func<LogValues, Exception?, string> f = (_, _) => "TestData";
            var s = new LogValues(f, "Foo");
            TestCollection(kvp, s);
            Assert.Equal("TestData", s.ToString());
        }

        [Fact]
        public void OneType()
        {
            var kvp = new[]
            {
                new KeyValuePair<string, object?>("name1", 1),
                new KeyValuePair<string, object?>("{OriginalFormat}", "Foo"),
            };

            Func<LogValues<int>, Exception?, string> f = (_, _) => "TestData";
            var s = new LogValues<int>(f, "Foo", "name1", 1);
            TestCollection(kvp, s);
            Assert.Equal(kvp[0].Value, s.Value);
            Assert.Equal("TestData", s.ToString());
        }

        [Fact]
        public void TwoTypes()
        {
            var kvp = new[]
            {
                new KeyValuePair<string, object?>("name1", 1),
                new KeyValuePair<string, object?>("name2", 2),
                new KeyValuePair<string, object?>("{OriginalFormat}", "Foo"),
            };

            Func<LogValues<int, int>, Exception?, string> f = (_, _) => "TestData";
            var s = new LogValues<int, int>(f, "Foo", new[] { "name1", "name2" }, 1, 2);
            TestCollection(kvp, s);
            Assert.Equal(kvp[0].Value, s.Value1);
            Assert.Equal(kvp[1].Value, s.Value2);
            Assert.Equal("TestData", s.ToString());
        }

        [Fact]
        public void ThreeTypes()
        {
            var kvp = new[]
            {
                new KeyValuePair<string, object?>("name1", 1),
                new KeyValuePair<string, object?>("name2", 2),
                new KeyValuePair<string, object?>("name3", 3),
                new KeyValuePair<string, object?>("{OriginalFormat}", "Foo"),
            };

            Func<LogValues<int, int, int>, Exception?, string> f = (_, _) => "TestData";
            var s = new LogValues<int, int, int>(f, "Foo", new[] { "name1", "name2", "name3" }, 1, 2, 3);
            TestCollection(kvp, s);
            Assert.Equal(kvp[0].Value, s.Value1);
            Assert.Equal(kvp[1].Value, s.Value2);
            Assert.Equal(kvp[2].Value, s.Value3);
            Assert.Equal("TestData", s.ToString());
        }

        [Fact]
        public void FourTypes()
        {
            var kvp = new[]
            {
                new KeyValuePair<string, object?>("name1", 1),
                new KeyValuePair<string, object?>("name2", 2),
                new KeyValuePair<string, object?>("name3", 3),
                new KeyValuePair<string, object?>("name4", 4),
                new KeyValuePair<string, object?>("{OriginalFormat}", "Foo"),
            };

            Func<LogValues<int, int, int, int>, Exception?, string> f = (_, _) => "TestData";
            var s = new LogValues<int, int, int, int>(f, "Foo", new[] { "name1", "name2", "name3", "name4" }, 1, 2, 3, 4);
            TestCollection(kvp, s);
            Assert.Equal(kvp[0].Value, s.Value1);
            Assert.Equal(kvp[1].Value, s.Value2);
            Assert.Equal(kvp[2].Value, s.Value3);
            Assert.Equal(kvp[3].Value, s.Value4);
            Assert.Equal("TestData", s.ToString());
        }

        [Fact]
        public void FiveTypes()
        {
            var kvp = new[]
            {
                new KeyValuePair<string, object?>("name1", 1),
                new KeyValuePair<string, object?>("name2", 2),
                new KeyValuePair<string, object?>("name3", 3),
                new KeyValuePair<string, object?>("name4", 4),
                new KeyValuePair<string, object?>("name5", 5),
                new KeyValuePair<string, object?>("{OriginalFormat}", "Foo"),
            };

            Func<LogValues<int, int, int, int, int>, Exception?, string> f = (_, _) => "TestData";
            var s = new LogValues<int, int, int, int, int>(f, "Foo", new[] { "name1", "name2", "name3", "name4", "name5" }, 1, 2, 3, 4, 5);
            TestCollection(kvp, s);
            Assert.Equal(kvp[0].Value, s.Value1);
            Assert.Equal(kvp[1].Value, s.Value2);
            Assert.Equal(kvp[2].Value, s.Value3);
            Assert.Equal(kvp[3].Value, s.Value4);
            Assert.Equal(kvp[4].Value, s.Value5);
            Assert.Equal("TestData", s.ToString());
        }

        [Fact]
        public void SixTypes()
        {
            var kvp = new[]
            {
                new KeyValuePair<string, object?>("name1", 1),
                new KeyValuePair<string, object?>("name2", 2),
                new KeyValuePair<string, object?>("name3", 3),
                new KeyValuePair<string, object?>("name4", 4),
                new KeyValuePair<string, object?>("name5", 5),
                new KeyValuePair<string, object?>("name6", 6),
                new KeyValuePair<string, object?>("{OriginalFormat}", "Foo"),
            };

            Func<LogValues<int, int, int, int, int, int>, Exception?, string> f = (_, _) => "TestData";
            var s = new LogValues<int, int, int, int, int, int>(f, "Foo", new[] { "name1", "name2", "name3", "name4", "name5", "name6" }, 1, 2, 3, 4, 5, 6);
            TestCollection(kvp, s);
            Assert.Equal(kvp[0].Value, s.Value1);
            Assert.Equal(kvp[1].Value, s.Value2);
            Assert.Equal(kvp[2].Value, s.Value3);
            Assert.Equal(kvp[3].Value, s.Value4);
            Assert.Equal(kvp[4].Value, s.Value5);
            Assert.Equal(kvp[5].Value, s.Value6);
            Assert.Equal("TestData", s.ToString());
        }

        [Fact]
        public void NTypes()
        {
            var kvp = new[]
            {
                new KeyValuePair<string, object?>("name1", 1),
                new KeyValuePair<string, object?>("name2", 2),
                new KeyValuePair<string, object?>("name3", 3),
                new KeyValuePair<string, object?>("name4", 4),
                new KeyValuePair<string, object?>("name5", 5),
                new KeyValuePair<string, object?>("name6", 6),
                new KeyValuePair<string, object?>("name7", 7),
            };

            var kvp2 = new[]
            {
                new KeyValuePair<string, object?>("name1", 1),
                new KeyValuePair<string, object?>("name2", 2),
                new KeyValuePair<string, object?>("name3", 3),
                new KeyValuePair<string, object?>("name4", 4),
                new KeyValuePair<string, object?>("name5", 5),
                new KeyValuePair<string, object?>("name6", 6),
                new KeyValuePair<string, object?>("name7", 7),
                new KeyValuePair<string, object?>("{OriginalFormat}", "Foo"),
            };

            Func<LogValuesN, Exception?, string> f = (_, _) => "TestData";
            var s = new LogValuesN(f, "Foo", kvp);
            TestCollection(kvp2, s);
            Assert.Equal(kvp2[0].Value, s[0].Value);
            Assert.Equal(kvp2[1].Value, s[1].Value);
            Assert.Equal(kvp2[2].Value, s[2].Value);
            Assert.Equal(kvp2[3].Value, s[3].Value);
            Assert.Equal(kvp2[4].Value, s[4].Value);
            Assert.Equal(kvp2[5].Value, s[5].Value);
            Assert.Equal(kvp2[6].Value, s[6].Value);
            Assert.Equal(kvp2[7].Value, s[7].Value);
            Assert.Equal("TestData", s.ToString());
        }

        private static void TestCollection(
            IReadOnlyList<KeyValuePair<string, object?>> expected,
            IReadOnlyList<KeyValuePair<string, object?>> actual)
        {
            Assert.Equal(expected, actual);
            Assert.Equal(expected.Count, actual.Count);

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => _ = actual[expected.Count]);
        }
    }
}
