using id3.BioSeal.Tools;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;

namespace id3.BioSeal
{
    public static class BioSealEncoder
    {
        #region public methods

        /// <summary>
        /// Generates BioSeal image given data
        /// </summary>
        /// <param name="bioSealData">The BioSeal data</param>
        /// <param name="webserviceURL">The BioSeal WebService URL</param>
        /// <param name="authenticationToken">The BioSeal WebService authentication token</param>
        /// <returns>Generated BioSeal image</returns>
        public static Bitmap EncodeImageOnline(BioSealData bioSealData, string webserviceURL, string authenticationToken)
        {
            // check if BioSealData is empty
            if (bioSealData.IsEmpty())
                throw new Exception("Empty BioSeal data");

            // generate BioSeal image online
            Bitmap bmp = null;

            try
            {
                string uri = webserviceURL + "/api/bioseal/create";
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.Expect100Continue = false;

                UTF8Encoding encoding = new UTF8Encoding();

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/octet-stream,application/json";
                request.Headers["ApiKeyAuth"] = authenticationToken;

                string requestBody = JSONTools.BuildJSONEncodedBiographicsFace(bioSealData.Biographics, bioSealData.FaceImageOnline);

                byte[] bytes = encoding.GetBytes(requestBody);
                request.ContentLength = bytes.Length;

                // generate BioSeal
                using (Stream requestStream = request.GetRequestStream())
                {
                    // Send the data.
                    requestStream.Write(bytes, 0, bytes.Length);
                }

                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    // read and save generated image
                    Stream stream = resp.GetResponseStream();
                    bmp = (Bitmap)Bitmap.FromStream(stream);
                }
            }
            catch (WebException ex)
            {
                // get HTTP status code
                int errorCode = 0;
                HttpStatusCode statusCode = ((HttpWebResponse)ex.Response).StatusCode;

                if (statusCode.ToString().ToLower() == "forbidden")
                    errorCode = -403;
                else
                    errorCode = -((int)statusCode);

                string info = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bmp;
        }

        #endregion
    }
}

