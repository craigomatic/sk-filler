using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using System.Reflection;

var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true)
            .Build();

var embeddingConfig = configBuilder.GetRequiredSection("EmbeddingConfig").Get<Config>();
var completionConfig = configBuilder.GetRequiredSection("CompletionConfig").Get<Config>();

var sk = Kernel.Builder.Configure(embeddingConfig, completionConfig);

var fields = new [] {
    new Field
    {
        IsRequired = true,
        Name = "First Name"    
    },
    new Field
    {
        IsRequired = true,
        Name = "Last Name"  
    },
    new Field
    {
        IsRequired = true,
        Name = "Phone Brand"      
    },
    new Field
    {
        IsRequired = false,
        Name = "Place of purchase"
    }
};

var semanticFiller = new SemanticFiller(sk, Assembly.GetEntryAssembly().LoadEmbeddedResource("sk_filler.skills.fillskill.skprompt.txt"));

Console.WriteLine("==This sample demonstrates how to fill in a form in a non-deterministic manner==");
Console.WriteLine();

var state = FillState.Incomplete;
var userInput = "Hi, I am having issues with my phone. Can you help me?";

Console.WriteLine("Bot says: Hi, I'm a bot. What can I help you with?");
Console.WriteLine($"User says: {userInput}");
Console.WriteLine();

while(!state.IsComplete)
{
    state = await semanticFiller.Execute(userInput, fields);

    //take the first field that is not filled in and prompt the user for it
    var field = state.Fields.FirstOrDefault(f => string.IsNullOrEmpty(f.Value));
    
    if (field != null)
    {
        Console.WriteLine($"Bot says: {field.Message}");
        Console.Write("User says: ");

        userInput = Console.ReadLine();
        field.Value = userInput;

        Console.WriteLine();
    }
}

var formFields = string.Join("\n", fields.Select(f => $"{f.Name}: {f.Value}"));
Console.WriteLine($"Thank you, form has been completed:\n{formFields}");