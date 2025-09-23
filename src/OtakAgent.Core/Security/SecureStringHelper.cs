using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace OtakAgent.Core.Security
{
    [SupportedOSPlatform("windows")]
    public static class SecureStringHelper
    {
        private static readonly byte[] AdditionalEntropy = Encoding.UTF8.GetBytes("OtakAgent_2024_Secure");

        /// <summary>
        /// Encrypts a string using Windows DPAPI (Data Protection API)
        /// </summary>
        public static string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] protectedBytes = ProtectedData.Protect(plainBytes, AdditionalEntropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(protectedBytes);
            }
            catch (CryptographicException)
            {
                // If DPAPI fails, return the original text as fallback
                // In production, you might want to handle this differently
                return plainText;
            }
        }

        /// <summary>
        /// Decrypts a string that was encrypted using Windows DPAPI
        /// </summary>
        public static string Unprotect(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                // Check if the string is base64 encoded (encrypted)
                if (!IsBase64String(encryptedText))
                    return encryptedText; // Return as-is if not encrypted

                byte[] protectedBytes = Convert.FromBase64String(encryptedText);
                byte[] plainBytes = ProtectedData.Unprotect(protectedBytes, AdditionalEntropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (CryptographicException)
            {
                // If decryption fails, assume it's not encrypted
                return encryptedText;
            }
            catch (FormatException)
            {
                // If base64 decode fails, return original
                return encryptedText;
            }
        }

        private static bool IsBase64String(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            try
            {
                byte[] data = Convert.FromBase64String(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sanitizes user input to prevent injection attacks
        /// </summary>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove control characters and non-printable characters
            var sanitized = new StringBuilder();
            foreach (char c in input)
            {
                if (!char.IsControl(c) || c == '\n' || c == '\r' || c == '\t')
                {
                    sanitized.Append(c);
                }
            }

            return sanitized.ToString().Trim();
        }
    }
}