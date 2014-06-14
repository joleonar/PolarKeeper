using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PolarKeeper
{
    public class ExerciseViewModel
    {
        public int Id { get; set; }
        public DateTime time { get; set; }
        public String sport { get; set; }
        public int calories { get; set; }
        public TimeSpan duration { get; set; }
        public int heartRateMax { get; set; }
        public int heartRateAvg { get; set; }
        public int heartRateRest { get; set; }
        public double distance { get; set; }
        public bool Uploaded { get; set; }
    }
}