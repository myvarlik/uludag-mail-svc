using Microsoft.AspNetCore.Mvc;
using uludag_mail_svc.Models;
using uludag_mail_svc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MailStoreDatabaseSettings>(builder.Configuration.GetSection("MailStoreDatabase"));
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));

// Add services to the container.
builder.Services.AddSingleton<MailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/apirun", () => "Api Runs");

app.MapPost("/list", ResponseModel ([FromBody] ListModel listModel, MailService mailService) => mailService.Get(listModel));
app.MapGet("/send", (MailService mailService) => mailService.Gonder());
app.MapDelete("/", ([FromQuery] string id, MailService mailService) => mailService.Remove(id));
app.MapPost("/", ([FromBody] MailModel mailModel, MailService mailService) => mailService.Create(mailModel));

app.Run();