using Microsoft.Extensions.Options;
using MongoDB.Driver;
using uludag_mail_svc.Models;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Net;

namespace uludag_mail_svc
{
    public class MailService
    {
        private readonly IMongoCollection<MailModel> _mailCollection;
        private readonly EmailConfiguration _mailConfig;

        public MailService(IOptions<MailStoreDatabaseSettings> mailStoreDatabaseSettings, IOptions<EmailConfiguration> mailConfig)
        {
            var mongoClient = new MongoClient(
                mailStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                mailStoreDatabaseSettings.Value.DatabaseName);

            _mailCollection = mongoDatabase.GetCollection<MailModel>(
                mailStoreDatabaseSettings.Value.MailCollectionName);

            _mailConfig = mailConfig.Value;
        }

        public ResponseModel GetAsync(ListModel listModel)
        {
            //var builder = Builders<SMSModel>.Filter;

            //var filterData = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(listModel.filterData);

            //var dynamicQuery = builder.Empty;
            //foreach (var item in filterData)
            //{
            //    dynamicQuery &= builder.Or(item.Key, item.Value);
            //}

            var offset = (listModel.current - 1) * listModel.pageSize;

            List<MailModel> data = _mailCollection.Find(_ => true).Skip(offset).Limit(listModel.pageSize).SortByDescending(i => i.eklenmeTarih).ToList();
            var count = _mailCollection.Find(_ => true).CountDocuments();

            ResponseModel result = new()
            {
                success = true,
                data = new
                {
                    current = listModel.current,
                    data = data,
                    pageSize = listModel.pageSize,
                    success = true,
                    total = count,
                }
            };

            return result;
        }

        public bool CreateAsync(MailModel newmail)
        {
            if (string.IsNullOrEmpty(newmail.mesaj))
            {
                return false;
            }

            newmail.eklenmeTarih = DateTime.Now;
            _mailCollection.InsertOneAsync(newmail);
            return true;
        }

        public bool RemoveAsync(string id)
        {
            _mailCollection.DeleteOne(x => x.id == id);
            return true;
        }

        public void Gonder()
        {
            FilterDefinition<MailModel> nameFilter = Builders<MailModel>.Filter.Eq(x => x.durum, 0);
            FilterDefinition<MailModel> combineFilters = Builders<MailModel>.Filter.And(nameFilter);

            List<MailModel> mailModels = _mailCollection.Find(combineFilters).ToList();

            foreach (var mail in mailModels)
            {
                if (string.IsNullOrEmpty(mail.mesaj))
                {
                    continue;
                }

                var smtpClient = new SmtpClient()
                {
                    Host = _mailConfig.SmtpServer,
                    Port = _mailConfig.Port,
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_mailConfig.UserName, _mailConfig.Password),
                    Timeout = 20000
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_mailConfig.From),
                    Subject = mail.mesajBasligi,
                    Body = mail.mesaj,
                    IsBodyHtml = true
                };

                if (mail.kisiler != null)
                    foreach (var kisi in mail.kisiler)
                    {
                        if (string.IsNullOrEmpty(kisi.mail))
                        {
                            continue;
                        }

                        Match Eslesme = Regex.Match(kisi.mail, "^\\S+@\\S+\\.\\S+$", RegexOptions.IgnoreCase);

                        if (Eslesme.Success)
                        {
                            mailMessage.To.Add(kisi.mail);
                        }
                    }

                if (mail.dosyalar != null)
                    foreach (var file in mail.dosyalar)
                    {
                        var bytes = Convert.FromBase64String(file.dosya);
                        var contents = new MemoryStream(bytes);

                        Attachment attachment = new Attachment(contents, file.adi);
                        mailMessage.Attachments.Add(attachment);
                    }
                try
                {
                    smtpClient.Send(mailMessage);

                    mail.durum = 1;
                    mail.gonderilmeTarih = DateTime.Now;
                    _mailCollection.ReplaceOne(x => x.id == mail.id, mail);
                }
                catch (Exception)
                {
                    mail.durum = 2;
                    _mailCollection.ReplaceOne(x => x.id == mail.id, mail);
                }
            }
        }
    }
}
