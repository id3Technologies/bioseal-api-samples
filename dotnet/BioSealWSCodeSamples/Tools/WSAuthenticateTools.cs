using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace BioSealWSCodeSamples
{
    public static class WSAuthenticateTools
    {
        // authentication WS parameters
        public static string GetAuthenticationToken(string authURI, string authKey)
        {
            string token = "";

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;

            UTF8Encoding encoding = new UTF8Encoding();

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(authURI);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/octet-stream,application/json";

            // data
            AuthenticationKey key = new AuthenticationKey(authKey);
            string requestBody = JSONTools.Serialize<AuthenticationKey>(key);

            byte[] bytes = encoding.GetBytes(requestBody);
            request.ContentLength = bytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            string responseString = "";
            using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
            {
                Stream stream = resp.GetResponseStream();
                StreamReader responseReader = new StreamReader(stream);
                responseString = responseReader.ReadToEnd();
            }

            if (String.IsNullOrEmpty(responseString))
                return null;

            BearerToken bearerToken = JSONTools.Deserialize<BearerToken>(responseString);
            token = bearerToken.Token;

            return "Bearer " + token;
        }
    }

    [DataContract]
    public class AuthenticationKey
    {
        [DataMember(Order = 1, Name = "key")]
        public string Key;

        public AuthenticationKey(string key)
        {
            Key = key;
        }
    }

    [DataContract]
    public class BearerToken
    {
        [DataMember(Order = 1, Name = "bearerToken")]
        public string Token;

        public BearerToken(string token)
        {
            Token = token;
        }
    }
}
