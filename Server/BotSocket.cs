using System.Runtime.InteropServices.JavaScript;

namespace Server;

using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using Json = Dictionary<string, string>;

public class BotSocket {
    
    private TcpListener _listener = null;
    private TcpClient _client = null;
    private Stream _stream = null;
    private int _port;

    private Queue<Json> _incomingMessages = new();
    private Queue<Json> _outgoingMessages = new();
    
    
    public BotSocket(int port) {
        try {
            _port = port;
            Thread thread = new Thread(() => CreateSocket(port));
            thread.Start();
        } catch (Exception e) {
          Console.WriteLine(e);
        }
    }
    
    public bool Connected => _stream != null && _client.Connected;

    
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
            if (_outgoingMessages.Count > 0) {
                Json outgoingMessage;

                lock (_outgoingMessages) {
                    outgoingMessage = _outgoingMessages.Dequeue();
                }
                SendMessageHelper(outgoingMessage);
            }

            Json incomingMessage = ReceiveMessage();
            if (incomingMessage.Count > 0) {
                lock (_incomingMessages) {
                    _incomingMessages.Enqueue(incomingMessage);
                }
            }

            Thread.Sleep(10);
            //wait for 5ms
        }
    }

    private void SendMessageHelper(Json message) {
        string json = JsonSerializer.Serialize(message);
        byte[] data = Encoding.UTF8.GetBytes(json + "\n");
        _stream.Write(data, 0, data.Length);
        _stream.Flush();
    }

    private Json ReceiveMessage() {
        string? json = null;
        try {

            using var reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);
            json = reader.ReadLine();

            if (json is null) {
                return new Dictionary<string, string>();
            }

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        }
        catch {
            Console.WriteLine("invalid received object " + json);
        }
        return new Dictionary<string, string>();
    }
}