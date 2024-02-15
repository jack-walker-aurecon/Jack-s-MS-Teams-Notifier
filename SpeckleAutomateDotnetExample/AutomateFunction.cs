using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Objects;

using SendGrid;
using SendGrid.Helpers.Mail;
using SendGrid.Extensions.DependencyInjection;
using Speckle.Automate.Sdk;

using SpeckleAutomateDotnetExample;

static class AutomateFunction
{
    public static async Task Run(
      AutomationContext automationContext,
      FunctionInputs functionInputs
    )
    {

        IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        var services = ConfigureServices(new ServiceCollection(), configuration).BuildServiceProvider();
        var client = services.GetRequiredService<ISendGridClient>();


        Console.WriteLine("Starting execution");
        _ = typeof(ObjectsKit).Assembly; // INFO: Force objects kit to initialize

        Console.WriteLine("Receiving version");
        var commitObject = await automationContext.ReceiveVersion();

        Console.WriteLine("Received version: " + commitObject);

        await SendGridHelper.Execute(functionInputs, automationContext, client);
    }

    static IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSendGrid(options => { options.ApiKey = configuration["SendGrid:ApiKey"]; });
        return services;
    }
}
