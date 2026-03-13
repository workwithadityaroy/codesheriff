using System.Net.Http.Json;
using System.Text;
using CodeSheriff.Application.Common.Interfaces;
using CodeSheriff.Application.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeSheriff.Infrastructure.Services.Email;

internal sealed class ResendEmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly ResendOptions _options;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        IHttpClientFactory httpClientFactory,
        IOptions<ResendOptions> options,
        ILogger<ResendEmailService> logger)
    {
        _http = httpClientFactory.CreateClient("resend");
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendWeeklyReportAsync(
        string toEmail,
        string displayName,
        WeeklyReportData data,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogWarning("Resend API key not configured — skipping weekly report email.");
            return;
        }

        var html = BuildHtml(displayName, data);

        var payload = new
        {
            from = $"{_options.FromName} <{_options.FromEmail}>",
            to = new[] { toEmail },
            subject = $"Your Weekly Code Quality Report — {DateTime.UtcNow:MMMM dd, yyyy}",
            html
        };

        var response = await _http.PostAsJsonAsync("emails", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Resend API error {Status}: {Body}", (int)response.StatusCode, body);
            throw new HttpRequestException(
                $"Resend API returned {(int)response.StatusCode}: {body}",
                inner: null,
                statusCode: response.StatusCode);
        }

        _logger.LogInformation("Weekly report email sent to {Email}", toEmail);
    }

    private static string BuildHtml(string displayName, WeeklyReportData data)
    {
        var sb = new StringBuilder();
        sb.Append("""
            <!DOCTYPE html>
            <html>
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
            </head>
            <body style="margin:0;padding:0;background:#0a0a0a;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" border="0" style="background:#0a0a0a;padding:40px 16px;">
                <tr><td align="center">
                  <table width="560" cellpadding="0" cellspacing="0" border="0" style="max-width:560px;width:100%;">

                    <!-- Logo -->
                    <tr><td style="padding-bottom:32px;">
                      <span style="display:inline-flex;align-items:center;gap:8px;">
                        <span style="display:inline-block;width:28px;height:28px;background:linear-gradient(135deg,#3b82f6,#7c3aed);border-radius:8px;"></span>
                        <span style="font-size:15px;font-weight:700;color:#ffffff;letter-spacing:-0.3px;">CodeSheriff</span>
                      </span>
                    </td></tr>

                    <!-- Heading -->
                    <tr><td style="padding-bottom:8px;">
                      <h1 style="margin:0;font-size:22px;font-weight:700;color:#ffffff;letter-spacing:-0.5px;">Weekly Code Quality Report</h1>
                    </td></tr>
                    <tr><td style="padding-bottom:32px;">
                      <p style="margin:0;font-size:14px;color:#737373;">
                        Hi <strong style="color:#a3a3a3;">
            """);
        sb.Append(System.Net.WebUtility.HtmlEncode(displayName));
        sb.Append("""
                        </strong> — here's your code quality summary for the past 7 days.
                      </p>
                    </td></tr>

                    <!-- Stats row -->
                    <tr><td style="padding-bottom:24px;">
                      <table width="100%" cellpadding="0" cellspacing="0" border="0">
                        <tr>
            """);

        AppendStatCell(sb, "PRs Reviewed", data.TotalReviewed.ToString(), "#3b82f6");
        AppendStatCell(sb, "Avg Tech Debt", $"{data.AverageTechDebtScore}/100", GetScoreColor(data.AverageTechDebtScore));
        AppendStatCell(sb, "Critical Issues", data.CriticalIssueCount.ToString(), data.CriticalIssueCount > 0 ? "#ef4444" : "#22c55e");

        sb.Append("""
                        </tr>
                      </table>
                    </td></tr>

                    <!-- Repos table -->
            """);

        if (data.Repos.Count > 0)
        {
            sb.Append("""
                    <tr><td style="padding-bottom:8px;">
                      <p style="margin:0;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:0.08em;color:#525252;">Repositories</p>
                    </td></tr>
                    <tr><td style="padding-bottom:32px;">
                      <table width="100%" cellpadding="0" cellspacing="8" border="0">
                        <tr>
                          <td style="font-size:11px;font-weight:600;color:#525252;padding-bottom:4px;">REPOSITORY</td>
                          <td align="center" style="font-size:11px;font-weight:600;color:#525252;padding-bottom:4px;">PRs</td>
                          <td align="center" style="font-size:11px;font-weight:600;color:#525252;padding-bottom:4px;">AVG DEBT</td>
                        </tr>
            """);

            foreach (var repo in data.Repos)
            {
                sb.Append($"""
                        <tr>
                          <td style="padding:10px 0;border-top:1px solid #262626;font-size:13px;color:#d4d4d4;font-family:monospace;">{System.Net.WebUtility.HtmlEncode(repo.FullName)}</td>
                          <td align="center" style="padding:10px 0;border-top:1px solid #262626;font-size:13px;color:#a3a3a3;">{repo.PrsReviewed}</td>
                          <td align="center" style="padding:10px 0;border-top:1px solid #262626;font-size:13px;color:{GetScoreColor(repo.AvgDebtScore)};font-weight:600;">{repo.AvgDebtScore}</td>
                        </tr>
                """);
            }

            sb.Append("""
                      </table>
                    </td></tr>
            """);
        }

        sb.Append("""
                    <!-- Footer -->
                    <tr><td style="border-top:1px solid #1a1a1a;padding-top:24px;">
                      <p style="margin:0;font-size:12px;color:#404040;">
                        You're receiving this because you have repositories connected to CodeSheriff.
                        <br/>© CodeSheriff — AI-Powered Code Review
                      </p>
                    </td></tr>

                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """);

        return sb.ToString();
    }

    private static void AppendStatCell(StringBuilder sb, string label, string value, string color)
    {
        sb.Append($"""
            <td width="33%" style="padding:4px;">
              <table width="100%" cellpadding="0" cellspacing="0" border="0"
                     style="background:#111111;border:1px solid #1f1f1f;border-radius:10px;padding:16px 12px;">
                <tr><td style="font-size:22px;font-weight:700;color:{color};letter-spacing:-0.5px;">{System.Net.WebUtility.HtmlEncode(value)}</td></tr>
                <tr><td style="font-size:11px;color:#525252;padding-top:4px;font-weight:500;text-transform:uppercase;letter-spacing:0.05em;">{System.Net.WebUtility.HtmlEncode(label)}</td></tr>
              </table>
            </td>
            """);
    }

    private static string GetScoreColor(decimal score) => score switch
    {
        <= 30 => "#22c55e",
        <= 60 => "#f59e0b",
        <= 80 => "#f97316",
        _ => "#ef4444"
    };
}
