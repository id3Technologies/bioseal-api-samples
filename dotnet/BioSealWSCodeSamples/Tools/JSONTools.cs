using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BioSealWSCodeSamples
{
    public static class JSONTools
    {
        #region Serialize / Parse methods

        public static string Serialize<T>(T data)
        {
            // serialize user data
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            serializer.WriteObject(stream, data);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string res = reader.ReadToEnd();

            //res = removeEscapedSlashes(res);
            return res;
        }

        public static T Deserialize<T>(string json)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            T data = (T)serializer.ReadObject(stream);
            stream.Close();

            return data;
        }

        public static string SerializeArray<T>(T[] array)
        {
            return Serialize<T[]>(array);
        }

        public static T[] DeserializeArray<T>(string json)
        {
            // deserialize user info
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T[]));
            T[] data = serializer.ReadObject(stream) as T[];
            stream.Close();

            return data;
        }

        #endregion

        #region Encoding methods

        // build biographic data
        // (cf. section 2.3 in specifications)
        public static byte[] BuildJSONEncodedBiographics(Dictionary<string, object> biographics)
        {
            BiographicData biographicData = new BiographicData();
            biographicData.Dictionary = biographics;

            // Recover data in JSON format
            MemoryStream memoryStream = new MemoryStream();
            DataContractJsonSerializerSettings settings =
                new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BiographicData), settings);
            serializer.WriteObject(memoryStream, biographicData);
            memoryStream.Position = 0;
            #if DEBUG
                StreamReader reader = new StreamReader(memoryStream);
                string chaine = reader.ReadToEnd();
            #endif

            byte[] biographicEncodedData = memoryStream.GetBuffer();

            // trim ending zeros
            biographicEncodedData = ByteTools.TrimEndByteArray(biographicEncodedData);

            return biographicEncodedData;
        }

        public static string BuildJSONEncodedBiographicsFace(Dictionary<string, object> biographics, Bitmap faceImage)
        {
            BiographicFaceData biographicFaceData = new BiographicFaceData();
            biographicFaceData.Dictionary = biographics;

            // convert face image to base 64
            string faceBase64String = Base64Tools.Base64EncodeImage(faceImage, ImageFormat.Png);
            biographicFaceData.FaceBase64String = faceBase64String;

            // Recover data in JSON format
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializerSettings settings =
                new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BiographicFaceData), settings);
            serializer.WriteObject(stream, biographicFaceData);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string res = reader.ReadToEnd();

            //res = removeEscapedSlashes(res);
            return res;
        }

        #endregion

        #region Parse methods

        // extract JSON-encoded into BiographicData
        public static Dictionary<string, object> ExtractBiographicsFromJSON(byte[] data)
        {
            // convert as UTF8 buffer
            string foo = Encoding.UTF8.GetString(data).TrimEnd('\0');
            data = Encoding.UTF8.GetBytes(foo);

            using (XmlDictionaryReader jsonReader = JsonReaderWriterFactory.CreateJsonReader(data, XmlDictionaryReaderQuotas.Max))
            {
                DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
                settings.UseSimpleDictionaryFormat = true;
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BiographicData), settings);

                BiographicData biographicData = (BiographicData)serializer.ReadObject(jsonReader);
                return biographicData.Dictionary;
            }
        }

        public static BioSealOnlineVerifyResult DeserializeBioSealOnlineMatchResult(string json)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.UseSimpleDictionaryFormat = true;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BioSealOnlineVerifyResult), settings);

            BioSealOnlineVerifyResult res = (BioSealOnlineVerifyResult)serializer.ReadObject(stream) as BioSealOnlineVerifyResult;
            return res;
        }

        // supervised JSON-encoded biographics data parse
        // format: {Fields: {key1:value1 , key2:value2 , ...} }
        public static Dictionary<string, object> ParseJSONBiographics(string text)
        {
            Dictionary<string, object> biographics = new Dictionary<string, object>();

            try
            {
                text = text.TrimEnd('\0'); // remove ending empty chars

                // get fields key/value
                string fieldsKey;
                string fieldsValue;
                getKeyValueFromJSONString(text, out fieldsKey, out fieldsValue);

                // get key/values list
                String[] keyValues = getKeyValueTexts(fieldsValue);
                foreach (String keyValue in keyValues)
                {
                    string key;
                    string value;
                    getKeyValueFromJSONString(keyValue, out key, out value);

                    if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(value))
                        biographics.Add(key, value);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return biographics;
        }

        // get key/value from JSON string (form {"<key>":"value"})
        private static void getKeyValueFromJSONString(string text, out string key, out string value)
        {
            // default values
            value = null;
            key = null;

            string info = text.Clone() as string;
            info = removeParenthesis(info);

            String[] keyValue = info.Split(new char[] { ':' }, 2);
            if (keyValue == null || keyValue.Length < 2)
                return;

            // ok, remove '\"' starting and ending characters
            key = removeEscapedCharacters(keyValue[0]);
            value = removeEscapedCharacters(keyValue[1]);
        }

        private static String[] getKeyValueTexts(string text)
        {
            string info = text.Clone() as string;
            info = removeParenthesis(info);

            String[] keyValues = info.Split(new char[] { ',' });
            return keyValues;
        }

        // remove opening and closing parenthesis
        private static string removeParenthesis(string text)
        {
            string res = text;

            if (res.StartsWith("{"))
                res = res.Substring(1);
            if (res.EndsWith("}"))
                res = res.Substring(0, res.Length - 1);

            return res;
        }

        private static string removeEscapedCharacters(string text)
        {
            string res = text;

            if (res.StartsWith("\""))
                res = res.Substring(1);
            if (res.EndsWith("\""))
                res = res.Substring(0, res.Length - 1);

            return res;
        }

        #endregion
    }


    /// <summary>
    /// Data container for the biographics of an individual.
    /// </summary>
    [DataContract]
    [XmlRoot(ElementName = "BiographicData")]
    class BiographicData
    {
        //[XmlIgnore]
        [DataMember(Name = "Fields")]
        private Dictionary<string, object> _fields;
        [XmlIgnore]
        ObservableCollection<CandidateBiographicItem> items;

        /// <summary>
        /// Initializes a new instance of the BiographicData class.
        /// </summary>
        public BiographicData()
        {
            _fields = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            items = new ObservableCollection<CandidateBiographicItem>();
            items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (CandidateBiographicItem item in e.NewItems)
                {
                    if (!_fields.ContainsKey(item.name))
                        _fields.Add(item.name, item.value);
                    else if ((string)_fields[item.name] != item.value)
                        _fields[item.name] = item.value;
                }
            }
            //else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            //{
            //    foreach(CandidateBiographicItem item in e.OldItems)
            //    {
            //        Remove(item.name);
            //    }
            //}
        }

        /// <summary>
        /// Gets a value indicating the number of elements in the Biographic Data.
        /// </summary>
        /// <returns>Number of biographical field.</returns>
        public int Count
        {
            get
            {
                return _fields.Count;
            }
        }

        /// <summary>
        /// Initiliazes the biographic data keys.
        /// </summary>
        /// <param name="keys">List of database fieldnames</param>
        public void Initialize(List<string> keys)
        {
            _fields.Clear();
            foreach (string key in keys)
            {
                _fields.Add(key, null);
                items.Add(new CandidateBiographicItem { name = key, value = null });
            }
        }

        ///// <summary>
        ///// Adds a new biographic data entry
        ///// </summary>
        ///// <param name="key">Fieldname in the database</param>
        ///// <param name="value">Value of the field</param>
        //public void Add(string key, object value)
        //{
        //    if (!_fields.ContainsKey(key))
        //        _fields.Add(key, value);
        //    else
        //        _fields[key] = value;
        //    //args = new KeyValuePair<string, string>(key, value); ?
        //    if (DataChanged != null)
        //        DataChanged(this, new DataChangedEventArgs(null, new KeyValuePair<string, object>(key, value)));
        //}

        /// <summary>
        /// Removes the corresponding element from the biographic data.
        /// </summary>
        /// <param name="key">Fieldname of the element to remove</param>
        public void Remove(string key)
        {
            if (_fields.ContainsKey(key))
            {
                KeyValuePair<string, object> oldValue = new KeyValuePair<string, object>(key, _fields[key]);
                _fields.Remove(key);
            }
        }

        /// <summary>
        /// Indicates whether the biographic data contains a element with this key.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if the biographic data contains such a fieldname
        /// False otherwise.</returns>
        public bool ContainsKey(string key)
        {
            return _fields.ContainsKey(key);
        }

        /// <summary>
        /// Gets the list of biographic data identifiers.
        /// </summary>
        [XmlIgnore]
        public List<string> Keys
        {
            get { return _fields.Keys.ToList(); }
        }

        /// <summary>
        /// Obtains or modify the element with the specified fieldname.
        /// </summary>
        /// <param name="key">Fieldname of the element</param>
        /// <returns>The element with the specified fieldname.</returns>
        public object this[string key]
        {
            get
            {
                if (_fields.ContainsKey(key))
                    return _fields[key];
                else
                    return null;
            }
            set
            {
                if (key != null)
                {
                    _fields[key] = value;

                    if (items.Count(p => String.Compare(p.name, key, true) == 0) == 0)
                        items.Add(new CandidateBiographicItem() { name = key, value = Convert.ToString(value) });
                    else
                        items.First(p => String.Compare(p.name, key, true) == 0).value = Convert.ToString(value);
                }
                //else
                //    throw new ArgumentException("The key " + key + " does not exist");
            }
        }

        /// <summary>
        /// Gets or sets the biographical fields as a Dictionary.
        /// </summary>
        [XmlIgnore]
        public Dictionary<string, object> Dictionary
        {
            get
            {
                return _fields;
            }
            set
            {
                this._fields = value;

                items.Clear();

                foreach (KeyValuePair<string, object> item in this._fields)
                {
                    items.Add(new CandidateBiographicItem { name = item.Key, value = item.Value as string });
                }
            }
        }

        [XmlIgnore]
        public ObservableCollection<CandidateBiographicItem> BiographicItemsList
        {
            get
            {
                //if (items.Count == 0 && _fields.Count != 0 || items.Count < _fields.Count)
                //{
                //    // items = new ObservableCollection<CandidateBiographicItem>();
                //    items.Clear();
                //    foreach (string key in _fields.Keys)
                //    {
                //        try
                //        {
                //            items.Add(new CandidateBiographicItem { name = key, value = Convert.ToString(_fields[key]) });
                //        }
                //        catch { }
                //    }
                //}
                //if (items.Count != _fields.Count)
                //{
                //    items.Clear();

                //    foreach (string key in _fields.Keys)
                //    {
                //        try
                //        {
                //            items.Add(new CandidateBiographicItem { name = key, value = Convert.ToString(_fields[key]) });
                //        }
                //        catch { }
                //    }
                //}
                return items;
            }
            //set
            //{
            //    _fields = new Dictionary<string, object>();

            //    foreach (CandidateBiographicItem item in value)
            //    {
            //        _fields.Add(item.name, item.value);
            //    }
            //    this.items = value;
            //}
        }
    };

    /// <summary>
    /// Data container for the biographics of an individual.
    /// </summary>
    [DataContract]
    [XmlRoot(ElementName = "BiographicFaceData")]
    class BiographicFaceData
    {
        //[XmlIgnore]
        [DataMember(Order = 1, Name = "biographics")]
        private Dictionary<string, object> _fields;
        [XmlIgnore]
        ObservableCollection<CandidateBiographicItem> items;

        //[XmlIgnore]
        [DataMember(Order = 2, Name = "faceImage")]
        public string FaceBase64String;

        /// <summary>
        /// Initializes a new instance of the BiographicData class.
        /// </summary>
        public BiographicFaceData()
        {
            _fields = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            items = new ObservableCollection<CandidateBiographicItem>();
            items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (CandidateBiographicItem item in e.NewItems)
                {
                    if (!_fields.ContainsKey(item.name))
                        _fields.Add(item.name, item.value);
                    else if ((string)_fields[item.name] != item.value)
                        _fields[item.name] = item.value;
                }
            }
            //else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            //{
            //    foreach(CandidateBiographicItem item in e.OldItems)
            //    {
            //        Remove(item.name);
            //    }
            //}
        }

        /// <summary>
        /// Gets a value indicating the number of elements in the Biographic Data.
        /// </summary>
        /// <returns>Number of biographical field.</returns>
        public int Count
        {
            get
            {
                return _fields.Count;
            }
        }

        /// <summary>
        /// Initiliazes the biographic data keys.
        /// </summary>
        /// <param name="keys">List of database fieldnames</param>
        public void Initialize(List<string> keys)
        {
            _fields.Clear();
            foreach (string key in keys)
            {
                _fields.Add(key, null);
                items.Add(new CandidateBiographicItem { name = key, value = null });
            }
        }

        /// <summary>
        /// Removes the corresponding element from the biographic data.
        /// </summary>
        /// <param name="key">Fieldname of the element to remove</param>
        public void Remove(string key)
        {
            if (_fields.ContainsKey(key))
            {
                KeyValuePair<string, object> oldValue = new KeyValuePair<string, object>(key, _fields[key]);
                _fields.Remove(key);
            }
        }

        /// <summary>
        /// Indicates whether the biographic data contains a element with this key.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if the biographic data contains such a fieldname
        /// False otherwise.</returns>
        public bool ContainsKey(string key)
        {
            return _fields.ContainsKey(key);
        }

        /// <summary>
        /// Gets the list of biographic data identifiers.
        /// </summary>
        [XmlIgnore]
        public List<string> Keys
        {
            get { return _fields.Keys.ToList(); }
        }

        /// <summary>
        /// Obtains or modify the element with the specified fieldname.
        /// </summary>
        /// <param name="key">Fieldname of the element</param>
        /// <returns>The element with the specified fieldname.</returns>
        public object this[string key]
        {
            get
            {
                if (_fields.ContainsKey(key))
                    return _fields[key];
                else
                    return null;
            }
            set
            {
                if (key != null)
                {
                    _fields[key] = value;

                    if (items.Count(p => String.Compare(p.name, key, true) == 0) == 0)
                        items.Add(new CandidateBiographicItem() { name = key, value = Convert.ToString(value) });
                    else
                        items.First(p => String.Compare(p.name, key, true) == 0).value = Convert.ToString(value);
                }
                //else
                //    throw new ArgumentException("The key " + key + " does not exist");
            }
        }

        /// <summary>
        /// Gets or sets the biographical fields as a Dictionary.
        /// </summary>
        [XmlIgnore]
        public Dictionary<string, object> Dictionary
        {
            get
            {
                return _fields;
            }
            set
            {
                this._fields = value;

                items.Clear();

                foreach (KeyValuePair<string, object> item in this._fields)
                {
                    items.Add(new CandidateBiographicItem { name = item.Key, value = item.Value as string });
                }
            }
        }

        [XmlIgnore]
        public ObservableCollection<CandidateBiographicItem> BiographicItemsList
        {
            get { return items; }
        }
    };

    /// <summary>
    /// Data container for the online verification result.
    /// </summary>
    [DataContract]
    [XmlRoot(ElementName = "BioSealOnlineVerifyResult")]
    public class BioSealOnlineVerifyResult
    {
        //[XmlIgnore]
        [DataMember(Order = 1, Name = "biographics")]
        private Dictionary<string, object> _fields;
        [XmlIgnore]
        ObservableCollection<CandidateBiographicItem> items;

        [DataMember(Order = 2, Name = "signatureStatus")]
        public bool SignatureStatus;

        [DataMember(Order = 3, Name = "faceMatchDecision")]
        public bool FaceMatchDecision;

        [DataMember(Order = 4, Name = "additionalInformation")]
        public string AdditionalInformation;

        /// <summary>
        /// Initializes a new instance of the BiographicData class.
        /// </summary>
        public BioSealOnlineVerifyResult()
        {
            _fields = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            items = new ObservableCollection<CandidateBiographicItem>();
            items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (CandidateBiographicItem item in e.NewItems)
                {
                    if (!_fields.ContainsKey(item.name))
                        _fields.Add(item.name, item.value);
                    else if ((string)_fields[item.name] != item.value)
                        _fields[item.name] = item.value;
                }
            }
            //else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            //{
            //    foreach(CandidateBiographicItem item in e.OldItems)
            //    {
            //        Remove(item.name);
            //    }
            //}
        }

        /// <summary>
        /// Gets a value indicating the number of elements in the Biographic Data.
        /// </summary>
        /// <returns>Number of biographical field.</returns>
        public int Count
        {
            get
            {
                return _fields.Count;
            }
        }

        /// <summary>
        /// Initiliazes the biographic data keys.
        /// </summary>
        /// <param name="keys">List of database fieldnames</param>
        public void Initialize(List<string> keys)
        {
            _fields.Clear();
            foreach (string key in keys)
            {
                _fields.Add(key, null);
                items.Add(new CandidateBiographicItem { name = key, value = null });
            }
        }

        /// <summary>
        /// Removes the corresponding element from the biographic data.
        /// </summary>
        /// <param name="key">Fieldname of the element to remove</param>
        public void Remove(string key)
        {
            if (_fields.ContainsKey(key))
            {
                KeyValuePair<string, object> oldValue = new KeyValuePair<string, object>(key, _fields[key]);
                _fields.Remove(key);
            }
        }

        /// <summary>
        /// Indicates whether the biographic data contains a element with this key.
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if the biographic data contains such a fieldname
        /// False otherwise.</returns>
        public bool ContainsKey(string key)
        {
            return _fields.ContainsKey(key);
        }

        /// <summary>
        /// Gets the list of biographic data identifiers.
        /// </summary>
        [XmlIgnore]
        public List<string> Keys
        {
            get { return _fields.Keys.ToList(); }
        }

        /// <summary>
        /// Obtains or modify the element with the specified fieldname.
        /// </summary>
        /// <param name="key">Fieldname of the element</param>
        /// <returns>The element with the specified fieldname.</returns>
        public object this[string key]
        {
            get
            {
                if (_fields.ContainsKey(key))
                    return _fields[key];
                else
                    return null;
            }
            set
            {
                if (key != null)
                {
                    _fields[key] = value;

                    if (items.Count(p => String.Compare(p.name, key, true) == 0) == 0)
                        items.Add(new CandidateBiographicItem() { name = key, value = Convert.ToString(value) });
                    else
                        items.First(p => String.Compare(p.name, key, true) == 0).value = Convert.ToString(value);
                }
                //else
                //    throw new ArgumentException("The key " + key + " does not exist");
            }
        }

        /// <summary>
        /// Gets or sets the biographical fields as a Dictionary.
        /// </summary>
        [XmlIgnore]
        public Dictionary<string, object> Biographics
        {
            get
            {
                return _fields;
            }
            set
            {
                this._fields = value;

                items.Clear();

                foreach (KeyValuePair<string, object> item in this._fields)
                {
                    items.Add(new CandidateBiographicItem { name = item.Key, value = item.Value as string });
                }
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2102.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://bias.biometricid.id3.eu/")]
    partial class CandidateBiographicItem : object, System.ComponentModel.INotifyPropertyChanged
    {

        private string nameField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
                this.RaisePropertyChanged("name");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
                this.RaisePropertyChanged("value");
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
