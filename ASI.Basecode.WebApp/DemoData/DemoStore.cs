using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace ASI.Basecode.WebApp.DemoData;

/// <summary>
/// The one in-memory demo store (register M-01). After 6.9 it holds only the demo pass token;
/// issuance, attendance and certificate reviews are real SQL rows and every issued PDF is retained in
/// the private file store. Nothing here touches SQL, the private file store or the Activity Log.
/// </summary>
public sealed class DemoStore
{
    private readonly string _tokenKey;

    public DemoStore(IOptions<DemoDataOptions> options)
    {
        var tokenKey = options?.Value?.TokenKey;
        if (string.IsNullOrWhiteSpace(tokenKey))
        {
            // The startup guard already refuses this; keep the store unusable rather than guess a key.
            throw new InvalidOperationException("DemoData:TokenKey is required when demo data is enabled.");
        }

        _tokenKey = tokenKey;
    }

    /// <summary>
    /// The stable demo pass token: HMAC-SHA256 of the registration id under
    /// <c>DemoData:TokenKey</c>. The same registration id always derives the same token, so a pass
    /// printed before a restart keeps scanning.
    /// </summary>
    public static string DerivePassToken(int registrationId, string tokenKey)
    {
        if (registrationId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(registrationId));
        }

        if (string.IsNullOrWhiteSpace(tokenKey))
        {
            throw new ArgumentException("A token key is required to derive a pass token.", nameof(tokenKey));
        }

        var message = Encoding.UTF8.GetBytes(registrationId.ToString(CultureInfo.InvariantCulture));
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(tokenKey), message);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>The stable demo pass token for a real registration id.</summary>
    public string GetPassToken(int registrationId) => DerivePassToken(registrationId, _tokenKey);
}
