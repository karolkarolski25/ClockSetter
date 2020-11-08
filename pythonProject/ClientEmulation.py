import socket
import random
import time


class ClientEmulator:

    server_ip = '0.0.0.0'
    server_port = 0

    def __init__(self, server_ip, server_port):
        self.server_ip = server_ip
        self.server_port = server_port

    def get_random_date(self):
        return f"{random.randint(1, 28)}.{random.randint(1, 12)}.{random.randint(2000, 2031)}"

    def get_random_time(self):
        return f"{random.randint(10, 99)}:{random.randint(10, 59)}:{random.randint(10, 59)}"

    def get_random_power_on_count(self):
        return random.randint(1, 99)

    def get_random_net_data(self):
        downloaded_data = round(random.random(), 4)
        sent_data = round(random.random(), 4)

        return downloaded_data, sent_data

    def emulate(self):
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.connect((self.server_ip, self.server_port))

            message = []

            for i in range(0, 25):
                net_data = self.get_random_net_data()

                message.append(
                    f"{self.get_random_date()}|{self.get_random_time()}|{self.get_random_power_on_count()}|" \
                    f"{net_data[0]}|{net_data[1]}")

                sock.send(message[i].encode('utf-8'))

                time.sleep(0.01)

            sock.send("KarolLaptop".encode('utf-8'))

        except Exception as ex:
            print(f"ERROR in Client emulation: {ex}")