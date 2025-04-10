using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using Url = Flurl.Url;

namespace Application.Extensions;

public static class StringExtensions
{
    public static T ToEnum<T>(this string value, T defaultValue) where T : struct
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        T result;
        return Enum.TryParse<T>(value, true, out result) ? result : defaultValue;
    }

    public static string Left(this string txt, int maxLength)
    {
        return txt.Substring(0, Math.Min(maxLength, txt.Length));
    }

    public static string NullEmptyString(this string s)
    {
        return (string.IsNullOrEmpty(s)) ? null : s;
    }

    public static string NullEmptyStringLeft(this string s, int maxLength)
    {
        return string.IsNullOrEmpty(s) ? null : s.Left(maxLength);
    }

    public static Uri Combine(this Uri uri, string name) => new Uri(Url.Combine(uri.ToString(), name));
    public static Uri Combine(this Uri uri1, Uri uri2) => new Uri(Url.Combine(uri1.ToString(), uri2.ToString()));

    public static string ToBase64(this string txt) => Convert.ToBase64String(Encoding.UTF8.GetBytes(txt));
    public static string ToBase64(this long num) => Convert.ToBase64String(Encoding.UTF8.GetBytes(num.ToString()));

    public static StringContent ToStringContent<T>(this T obj, Func<T, string>? func = null, string mediaType = "application/json")
    {
        if (func != null)
        {
            return new StringContent(func(obj), Encoding.UTF8, mediaType);
        }
        else
        {
            if (obj is string str) return new StringContent(str, Encoding.UTF8, mediaType);

            var json = JsonConvert.SerializeObject(obj);
            return new StringContent(json, Encoding.UTF8, mediaType);
        }
    }
}
