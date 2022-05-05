using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PI.GestaoHospitalar.Core.Helpers
{
    public static class JsonHelper
    {
        public static string Serialize(this object value, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Serialize(value, options);
        }
        public async static Task<string> SerializeAsync(this object value, JsonSerializerOptions options = null)
        {
            return await Task.Run(() => Serialize(value, options));
        }

        public static T Deserialize<T>(this string json, JsonSerializerOptions options = null)
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }

        public async static Task<T> DeserializeAsync<T>(this string json, JsonSerializerOptions options = null)
        {
            return await Task.Run(() => Deserialize<T>(json, options));
        }

        public static string SelectToken(this string value, string path)
        {
            return JObject.Parse(value).SelectToken(path, false).ToString();
        }

        public static async Task<string> SelectTokenAsync(this string value, string path)
        {
            return await Task.Run(() => SelectToken(value, path));
        }

        public static string AddNewProperty(this string value, string name, string content)
        {
            var newProperty = new JProperty(name, content);
            var jObject = JObject.Parse(value);
            jObject.Last.AddAfterSelf(newProperty);
            return jObject.ToString();
        }

        public static async Task<string> AddNewPropertyAsync(this string value, string name, string content)
        {
            return await Task.Run(() => AddNewProperty(value, name, content));
        }
    }
}
