using System.Text.Json.Serialization;
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace KairosWebAPI.Models.ResponseResults
{
    public class KSingleResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string? odatacontext { get; set; }
        public T? value { get; set; }
    }
}