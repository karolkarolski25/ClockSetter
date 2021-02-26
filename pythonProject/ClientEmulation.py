import socket
import random
import time


class ClientEmulator:

    server_ip = '0.0.0.0'
    server_port = 0

    def __init__(self, server_ip, server_port):
        self.server_ip = server_ip
        self.server_port = server_port

    def get_random_date(self, i):
        return str(i) + "/01/2020"

    def get_random_time(self):
        return str(random.randint(0, 600))

    def get_random_power_on_count(self):
        return str(random.randint(1, 99))

    def get_random_net_data(self):
        downloaded_data = round(random.random(), 4)
        sent_data = round(random.random(), 4)

        return downloaded_data, sent_data

    def emulate(self):
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.connect((self.server_ip, self.server_port))

            message = []

            for i in range(0, 31):
                net_data = self.get_random_net_data()

                message.append(
                    self.get_random_date(i + 1) + "|" + self.get_random_time()+ "|" + self.get_random_power_on_count()
                    + "|" + str(net_data[0]) + "|" + str(net_data[1]))

                sock.send(message[i].encode('utf-8'))

                time.sleep(0.01)

            sock.send("KarolLaptop".encode('utf-8'))

        except Exception as ex:
            print("ERROR in Client emulation: " + ex)