namespace legallead.reader.service.tests
{
    public class WorkerTest
    {
        [Fact]
        public void WorkerCanExecute()
        {
            var sut = new MockWorker();
            Assert.True(sut.CanExecute());
        }
    }
}
