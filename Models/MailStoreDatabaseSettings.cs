using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace uludag_mail_svc.Models
{
    public class MailStoreDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string MailCollectionName { get; set; } = null!;
    }
}