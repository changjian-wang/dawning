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

            return config.Type?.ToLowerInvariant() switch
            {
                "file" => LoadFromFile(config),
                "store" => LoadFromStore(config),
                "azurekeyvault" => throw new InvalidOperationException(
                    "Certificate type 'AzureKeyVault' is not supported yet. Please use Type=File or Type=Store instead."
                ),
                _ => throw new InvalidOperationException(
                    $"Unknown certificate type: {config.Type}"
                ),
            };
        }

        /// <summary>
        /// Load certificate from file
        /// </summary>
        private static X509Certificate2 LoadFromFile(CertificateSource config)
        {
            if (string.IsNullOrEmpty(config.Path))
            {
                throw new InvalidOperationException("Certificate path is required when Type=File");
            }

            if (!File.Exists(config.Path))
            {
                throw new FileNotFoundException($"Certificate file not found: {config.Path}");
            }

            return X509CertificateLoader.LoadPkcs12FromFile(
                config.Path,
                config.Password,
                X509KeyStorageFlags.DefaultKeySet
            );
        }

        /// <summary>
        /// Load certificate from certificate store
        /// </summary>
        private static X509Certificate2 LoadFromStore(CertificateSource config)
        {
            if (!Enum.TryParse<StoreLocation>(config.StoreLocation ?? "CurrentUser", ignoreCase: true, out var storeLocation))
            {
                throw new InvalidOperationException(
                    $"Invalid StoreLocation value: '{config.StoreLocation}'. Valid values are: {string.Join(", ", Enum.GetNames<StoreLocation>())}"
                );
            }

            var storeName = string.IsNullOrEmpty(config.StoreName) ? "My" : config.StoreName;

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

            var now = DateTime.UtcNow;
            var all = certificates.OfType<X509Certificate2>().ToList();

            X509Certificate2? selected = null;
            try
            {
                selected = all
                    .Where(c => c.HasPrivateKey && c.NotAfter > now)
                    .OrderByDescending(c => c.NotAfter)
                    .FirstOrDefault();

                // Compute diagnostic stats while certificates are still alive (before Dispose).
                var total = all.Count;
                var expiredCount = all.Count(c => c.NotAfter <= now);
                var noPrivateKeyCount = all.Count(c => !c.HasPrivateKey);

                if (selected == null)
                {
                    string reason;
                    if (total > 0 && expiredCount == total)
                    {
                        reason = $"all {total} matching certificate(s) have expired";
                    }
                    else if (total > 0 && noPrivateKeyCount == total)
                    {
                        reason = $"none of the {total} matching certificate(s) have an accessible private key (check permissions or install the certificate with its private key)";
                    }
                    else
                    {
                        reason = $"no certificate among the {total} matching one(s) is both non-expired and has an accessible private key ({expiredCount} expired, {noPrivateKeyCount} without private key)";
                    }

                    throw new InvalidOperationException(
                        $"No valid certificate found in store: {reason}."
                    );
                }

                return selected;
            }
            finally
            {
                foreach (var c in all)
                {
                    if (!ReferenceEquals(c, selected))
                    {
                        c.Dispose();
                    }
                }
            }
        }
    }
}
