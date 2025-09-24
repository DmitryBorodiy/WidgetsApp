using BetterWidgets.Extensions;
using Newtonsoft.Json;
using System.Windows.Media;

namespace BetterWidgets.Behaviours.Converters
{
    public sealed class JsonColorConverter : JsonConverter<Color>
    {
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(!hasExistingValue) return Colors.Transparent;

            string hex = reader.Value.ToString();
            Color value = (Color)ColorConverter.ConvertFromString(hex);

            return value;
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            if(value == default) return;

            string hex = value.ToHex();
            writer.WriteValue(hex);
        }
    }
}
