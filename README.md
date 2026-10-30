# AI Office LAN

A Windows desktop prototype for a stylized virtual office where local AI agents can join over LAN, appear as avatars, and collaborate in a shared room. This is a practical starter app that is designed to run on Windows and evolve into a real connected multi-agent workspace.

## What this starter includes

- 3D-inspired office layout with desks, walls, and lobby
- LAN discovery and peer presence announcements
- local or remote agent cards with clearance labels
- sample AI agents to immediately test the office shell
- a connector interface so your own AI can be plugged in later
- no heavy engine dependency; it is simple, lightweight, and fast to iterate on

## Windows setup

1. Install Visual Studio 2022 with .NET desktop development workload.
2. Open `AiOfficeLan.sln`.
3. Restore NuGet packages.
4. Set `AiOfficeLan` as the startup project.
5. Build and run.

## Usage

- Click Host Office to start broadcasting the office on the LAN.
- Click Join Office to listen for peers on the LAN.
- Click Add Sample AI to spawn another agent in the office.
- Use the list to inspect connected AIs.

## How to plug in your own AI

Open `Agents/IAiConnector.cs` and replace the sample `EchoAgentConnector` implementation with your own model integration.

Example pattern:

```csharp
public sealed class MyAiConnector : IAgentConnector
{
    public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        // Call your own model or service here.
        return await Task.FromResult("My AI response: " + prompt);
    }
}
```

Then wire it into `MainWindow.xaml.cs` by replacing the current connector initialization.

## Project structure

- `App.xaml` and `App.xaml.cs` – app bootstrap
- `MainWindow.xaml` and `MainWindow.xaml.cs` – office UI and rendering
- `Models/AgentEntry.cs` – UI agent model
- `Models/PeerAnnouncement.cs` – LAN discovery payload
- `Networking/LanBroadcastService.cs` – UDP LAN host/join logic
- `Agents/IAiConnector.cs` – AI connector abstraction

## Notes

This is a visual office shell and a solid base for a multi-AI collaboration platform. It is intentionally lightweight and easy to extend.

If you want the next step, I can continue by building:

- a room access / clearance gate system
- proper animation and movement between desks
- a proper meeting room hub
- real chat / message exchange between AI peers
- a richer 3D scene with true perspective camera movement
- app packaging for installation on Windows
