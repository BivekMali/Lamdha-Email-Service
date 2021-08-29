using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System.Net;
using System.Net.Mail;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace EmailService
{
    /// <summary>
    /// Model for User details
    /// </summary>
    public class MyObject
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public long Number { get; set; }
    }

    public class Function
    {
        #region Public Mathods
        /// <summary>
        /// A simple function that send mail
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void FunctionHandler(MyObject input, ILambdaContext context)
        {
            MyObject owner = getOwnerDetails();
            var fromAddress = new MailAddress(owner.Email, owner.Name);
            var toAddress = new MailAddress(owner.Email, owner.Name);
            string fromPassword = owner.Password;
            string subject = $"Potential Client {input.Name}";
            string body = $"Hello, This an pontential client {input.Name}, try to contact as soon as possible. His email is {input.Email} and phone number is {input.Number}";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
        #endregion

        #region Private Region
        /// <summary>
        /// To get owner details
        /// </summary>
        /// <returns>Owner details</returns>
        private MyObject getOwnerDetails()
        {
            MyObject owner = new MyObject();
            owner.Email = getValuefromSSM("email");
            owner.Password = getValuefromSSM("password");
            owner.Name = getValuefromSSM("name");
            return owner;
        }

        /// <summary>
        /// Connect to SSM paramneter store and get details.
        /// </summary>
        /// <param name="name">Name of the enviroment</param>
        /// <returns>Value or paramneter store</returns>
        private string getValuefromSSM(string name)
        {
            string value = null;
            var request = new GetParameterRequest()
            {
                Name = $"/ferricSkeleton/{name}"
            };
            using (var client = new AmazonSimpleSystemsManagementClient(Amazon.RegionEndpoint.GetBySystemName("ap-south-1")))
            {
                var response = client.GetParameterAsync(request).GetAwaiter().GetResult();
                value = response.Parameter.Value;
            }
            return value;
        }
        #endregion
    }
}