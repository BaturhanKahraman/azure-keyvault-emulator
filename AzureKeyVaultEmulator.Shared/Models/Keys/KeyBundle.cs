using System.Text.Json.Serialization;

namespace AzureKeyVaultEmulator.Shared.Models.Keys
{
    public class KeyBundle : TaggedModel
    {
        [JsonPropertyName("key")]
        public JsonWebKeyModel Key { get; set; } = new();

        [JsonPropertyName("attributes")]
        public KeyAttributesModel Attributes { get; set; } = new();
    }
}
