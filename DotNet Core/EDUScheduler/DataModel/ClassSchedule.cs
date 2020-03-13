using System;
using System.Collections.Generic;

namespace EDUScheduler
{
    public class ClassSchedule
    {

        public ClassSchedule (DateTime Day1StartTime )
        {
            
        }
        public string SISID { get; set; }

        public string GroupID { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public DateTime? Day1StartTime { get; set; }

        public DateTime? Day1EndTime { get; set; }

        public DateTime? Day2StartTime { get; set; }

        public DateTime? Day2EndTime { get; set; }        

        public DateTime? Day3StartTime { get; set; }

        public DateTime? Day3EndTime { get; set; }

        public DateTime? Day4StartTime { get; set; }

        public DateTime? Day4EndTime { get; set; }

        public DateTime? Day5StartTime { get; set; }

        public DateTime? Day5EndTime { get; set; }        

        public DateTime? Day6StartTime { get; set;}

        public DateTime? Day6EndTime { get; set; }    
    }
}