import speech_recognition as sr
import sys
import socket

host, port = "127.0.0.1", 5050
# SOCK_STREAM means TCP socket

try:
    conn = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    # Connect to the server and send the data
    conn.connect((host, port)) 
except:
    print("Error:", sys.exc_info()[0])

def listen_microphone():
    recognizer = sr.Recognizer()
    recognizer.dynamic_energy_threshold = False
    recognizer.energy_threshold = 700
    recognizer.dynamic_energy_adjustment_ratio = 1.7
    # Adjust the microphone device index based on your system
    with sr.Microphone() as source:

        while True:
            data = conn.recv(1024)
            if data == "stop": break
            try:
                audio_data = recognizer.listen(source, timeout = None)  # Adjust the timeout as needed
                text = recognizer.recognize_google(audio_data, language=str(sys.argv[1]))  # You can choose a different recognizer or API

                if text:
                    print(text)
                    conn.sendall(text.encode("utf-8"))
            except sr.UnknownValueError:
                conn.sendall("Error: text not recognized".encode("utf-8"))
            except sr.RequestError as e:
                conn.sendall("Error: recogniztion request failed".encode("utf-8"))
            except socket.error:  
                conn.shutdown(2)  
                conn.close()
                print('connection error')
                break
            except:
                err = "Error: " + sys.exc_info()[0]
                conn.sendall(err.encode("utf-8"))
                conn.shutdown(2) 
                conn.close()
                break

    

if __name__ == "__main__":
    listen_microphone()