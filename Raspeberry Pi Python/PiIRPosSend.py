# import the necessary packages
import cv2
from picamera.array import PiRGBArray
from picamera import PiCamera
import socket
import time

radius = 41

# initialize the camera and grab a reference to the raw camera capture
camera = PiCamera()
camera.resolution = (640, 480)
camera.framerate = 32
rawCapture = PiRGBArray(camera, size=(640, 480))
 
# allow the camera to warmup
time.sleep(0.1)

host, port  = "169.254.41.245", 25001
data = "0,0,0"

sock=socket.socket(socket.AF_INET, socket.SOCK_STREAM)
try:
    sock.connect((host,port))
    sock.sendall("START".encode("utf-8"))
    # capture frames from the camera
    for frame in camera.capture_continuous(rawCapture, format="bgr", use_video_port=True):
        color = frame.array
        gray = cv2.cvtColor(color, cv2.COLOR_BGR2GRAY)

        blur = cv2.GaussianBlur(gray, (radius, radius), 0)
        (minVal, maxVal, minLoc, maxLoc) = cv2.minMaxLoc(blur)
        cv2.circle(color, maxLoc, radius, (255, 0, 0), 2)

        # show the frame
        cv2.imshow("Frame", color)
        key = cv2.waitKey(1) & 0xFF
 
        # clear the stream in preparation for the next frame
        rawCapture.truncate(0)

        sock.sendall(("{:5.4f}".format(maxLoc[0] / 640) + "," + "{:5.4f}".format(maxLoc[1] / 480)).encode("utf-8"))

        # if the `q` key was pressed, break from the loop
        if key == ord("q"):
            break
finally:
    sock.close()