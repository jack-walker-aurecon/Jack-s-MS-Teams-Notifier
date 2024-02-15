using SendGrid;
using SendGrid.Helpers.Mail;

using Sentry;

using Speckle.Automate.Sdk;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using Speckle.Core.Models.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using User = Speckle.Core.Api.User;

namespace SpeckleAutomateDotnetExample
{
    public static class SendGridHelper
    {
        internal static async Task Execute(FunctionInputs inputs, AutomationContext context)
        {
            Base? version = await context.ReceiveVersion();

            int count = version.Flatten().Count();

            Commit? commit = await context.SpeckleClient.CommitGet(context.AutomationRunData.ProjectId, context.AutomationRunData.VersionId);

            if (commit == null)
            {
                Console.WriteLine("The run failed as the commit could not be retrieved");
                context.MarkRunFailed("The run failed as the commit could not be retrieved");
            }
            else
            {
                Console.WriteLine("Creating email objects for sending");
                User account = await context.SpeckleClient.ActiveUserGet();
                if (account == null)
                {
                    context.MarkRunFailed("The active account could not be retrieved");
                    Console.WriteLine("Could not find account");
                }
                else
                {
                    string name = account.name;
                    string email = account.email;
                    var _client = new SendGridClient(inputs.SendGridAPIKey);
                    var from = new EmailAddress(email, "Speckle Automate SendGrid Sender");
                    var subject = $"New commit to {context.AutomationRunData.BranchName}: {commit.message}";
                    var to = new EmailAddress(email, name);

                    string author = commit.authorName;
                    //plain text
                    StringBuilder _stringBuilder = new StringBuilder();
                    _stringBuilder.AppendLine($"Hey {name}!");
                    _stringBuilder.AppendLine($"{author} just added a new commit with the id {commit.id} to your model {commit.branchName}. It has {count} objects in it.");
                    _stringBuilder.AppendLine($"You can access it here: {context.AutomationRunData.SpeckleServerUrl}/projects/{context.AutomationRunData.ProjectId}/models{context.AutomationRunData.ModelId}");

                    //html
                    StringBuilder _htmlBuilder = new StringBuilder();
                    _htmlBuilder.AppendLine($"Hey <i>{name}</i>!");
                    _stringBuilder.AppendLine($"{author} just added a new commit with the id <strong>{commit.id}<strong> to your model {commit.branchName}. It has <strong>{count}</strong> objects in it.");
                    _stringBuilder.AppendLine($"You can access it <a href={context.AutomationRunData.SpeckleServerUrl}/projects/{context.AutomationRunData.ProjectId}/models{context.AutomationRunData.ModelId}>here</a>");

                    var message = MailHelper.CreateSingleEmail(from, to, subject, _stringBuilder.ToString(), _htmlBuilder.ToString());
                    Console.WriteLine("Created email data");

                    Console.WriteLine("Sending email data");
                    var response = await _client.SendEmailAsync(message);
                    Console.WriteLine("Email data sent");

                    HttpContent content = response.Body;
                    string contentAsString = await content.ReadAsStringAsync();
                    Console.WriteLine($"{response.StatusCode}");
                    Console.WriteLine(contentAsString);

                    if (response.IsSuccessStatusCode) context.MarkRunFailed($"{response.StatusCode}");
                    else context.MarkRunSuccess($"{response.StatusCode}");
                }


            }

        }
    }
}
