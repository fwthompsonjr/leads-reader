using legallead.models.Search;
using legallead.permissions.api.Model;
using legallead.records.search.Classes;
using System.Diagnostics;
using System.Xml.Linq;

namespace legallead.reader.service.tests
{
    public class WebMapperTests
    {
        [Theory]
        [InlineData("denton")]
        [InlineData("harris")]
        public void RequestCanMapSearch(string name)
        {
            var issue = Record.Exception(() =>
            {
                var sut = MockUtility.GetRequest(name);
                Assert.NotNull(sut);
                var mapped = WebMapper.MapFrom<UserSearchRequest, SearchRequest>(sut);
                Assert.NotNull(mapped);
            });
            Assert.Null(issue);
        }

        [Theory]
        // [InlineData("denton")]
        [InlineData("harris")]
        public void RequestCanMapParameter(string name)
        {
            var issue = Record.Exception(() =>
            {
                var sut = MockUtility.GetRequest(name);
                Assert.NotNull(sut);
                var mapped = WebMapper.MapFrom<UserSearchRequest, SearchNavigationParameter>(sut);
                Assert.NotNull(mapped);
                // mapped should have keys
                Assert.NotEmpty(mapped.Keys);
            });
            Assert.Null(issue);
        }

        [Theory]
        [InlineData("denton")]
        [InlineData("harris")]
        public void RequestCanMapInteractive(string name)
        {
            var issue = Record.Exception(() =>
            {
                var sut = MockUtility.GetRequest(name);
                Assert.NotNull(sut);
                var mapped = WebMapper.MapFrom<UserSearchRequest, WebInteractive>(sut);
                Assert.NotNull(mapped);
            });
            Assert.Null(issue);
        }

        [Fact]
        public void RequestCanFetchHarris()
        {
            if (!Debugger.IsAttached) return;
            const string name = "harris";
            var issue = Record.Exception(() =>
            {
                var sut = MockUtility.GetRequest(name);
                Assert.NotNull(sut);
                var mapped = WebMapper.MapFrom<UserSearchRequest, WebInteractive>(sut);
                Assert.NotNull(mapped);
                _ = mapped.Fetch();
            });
            Assert.Null(issue);

        }
    }
}
