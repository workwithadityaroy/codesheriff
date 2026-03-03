using System.Security.Cryptography;
using System.Text;

namespace CodeSheriff.API.Services;

public static class WebhookSignatureValidator
{
    public static bool IsValid(byte[] body, string secret, string? signatureHeader)
    {
        if (string.IsNullOrEmpty(signatureHeader))
            return false;

        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var hash = HMACSHA256.HashData(secretBytes, body);
        var expected = "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signatureHeader));
    }
}
