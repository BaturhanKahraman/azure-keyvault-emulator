﻿using System.Text.Json.Serialization;

namespace AzureKeyVaultEmulator.Shared.Models
{
    public class AttributeBase
    {
        public AttributeBase()
        {
            var now = DateTimeOffset.Now;

            Created = now.ToUnixTimeSeconds();
            Updated = now.ToUnixTimeSeconds();
            NotBefore = now.ToUnixTimeSeconds();
            Expiration = now.AddDays(365).ToUnixTimeSeconds();
        }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("exp")]
        public long Expiration { get; set; }

        [JsonPropertyName("nbf")]
        public long NotBefore { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("updated")]
        public long Updated { get; set; }

        [JsonPropertyName("recoveryLevel")]
        public string RecoveryLevel = DeletionRecoveryLevel.Purgable.ToString();

        public void Update() => Updated = DateTimeOffset.Now.ToUnixTimeSeconds();
    }
}
