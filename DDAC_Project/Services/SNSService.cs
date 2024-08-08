using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace DDAC_Project.Services
{
    public class SNSService
    {
        private readonly IAmazonSimpleNotificationService _snsClient;

        public SNSService(IAmazonSimpleNotificationService snsClient)
        {
            _snsClient = snsClient;
        }

        public async Task SendEmailNotification(string email, string subject, string message)
        {
            var request = new PublishRequest
            {
                Message = message,
                Subject = subject,
                MessageStructure = "string",
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                { "email", new MessageAttributeValue { DataType = "String", StringValue = email } }
            },
                TargetArn = "arn:aws:sns:us-east-1:103756122619:RedshiftSNS:28b861eb-72e1-4d2a-a3b9-6c801ff4dd43" // Replace with your SNS application ARN
            };

            await _snsClient.PublishAsync(request);
        }
    }
}
