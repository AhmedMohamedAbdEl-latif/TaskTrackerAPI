using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskTrackerAPI.Validation
{
    /// <summary>
    /// Treats timestamps without an explicit offset as UTC and normalizes
    /// timestamps with offsets to UTC. SQL datetime2 values in this API are
    /// stored as UTC because datetime2 itself does not retain a time zone.
    /// </summary>
    public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String ||
                !DateTimeOffset.TryParse(
                    reader.GetString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsed))
            {
                throw new JsonException("Timestamp must be a valid ISO 8601 value.");
            }

            return parsed.UtcDateTime;
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options)
        {
            // Existing datetime2 values are defined by this API as UTC ticks.
            writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
        }
    }

    public sealed class NullableUtcDateTimeJsonConverter : JsonConverter<DateTime?>
    {
        public override bool HandleNull => true;

        public override DateTime? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            return new UtcDateTimeJsonConverter().Read(
                ref reader,
                typeof(DateTime),
                options);
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime? value,
            JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            new UtcDateTimeJsonConverter().Write(writer, value.Value, options);
        }
    }
}
