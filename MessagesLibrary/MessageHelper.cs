using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MessagesLibrary
{
    public class MessageHelper
    {
        // Load video settings into map
        public static Dictionary<string, object> LoadSettingsFromConfig(string configFilePath)
        {
            var settings = new Dictionary<string, object>();
            if (File.Exists(configFilePath))
            {
                // Read config file
                try
                {
                    JObject root = null;
                    using (StreamReader reader = File.OpenText(configFilePath))
                    {
                        root = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                    }
                    if (root != null)
                    {
                        LoadJsonIntoMap(root, ref settings);
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"LoadSettingsFromConfig error {ex.Message}");
                    Debug.Assert(false);
                }
            }
            return settings;
        }

        public static void LoadJsonIntoMap(JObject json, ref Dictionary<string, object> map)
        {
            foreach (var item in json) 
            {
                if (item.Value.Type == JTokenType.Object)
                {
                    var mapTemp = new Dictionary<string, object>();
                    LoadJsonIntoMap((JObject)item.Value, ref mapTemp);
                    map[item.Key] = mapTemp;
                }
                else if (item.Value.Type == JTokenType.Array)
                {
                    List<object> array = new List<object>();
                    foreach(var arrayItem in item.Value) 
                    {
                        array.Add(JsonToAnyValue((JToken)arrayItem));
                    }
                    map[item.Key] = array;
                }
                else
                {
                    map[item.Key] = JsonToAnyValue((JToken)item.Value);
                }
            }
        }

        // Convert json value to std::any object
        private static object JsonToAnyValue(JToken json)
        {
            object value = null;
            try
            {
                switch (json.Type)
                {
                    case JTokenType.Boolean:
                        {
                            value = (Boolean)json;
                            break;
                        }
                    case JTokenType.Integer:
                        {
                            value = (Int64)json;
                            break;
                        }
                    case JTokenType.String:
                        {
                            value = (string)json;
                            break;
                        }
                    case JTokenType.Float:
                        {
                            value = (float)json;
                            break;
                        }
                    case JTokenType.Array:
                        {
                            var array = new List<object>();
                            foreach (var item in json.Children<JObject>())
                            {
                                array.Add(JsonToAnyValue(item));
                            }
                            value = array;
                            break;
                        }
                    default:
                        {
                            Debug.WriteLine($"MessageHelper::JsonToAnyValue unsupported std::any type: json.Type");
                            Debug.Assert(false);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MessageHelper::JsonToAnyValue bad_any_cast: {ex.Message}");
                Debug.Assert(false);
            }
            return value;
        }

        internal static JToken AnyValueToJson(object value)
        {
            var result = JToken.FromObject(value);
            return result;
        }
    }
}
