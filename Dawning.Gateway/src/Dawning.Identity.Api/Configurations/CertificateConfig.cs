using System.Security.Cryptography.X509Certificates;

namespace Dawning.Identity.Api.Configurations
{
    /// <summary>
    /// 证书配置
    /// </summary>
    public class CertificateConfig
    {
        /// <summary>
        /// 签名证书
        /// </summary>
        public CertificateSource? Signing { get; set; }

        /// <summary>
        /// 加密证书
        /// </summary>
        public CertificateSource? Encryption { get; set; }
    }

    /// <summary>
    /// 证书来源配置
    /// </summary>
    public class CertificateSource
    {
        /// <summary>
        /// 证书来源类型 (File, Store, AzureKeyVault)
        /// </summary>
        public string Type { get; set; } = "File";

        /// <summary>
        /// 文件路径（Type=File 时使用）
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 文件密码（Type=File 时使用）
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 存储位置（Type=Store 时使用）
        /// </summary>
        public string? StoreLocation { get; set; } = "CurrentUser";

        /// <summary>
        /// 存储名称（Type=Store 时使用）
        /// </summary>
        public string? StoreName { get; set; } = "My";

        /// <summary>
        /// 证书主题名称（Type=Store 时使用）
        /// </summary>
        public string? SubjectName { get; set; }

        /// <summary>
        /// 证书指纹（Type=Store 时使用）
        /// </summary>
        public string? Thumbprint { get; set; }

        /// <summary>
        /// Azure Key Vault URL（Type=AzureKeyVault 时使用）
        /// </summary>
        public string? KeyVaultUrl { get; set; }

        /// <summary>
        /// 证书名称（Type=AzureKeyVault 时使用）
        /// </summary>
        public string? CertificateName { get; set; }
    }

    /// <summary>
    /// 证书加载器
    /// </summary>
    public static class CertificateLoader
    {
        /// <summary>
        /// 加载证书
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
        /// 从文件加载证书
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
        /// 从证书存储加载证书
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
