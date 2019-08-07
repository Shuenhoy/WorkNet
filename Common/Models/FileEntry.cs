using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkNet.Common.Models
{
    public class FileEntry
    {
        public int FileEntryID { get; set; }
        public string SeaweedId { get; set; }
        [Column(TypeName = "jsonb")]
        [JsonIgnore]
        [JsonPropertyName("__metaraw")]
        public string Metadata { get; set; }
        [NotMapped]
        [JsonPropertyName("Metadata")]
        public JsonElement _Metadata
        {
            get => JsonDocument.Parse(Metadata is null ? "{}" : Metadata).RootElement;
            set => Metadata = value.ToString();
        }

        public int Size { get; set; }
        public string ETag { get; set; }
        public List<string> Tags { get; set; }
        public string ExtName { get; set; }
        public string FileName { get; set; }
        public string Namespace { get; set; }

    }
}