using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions;

public static class StreamExtensions
{
    public static T ToDeserializedJson<T>(this Stream stream)
    {
        if (stream == null || stream.CanRead == false)
            return default(T);

        using (var sr = new StreamReader(stream))
        using (var jtr = new JsonTextReader(sr))
        {
            var js = new Newtonsoft.Json.JsonSerializer();
            var searchResult = js.Deserialize<T>(jtr);
            return searchResult;
        }
    }

    public static async Task<string> ToStringAsync(this Stream stream)
    {
        string content = null;

        if (stream != null)
            using (var sr = new StreamReader(stream))
                content = await sr.ReadToEndAsync();

        return content;
    }

    public static Task<string> ToStringAsync(this IHeaderDictionary headerDictionary)
    {
        var sb = new StringBuilder();
        foreach (var dic in headerDictionary)
        {
            sb.AppendFormat("{0} - {1}", dic.Key.ToString(), dic.Value.ToString());
            sb.Append(Environment.NewLine);
        }

        return Task.FromResult(sb.ToString());
    }
}
