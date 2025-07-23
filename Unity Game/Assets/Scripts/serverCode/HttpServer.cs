using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Server
{


    public class HttpServer
    {
        private static readonly int START_EXTERNAL_PORT = 26100;
        private static readonly int STOP_EXTERNAL_PORT = 26599;

        private static int GLOBAL_ID = 1;

        public static int GetGlobalBotID()
        {
            return GLOBAL_ID++;
        }

        /**
         * This is a blocking call
         * registerBot: function that takes in string name and returns tuple of (id, portNumber)
         */
        public static void Run(EpicFactory epicFactory)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://*:5001/"); // CHANGE BACK TO 5000 TODO
            listener.Start();
            
            while (true) {
                var context = listener.GetContext(); // blocks
                var request = context.Request;
                var response = context.Response;

                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/register")
                {

                    Debug.Log($"New Bot requested to register from {context.Request.RemoteEndPoint?.Address}");
                    string bodyStr;
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        bodyStr = reader.ReadToEnd();
                    }
                    try
                    {
                        HandleRegisterEndpoint(bodyStr, request, response, epicFactory);
                        continue;
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Failed to initialize bot from /register message: {bodyStr}. Error: {e.Message} {e.StackTrace}");
                    }
                    WriteJsonResponse(response, new { status = "Failed to register" });
                }

                else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/")
                {
                    WriteJsonResponse(response, new { status = "Hello World!" });
                }
            }

        }

        [Serializable]
        public class RegisterRequest
        {
            public string name;
            public int test_game_size = 6;    // default 6
            public int test_hand_count = 6;   // default 6
        }

        public static void HandleRegisterEndpoint(string bodyStr, HttpListenerRequest request, HttpListenerResponse response, EpicFactory epicFactory)
        {
            Debug.Log($"New Bot requested to register from {request.RemoteEndPoint?.Address}");

            if (string.IsNullOrEmpty(bodyStr))
            {
                WriteBadRequestResponse(response, new { error = "Request body is required" });
                return;
            }

            RegisterRequest data = null;

            try
            {
                data = JsonConvert.DeserializeObject<RegisterRequest>(bodyStr);
            }
            catch (Exception e)
            {
                Debug.Log($"JSON deserialization error: {e.Message}");
                WriteBadRequestResponse(response, new { error = "Malformed JSON" });
                return;
            }

            // Validate 'name'
            if (string.IsNullOrEmpty(data.name))
            {
                WriteBadRequestResponse(response, new { error = "Name is required" });
                return;
            }
            if (data.name.Length > 30)
            {
                WriteBadRequestResponse(response, new { error = "Names have a max length of 30 characters." });
                return;
            }

            // Validate test_game_size
            if (data.test_game_size < 2 || data.test_game_size > 6)
            {
                data.test_game_size = 6;  // reset to default if out of range
            }

            // Validate test_hand_count
            if (data.test_hand_count < 1 || data.test_hand_count > 24)
            {
                data.test_hand_count = 6; // reset to default if out of range
            }

            try
            {
                int botId = GetGlobalBotID();
                int portNumber = GetOpenPort();

                Bot newBot = new Bot(botId, portNumber, data.name, Epic.STARTING_BANK);
                epicFactory.RegisterBot(newBot, data.test_game_size, data.test_hand_count);

                var responseData = new { id = botId, portNumber = portNumber };
                WriteJsonResponse(response, responseData);
                return;
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to register bot: {e.Message} {e.StackTrace}");
                WriteJsonResponse(response, new { error = "Failed to register bot" }, 500);
                return;
            }
        }


        private static void WriteJsonResponse(HttpListenerResponse response, object obj, int statusCode = 200)
        {
            string json = JsonConvert.SerializeObject(obj);
            Debug.Log(obj);
            Debug.Log(json);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.StatusCode = statusCode;
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        private static void WriteBadRequestResponse(HttpListenerResponse response, object errorObject)
        {
            string json = JsonConvert.SerializeObject(errorObject);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            response.StatusCode = 400; // Bad Request
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        private static int GetOpenPort()
        {
            int randomPort = -1;
            for (int ct = 0; ct < 300; ct++)
            {
                randomPort = Program.Random(START_EXTERNAL_PORT, STOP_EXTERNAL_PORT);
                try
                {
                    var listener = new TcpListener(IPAddress.Any, randomPort); // Port 0 = let OS assign
                    listener.Start();
                    listener.Stop();
                    break;
                }
                catch (SocketException)
                {
                    Debug.Log("Tried to assign to in-use port " + randomPort.ToString());
                }
            }

            return randomPort;
        }
    }

}