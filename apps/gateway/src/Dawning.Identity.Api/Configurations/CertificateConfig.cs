using System.Security.Cryptography.X509Certificates;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// Certificate configuration
    /// </summary>
    public class CertificateConfig
    {
        /// <summary>
        /// Signing certificate
        /// </summary>
        public CertificateSource? Signing { get; set; }

        /// <summary>
        /// Encryption certificate
        /// </summary>
        public CertificateSource? Encryption { get; set; }
    }

    /// <summary>
    /// Certificate source configuration
    /// </summary>
    public class CertificateSource
    {
        /// <summary>
        /// Certificate source type (File, Store, AzureKeyVault)
        /// </summary>
        public string Type { get; set; } = "File";

        /// <summary>
        /// File path (used when Type=File)
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// File password (used when Type=File)
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Store location (used when Type=Store)
        /// </summary>
        public string? StoreLocation { get; set; } = "CurrentUser";

        /// <summary>
        /// Store name (used when Type=Store)
        /// </summary>
        public string? StoreName { get; set; } = "My";

        /// <summary>
        /// Certificate subject name (used when Type=Store)
        /// </summary>
        public string? SubjectName { get; set; }

        /// <summary>
        /// Certificate thumbprint (used when Type=Store)
        /// </summary>
        public string? Thumbprint { get; set; }

        /// <summary>
        /// Azure Key Vault URL (used when Type=AzureKeyVault)
        /// </summary>
        public string? KeyVaultUrl { get; set; }

        /// <summary>
        /// Certificate name (used when Type=AzureKeyVault)
        /// </summary>
        public string? CertificateName { get; set; }
    }

    /// <summary>
    /// Certificate loader
    /// </summary>
    public static class CertificateLoader
    {
        /// <summary>
        /// Load certificate
        /// </summary>
        public static X509Certificate2? LoadCertificate(CertificateSource? config)
        {
            if (config == null)
            {
                return null;
            }

            return config.Type.ToLowerInvariant() switch
            {
                "file" => LoadFromFile(config),
                "store" => LoadFromStore(config),
                "azurekeyvault" => throw new NotImplementedException(
                    "Azure Key Vault integration not implemented yet"
                ),
                _ => throw new InvalidOperationException(
                    $"Unknown certificate type: {config.Type}"
                ),
            };
        }

        /// <summary>
        /// Load certificate from file
        /// </summary>
        private static X509Certificate2? LoadFromFile(CertificateSource config)
        {
            if (string.IsNullOrEmpty(config.Path))
            {
                throw new InvalidOperationException("Certificate path is required when Type=File");
            }

            if (!File.Exists(config.Path))
            {
                throw new FileNotFoundException($"Certificate file not found: {config.Path}");
            }

            return string.IsNullOrEmpty(config.Password)
                ? new X509Certificate2(config.Path)
                : new X509Certificate2(config.Path, config.Password);
        }

        /// <summary>
        /// Load certificate from certificate store
        /// </summary>
        private static X509Certificate2? LoadFromStore(CertificateSource config)
        {
            var storeLocation = Enum.Parse<StoreLocation>(config.StoreLocation ?? "CurrentUser");
            var storeName = Enum.Parse<StoreName>(config.StoreName ?? "My");

            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certificates;

            if (!string.IsNullOrEmpty(config.Thumbprint))
            {
                certificates = store.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    config.Thumbprint,
                    validOnly: false
                );
            }
            else if (!string.IsNullOrEmpty(config.SubjectName))
            {
                certificates = store.Certificates.Find(
                    X509FindType.FindBySubjectName,
                    config.SubjectName,
                    validOnly: false
                );
            }
            else
            {
                throw new InvalidOperationException(
                    "Either Thumbprint or SubjectName is required when Type=Store"
                );
            }

            if (certificates.Count == 0)
            {
                throw new InvalidOperationException("Certificate not found in store");
            }

            return certificates[0];
        }
    }
}
