
using TinyCsvParser.Mapping;

namespace EDUScheduler
{
    public class CsvClassScheduleMapping : CsvMapping<ClassSchedule>
    {
        public CsvClassScheduleMapping()
            : base()
        {
            MapProperty(0, x => x.GroupID);
            MapProperty(1, x => x.SISID);
            MapProperty(2, x => x.StartDateTime);
            MapProperty(3, x => x.EndDateTime);

            MapProperty(4, x => x.Day1StartTime);
            MapProperty(5, x => x.Day1EndTime);

            MapProperty(6, x => x.Day2StartTime);
            MapProperty(7, x => x.Day2EndTime);

            MapProperty(8, x => x.Day3StartTime);
            MapProperty(9, x => x.Day3EndTime);

            MapProperty(10, x => x.Day4StartTime);
            MapProperty(11, x => x.Day4EndTime);

            MapProperty(12, x => x.Day5StartTime);
            MapProperty(13, x => x.Day5EndTime);            

            MapProperty(14, x => x.Day6StartTime);
            MapProperty(15, x => x.Day6EndTime);      
        }
    }
}