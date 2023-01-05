using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace BioSealWSCodeSamples
{
    class Base64Tools
    {
        public static string Base64Encode(string text)
        {
            var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(textBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Base64EncodeImage(Bitmap bmp, ImageFormat format)
        {
            string res = null;

            if (bmp == null)
                return null;

            using (var ms = new MemoryStream())
            {
                using (var bitmap = (Bitmap)(bmp as Image).Clone())
                {
                    bitmap.Save(ms, format);
                    res = Convert.ToBase64String(ms.GetBuffer()); // get Base64
                }
            }

            return res;
        }
    }
}
