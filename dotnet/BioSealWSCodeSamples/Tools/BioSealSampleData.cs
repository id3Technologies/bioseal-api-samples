using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BioSealWSCodeSamples
{
    [DataContract]
    public class BioSealSampleData
    {
        [DataMember(Order = 1, Name = "payload")]
        public BioSealSamplePayload Payload;
    }

    [DataContract]
    public class BioSealSampleVerifyResult
    {
        [DataMember(Order = 1, Name = "payload")]
        public BioSealSamplePayload Payload;

        [DataMember(Order = 2, Name = "metadata")]
        public BioSealMetadata Metadata;
    }

    [DataContract]
    public class BioSealSamplePayload
    {
        // PUT YOUR BIOGRAPHIC FIELDS HERE

        // Personal information

        [DataMember(Order = 1, Name = "first_name")]
        public string LastName;

        [DataMember(Order = 2, Name = "last_name")]
        public string FirstName;

        [DataMember(Order = 3, Name = "email")]
        public string Email;

        [DataMember(Order = 4, Name = "date_of_birth")]
        public string DateOfBirth;

        [DataMember(Order = 5, Name = "place_of_birth")]
        public string PlaceOfBirth;

        [DataMember(Order = 6, Name = "gender")]
        public string Gender;

        [DataMember(Order = 7, Name = "country")]
        public string Country;

        [DataMember(Order = 8, Name = "address")]
        public string Address;

        [DataMember(Order = 9, Name = "phone")]
        public string Phone;

        [DataMember(Order = 10, Name = "id")]
        public string Id;

        // Document information

        [DataMember(Order = 101, Name = "doc_type")]
        public string DocumentType;

        [DataMember(Order = 102, Name = "doc_id")]
        public string DocumentId;

        [DataMember(Order = 103, Name = "doc_date_of_issue")]
        public string DocumentDateOfIssue;

        [DataMember(Order = 104, Name = "doc_date_of_expiration")]
        public string DocumentDateOfExpiration;

        [DataMember(Order = 105, Name = "doc_place_of_issue")]
        public string DocumentPlaceOfIssue;

        // Misc

        [DataMember(Order = 201, Name = "misc")]
        public string Misc;

        // Face image in base64

        [DataMember(Order = 1000, Name = "face_image", IsRequired = false, EmitDefaultValue = false)]
        public string FaceImageBase64;

        public void FromDictionary(Dictionary<string, object> dict)
        {
            if (dict == null)
                return;

            if (dict.ContainsKey("first_name"))
                this.FirstName = dict["first_name"] as string;
            if (dict.ContainsKey("last_name"))
                this.LastName = dict["last_name"] as string;
            if (dict.ContainsKey("email"))
                this.Email = dict["email"] as string;
            if (dict.ContainsKey("date_of_birth"))
                this.DateOfBirth = dict["date_of_birth"] as string;
            if (dict.ContainsKey("place_of_birth"))
                this.PlaceOfBirth = dict["place_of_birth"] as string;
            if (dict.ContainsKey("gender"))
                this.Gender = dict["gender"] as string;
            if (dict.ContainsKey("country"))
                this.Country = dict["country"] as string;
            if (dict.ContainsKey("address"))
                this.Address = dict["address"] as string;
            if (dict.ContainsKey("phone"))
                this.Phone = dict["phone"] as string;
            if (dict.ContainsKey("id"))
                this.Id = dict["id"] as string;

            if (dict.ContainsKey("doc_type"))
                this.DocumentType = dict["doc_type"] as string;
            if (dict.ContainsKey("doc_id"))
                this.DocumentId = dict["doc_id"] as string;
            if (dict.ContainsKey("doc_date_of_issue"))
                this.DocumentDateOfIssue = dict["doc_date_of_issue"] as string;
            if (dict.ContainsKey("doc_date_of_expiration"))
                this.DocumentDateOfExpiration = dict["doc_date_of_expiration"] as string;
            if (dict.ContainsKey("doc_place_of_issue"))
                this.DocumentPlaceOfIssue = dict["doc_place_of_issue"] as string;

            if (dict.ContainsKey("misc"))
                this.Misc = dict["misc"] as string;
        }

        public string Serialize(string faceBase64String)
        {
            BioSealSampleData data = new BioSealSampleData();
            data.Payload = this;
            data.Payload.FaceImageBase64 = faceBase64String;
            return JSONTools.Serialize(data);
        }

        public void Reset()
        {
            this.FirstName = String.Empty;
            this.LastName = String.Empty;
            this.Email = String.Empty;
            this.DateOfBirth = String.Empty;
            this.PlaceOfBirth = String.Empty;
            this.Gender = String.Empty;
            this.Country = String.Empty;
            this.Address = String.Empty;
            this.Phone = String.Empty;
            this.Id = String.Empty;

            this.DocumentType = String.Empty;
            this.DocumentId = String.Empty;
            this.DocumentDateOfIssue = String.Empty;
            this.DocumentDateOfExpiration = String.Empty;
            this.DocumentPlaceOfIssue = String.Empty;

            this.Misc = String.Empty;

            this.FaceImageBase64 = String.Empty;
        }

        public void SetBiographics()
        {
            // Personal information
            this.FirstName = "John";
            this.LastName = "Doe";

            this.PlaceOfBirth = "Houston, Texas";
            this.Gender = "M";
            this.Country = "USA";

            // Document information
            this.DocumentDateOfIssue = "2021-04-20";
        }
    }
}
