import socket
from model import Model

backlog = 1
size = 1024
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind(('127.0.0.1', 5062))
s.listen(backlog)

try:
    print ("is waiting")
    client, address = s.accept()

    model = Model()
    
    while 1:
        data = client.recv(size).strip()
        if data:
            print (data.decode())
            if(data.decode() == "Analyze"):
                prediction = model.find_match()
                print("Got a prediction")
                print(prediction)
                client.send(str(prediction[0]).encode('utf-8'))
                print("Sent Message\n")
except:
    print("closing socket")
    client.close()
    s.close()