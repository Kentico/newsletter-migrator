namespace NewsletterMigrator
{
    internal class ProgramConfiguration
    {
        public string OldDbConnectionString { get; set; }

        public string NewDbConnectionString { get; set; }

        public string IssuesFilter { get; set; }

        public string TemplatesFilter { get; set; }
    }
}