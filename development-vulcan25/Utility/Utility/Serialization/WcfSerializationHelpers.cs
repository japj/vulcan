using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Vulcan.Utility.Serialization
{
    /// <summary>
    /// Utility class.
    /// WCF Serialization helper functions
    /// </summary>
    public static class WcfSerializationHelpers
    {
        /// <summary>
        /// Returns a string containing the specified object
        /// serialized to XML using the WCF serialization engine
        /// <see cref="DataContractSerializer"/>
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="instance">Object to serialize</param>
        /// <returns>XML string</returns>
        /// <remarks>Object must be serializable.
        /// Contains no exception handling</remarks>
        public static string WcfSerializeToString<T>(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            StringBuilder builder = new StringBuilder();

            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "    ";
            xmlWriterSettings.OmitXmlDeclaration = true;

            using (XmlWriter xmlWriter = XmlTextWriter.Create(builder, xmlWriterSettings))
            using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(writer, instance);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns an instance of the specified type from
        /// the XML string provided using the WCF serialization engine
        /// <see cref="DataContractSerializer"/>
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="xml">Serialized object XML</param>
        /// <returns>Instance of deserialized object</returns>
        /// <remarks>Contains no exception handling</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Advanced method provided for advanced developers.")]
        public static T WcfDeserializeFromString<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml");
            }

            T instance;

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(xml)))
            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(xmlReader))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                instance = (T)serializer.ReadObject(reader);
            }

            return instance;
        }

        /// <summary>
        /// Determines if the instance is WCF serializable
        /// </summary>
        /// <param name="check">Object to be checked</param>
        /// <returns>True if the object type supports WCF serialization</returns>
        public static bool IsWcfSerializable(object check)
        {
            if (check == null)
            {
                throw new ArgumentNullException("check");
            }

            Type checkType = check.GetType();
            Attribute[] attributes = Attribute.GetCustomAttributes(checkType);

            bool hasSerializableAttribute = false;

            foreach (Attribute attribute in attributes)
            {
                if (attribute is DataContractAttribute)
                {
                    hasSerializableAttribute = true;
                    break;
                }
            }

            return hasSerializableAttribute;
        }

        /// <summary>
        /// Serializes and deserializes an instance using WCF 
        /// serialization to test instance serialization. Useful
        /// for ensuring classes are correctly marked up to be
        /// serialized
        /// </summary>
        /// <typeparam name="T">Instance type</typeparam>
        /// <param name="instance">Instance</param>
        /// <returns>New instance after serialization / deserialization</returns>
        public static T WcfRoundtripSerialize<T>(T instance) where T : class, new()
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            string xml = WcfSerializeToString<T>(instance);

            T readbackInstance = WcfDeserializeFromString<T>(xml);

            return readbackInstance;
        }
    }
}
