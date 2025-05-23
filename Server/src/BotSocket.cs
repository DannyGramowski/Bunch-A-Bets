using System.Runtime.InteropServices.JavaScript;

namespace Server;

using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using Json = Dictionary<string, object>;
using System.Text.RegularExpressions;

public class BotSocket {
    
    private TcpListener? _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private int _port;

    private Queue<Json> _incomingMessages = new();
    private Queue<Json> _outgoingMessages = new();
    
    
    public BotSocket(int port) {
        try {
            _port = port;
            Thread thread = new Thread(() => CreateSocket(port));
            thread.Start();
        } catch (Exception e) {
          Console.WriteLine("Error initializing bot: " + e);
        }
    }
    
    public bool Connected => _stream != null && (_client?.Connected ?? false);

    
    public void SendMessage(Json message) {
        lock (_outgoingMessages) {
            _outgoingMessages.Enqueue(message);
        }
    }

    public Json ReadMessage() {
        lock (_incomingMessages) {
            return _incomingMessages.Dequeue();
        }
    }

    public bool HasMessageReceived() {
        lock(_incomingMessages) return _incomingMessages.Count > 0;
    }

    private void CreateSocket(int port) {
        _listener = new TcpListener(IPAddress.Parse(ServerUtils.IP), port);
        _listener.Start();
        Console.WriteLine($"Server started on {ServerUtils.IP}:{port} waiting for connections...");

        _client = _listener.AcceptTcpClient();
        _stream = _client.GetStream();
        Console.WriteLine("Client connected on port " + port);
        
        SendMessage(new Json() { {"Welcome", "hi"} });
        
        while (true) {
            if (_outgoingMessages.Count > 0)
            {
                Json outgoingMessage;

                lock (_outgoingMessages)
                {
                    outgoingMessage = _outgoingMessages.Dequeue();
                }
                SendMessageHelper(outgoingMessage);
            }

            List<Json> incomingMessage = ReceiveMessage();
            if (incomingMessage.Count > 0) {
                lock (_incomingMessages) {
                    foreach (Json j in incomingMessage)
                    {
                        _incomingMessages.Enqueue(j);
                    }
                }
            }

            Thread.Sleep(10);
            //wait for 5ms
        }
    }

    private void SendMessageHelper(Json message) {
        string json = JsonSerializer.Serialize(message);
        byte[] data = Encoding.UTF8.GetBytes(json + "\n");
        if (_stream == null) return; // TODO this doesn't fully handle when bots close connections
        _stream.Write(data, 0, data.Length);
        _stream.Flush();
    }

    public List<Json> ReceiveMessage() {
        string? json = null;
        try {
            if (_stream == null || !_stream.DataAvailable) return new List<Json>();

            var buffer = new byte[4096];
            var allBytes = new List<byte>();

            while (_stream.DataAvailable)
            {
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // connection closed
                allBytes.AddRange(buffer[..bytesRead]);
            }

            json = Encoding.UTF8.GetString(allBytes.ToArray());

            if (json is null || json.Length < 2)
            {
                return new List<Json>();
            }

            List<string> messages = new List<string>();

            foreach (Match match in Regex.Matches(json, @"\{[^}]*\}"))
            {
                messages.Add(match.Value);
            }

            if (messages.Count == 0)
            {
                return new List<Json>();
            }

            return messages.Select(m => JsonSerializer.Deserialize<Json>(m) ?? new()).ToList();
        }
        catch {
            Console.WriteLine("Invalid received object " + json);
        }
        return new List<Json>();
    }
}