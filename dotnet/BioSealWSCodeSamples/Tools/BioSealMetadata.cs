using System.Runtime.Serialization;

namespace BioSealWSCodeSamples
{
    [DataContract]
    public class BioSealMetadata
    {
        [DataMember(Order = 1, Name = "contains_face_image")]
        public bool containFaceImage;

        [DataMember(Order = 2, Name = "contains_face_template")]
        public bool containFaceTemplate;

        [DataMember(Order = 3, Name = "contains_finger_template")]
        public bool containFingerTemplate;

        [DataMember(Order = 4, Name = "signature_status")]
        public bool SignatureStatus;

        [DataMember(Order = 5, Name = "face_match_decision")]
        public bool? FaceMatchDecision;

        [DataMember(Order = 6, Name = "additional_information")]
        public string AdditionalInfo;
    }
}
