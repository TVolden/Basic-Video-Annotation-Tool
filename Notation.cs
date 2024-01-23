namespace Video_Annotation
{
    public enum Choice { Confused, Frustrated, Bored, Other}

    public class Notation
    {
        public Choice Expression { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
    }
}
