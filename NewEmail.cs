using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Trianon.CAM.JTIEmail
{
    public class NewEmail
    {
        private readonly ILogger<NewEmail> _logger;

        public NewEmail(ILogger<NewEmail> logger)
        {
            _logger = logger;
        }

        [Function("NewEmail")]
        public static async Task<MultiResponse> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("HttpExample");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            // Parse query parameter
            string reqBody = await new StreamReader(req.Body).ReadToEndAsync();
            var email = JsonSerializer.Deserialize<Email>(reqBody);

            var emailBodyContent = Regex.Replace(email.EmailBody, "<.*?>", string.Empty);
            emailBodyContent = emailBodyContent.Replace("\\r\\n", " ");
            emailBodyContent = emailBodyContent.Replace(@"&nbsp;", " ");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await response.WriteStringAsync(emailBodyContent);

            var emailMessage = new EmailMessage 
            {
                Id = email.Id,
                From = email.From,
                To = email.To,
                Subject = email.Subject,
                Content = emailBodyContent,
                ConversationId = email.ConversationId,
                ReceivedTime = email.ReceivedTime,
                Attachments = email.Attachments.Select(x => x.Id).ToList(),
                Absence = new Absence 
                {
                    Id = email.Id,
                }
            };
 
            // Return a response to both HTTP trigger and storage output binding.
            return new MultiResponse()
            {
                // Write a single message.
                Message = emailMessage,
                HttpResponse = response
            };
        }
    }

    public class MultiResponse
    {
        [QueueOutput("jti-email-queue", Connection = "AzureWebJobsStorage")]
        public EmailMessage Message { get; set; }
        public HttpResponseData HttpResponse { get; set; }

    }

    public class EmailMessage 
    {
        public string Id { get; set; }

        public string ConversationId { get; set; }

        public DateTimeOffset? ReceivedTime { get; set; }

        public string Content { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public List<string> Attachments { get; set; } = new List<string>();

        public Absence Absence { get; set; }
    }

    public class Absence 
    {
        public string Id { get; set;}
    }

    public partial class Email
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("receivedTime")]
        public DateTimeOffset? ReceivedTime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("emailBody")]
        public string EmailBody { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("attachments")]
        public List<Attachment> Attachments { get; set; }
    }

    public partial class Attachment
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("@odata.type")]
        public string OdataType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("lastModifiedDateTime")]
        public DateTimeOffset? LastModifiedDateTime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("size")]
        public long? Size { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("isInline")]
        public bool? IsInline { get; set; }

        [JsonPropertyName("contentId")]
        public Guid? ContentId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("contentBytes")]
        public string ContentBytes { get; set; }
    }


}
