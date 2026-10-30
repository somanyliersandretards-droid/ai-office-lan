using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using AiOfficeLan.Models;

namespace AiOfficeLan.Networking;

public sealed class LanBroadcastService : IDisposable
{
    private const int Port = 18080;
    private readonly UdpClient _udp;
    private readonly System.Threading.CancellationTokenSource _cts = new();
    private readonly ConcurrentDictionary<string, PeerAnnouncement> _peers = new();
    private readonly string _localId = Guid.NewGuid().ToString("N");
    private readonly string _localName = $"Local-{Environment.UserName}";
    private readonly Thread _listenerThread;

    public event Action<PeerAnnouncement>? PeerDiscovered;
    public event Action<string>? StatusChanged;

    public LanBroadcastService()
    {
        _udp = new UdpClient();
        _udp.Client.EnableBroadcast = true;
        _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _listenerThread = new Thread(ListenLoop)
        {
            IsBackground = true
        };
    }

    public void StartHost()
    {
        Bind();
        NotifyStatus("Hosting office");
        BroadcastAnnouncement(new PeerAnnouncement
        {
            Id = _localId,
            Name = _localName,
            Role = "Host",
            Clearance = "Level-9",
            X = 420,
            Y = 200,
            Z = 0
        });
        _listenerThread.Start();
    }

    public void Join()
    {
        Bind();
        NotifyStatus("Listening for office peers");
        _listenerThread.Start();
        BroadcastAnnouncement(new PeerAnnouncement
        {
            Id = _localId,
            Name = _localName,
            Role = "Agent",
            Clearance = "Level-4",
            X = 520,
            Y = 300,
            Z = 0
        });
    }

    public void BroadcastAnnouncement(PeerAnnouncement peer)
    {
        try
        {
            var payload = JsonSerializer.Serialize(peer);
            var bytes = Encoding.UTF8.GetBytes(payload);
            var endpoint = new IPEndPoint(IPAddress.Broadcast, Port);
            _udp.Send(bytes, bytes.Length, endpoint);
        }
        catch (Exception)
        {
            NotifyStatus("LAN announcement failed");
        }
    }

    private void Bind()
    {
        try
        {
            _udp.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
        }
        catch (SocketException)
        {
            // Another process may already be bound. Keep going.
        }
    }

    private void ListenLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                IPEndPoint remote = new(IPAddress.Any, 0);
                var data = _udp.Receive(ref remote);
                if (data is null || data.Length == 0)
                {
                    continue;
                }

                var json = Encoding.UTF8.GetString(data);
                var peer = JsonSerializer.Deserialize<PeerAnnouncement>(json);
                if (peer is null || string.IsNullOrEmpty(peer.Id))
                {
                    continue;
                }

                if (peer.Id == _localId)
                {
                    continue;
                }

                _peers[peer.Id] = peer;
                PeerDiscovered?.Invoke(peer);
                NotifyStatus($"Peer detected: {peer.Name}");
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception)
            {
                continue;
            }
        }
    }

    private void NotifyStatus(string message)
    {
        StatusChanged?.Invoke(message);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _udp.Close();
    }
}
