using System.Text.Json.Serialization;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace KairosWebAPI.Models.ResponseResults
{
    public class DocTypeResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string? odatacontext { get; set; }
        public List<T>? value { get; set; }
    }

    public class DocTypeValue
    {
        public string? DocTypeID { get; set; }
        public string? Description { get; set; }
    }
}

