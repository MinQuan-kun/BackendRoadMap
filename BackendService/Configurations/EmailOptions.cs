namespace BackendService.Configurations
{
    public class EmailOptions
    {
        public SenderOptions Sender { get; set; } = new();
        public CredentialOptions Credential { get; set; } = new();
    }

    public class SenderOptions
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CredentialOptions
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
