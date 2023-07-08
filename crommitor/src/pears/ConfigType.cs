namespace comi.pears
{
    public class CommitType
    {
        public string value { get; set; }
        public string description { get; set; }
    }
    public class ConfigType
    {
        public string useplatform { get; set; }
        public string projpath { get; set; }
        public string branch { get; set; }
        public string title { get; set; }
        public CommitType[] committypes { get; set; }
        public CommitType[] onlyusers { get; set; }
        public string outputdir { get; set; }
        public string description { get; set; }
    }
}
