using legallead.records.search.Models;

namespace legallead.reader.service.tests
{
    internal static class MockPersonAddressProvider
    {

        public static List<PersonAddress> GetPeople(int count)
        {
            var persons = new List<PersonAddress>();
            while (persons.Count < count) { persons.AddRange(Samples); }
            return persons.Take(count).ToList();
        }

        private static List<PersonAddress>? samples;
        private static List<PersonAddress> Samples => samples ??= SamplePersonAddress();

        private static List<PersonAddress> SamplePersonAddress()
        {
            var qte = '"'.ToString();
            var content = _sampleAddressText.Replace("~", qte);
            var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();
            var people = new List<PersonAddress>();
            lines.ForEach(l =>
            {
                if (lines.IndexOf(l) != 0)
                {
                    var pieces = l.Split(',');
                    var item = new PersonAddress();
                    item.Name = pieces[0].Replace(qte, string.Empty);
                    item.Zip = pieces[3].Replace(qte, string.Empty);
                    item.Address1 = pieces[4].Replace(qte, string.Empty);
                    item.Address2 = pieces[5].Replace(qte, string.Empty);
                    item.Address3 = pieces[6].Replace(qte, string.Empty);
                    item.CaseNumber = pieces[7].Replace(qte, string.Empty);
                    item.DateFiled = pieces[8].Replace(qte, string.Empty);
                    item.Court = pieces[9].Replace(qte, string.Empty);
                    item.CaseType = pieces[10].Replace(qte, string.Empty);
                    item.Plantiff = pieces[11].Replace(qte, string.Empty);
                    people.Add(item);
                }
            });
            return people;
        }

        private static readonly string _sampleAddressText = ("Name,FirstName,LastName,Zip,Address1,Address2,Address3,CaseNumber,DateFiled,Court,CaseType,CaseStyle,Plantiff,County,Cou" +
            "rtAddress" + Environment.NewLine +
            "~McCraw, Susan~,Susan,McCraw,00000,No Match Found,,Not Matched 00000,GA1-0260-2019,12/17/2019,Probate Courts,Probate - G" +
            "uardianship for an Adult,In the Guardianship Of Jean Neal,Jean Neal,Collin,~2100 Bloomdale Road, McKinney, TX 75071~" + Environment.NewLine +
            "~Hanna, Frank A.~,Frank,Hanna,94506,31 Lilly CT,,~Danville, CA 94506~,PB1-2039-2019,12/17/2019,Probate Courts,Probate -" +
            "Small Estate Proceedings,In the Estate of Doris Hanna,,Collin,~2100 Bloomdale Road, McKinney, TX 75071~" + Environment.NewLine +
            "~NORDYKE, CANDICE ANN~,CANDICE,NORDYKE,75033,8505 TANGLEROSE DR.,,~FRISCO, TX 75033~,PB1-2040-2019,12/17/2019,Probate Co" +
            "urts,Probate - Independent Administration,In the Estate Of DAVID WAYNE WHITTEN,,Collin,~2100 Bloomdale Road, McKinney, T" +
            "X 75071~" + Environment.NewLine +
            "~AVERY, LUIS L.~,LUIS,AVERY,01740,397 Berlin Road,,~Bolton, MA 01740~,PB1-2041-2019,12/17/2019,Probate Courts,Probate -" +
            "Independent Administration,In the Estate Of DONALD G. AVERY,,Collin,~2100 Bloomdale Road, McKinney, TX 75071~" + Environment.NewLine +
            "~O'Neal, Sallianne~,Sallianne,O'Neal,76258,9110 Highway 377,,~Pilot Point, TX 76258~,PB1-2042-2019,12/17/2019,Probate Co" +
            "urts,Probate - Independent Administration,In the Estate Of Patricia Ann O'Neal,,Collin,~2100 Bloomdale Road, McKinney, T" +
            "X 75071~" + Environment.NewLine +
            "~Nicks, David~,David,Nicks,75001,~c/o Dena L. Mathis, Mathis Legal PLLC~,~15851 Dallas Parkway, Suite 800~,~Dallas, TX 7" +
            "5001~,PB1-2043-2019,12/17/2019,Probate Courts,Probate - Independent Administration,In the Estate Of Lori Ann Nicks,,Coll" +
            "in,~2100 Bloomdale Road, McKinney, TX 75071~" + Environment.NewLine);
    }
}
