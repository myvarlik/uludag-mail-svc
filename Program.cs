using Microsoft.AspNetCore.Mvc;
using uludag_mail_svc.Models;
using uludag_mail_svc;
using EasyNetQ;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MailStoreDatabaseSettings>(builder.Configuration.GetSection("MailStoreDatabase"));
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));

// Add services to the container.
builder.Services.AddSingleton<MailService>();

string rabbitmqConnectionString = "host=10.245.195.44:5672;virtualhost=/;username=myvarlik;password=celeron504";
var bus = RabbitHutch.CreateBus(rabbitmqConnectionString);
// Kuyruk oluþtur
var queue = bus.Advanced.QueueDeclare("mail-gonder");
// Kuyruða abone ol
bus.Advanced.Consume(queue, (body, properties, info) =>
{
    var message = Encoding.UTF8.GetString(body.Span);
    MailModel mail = System.Text.Json.JsonSerializer.Deserialize<MailModel>(message);
    MailService.TekliGonder(mail);
    return Task.CompletedTask;
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/apirun", () => "Api Runs");

app.MapPost("/list", ResponseModel ([FromBody] ListModel listModel, MailService mailService) => mailService.Get(listModel));
app.MapGet("/send", (MailService mailService) => mailService.Gonder());
app.MapDelete("/", ([FromQuery] string id, MailService mailService) => mailService.Remove(id));
app.MapPost("/", ([FromBody] MailModel mailModel, MailService mailService) => mailService.Create(mailModel));

app.Run();