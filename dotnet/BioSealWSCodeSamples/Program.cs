using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace BioSealWSCodeSamples
{
    class Program
    {
        // WS parameters
        private static readonly string authUrl_ = "https://bioseal.id3.eu";
        private static readonly string authKey_ = "<PUT_YOUR_AUTHENTICATION_TOKEN_HERE>";
        private static readonly string authUri_ = $"{authUrl_}/authenticate";

        static void Main(string[] args)
        {
            try
            {
                TestGenerationVerification();
            }
            catch (WebException e)
            {
                string response = (e.Response == null) ? String.Empty : new StreamReader(e.Response.GetResponseStream()).ReadToEnd();

                Console.WriteLine();
                Console.WriteLine("Exception Message:" + e.Message);

                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    Console.WriteLine("Status code: {0}", ((HttpWebResponse) e.Response).StatusCode);
                    Console.WriteLine("Status description: {0}", ((HttpWebResponse) e.Response).StatusDescription);
                    Console.WriteLine("Response: {0}", response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static void TestGenerationVerification()
        {
            int nbTests = 100;

            // use case parameters
            string useCaseId = "<PUT_YOUR_USE_CASE_ID_HERE>";
            string useCaseVersion = "<PUT_YOUR_USE_CASE_VERSION_HERE>";

            // formats & resolutions

            List<string> formats = new List<string>();
            formats.Add("QrCode");
            formats.Add("DataMatrix");
            formats.Add(""); // default
            int nbFormats = formats.Count;

            List<string> resolutions = new List<string>();
            resolutions.Add("Standard");
            resolutions.Add("HighResolution");
            resolutions.Add(""); // default
            int nbResolutions = resolutions.Count;

            // tests
            for (int index = 0; index < nbTests; index++)
            {
                Console.WriteLine("TEST " + (index + 1).ToString());

                foreach (string format in formats)
                {
                    foreach (string resolution in resolutions)
                    {
                        Console.WriteLine("Format \"" + format + "\", Resolution \"" + resolution + "\"");

                        string faceImagePath = @"../../TestData/stallone1.jpg";
                        Image faceImage = Bitmap.FromFile(faceImagePath);

                        ////////////////////////////// QUALITY ////////////////////////////

                        Console.WriteLine("Checking image quality... ");
                        int quality = checkQuality(faceImage);


                        //////////////////////////// GENERATION ///////////////////////////

                        // generate biographics
                        BioSealSamplePayload payload = new BioSealSamplePayload();
                        payload.SetBiographics();

                        Console.WriteLine("Generating BioSeal... ");
                        bool storeTemplate = true;
                        bool storePicture = false;
                        Image bioSealImage = generateBioSeal(payload, faceImage, format, resolution,
                            storeTemplate, storePicture, useCaseId, useCaseVersion);
                        string filename = "Stallone_BioSeal";
                        if (!String.IsNullOrEmpty(format))
                            filename += "_" + format;
                        if (!String.IsNullOrEmpty(resolution))
                            filename += "_" + resolution;
                        filename += ".png";
                        //string filepath = Path.Combine(@"<YOUR_PATH>", filename);
                        //bioSealImage.Save(filepath);

                        float dpiX = bioSealImage.HorizontalResolution;
                        float dpiY = bioSealImage.VerticalResolution;


                        //////////////////////////// VERIFICATION /////////////////////////


                        Console.WriteLine("Verifying BioSeal... ");

                        string faceImageProbePath = @"../../TestData/stallone2.jpg";
                        Image faceImageProbe = Bitmap.FromFile(faceImageProbePath);

                        BioSealSampleVerifyResult result = verifyBioSeal<BioSealSampleVerifyResult>(bioSealImage, faceImageProbe, format);

                        // check match decision
                        bool? decision = null;
                        if (((BioSealSampleVerifyResult)result).Metadata.containFaceTemplate)
                            decision = ((BioSealSampleVerifyResult)result).Metadata.FaceMatchDecision;
                        if (decision != null && !(bool)decision)
                            throw new Exception("ERROR: faces don't match");

                        // check biographics
                        Dictionary<string, object> biographicsRef = DictionaryTools.ConvertBiographicsToDictionary(payload);
                        Dictionary<string, object> biographicsRes = DictionaryTools.ConvertBiographicsToDictionary(result.Payload);
                        if (!DictionaryTools.AreDictionariesIdentical(biographicsRef, biographicsRes))
                            throw new Exception("ERROR: biographics don't match");

                        Console.Write(" / MATCH");
                        Console.WriteLine();
                        Console.WriteLine();
                    }
                }
            }
        }

        private static int checkQuality(Image faceImage)
        {
            Stream faceImageStream = RequestHelper.ImageToStream(faceImage, faceImage.RawFormat);
            string resCheck = RequestHelper.PostMultipart(
                authUrl_ + "/api/image",
                "quality",
                "POST",
                WSAuthenticateTools.GetAuthenticationToken(authUri_, authKey_),
                new Dictionary<string, object>() {
                    { "face_image", new FormFile() { Name = "face_image.jpg", ContentType = "image/jpeg", Stream = faceImageStream } }
            });

            faceImageStream.Dispose();
            FaceImageQuality quality = JSONTools.Deserialize<FaceImageQuality>(resCheck);
            return quality.quality;
        }

        private static int checkQuality(string imagePath)
        {
            try
            {
                Bitmap faceImage = new Bitmap(imagePath);
                int quality = checkQuality(faceImage);
                //Console.WriteLine("Quality = " + quality + "%");

                return quality;
            }
            catch (WebException ex)
            {
                Console.WriteLine("WEB ERROR:" + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return -1; // failure
        }

        private static Image generateBioSeal(BioSealSamplePayload payload, Image faceImage = null,
            string format = "", string resolution = "",
            bool storeTemplate = true, bool storePicture = false,
            string useCaseId = "XXXX", string useCaseVersion = "-1",
            int width2DCode = 472, int height2DCode = 472)
        {
            string uri = authUrl_ + "/api/bioseal/create";
            bool hasFace = (storeTemplate || storePicture);
            if (hasFace)
                uri += "/biometric";

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (!String.IsNullOrEmpty(format))
            {
                parameters.Add("format", format);

                // datamatrix / qr specific

                if (format == "DataMatrix" || format == "QrCode")
                {
                    parameters.Add("2d_code_width", width2DCode.ToString()); // pixels
                    parameters.Add("2d_code_height", height2DCode.ToString());
                    //parameters.Add("2d_code_margin", "10");
                    //parameters.Add("2d_code_encoding_mode", "BASE32");
                }
            }

            if (!String.IsNullOrEmpty(resolution))
                parameters.Add("target_ppi", resolution);

            // face mode
            parameters.Add("store_template", storeTemplate ? "true" : "false");
            parameters.Add("store_picture", storePicture ? "true" : "false");

            // use case
            parameters.Add("use_case_id", useCaseId);
            parameters.Add("use_case_version", useCaseVersion);

            bool hasFirstParamProcessed = false;
            foreach (KeyValuePair<string, string> keyValue in parameters)
            {
                string param = keyValue.Key;
                string value = keyValue.Value.ToString();

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

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;

            UTF8Encoding encoding = new UTF8Encoding();

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/octet-stream,application/json";
            string token = WSAuthenticateTools.GetAuthenticationToken(authUri_, authKey_);
            if (token.StartsWith("Bearer "))
                request.Headers["Authorization"] = token;
            else
                request.Headers["ApiKeyAuth"] = token;

            string datetime = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");

            string faceBase64String = null;
            if (hasFace && faceImage != null)
            {
                Bitmap faceBitmap = (Bitmap)faceImage;
                faceBase64String = Base64Tools.Base64EncodeImage(faceBitmap, ImageFormat.Jpeg);
            }

            // serialize biographics + face data
            string requestBody = payload.Serialize(faceBase64String, storeTemplate, storePicture);
            byte[] bytes = encoding.GetBytes(requestBody);
            request.ContentLength = bytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                // send the data
                requestStream.Write(bytes, 0, bytes.Length);
            }

            Image bioSealImage;
            using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
            {
                // read and save generated image
                Stream stream = resp.GetResponseStream();
                bioSealImage = Bitmap.FromStream(stream);
            }

            return bioSealImage;
        }

        private static T verifyBioSeal<T>(Image bioSealImage, Image faceImageProbe, string format = "")
        {
            Stream bioSealStream = RequestHelper.ImageToStream(bioSealImage, bioSealImage.RawFormat);
            Stream faceImageProbeStream = RequestHelper.ImageToStream(faceImageProbe, faceImageProbe.RawFormat);

            string resVerify = RequestHelper.PostMultipart(
                authUrl_ + "/api/bioseal",
                "verify",
                "POST",
                WSAuthenticateTools.GetAuthenticationToken(authUri_, authKey_),
                new Dictionary<string, object>() {
                    { "format", format },
                    { "face_image", new FormFile() { Name = "face_image.jpg", ContentType = "image/jpeg", Stream = faceImageProbeStream } },
                    { "bioseal_file", new FormFile() { Name = "bioseal_file.png", ContentType = "image/png", Stream = bioSealStream } }
            });

            bioSealStream.Dispose();
            faceImageProbeStream.Dispose();

            T matchResult = JSONTools.Deserialize<T>(resVerify);
            return matchResult;
        }

    }

    [DataContract]
    public class FaceImageQuality
    {
        [DataMember(Order = 1, Name = "quality")]
        public int quality;
    }
}
