using Newtonsoft.Json;

namespace BetterWidgets.Behaviours.Converters
{
    public sealed class UnixTimeZoneConverter : JsonConverter<TimeZoneInfo>
    {
        public override TimeZoneInfo ReadJson(JsonReader reader, Type objectType, TimeZoneInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string value = reader.Value.ToString();

            if(string.IsNullOrWhiteSpace(value)) return null;

            int timezone = Convert.ToInt32(value);
            var offset = TimeSpan.FromSeconds(timezone);

            return TimeZoneInfo.CreateCustomTimeZone($"UTC{offset.Hours:+#;-#;0}", offset, $"UTC{offset}", $"UTC{offset}");
        }

        public override void WriteJson(JsonWriter writer, TimeZoneInfo value, JsonSerializer serializer)
        {
            writer.WriteValue(value.BaseUtcOffset.TotalSeconds.ToString());
        }
    }
}
