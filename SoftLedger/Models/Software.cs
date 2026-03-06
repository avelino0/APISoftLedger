namespace SoftLedger.Models
{
    public class Software
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string MachineName { get; set; }
        public string UserName { get; set; }
        public string SoftwareName { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string InstallDate { get; set; }

        public bool IsMicrosoftProduct { get; set; }
        public bool IsWindowsProduct { get; set; }
        public bool IsOfficeProduct { get; set; }
        public bool HasLicenseKeyAndActivated { get; set; }

        public DateTime CollectionDate { get; set; }
    }
}
