namespace comi.src.pears
{
    public class RepositoryInfo
    {
        public string name { get; set; }
        public string tagbri { get; set; }
        public string commitbri { get; set; }
        public string issuebri { get; set; }
    }
    public class Repositories
    {
        public List<RepositoryInfo> platform { get; set;}
    }
}
