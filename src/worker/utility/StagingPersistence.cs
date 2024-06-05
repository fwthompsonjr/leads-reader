using legallead.jdbc.entities;
using legallead.jdbc.helpers;
using legallead.jdbc.implementations;
using legallead.jdbc.interfaces;
using legallead.records.search.Interfaces;
using legallead.records.search.Models;

namespace legallead.reader.component.utility
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
        "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "Avoid duplication of code.")]
    public class StagingPersistence : IStagingPersistence
    {
        private const jdbc.SearchTargetTypes targetType = jdbc.SearchTargetTypes.Staging;
        private readonly IUserSearchRepository _repo;
        public StagingPersistence()
        {
            var command = new DapperExecutor();
            var context = new DataContext(command);
            _repo = new UserSearchRepository(context);
        }

        public KeyValuePair<bool, string> Add(string searchid, string key, string value)
        {
            return AddAsync(searchid, key, value).Result;
        }


        public KeyValuePair<bool, string> Add(string searchid, string key, byte[] value)
        {
            return AddAsync(searchid, key, value).Result;
        }

        public async Task<KeyValuePair<bool, string>> AddAsync(string searchid, string key, string value)
        {
            var response = await _repo.Append(targetType, searchid, value, key);
            return response;
        }
        public async Task<KeyValuePair<bool, string>> AddAsync(string searchid, string key, byte[] value)
        {
            var response = await _repo.Append(targetType, searchid, value, key);
            return response;
        }


        public KeyValuePair<bool, StagedContentModel> Fetch(string searchid, string key)
        {
            return FetchAsync(searchid, key).Result;
        }

        public async Task<KeyValuePair<bool, StagedContentModel>> FetchAsync(string searchid, string key)
        {
            var failure = new KeyValuePair<bool, StagedContentModel>(false, new());
            var response = await _repo.GetStaged(searchid, key);
            if (!response.Key) return failure;
            var responded = response.Value;
            if (responded is not SearchStagingDto obj) return failure;
            var model = new StagedContentModel
            {
                Id = obj.Id,
                StagingType = obj.StagingType,
                LineNbr = obj.LineNbr,
                LineData = obj.LineData,
                LineText = obj.LineText,
                IsBinary = obj.IsBinary,
                CreateDate = obj.CreateDate
            };
            return new KeyValuePair<bool, StagedContentModel>(true, model);
        }
    }
}
