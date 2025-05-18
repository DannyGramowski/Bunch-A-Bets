import socket
import requests
import json

def create_websocket(port):
    host = '127.0.0.1'  # Must match the server's IP

    # Create a socket object
    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    try:
        # Connect to the server
        client_socket.connect((host, port))
        print("Connected to server!")
        response = client_socket.recv(100)
        print('response', response)
        # Optional: send data
        message = json.dumps({'womp': 'womp'})
        client_socket.sendall(message.encode('utf-8'))

    except ConnectionRefusedError:
        print("Failed to connect. Is the server running?")

    finally:
        client_socket.close()

def register():
    req = requests.post('http://localhost:5000/register?name=randobot')
    data = json.loads(req.text)
    print(data)

    create_websocket(data['portNumber'])

    #infinite loop to keep the window open when created by c#
    while True:
        ...


def main():
    register()

if __name__ == "__main__":
    main()