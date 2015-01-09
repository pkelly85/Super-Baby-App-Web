using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;

namespace SuperBabyWCF
{
    public class EncryptionDecryption
    {
        public class Encoding
        {
            #region Variable Declaration

            private static string keyString = "27SEP83M-02FE-B201-2BOB-26NOV1983BYE";

            #endregion

            #region Methods/Functions

            /// <summary>
            /// Get Encrpted Value of Passed value
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public static string GetEncrypt(string value)
            {
                return Encrypt(keyString, value);
            }

            /// <summary>
            /// Get Decrypted value of passed encrypted string
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public static string GetDecrypt(string value)
            {
                return Decrypt(keyString, value);
            }

            /// <summary>
            /// Encrypt value
            /// </summary>
            /// <param name="Passphrase"></param>
            /// <param name="Message"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            private static string Encrypt(string Passphrase, string Message)
            {
                //try
                //{
                byte[] Results;
                System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

                // Step 1. We hash the passphrase using MD5
                // We use the MD5 hash generator as the result is a 128 bit byte array
                // which is a valid length for the TripleDES encoder we use below

                MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
                byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));

                // Step 2. Create a new TripleDESCryptoServiceProvider object
                TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

                // Step 3. Setup the encoder
                TDESAlgorithm.Key = TDESKey;
                TDESAlgorithm.Mode = CipherMode.ECB;
                TDESAlgorithm.Padding = PaddingMode.PKCS7;

                // Step 4. Convert the input string to a byte[]
                byte[] DataToEncrypt = UTF8.GetBytes(Message);

                // Step 5. Attempt to encrypt the string
                try
                {
                    ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                    Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
                }
                finally
                {
                    // Clear the TripleDes and Hashprovider services of any sensitive information
                    TDESAlgorithm.Clear();
                    HashProvider.Clear();
                }

                // Step 6. Return the encrypted string as a base64 encoded string
                return Convert.ToBase64String(Results);
                //}
                //catch (Exception)
                //{
                //    throw;
                //}
            }

            public static bool IsNullOrEmpty(string s)
            {
                if (s == null) return true;
                if (s.Trim() == string.Empty) return true;
                return false;
            }

            /// <summary>
            /// decrypt value
            /// </summary>
            /// <param name="Passphrase"></param>
            /// <param name="Message"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            private static string Decrypt(string Passphrase, string Message)
            {
                //try
                //{
                if (IsNullOrEmpty(Message))
                    return string.Empty;
                byte[] Results;
                System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

                // Step 1. We hash the passphrase using MD5
                // We use the MD5 hash generator as the result is a 128 bit byte array
                // which is a valid length for the TripleDES encoder we use below

                MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
                byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));

                // Step 2. Create a new TripleDESCryptoServiceProvider object
                TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

                // Step 3. Setup the decoder
                TDESAlgorithm.Key = TDESKey;
                TDESAlgorithm.Mode = CipherMode.ECB;
                TDESAlgorithm.Padding = PaddingMode.PKCS7;

                Message = Message.Replace(" ", "+"); // Replace space with plus sign in encrypted value if any.- kalpesh joshi [09/05/2013]

                // Step 4. Convert the input string to a byte[]
                byte[] DataToDecrypt = Convert.FromBase64String(Message);

                // Step 5. Attempt to decrypt the string
                try
                {
                    ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                    Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
                }
                catch
                {
                    return "";
                }
                finally
                {
                    // Clear the TripleDes and Hashprovider services of any sensitive information
                    TDESAlgorithm.Clear();
                    HashProvider.Clear();
                }

                // Step 6. Return the decrypted string in UTF8 format
                return UTF8.GetString(Results);
                //}
                //catch (Exception)
                //{
                //    throw;
                //}

            }

            #endregion

            #region Serialize and Deserialize

            /// <summary>
            /// Serializes an object to Xml as a string.
            /// </summary>
            /// <typeparam name="T">Datatype T.</typeparam>
            /// <param name="ToSerialize">Object of type T to be serialized.</param>
            /// <returns>Xml string of serialized type T object.</returns>
            public static string SerializeToXmlString<T>(T ToSerialize)
            {
                string xmlstream = String.Empty;

                using (MemoryStream memstream = new MemoryStream())
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    XmlTextWriter xmlWriter = new XmlTextWriter(memstream, System.Text.Encoding.UTF8);

                    xmlSerializer.Serialize(xmlWriter, ToSerialize);
                    xmlstream = UTF8ByteArrayToString(((MemoryStream)xmlWriter.BaseStream).ToArray());
                }

                return xmlstream;
            }

            /// <summary>
            /// Deserializes Xml string of type T.
            /// </summary>
            /// <typeparam name="T">Datatype T.</typeparam>
            /// <param name="XmlString">Input Xml string from which to read.</param>
            /// <returns>Returns rehydrated object of type T.</returns>
            public static T DeserializeXmlString<T>(string XmlString)
            {
                T tempObject = default(T);

                using (MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(XmlString)))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, System.Text.Encoding.UTF8);

                    tempObject = (T)xs.Deserialize(memoryStream);
                }

                return tempObject;
            }

            // Convert Array to String
            public static String UTF8ByteArrayToString(Byte[] ArrBytes)
            { return new UTF8Encoding().GetString(ArrBytes); }

            // Convert String to Array
            public static Byte[] StringToUTF8ByteArray(String XmlString)
            { return new UTF8Encoding().GetBytes(XmlString); }

            #endregion
        }
    }
}