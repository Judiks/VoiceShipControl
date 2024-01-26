import speech_recognition as sr
import sys

def listen_microphone():
    recognizer = sr.Recognizer()
    # Adjust the microphone device index based on your system
    with sr.Microphone() as source:

        while True:
            try:
                audio_data = recognizer.listen(source, timeout=2)  # Adjust the timeout as needed
                text = recognizer.recognize_google(audio_data, language=str(sys.argv[1]))  # You can choose a different recognizer or API

                if text:
                    print(text)
                    break

            except sr.UnknownValueError:
                print("EX1")
            except sr.RequestError as e:
                print("EX2")
            except KeyboardInterrupt:
                print("Program terminated.")
                break

if __name__ == "__main__":
    listen_microphone()