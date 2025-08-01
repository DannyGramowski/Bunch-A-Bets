﻿
using System.Text;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;

using Json = System.Collections.Generic.Dictionary<string, object>;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Server {
    
    public class BotSocket
    {


        private TcpListener? _listener;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private int _port;

        private Queue<Json> _incomingMessages = new();
        private Queue<Json> _outgoingMessages = new();
        public bool Ready = false;

        public BotSocket(int port, Bot bot)
        {
            Debug.Log("Init botsocket");
            try
            {
                _port = port;
                Thread thread = new Thread(() => CreateSocket(port, bot));
                thread.Start();
            }
            catch (Exception e)
            {
                Debug.Log("Error initializing bot: " + e);
            }
        }

        public bool Connected => _stream != null && (_client?.Connected ?? false);


        public void SendMessage(Json message)
        {
            lock (_outgoingMessages)
            {
                _outgoingMessages.Enqueue(message);
            }
        }

        public Json ReadMessage()
        {
            lock (_incomingMessages)
            {
                return _incomingMessages.Dequeue();
            }
        }

        public bool HasMessageReceived()
        {
            lock (_incomingMessages) return _incomingMessages.Count > 0;
        }

        private void CreateSocket(int port, Bot bot)
        {
            //to create bots for testing
            if (port == -1) return;

            // Needs error handling here in case we accidentally double-assigned a port

            try
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                Debug.Log($"Server started on {ServerUtils.IP}:{port} waiting for connections...");
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to open up listener on port {port} for bot: {bot.Name} ({bot.ID}). Error: {e.Message} {e.StackTrace}");
            }

            try
            {
                _client = _listener.AcceptTcpClient();
                _stream = _client.GetStream();
                Ready = true;
                Debug.Log("Client connected on port " + port);

                SendMessage(new Json() { { "Welcome", "hi" } });
                bot.TryStartEpic();
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to receive a connection message on {port} for bot: {bot.Name} ({bot.ID}). Error: {e.Message} {e.StackTrace}");
            }

            while (true)
            {
                try
                {
                    if (_outgoingMessages.Count > 0)
                    {
                        Json outgoingMessage;

                        lock (_outgoingMessages)
                        {
                            outgoingMessage = _outgoingMessages.Dequeue();
                        }
                        SendMessageHelper(outgoingMessage);
                    }
                } catch (Exception e)
                {
                    Debug.Log($"Failed to send messages to: {bot.Name} ({bot.ID}). Error: {e.Message} {e.StackTrace}");
                }

                try
                {
                    List<Json> incomingMessage = ReceiveMessage();
                    if (incomingMessage.Count > 0)
                    {
                        lock (_incomingMessages)
                        {
                            foreach (Json j in incomingMessage)
                            {
                                _incomingMessages.Enqueue(j);
                            }
                        }
                    }

                } catch (Exception e)
                {
                    Debug.Log($"Failed to receive messages from: {bot.Name} ({bot.ID}). Error: {e.Message} {e.StackTrace}");
                }
                
                Thread.Sleep(10);
            }
        }

        private void SendMessageHelper(Json message)
        {
            string json = JsonConvert.SerializeObject(message); // NOTE this is not used anymore and any GetLogs method during production will break / not work / not look right
            byte[] data = Encoding.UTF8.GetBytes(json + "\n");
            if (_stream == null) return;
            try
            {
                _stream.Write(data, 0, data.Length);
                _stream.Flush();
            }
            catch (IOException e)
            {
                Debug.Log("Error when sending message to bot: " + e.Message);
            }
            catch (Exception e)
            {
                Debug.Log("Uncaught error when sending message to bot: " + e.Message);
            }
        }

        public List<Json> ReceiveMessage()
        {
            string? json = null;
            try
            {
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
                    Debug.Log("Invalid received object " + json);
                    return new List<Json>();
                }

                return messages.Select(m => JsonConvert.DeserializeObject<Json>(m) ?? new()).ToList();
            }
            catch
            {
                Debug.Log("Invalid received object " + json);
            }
            return new List<Json>();
        }

        public void CloseSocket()
        {
            try
            {
                _listener?.Stop();
            }
            catch
            {
                Debug.Log("Error: Failed to close socket with port " + _port); // womp womp
            }
        }
    }
}