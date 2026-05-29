namespace SoftLedger.Models
{
    public class Software
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string MachineName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string SoftwareName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string InstallDate { get; set; } = string.Empty;

        public bool IsMicrosoftProduct { get; set; }
        public bool IsWindowsProduct { get; set; }
        public bool IsOfficeProduct { get; set; }
        public bool HasLicenseKeyAndActivated { get; set; }

        public DateTime CollectionDate { get; set; }
    }
}
