using legallead.jdbc.helpers;
using legallead.jdbc.implementations;
using legallead.jdbc.interfaces;
using legallead.records.search.Interfaces;

namespace legallead.reader.component.utility
{
    public class StatusPersistence : IStatusPersistence
    {
        private const jdbc.SearchTargetTypes targetType = jdbc.SearchTargetTypes.Status;
        private readonly IUserSearchRepository _repo;
        public StatusPersistence()
        {
            var command = new DapperExecutor();
            var context = new DataContext(command);
            _repo = new UserSearchRepository(context);
        }

        public void Status(string searchid, string message)
        {
            _ = Task.Run(async () =>
            {
                await _repo.Append(targetType, searchid, message);
            }).ConfigureAwait(false);
        }
    }
}
