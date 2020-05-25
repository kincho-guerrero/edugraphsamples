
using TinyCsvParser.Mapping;

namespace SDSConvertGroupToTeam
{
    public class CsvSectionMapping : CsvMapping<SectionUsage>
    {
        public CsvSectionMapping()
            : base()
        {
            MapProperty(0, x => x.GraphId);
            MapProperty(1, x => x.SisName);
          
        }
    }
}