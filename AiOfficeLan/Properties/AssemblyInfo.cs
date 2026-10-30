namespace AiOfficeLan.Agents;

public interface IAgentConnector
{
    Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default);
}

public sealed class EchoAgentConnector : IAgentConnector
{
    public Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var response = $"Echo: {prompt}";
        return Task.FromResult(response);
    }
}

// Replace this with your own model connector later.
// Example:
// public sealed class MyCustomAiConnector : IAgentConnector
// {
//     public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
//     {
//         // call your own API or local model here
//         return await Task.FromResult("Working response");
//     }
// }
