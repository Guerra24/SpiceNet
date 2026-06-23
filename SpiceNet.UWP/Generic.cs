using SpiceNet.UWP.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpiceNet.UWP;

public static class JsonSettings
{
    public static JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true,
        TypeInfoResolver = JsonSourceGenerationContext.Default,
    };
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, IncludeFields = true)]
[JsonSerializable(typeof(ServerProfile))]
[JsonSerializable(typeof(ObservableCollection<ServerProfile>))]
public partial class JsonSourceGenerationContext : JsonSerializerContext
{

}