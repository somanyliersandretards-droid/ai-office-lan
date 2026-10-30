using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AiOfficeLan.Agents;
using AiOfficeLan.Models;
using AiOfficeLan.Networking;

namespace AiOfficeLan;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<AgentEntry> _agents = new();
    private readonly Dictionary<string, AgentEntry> _agentLookup = new();
    private readonly LanBroadcastService _lan;
    private readonly IAgentConnector _localConnector = new EchoAgentConnector();
    private readonly Random _rng = new();
    private int _nextAgentIndex;

    public MainWindow()
    {
        InitializeComponent();
        AiListBox.ItemsSource = _agents;

        _lan = new LanBroadcastService();
        _lan.PeerDiscovered += OnPeerDiscovered;
        _lan.StatusChanged += OnStatusChanged;

        Loaded += (_, _) =>
        {
            SeedSampleAgents();
            BuildOfficeLayout();
        };
    }

    private void HostOffice_Click(object sender, RoutedEventArgs e)
    {
        _lan.StartHost();
        StatusLabel.Text = "Hosting office";
        StatusLabel.Foreground = new SolidColorBrush(Colors.LimeGreen);
    }

    private void JoinOffice_Click(object sender, RoutedEventArgs e)
    {
        _lan.Join();
        StatusLabel.Text = "Joining office";
        StatusLabel.Foreground = new SolidColorBrush(Colors.SkyBlue);
    }

    private void AddSampleAi_Click(object sender, RoutedEventArgs e)
    {
        var color = PickColor();
        var x = 220 + (_rng.Next(0, 5) * 90);
        var y = 120 + (_rng.Next(0, 5) * 80);
        AddAgent($"AI-{_nextAgentIndex++}", $"AI-{_nextAgentIndex}", "Operator", "Level-3", color, new Point(x, y));
    }

    private void SeedSampleAgents()
    {
        AddAgent("Ava", "Ava", "Lead Strategist", "Level-7", Colors.Cyan, new Point(260, 180));
        AddAgent("Nova", "Nova", "Ops Coordinator", "Level-5", Colors.MediumPurple, new Point(560, 280));
        AddAgent("Orion", "Orion", "Security Monitor", "Level-9", Colors.LimeGreen, new Point(430, 120));
    }

    private void AddAgent(string id, string name, string role, string clearance, Color color, Point position)
    {
        if (_agentLookup.ContainsKey(id))
        {
            return;
        }

        var agent = new AgentEntry
        {
            Id = id,
            Name = name,
            Role = role,
            Clearance = clearance,
            Color = color,
            Position = position,
            Label = $"{name} [{clearance}]"
        };

        _agentLookup[id] = agent;
        _agents.Add(agent);
        RenderAgents();
    }

    private void OnPeerDiscovered(PeerAnnouncement peer)
    {
        Dispatcher.Invoke(() =>
        {
            if (_agentLookup.TryGetValue(peer.Id, out var existing))
            {
                existing.Name = peer.Name;
                existing.Role = peer.Role;
                existing.Clearance = peer.Clearance;
                existing.Position = new Point(peer.X, peer.Y);
                existing.Label = $"{peer.Name} [{peer.Clearance}]";
                existing.Color = PickColor();
            }
            else
            {
                var entry = new AgentEntry
                {
                    Id = peer.Id,
                    Name = peer.Name,
                    Role = peer.Role,
                    Clearance = peer.Clearance,
                    Position = new Point(peer.X, peer.Y),
                    Color = PickColor(),
                    Label = $"{peer.Name} [{peer.Clearance}]"
                };

                _agentLookup[peer.Id] = entry;
                _agents.Add(entry);
            }

            RenderAgents();
        });
    }

    private void OnStatusChanged(string message)
    {
        Dispatcher.Invoke(() =>
        {
            StatusLabel.Text = message;
            StatusLabel.Foreground = new SolidColorBrush(Colors.White);
        });
    }

    private void BuildOfficeLayout()
    {
        OfficeCanvas.Children.Clear();

        var floor = new Rectangle
        {
            Width = 900,
            Height = 620,
            RadiusX = 22,
            RadiusY = 22,
            Fill = new SolidColorBrush(Color.FromRgb(15, 23, 42))
        };
        Canvas.SetLeft(floor, 20);
        Canvas.SetTop(floor, 20);
        OfficeCanvas.Children.Add(floor);

        var lobby = new Rectangle
        {
            Width = 220,
            Height = 120,
            Fill = new SolidColorBrush(Color.FromRgb(22, 163, 74)),
            Opacity = 0.35
        };
        Canvas.SetLeft(lobby, 350);
        Canvas.SetTop(lobby, 500);
        OfficeCanvas.Children.Add(lobby);

        var wallBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85));
        var deskBrush = new SolidColorBrush(Color.FromRgb(30, 41, 59));

        for (int x = 80; x < 820; x += 180)
        {
            for (int y = 80; y < 500; y += 130)
            {
                var desk = new Rectangle
                {
                    Width = 95,
                    Height = 60,
                    Fill = deskBrush,
                    RadiusX = 10,
                    RadiusY = 10
                };

                Canvas.SetLeft(desk, x);
                Canvas.SetTop(desk, y);
                OfficeCanvas.Children.Add(desk);
            }
        }

        OfficeCanvas.Children.Add(new Rectangle
        {
            Width = 900,
            Height = 8,
            Fill = wallBrush
        });

        OfficeCanvas.Children.Add(new Rectangle
        {
            Width = 900,
            Height = 8,
            Fill = wallBrush
        });

        var topWall = new Rectangle
        {
            Width = 8,
            Height = 620,
            Fill = wallBrush
        };
        Canvas.SetLeft(topWall, 20);
        Canvas.SetTop(topWall, 20);
        OfficeCanvas.Children.Add(topWall);

        var rightWall = new Rectangle
        {
            Width = 8,
            Height = 620,
            Fill = wallBrush
        };
        Canvas.SetLeft(rightWall, 912);
        Canvas.SetTop(rightWall, 20);
        OfficeCanvas.Children.Add(rightWall);

        var bottomWall = new Rectangle
        {
            Width = 900,
            Height = 8,
            Fill = wallBrush
        };
        Canvas.SetLeft(bottomWall, 20);
        Canvas.SetTop(bottomWall, 632);
        OfficeCanvas.Children.Add(bottomWall);

        RenderAgents();
    }

    private void RenderAgents()
    {
        var agentBorders = OfficeCanvas.Children
            .OfType<Border>()
            .Where(b => b.Tag is string tag && tag == "agent-avatar")
            .ToList();

        foreach (var border in agentBorders)
        {
            OfficeCanvas.Children.Remove(border);
        }

        foreach (var agent in _agents)
        {
            var avatar = new Border
            {
                Width = 120,
                Height = 52,
                CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(agent.Color),
                Opacity = 0.95,
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1.5),
                Padding = new Thickness(8, 4, 8, 4),
                Tag = "agent-avatar"
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var dot = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.White,
                Margin = new Thickness(0, 0, 8, 0)
            };

            var label = new TextBlock
            {
                Text = agent.Label,
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };

            stack.Children.Add(dot);
            stack.Children.Add(label);
            avatar.Child = stack;

            Canvas.SetLeft(avatar, agent.Position.X);
            Canvas.SetTop(avatar, agent.Position.Y);
            OfficeCanvas.Children.Add(avatar);
        }
    }

    private static Color PickColor()
    {
        var palette = new[]
        {
            Colors.Cyan,
            Colors.MediumPurple,
            Colors.LimeGreen,
            Colors.Orange,
            Colors.Pink,
            Colors.DeepSkyBlue,
            Colors.Salmon,
            Colors.Khaki
        };

        return palette[(DateTime.UtcNow.Millisecond + DateTime.UtcNow.Second) % palette.Length];
    }
}
