using System.Text.Json.Serialization;
using System.Text.Json;

namespace BlogPlatformCleanArchitecture.Domain.Enums
{
    public class RoleJsonConverter : JsonConverter<Role>
    {
        public override Role Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var roleValue = reader.GetString();
            if (Enum.TryParse(typeof(Role), roleValue, true, out var role) && role is Role validRole)
            {
                return validRole;
            }

            throw new JsonException($"Invalid role: {roleValue}. Allowed values are Reader, Author, and Admin.");
        }

        public override void Write(Utf8JsonWriter writer, Role value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
