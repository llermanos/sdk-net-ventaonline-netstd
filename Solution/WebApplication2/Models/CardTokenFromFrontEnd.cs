using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication2.Models
{
    public class CardTokenFromFrontEnd
    {
        // -----------------------------
        // Propiedades que mapean al JSON
        // -----------------------------

        [JsonPropertyName("id")]                       // System.Text.Json
        // [JsonProperty("id")]                         // Newtonsoft
        public string Id { get; set; }                 // GUID en forma de string

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("card_number_length")]
        public int CardNumberLength { get; set; }

        [JsonPropertyName("date_created")]
        public DateTime DateCreated { get; set; }      // ISO 8601

        [JsonPropertyName("bin")]
        public string Bin { get; set; }

        [JsonPropertyName("last_four_digits")]
        public string LastFourDigits { get; set; }

        [JsonPropertyName("security_code_length")]
        public int SecurityCodeLength { get; set; }

        [JsonPropertyName("expiration_month")]
        public int ExpirationMonth { get; set; }

        [JsonPropertyName("expiration_year")]
        public int ExpirationYear { get; set; }

        [JsonPropertyName("date_due")]
        public DateTime DateDue { get; set; }

        // ---------------------------------
        // Métodos de ayuda (opcional)
        // ---------------------------------

        /// <summary>
        /// Convierte el objeto a JSON usando System.Text.Json.
        /// </summary>
        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // si quieres formato legible
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(this, options);
        }

        /// <summary>
        /// Crea una instancia de CardTokenFromFrontEnd a partir de un string JSON.
        /// </summary>
        public static CardTokenFromFrontEnd FromJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // permite que el JSON sea más flexible
            };
            return JsonSerializer.Deserialize<CardTokenFromFrontEnd>(json, options);
        }
    }
}
