#!/usr/bin/env python

import threading

from ClientEmulation import ClientEmulator
from TcpServer import TcpServer

if __name__ == "__main__":
    tcp_server = TcpServer('127.0.0.1', 2048)
    client_emulator = ClientEmulator('127.0.0.1', 2048)

    t1 = threading.Thread(target=tcp_server.receive_data, args=[])
    t2 = threading.Thread(target=client_emulator.emulate, args=[])

    t1.start()
    t2.start()


