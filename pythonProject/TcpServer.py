import socket

from DatabaseService import DatabaseService
from ChartsService import ChartService


def print_received_data(database_content, computer_name):
    print("\nComputer name: " + computer_name)

    print("\nDatabase content: \n")

    for i in range(len(database_content)):
        for j in range(len(database_content[i])):
            print(database_content[i][j], end=" "),
        print()


class TcpServer:

    server_ip = '0.0.0.0'
    server_port = 0

    def __init__(self, server_ip, server_port):
        self.server_ip = server_ip
        self.server_port = server_port

    def receive_data(self):
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.bind((self.server_ip, self.server_port))
            sock.listen(1)

            print("\nStarting server")

            conn, addr = sock.accept()

            print('Connection address:', addr)

            database_content = []
            computer_name = ""

            while True:
                data = conn.recv(500).decode("utf-8")

                if not data:
                    break

                database_entry = data.split("|")

                if len(database_entry) == 1:
                    computer_name = database_entry[0]
                    break
                else:
                    database_content.append(database_entry)

            conn.close()

            print_received_data(database_content, computer_name)

            ChartService(database_content, computer_name).generate_chart()

        except Exception as ex:
            print("ERROR in Server: " + ex)
