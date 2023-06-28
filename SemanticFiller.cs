using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

public class SemanticFiller
{
    private List<string> _MessageHistory = new List<string>();
    private readonly ISKFunction _Function;

    public SemanticFiller(IKernel sk, string function)
    {
        _Function = sk.CreateSemanticFunction(function,
            "evaluate",
            "fillskill",
            maxTokens: 4096);
    }

    public async Task<FillState> Execute(string input, IEnumerable<Field> desiredFields)
    {
        //naively prune older message history items to avoid context window limits
        if (_MessageHistory.Count >= 30)
        {
            _MessageHistory.RemoveRange(0, 10);
        }

        var contextVariables = new ContextVariables(input);
        contextVariables["messageHistory"] = string.Join('\n',_MessageHistory);
        contextVariables["fields"] = string.Join(",", desiredFields.Select(f=> $"Name: {f.Name}, Value: {f.Value}"));

        var fnResult = await _Function.InvokeAsync(new SKContext(contextVariables));

        if (fnResult.ErrorOccurred)
        {
            throw new Exception(fnResult.LastErrorDescription);
        }

        //store the input for the next turn
        _MessageHistory.Add(input);
        
        var result = JsonSerializer.Deserialize<IEnumerable<FieldViewModel>>(fnResult.Result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //update the desired fields with the result
        foreach (var item in desiredFields)
        {
            var resultField = result.FirstOrDefault(f => f.Name == item.Name);

            if (resultField == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.Value) &&
                !string.IsNullOrWhiteSpace(resultField.Value))
            {
                item.Value = resultField.Value;
            }
        }

        return new FillState
        {
            Fields = result,
            IsComplete = result.All(f => !string.IsNullOrEmpty(f.Value))
        };
    }
}