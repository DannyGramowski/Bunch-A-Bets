import socket
import requests
import json
import random
import time
import sys

client_socket: socket.socket = None
HOST_ADDR = '64.201.234.55'  # Must match the server's IP; note that this is provided through the dockerfile

def create_websocket(port):
    host = HOST_ADDR

    # Create a socket object
    global client_socket
    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    try:
        # Connect to the server
        client_socket.connect((host, port))
        client_socket.settimeout(1.0) # You don't need to add a timeout, but this allows us to Ctrl + C out of the program
        print("Connected to server!")
        while True:
            get_and_handle_message()

    except ConnectionRefusedError:
        print("Failed to connect. Is the server running?")

    finally:
        client_socket.close()

def get_and_handle_message():
    try:
        incoming_str = client_socket.recv(4096)
        print('Received message from server:', incoming_str)
        for msg_part in [s for s in incoming_str.decode('utf-8').split('\n') if s]: 
            incoming_message = json.loads(msg_part)
            command_type = incoming_message.get('command')
            
            command_callbacks = {
                'request_action': handle_request_action,
                'confirm_action': handle_confirm_action,
                'log_data': handle_log_data,
                'receive_chat': handle_chat_message,
                'hand_result': handle_hand_result,
            }

            if command_callbacks.get(command_type):
                command_callbacks[command_type](incoming_message)

    except KeyboardInterrupt as k:
        raise k
    except TimeoutError as t:
        pass
    except Exception as e:
        print('Error during message handling:', e)

def handle_request_action(msg: dict) -> None:
    # In this demo script, we avoid taking the game state into account, and just take a random action every time.
    # You should probably not do that.
    random_value = random.randint(1, 10)
    if random_value <= 2:
        raise_amount = random.randint(1, 21) * 10
        if raise_amount > 200:
            raise_amount = 2000 # very big bet; if this is larger than your bank, you'll be all in
        take_action('raise', msg['highest_bid_value'] + raise_amount)
    elif random_value <= 4:
        take_action('fold')
        if random_value == 4:
            send_chat("I always get the worst cards!")
    else:
        take_action('call')

def handle_confirm_action(msg: dict) -> None:
    if msg['result'] == 'error':
        # We only do this 10% of the time - no need to spam chat! (You'll only be allowed to send one message every 5 seconds anyways)
        print('Took an illegal action with the following server error:', msg['error'])

def handle_log_data(msg: dict) -> None:
    return

def handle_chat_message(msg: dict) -> None:
    if random.randint(1, 10) == 1:
        # We only do this 10% of the time - no need to spam chat! (You'll only be allowed to send one message every 5 seconds anyways)
        send_chat(f"Haha! Thats pretty funny, {msg['author']['name']}")

def handle_hand_result(msg: dict) -> None:
    return

def take_action(action: str, bet_amount: int = 0) -> None:
    msg = {
        'command': 'take_action',
        'action_type': action,
    }
    if bet_amount:
        msg['raise_amount'] = str(bet_amount)
    send_message(msg)

def send_chat(message: str) -> None:
    send_message({
        'command': 'send_chat',
        'message': message,
    })

def request_logs() -> None:
    send_message({
        'command': 'get_logs',
    })

def send_message(msg: dict):
    message = json.dumps(msg)
    print('Message sent to server:', message)
    client_socket.sendall(message.encode('utf-8'))

def register() -> None:
    name = f'Randobot {"".join([str(random.randint(0, 9)) for _ in range(5)] )}'
    print(name)
    req = requests.post(f'http://{HOST_ADDR}:5000/register', json={
        'name': name,
        'test_game_size': 6,
        'test_hand_count': 6,
    })

    if req.status_code != 200:
        print("Failed to connect via HTTP. Is the server running?")
        sys.exit()

    data = json.loads(req.text)
    print(data)

    create_websocket(data['portNumber'])

def main():
    register()
    print('Shutting down bot.')

if __name__ == "__main__":
    main()