using System.Globalization;
using Common.Shared;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using CS = Common.Shared.I18n.S;

var rows = new List<Row>();
var s = CS.Inst;

s.Library.Values.First().Keys.Order().ForEach(key =>
{
    Throw.DataIf(!Key.IsValid(key), "invalid key");
    rows.Add(new Row()
    {
        Key = key,
        En = s.Library[CS.EN][key].Raw,
        Es = s.Library[CS.ES][key].Raw,
        Fr = s.Library[CS.FR][key].Raw,
        De = s.Library[CS.DE][key].Raw,
        It = s.Library[CS.IT][key].Raw
    });
});

using (var writer = new StreamWriter("strings.csv"))
using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
{
    csv.WriteRecords(rows);
}

public class Row
{
    [Name("key")]
    public string Key { get; set; }
    [Name("en")]
    public string En { get; set; }
    [Name("es")]
    public string Es { get; set; }
    [Name("fr")]
    public string Fr { get; set; }
    [Name("de")]
    public string De { get; set; }
    [Name("it")]
    public string It { get; set; }
}