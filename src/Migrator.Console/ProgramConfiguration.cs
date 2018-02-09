namespace NewsletterTemplateMigrator
{
    internal class ProgramConfiguration
    {
        public string OldDbConnectionString { get; set; }
        public string NewDbConnectionString { get; set; }
        public string PathToBackup { get; set; }
    }
}