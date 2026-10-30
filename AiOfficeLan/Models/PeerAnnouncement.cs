using System.Windows;
using System.Windows.Media;

namespace AiOfficeLan.Models;

public sealed class AgentEntry
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Clearance { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public Color Color { get; set; } = Colors.Cyan;
    public Point Position { get; set; } = new();
}
