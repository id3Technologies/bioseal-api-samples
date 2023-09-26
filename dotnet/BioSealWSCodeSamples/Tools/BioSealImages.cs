using System.Runtime.Serialization;

namespace BioSealWSCodeSamples
{
    [DataContract]
    public class BioSealImages
    {
        [DataMember(Order = 1, Name = "faceImage")]
        public string FaceImageBase64;
    }
}
