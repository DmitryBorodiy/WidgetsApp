using Newtonsoft.Json;

namespace BetterWidgets.Behaviours.Converters
{
    public sealed class StringTimeZoneConverter : JsonConverter<TimeZoneInfo>
    {
        public override TimeZoneInfo ReadJson(JsonReader reader, Type objectType, TimeZoneInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string value = reader.Value.ToString();

            var timezones = TimeZoneInfo.GetSystemTimeZones();
            var timezone = timezones.FirstOrDefault(tz => tz.DisplayName == value);

            return timezone;
        }

        public override void WriteJson(JsonWriter writer, TimeZoneInfo value, JsonSerializer serializer)
        {
            writer.WriteValue(value.DisplayName);
        }
    }
}
