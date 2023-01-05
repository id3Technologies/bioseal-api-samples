using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BioSealWSCodeSamples
{
    class DictionaryTools
    {
        public static Dictionary<string, object> ConvertBiographicsToDictionary(object biographics)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            Type type = biographics.GetType();
            FieldInfo[] infos = type.GetFields();

            foreach (FieldInfo info in infos)
            {
                string key = info.Name;
                object value = info.GetValue(biographics);
                if (value == null)
                    continue;

                Type valueType = value.GetType();

                dict.Add(key, value);
            }

            return dict;
        }

        public static bool AreDictionariesIdentical(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
        {
            // handle null cases
            if (dict1 == null && dict2 == null)
                return true;
            if (dict1 == null || dict2 == null)
                return false;

            // remove face data
            if (dict1.ContainsKey("FaceImageBase64"))
                dict1.Remove("FaceImageBase64");
            if (dict2.ContainsKey("FaceImageBase64"))
                dict2.Remove("FaceImageBase64");

            // check length
            if (dict1.Count != dict1.Count)
                return false;

            // keys/values comparison
            int index = 0;
            foreach (KeyValuePair<string, object> keyValue1 in dict1)
            {
                string key1 = keyValue1.Key;
                object value1 = keyValue1.Value;

                KeyValuePair<string, object> keyValue2 = dict2.ElementAt(index);
                string key2 = keyValue2.Key;
                object value2 = keyValue2.Value;

                if (String.Compare(key1, key2) != 0)
                    return false;

                if (String.Compare(value1 as String, value2 as String) != 0)
                    return false;

                index++;
            }

            return true;
        }
    }
}
