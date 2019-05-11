import socket

host, port  = "169.254.41.245", 25001 #Do not use: 169.254.206.102
sock=socket.socket(socket.AF_INET, socket.SOCK_STREAM)
data = "7"

try:
    sock.bind((host,port))
    sock.listen(1)
    conn, addr = sock.accept()
    print('Connection address:', addr)

    while 1:
        data = conn.recv(50)
        if not data: break;
        print("received data:", data.decode())
        
        sockUnity.sendto(data, (hostUnity, portUnity))
    conn.close()
finally:
    sock.close()