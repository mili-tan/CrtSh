using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrtSh
{
    public class CrtshSharp
    {
        public string Url { get; set; } = "https://crt.sh/";

        public CrtshSharp(string url = "https://crt.sh/")
        {
            Url = url;
        }

        public async Task<List<CertificateInformation>> Search(string query)
        {
            var response = await new HttpClient().GetStringAsync(Url + $"?q={query}&output=json");
            return JsonSerializer.Deserialize<List<CertificateInformation>>(response) ?? [];
        }

        public class CertificateInformation
        {
            [JsonPropertyName("issuer_ca_id")]
            public int? IssuerCaId { get; set; }

            [JsonPropertyName("issuer_name")]
            public string? IssuerName { get; set; }

            [JsonPropertyName("common_name")]
            public string? CommonName { get; set; }

            [JsonPropertyName("name_value")]
            public string? NameValue { get; set; }

            [JsonPropertyName("id")]
            public long? Id { get; set; }

            [JsonPropertyName("entry_timestamp")]
            public DateTime? EntryTimestamp { get; set; }

            [JsonPropertyName("not_before")]
            public DateTime? NotBefore { get; set; }

            [JsonPropertyName("not_after")]
            public DateTime? NotAfter { get; set; }

            [JsonPropertyName("serial_number")]
            public string? SerialNumber { get; set; }

            [JsonPropertyName("result_count")]
            public int? ResultCount { get; set; }

            public override string ToString()
            {
                return
                    $"Issuer CA ID: {IssuerCaId} " + Environment.NewLine +
                    $"Issuer Name: {IssuerName} " + Environment.NewLine +
                    $"Common Name: {CommonName} " + Environment.NewLine +
                    $"Name Value: {NameValue} " + Environment.NewLine +
                    $"ID: {Id} " + Environment.NewLine +
                    $"Entry Timestamp: {EntryTimestamp} " + Environment.NewLine +
                    $"Not Before: {NotBefore} " + Environment.NewLine +
                    $"Not After: {NotAfter} " + Environment.NewLine +
                    $"Serial Number: {SerialNumber} " + Environment.NewLine +
                    $"Result Count: {ResultCount} ";
            }
        }
    }
}
