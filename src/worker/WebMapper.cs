using AutoMapper;
using legallead.models.Search;
using legallead.permissions.api.Model;
using legallead.reader.component.utility;
using legallead.records.search.Classes;
using legallead.records.search.Models;
using Newtonsoft.Json;
using System.Xml;

namespace legallead.reader.component
{
    using PropXml = records.search.Properties.Resources;
    public static class WebMapper
    {

        public static T? MapFrom<TK, T>(TK source) where T : class, new()
        {
            if (source == null) return default;
            return Mapper.Map<T>(source);
        }

        private static MapperConfiguration? _configuration;
        private static IMapper? _mapper;

        private static MapperConfiguration Configuration => _configuration ??= GetConfiguration();
        private static IMapper Mapper => _mapper ??= Configuration.CreateMapper();

        private static MapperConfiguration GetConfiguration()
        {
            return new MapperConfiguration(c =>
            {
                c.CreateMap<UserSearchRequest, SearchRequest>()
                    .ForMember(x => x.WebId, opt => opt.MapFrom(y => GetWebIndex(y.County.Name, y.State)))
                    .ForMember(x => x.County, opt => opt.MapFrom(y => y.County.Name))
                    .ForMember(x => x.State, opt => opt.MapFrom(y => y.State))
                    .ForMember(x => x.StartDate, opt => opt.MapFrom(y => UnixDateToString(y.StartDate)))
                    .ForMember(x => x.EndDate, opt => opt.MapFrom(y => UnixDateToString(y.EndDate)));

                c.CreateMap<UserSearchRequest, SearchNavigationParameter>()
                    .ConvertUsing(ConvertTo);
                c.CreateMap<UserSearchRequest, WebInteractive>()
                    .ConvertUsing(ConvertTo);
            });
        }
        private static int GetWebIndex(string? county, string? st)
        {
            if (string.IsNullOrWhiteSpace(county) ||
                string.IsNullOrWhiteSpace(st)) return 0;
            var lookup = $"{st.ToLower()}-{county.Replace(' ', '-').ToLower()}";
            if (lookup.Equals("tx-collin")) return 20;
            if (lookup.Equals("tx-tarrant")) return 10;
            return 0;
        }
        private static string UnixDateToString(long unixTime)
        {
            var dte = DateTimeOffset.FromUnixTimeMilliseconds(unixTime).Date;
            return dte.ToString("yyyy-MM-dd");
        }

        private static WebInteractive ConvertTo(UserSearchRequest source, WebInteractive dest)
        {
            var startDate = DateTimeOffset.FromUnixTimeMilliseconds(source.StartDate).Date;
            var endingDate = DateTimeOffset.FromUnixTimeMilliseconds(source.EndDate).Date;
            var parameter = MapFrom<UserSearchRequest, SearchNavigationParameter>(source);
            var serialized = JsonConvert.SerializeObject(parameter);
            var translated = JsonConvert.DeserializeObject<WebNavigationParameter>(serialized) ?? new();
            WebInteractive interactive = translated.Id switch
            {
                0 => new WebInteractive(translated, startDate, endingDate),
                10 => new TarrantWebInteractive(translated, startDate, endingDate),
                20 => new CollinWebInteractive(translated, startDate, endingDate),
                _ => new WebInteractive(translated, startDate, endingDate)
            };
            interactive.Persistence = new StagingPersistence();
            interactive.StatusReporting = new StatusPersistence();
            return interactive;
        }
        private static SearchNavigationParameter ConvertTo(UserSearchRequest source, SearchNavigationParameter dest)
        {
            dest ??= new();
            dest.Id = GetWebIndex(source.County.Name, source.State);
            dest.StartDate = DateTimeOffset.FromUnixTimeMilliseconds(source.StartDate).Date;
            dest.EndDate = DateTimeOffset.FromUnixTimeMilliseconds(source.EndDate).Date;

            if (dest.Id == 0) DentonCountyNavigationMap(source, dest);
            if (dest.Id == 10) TarrantCountyNavigationMap(source, dest);
            if (dest.Id == 20) CollinCountyNavigationMap(source, dest);
            return dest;
        }

        private static void CollinCountyNavigationMap(UserSearchRequest source, SearchNavigationParameter dest)
        {
            const string collinCountyIndex = "20";
            var accepted = "0,1,2,3,4".Split(',');
            var cbxIndex = source.Details.Find(x => x.Name == "Search Type")?.Value ?? accepted[0];
            if (!accepted.Contains(cbxIndex)) cbxIndex = accepted[0];
            var idx = int.Parse(cbxIndex).ToString();
            AppendKeys(dest, collinCountyIndex);
            AppendInstructions(dest, collinCountyIndex);
            AppendCaseInstructions(dest, collinCountyIndex);
            var keyZero = new SearchNavigationKey { Name = "searchTypeSelectedIndex", Value = idx };
            // add key for combo-index
            dest.Keys.Add(keyZero);
        }
        private static void DentonCountyNavigationMap(UserSearchRequest source, SearchNavigationParameter dest)
        {
            const string dentonCountyIndex = "1";
            var accepted = "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15".Split(',');
            var cbxIndex = source.Details.Find(x => x.Name == "Court Type")?.Value ?? accepted[0];
            if (!accepted.Contains(cbxIndex)) cbxIndex = accepted[0];
            var idx = (int.Parse(cbxIndex) - 1).ToString();
            AppendKeys(dest, dentonCountyIndex);
            AppendInstructions(dest, dentonCountyIndex);
            AppendCaseInstructions(dest, dentonCountyIndex);
            var keyZero = new SearchNavigationKey { Name = "SearchComboIndex", Value = idx };
            var caseSearch = new SearchNavigationKey
            {
                Name = "CaseSearchType",
                Value = dentonLinkMap["0"]
            };
            var districtSearch = new SearchNavigationKey
            {
                Name = "DistrictSearchType",
                Value = "/html/body/table/tbody/tr[2]/td/table/tbody/tr[1]/td[2]/a[2]"
            };
            // add key for combo-index
            dest.Keys.Add(keyZero);
            // add key for case-search
            var caseSearchType = source.Details.Find(x => x.Name == "Case Type")?.Value ?? "0";
            if (caseSearchType != "0" && dentonLinkMap.ContainsKey(caseSearchType))
            {
                caseSearch.Value = dentonLinkMap[caseSearchType];
            }
            dest.Keys.Add(caseSearch);
            if (caseSearchType == "2")
            {
                dest.Keys.Add(districtSearch);
            }
        }

        private static void TarrantCountyNavigationMap(UserSearchRequest source, SearchNavigationParameter dest)
        {
            const string tarrantCountyIndex = "10";
            var accepted = "0,1,2,3,4,5,6,7,8,9,10".Split(',');
            var cbxIndex = source.Details.Find(x => x.Name == "Court Type")?.Value ?? accepted[0];
            if (!accepted.Contains(cbxIndex)) cbxIndex = accepted[0];
            var idx = int.Parse(cbxIndex).ToString();
            AppendKeys(dest, tarrantCountyIndex);
            AppendInstructions(dest, tarrantCountyIndex);
            AppendCaseInstructions(dest, tarrantCountyIndex);
            var keyZero = new SearchNavigationKey { Name = "SearchComboIndex", Value = idx };
            // add key for combo-index
            dest.Keys.Add(keyZero);
        }
        private static void AppendKeys(SearchNavigationParameter dest, string index)
        {
            var dates = new[] { "startDate", "endDate" };
            var nodeWebSite = WebSettings.DocumentElement?.ChildNodes[0];
            if (nodeWebSite == null) return;
            var list = nodeWebSite.ChildNodes.Cast<XmlNode>().ToList();
            var settings = list.Find(x => x.Attributes?.GetNamedItem("id")?.Value == index);
            if (settings != null && settings.HasChildNodes)
            {
                foreach (XmlNode item in settings.ChildNodes)
                {
                    var name = item.Attributes?.GetNamedItem("name")?.Value;
                    if (string.IsNullOrEmpty(name)) continue;
                    if (dates.Contains(name))
                    {
                        var dte = name.Equals(dates[0]) ? dest.StartDate : dest.EndDate;
                        var dtstring = dte.ToString("MM/dd/yyyy");
                        dest.Keys.Add(new SearchNavigationKey { Name = name, Value = dtstring });
                        continue;
                    }
                    if (!item.HasChildNodes) { continue; }
                    if (item.ChildNodes[0] is not XmlCDataSection section) continue;
                    var key = new SearchNavigationKey { Name = name, Value = section.Data };
                    dest.Keys.Add(key);
                }
            }
        }

        private static void AppendInstructions(SearchNavigationParameter dest, string index)
        {
            var nodeWebSite = WebSettings.DocumentElement?.ChildNodes[1];
            if (nodeWebSite == null) return;
            var list = nodeWebSite.ChildNodes.Cast<XmlNode>().ToList();
            var settings = list.Find(x =>
                x.Attributes?.GetNamedItem("id")?.Value == index &&
                x.Name.Equals("instructions"));
            if (settings != null && settings.HasChildNodes)
            {
                foreach (XmlNode item in settings.ChildNodes)
                {
                    var name = item.Attributes?.GetNamedItem("name")?.Value;
                    var instructionType = item.Attributes?.GetNamedItem("type")?.Value;
                    var by = item.Attributes?.GetNamedItem("By")?.Value;
                    var friendlyName = item.Attributes?.GetNamedItem("FriendlyName")?.Value;
                    if (string.IsNullOrEmpty(name)) continue;
                    if (!item.HasChildNodes) { continue; }
                    if (item.ChildNodes[0] is not XmlCDataSection section) continue;
                    var instruction = new SearchNavInstruction
                    {
                        Name = name ?? string.Empty,
                        CommandType = instructionType ?? string.Empty,
                        By = by ?? string.Empty,
                        FriendlyName = friendlyName ?? string.Empty,
                        Value = section.Data
                    };
                    dest.Instructions.Add(instruction);
                }
            }
        }


        private static void AppendCaseInstructions(SearchNavigationParameter dest, string index)
        {
            var nodeWebSite = WebSettings.DocumentElement?.ChildNodes[1];
            if (nodeWebSite == null) return;
            var list = nodeWebSite.ChildNodes.Cast<XmlNode>().ToList();
            var settings = list.Find(x =>
                x.Attributes?.GetNamedItem("id")?.Value == index &&
                x.Attributes?.GetNamedItem("type")?.Value == "normal" &&
                x.Name.Equals("caseInspection"));
            if (settings != null && settings.HasChildNodes)
            {
                foreach (XmlNode item in settings.ChildNodes)
                {
                    var name = item.Attributes?.GetNamedItem("name")?.Value;
                    var instructionType = item.Attributes?.GetNamedItem("type")?.Value;
                    var by = item.Attributes?.GetNamedItem("By")?.Value;
                    var friendlyName = item.Attributes?.GetNamedItem("FriendlyName")?.Value;
                    if (string.IsNullOrEmpty(name)) continue;
                    if (!item.HasChildNodes) { continue; }
                    if (item.ChildNodes[0] is not XmlCDataSection section) continue;
                    var instruction = new SearchNavInstruction
                    {
                        Name = name ?? string.Empty,
                        CommandType = instructionType ?? string.Empty,
                        By = by ?? string.Empty,
                        FriendlyName = friendlyName ?? string.Empty,
                        Value = section.Data
                    };
                    dest.CaseInstructions.Add(instruction);
                }
            }
        }

        private static readonly Dictionary<string, string> dentonLinkMap = new() {
                { "0", "//a[@class='ssSearchHyperlink'][contains(text(),'County Court: Civil, Family')]" },
                { "1", "//a[@class='ssSearchHyperlink'][contains(text(),'Criminal Case Records')]" },
                { "2", "//a[@class='ssSearchHyperlink'][contains(text(),'District Court Case')]" }
            };

        private static XmlDocument WebSettings => webSettingDoc ??= GetWebSetting();
        private static readonly string webSetting = PropXml.xml_settings_xml;
        private static XmlDocument? webSettingDoc;
        private static XmlDocument GetWebSetting()
        {
            var doc = new XmlDocument();
            doc.LoadXml(webSetting);
            return doc;
        }
    }
}
