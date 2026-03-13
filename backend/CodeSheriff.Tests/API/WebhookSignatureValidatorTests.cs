using System.Security.Cryptography;
using System.Text;
using CodeSheriff.API.Services;
using FluentAssertions;

namespace CodeSheriff.Tests.API;

public sealed class WebhookSignatureValidatorTests
{
    private const string Secret = "test-webhook-secret";

    private static string ComputeSignature(byte[] body, string secret)
    {
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var hash = HMACSHA256.HashData(secretBytes, body);
        return "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    [Fact]
    public void IsValid_CorrectSignature_ShouldReturnTrue()
    {
        var body = Encoding.UTF8.GetBytes("{\"action\":\"opened\"}");
        var signature = ComputeSignature(body, Secret);

        WebhookSignatureValidator.IsValid(body, Secret, signature).Should().BeTrue();
    }

    [Fact]
    public void IsValid_TamperedBody_ShouldReturnFalse()
    {
        var originalBody = Encoding.UTF8.GetBytes("{\"action\":\"opened\"}");
        var signature = ComputeSignature(originalBody, Secret);
        var tamperedBody = Encoding.UTF8.GetBytes("{\"action\":\"closed\"}");

        WebhookSignatureValidator.IsValid(tamperedBody, Secret, signature).Should().BeFalse();
    }

    [Fact]
    public void IsValid_WrongSecret_ShouldReturnFalse()
    {
        var body = Encoding.UTF8.GetBytes("{\"action\":\"opened\"}");
        var signature = ComputeSignature(body, "wrong-secret");

        WebhookSignatureValidator.IsValid(body, Secret, signature).Should().BeFalse();
    }

    [Fact]
    public void IsValid_NullSignatureHeader_ShouldReturnFalse()
    {
        var body = Encoding.UTF8.GetBytes("{\"action\":\"opened\"}");

        WebhookSignatureValidator.IsValid(body, Secret, null).Should().BeFalse();
    }
}
