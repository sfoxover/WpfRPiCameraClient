using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessagesLibrary
{
    public class MessageHelper
    {
        internal static void LoadJsonIntoMap(JObject json, Dictionary<string, object> map)
        {
            var items = json.Children();
            foreach(var item in items) 
            {
                if (item.type() == Json::ValueType::objectValue)
                {
                    std::map < std::string, std::any > mapTemp;
                    LoadJsonIntoMap(item, mapTemp);
                    map[name] = mapTemp;
                }
                else
                {
                    map[name] = MessageHelper::JsonToAnyValue(item);
                }
            }
        }

        internal static JToken AnyValueToJson(object value)
        {
            Json::Value value;
            try
            {
                if (object.type() == typeid(bool))
                {
                    value = std::any_cast<bool>(object);
                }
                else if (object.type() == typeid(short))
                {
                    value = std::any_cast<short>(object);
                }
                else if (object.type() == typeid(int))
                {
                    value = std::any_cast<int>(object);
                }
                else if (object.type() == typeid(const char*))
        {
                    value = std::any_cast <const char*> (object);
                }
        else if (object.type() == typeid(std::string))
                {
                    value = std::any_cast < std::string> (object);
                }
                else if (object.type() == typeid(int64_t))
                {
                    value = std::any_cast<int64_t>(object);
                }
                else if (object.type() == typeid(uint64_t))
                {
                    value = std::any_cast<uint64_t>(object);
                }
                else
                {
                    std::cerr << "MessageHelper::AnyValueToJson unsupported std::any type: " << object.type().name() << std::endl;
                    assert(false);
                }
            }
            catch (std::bad_any_cast &err)
    {
                std::cerr << "MessageHelper::AnyValueToJson bad_any_cast: " << err.what() << std::endl;
                assert(false);
            }
            return value;
            }
    }
}
