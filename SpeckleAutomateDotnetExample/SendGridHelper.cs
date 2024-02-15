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

namespace SpeckleAutomateDotnetExample
{
    public static class SendGridHelper
    {
        internal static async Task Execute(FunctionInputs inputs, AutomationContext context)
        {
            Base? version = await context.ReceiveVersion();

            int count = version.Flatten().Count();

            Commit? commit = await context.SpeckleClient.CommitGet(context.AutomationRunData.ProjectId, context.AutomationRunData.VersionId);

            if (commit == null) context.MarkRunFailed("The run failed as the commit could not be retrieved");
            else
            {
                string name = context.SpeckleClient.Account.userInfo.name;
                string email = context.SpeckleClient.Account.userInfo.email;
                var _client = new SendGridClient(inputs.SendGridAPIKey);
                var from = new EmailAddress("jack.walker@aurecongroup.com", "Jack Walker");
                var subject = $"New commit to {context.AutomationRunData.BranchName}: {commit.message}";
                var to = new EmailAddress(email, name);


                string author = commit?.authorName == null ? "" : commit.authorName;
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
                var response = await _client.SendEmailAsync(message);
                HttpContent content = response.Body;
                string contentAsString = await content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) context.MarkRunFailed($"{response.StatusCode}: {contentAsString}");
                else context.MarkRunSuccess($"{response.StatusCode}: {contentAsString}");
            }

        }
    }
}
