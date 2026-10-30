namespace AiOfficeLan.Models;

public sealed class PeerAnnouncement
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = "AI Agent";
    public string Role { get; set; } = "Operator";
    public string Clearance { get; set; } = "Level-1";
    public double X { get; set; } = 0;
    public double Y { get; set; } = 0;
    public double Z { get; set; } = 0;
}
