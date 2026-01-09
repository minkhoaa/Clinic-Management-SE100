using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace ClinicManagement_API.Features.billing_service.helper;


public sealed class VnPayOptions
{
    public string Version { get; init; } = "2.1.0";
    public string TmnCode { get; init; } = default!;
    public string HashSecret { get; init; } = default!;
    public string PaymentUrl { get; init; } = default!;
    public string ReturnUrl { get; init; } = default!;
    public string IpnUrl { get; init; } = default!;
    public string Locale { get; init; } = "vn";
    public string CurrCode { get; init; } = "VND";
}

public static class VnPayHelper
{
    public static string BuildPaymentUrl(string baseUrl, SortedDictionary<string, string> vnpParams, string hashSecret)
    {
        // Hash data: key=value&key=value... (url-encode key & value)
        var hashData = string.Join("&",
            vnpParams
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

        var secureHash = HmacSha512(hashSecret, hashData);

        // Build actual redirect URL
        var url = QueryHelpers.AddQueryString(baseUrl, vnpParams.ToDictionary(x => x.Key, x => x.Value));
        url = QueryHelpers.AddQueryString(url, "vnp_SecureHash", secureHash);
        return url;
    }

    public static bool VerifySignature(IDictionary<string, string> allParams, string hashSecret)
    {
        if (!allParams.TryGetValue("vnp_SecureHash", out var received) || string.IsNullOrWhiteSpace(received))
            return false;
        var data = new Dictionary<string, string>(allParams, StringComparer.Ordinal);
        data.Remove("vnp_SecureHash");
        data.Remove("vnp_SecureHashType");

        var sorted = new SortedDictionary<string, string>(data, StringComparer.Ordinal);
        var hashData = string.Join("&",
            sorted
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

        var computed = HmacSha512(hashSecret, hashData);
        return string.Equals(computed, received, StringComparison.OrdinalIgnoreCase);
    }

    private static string HmacSha512(string key, string input)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}

    
