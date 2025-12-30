#!/usr/bin/env pwsh

# Generate self-signed certificates using OpenSSL

$CertDir = "C:\github\dawning\deploy\helm\dawning\files\certificates"
$SigningCertPath = "$CertDir\signing.pfx"
$EncryptionCertPath = "$CertDir\encryption.pfx"
$CertPassword = "DawningCert@2024"

# Create directory if it doesn't exist
if (!(Test-Path $CertDir)) {
    New-Item -ItemType Directory -Path $CertDir -Force | Out-Null
    Write-Host "Created certificate directory: $CertDir"
}

# Remove old certificates
if (Test-Path $SigningCertPath) {
    Remove-Item $SigningCertPath -Force
    Write-Host "Removed old signing certificate"
}
if (Test-Path $EncryptionCertPath) {
    Remove-Item $EncryptionCertPath -Force
    Write-Host "Removed old encryption certificate"
}

# Generate signing certificate using OpenSSL
Write-Host "Generating signing certificate..."
openssl req -x509 -newkey rsa:2048 -keyout "$CertDir\signing.key" `
    -out "$CertDir\signing.crt" -days 3650 -nodes `
    -subj "/CN=dawning-signing"

openssl pkcs12 -export -in "$CertDir\signing.crt" -inkey "$CertDir\signing.key" `
    -out $SigningCertPath -password pass:$CertPassword

Remove-Item "$CertDir\signing.key", "$CertDir\signing.crt" -Force

Write-Host "Signing certificate created: $SigningCertPath"

# Generate encryption certificate using OpenSSL  
Write-Host "Generating encryption certificate..."
openssl req -x509 -newkey rsa:2048 -keyout "$CertDir\encryption.key" `
    -out "$CertDir\encryption.crt" -days 3650 -nodes `
    -subj "/CN=dawning-encryption"

openssl pkcs12 -export -in "$CertDir\encryption.crt" -inkey "$CertDir\encryption.key" `
    -out $EncryptionCertPath -password pass:$CertPassword

Remove-Item "$CertDir\encryption.key", "$CertDir\encryption.crt" -Force

Write-Host "Encryption certificate created: $EncryptionCertPath"

Write-Host ""
Write-Host "========================================"
Write-Host " Certificates Generated Successfully"
Write-Host "========================================"
Write-Host "Signing cert:    $SigningCertPath"
Write-Host "Encryption cert: $EncryptionCertPath"
Write-Host "Password:        $CertPassword"
Write-Host "========================================"
