using legallead.reader.service.services;

namespace legallead.reader.service.tests.svc
{
    public class IndexReaderTests
    {
        [Fact]
        public void ServiceCanRebuild()
        {
            var exception = Record.Exception(() =>
            {
                var reader = new IndexReader();
                reader.Rebuild();
            });
            Assert.Null(exception);
        }

        [Fact]
        public void ServiceCanGetIndexes()
        {
            var exception = Record.Exception(() =>
            {
                var reader = new IndexReader();
                _ = reader.Indexes;
            });
            Assert.Null(exception);
        }
    }
}
