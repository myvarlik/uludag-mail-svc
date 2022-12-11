using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.IO;

namespace uludag_mail_svc.Models
{
    public class MailModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }
        public string mesaj { get; set; } = null!;
        public string mesajBasligi { get; set; } = null!;
        public List<Kisiler>? kisiler { get; set; }
        public List<Dosyalar>? dosyalar { get; set; }
        public int durum { get; set; } = 0;
        public string hata { get; set; } = null!;
        public DateTime eklenmeTarih { get; set; }
        public DateTime gonderilmeTarih { get; set; }

    }

    public class Kisiler
    {
        public string adi { get; set; } = null!;
        public string mail { get; set; } = null!;
    }

    public class Dosyalar
    {
        public string adi { get; set; } = null!;
        public string dosya { get; set; } = null!;
    }
}