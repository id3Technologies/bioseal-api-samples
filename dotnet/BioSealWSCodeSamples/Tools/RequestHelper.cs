using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;

namespace BioSealWSCodeSamples
{
    public class FormFile
    {
        public string Name { get; set; }

        public string ContentType { get; set; }

        public string FilePath { get; set; }

        public Stream Stream { get; set; }
    }

    public static class RequestHelper
    {

        public static string PostMultipart(string url, string operation, string method, string token, Dictionary<string, object> parameters = null)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;

            // build URI with parameters if existing
            string uri = url + "/" + operation;
            bool hasForm = false;
            if (parameters != null && parameters.Count > 0)
            {
                bool hasFirstParamProcessed = false;
                foreach (KeyValuePair<string, object> keyValue in parameters)
                {
                    string param = keyValue.Key;
                    string value = keyValue.Value.ToString();

                    if (keyValue.Value is FormFile)
                    {
                        hasForm = true;
                        continue;
                    }

                    // handle booleans
                    //if (keyValue.Value is Boolean)
                    //    value = (Convert.ToUInt32(keyValue.Value)).ToString();

                    if (String.IsNullOrEmpty(value))
                        continue;

                    if (!hasFirstParamProcessed)
                    {
                        uri += "?";
                        hasFirstParamProcessed = true;
                    }
                    else
                        uri += "&";

                    uri += param + "=" + value;
                }
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.ContentType = "application/json"; // default
            request.Accept = "application/json";
            if (token.StartsWith("Bearer "))
                request.Headers["Authorization"] = token;
            else
                request.Headers["ApiKeyAuth"] = token;
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;

            if (hasForm)
            {
                // update content type
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                request.ContentType = "multipart/form-data; boundary=" + boundary;

                using (Stream requestStream = request.GetRequestStream())
                {
                    foreach (KeyValuePair<string, object> pair in parameters)
                    {
                        if (pair.Value is FormFile)
                        {
                            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                            FormFile file = pair.Value as FormFile;
                            string header = "Content-Disposition: form-data; name=\"" + pair.Key + "\"; filename=\"" + file.Name + "\"\r\nContent-Type: " + file.ContentType + "\r\n\r\n";
                            byte[] bytes = Encoding.UTF8.GetBytes(header);
                            requestStream.Write(bytes, 0, bytes.Length);
                            byte[] buffer = new byte[32768];
                            int bytesRead;
                            if (file.Stream == null)
                            {
                                // upload from file
                                using (FileStream fileStream = File.OpenRead(file.FilePath))
                                {
                                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                        requestStream.Write(buffer, 0, bytesRead);
                                    fileStream.Close();
                                }
                            }
                            else
                            {
                                // upload from given stream
                                while ((bytesRead = file.Stream.Read(buffer, 0, buffer.Length)) != 0)
                                    requestStream.Write(buffer, 0, bytesRead);
                            }
                        }
                        else
                        {
                            //// URI parameter
                            //string data = "Content-Disposition: form-data; name=\"" + pair.Key + "\"\r\n\r\n" + pair.Value;
                            //byte[] bytes = Encoding.UTF8.GetBytes(data);
                            //requestStream.Write(bytes, 0, bytes.Length);
                        }
                    }

                    byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    requestStream.Write(trailer, 0, trailer.Length);
                    requestStream.Close();
                }
            }

            using (WebResponse response = request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(responseStream))
                    return reader.ReadToEnd();
            }
        }

        public static Stream ImageToStream(Image image, ImageFormat format)
        {
            var stream = new MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }

        public static byte[] ImageFileToByteArray(string imagePath)
        {
            FileStream fs = File.OpenRead(imagePath);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            return bytes;
        }

    }
}
