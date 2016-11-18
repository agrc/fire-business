using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace fire_business_soe.Encoding
{
    public abstract class JsonEndpoint
    {
        /// <summary>
        ///     Simplified method for returning json
        /// </summary>
        /// <param name="response"> The response parsed to json by json.net and converted to a byte array. </param>
        /// <returns> </returns>
        internal static byte[] Json(object response)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var value = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response, settings));

            return value;
        }
    }
}