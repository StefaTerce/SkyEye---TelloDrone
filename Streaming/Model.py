import numpy as np
import cv2
import urllib.request
import torch
import torch.nn as nn
from torchvision import transforms
from PIL import Image
import os

# Define the path to the trained model

script_directory = os.path.dirname(os.path.abspath(__file__))
project_directory = os.path.dirname(script_directory)
print("Current Directory:", project_directory)
current_directory = os.path.join(project_directory, "Streaming\\image_classifier_8.pt")


model_path = current_directory

class ImageClassifier(nn.Module):
    def __init__(self):
        super(ImageClassifier, self).__init__()
        self.conv_layers = nn.Sequential(
            nn.Conv2d(3, 16, kernel_size=3, padding=1),
            nn.ReLU(),
            nn.MaxPool2d(2),
            nn.Conv2d(16, 32, kernel_size=3, padding=1),
            nn.ReLU(),
            nn.MaxPool2d(2),
        )
        self.fc_layers = nn.Sequential(
            nn.Linear(32 * 56 * 56, 128),
            nn.ReLU(),
            nn.Linear(128, 3),  # Three classes: "red_circle", "green_circle", and "no_circles"
        )

    def forward(self, x):
        x = self.conv_layers(x)
        x = torch.flatten(x, 1)
        x = self.fc_layers(x)
        return x

# Create an instance of the ImageClassifier
classifier = ImageClassifier()
classifier.load_state_dict(torch.load(model_path))
classifier.eval()

def preprocess_image(image):
    transform = transforms.Compose([
        transforms.Resize((224, 224)),
        transforms.ToTensor(),
        transforms.Normalize((0.485, 0.456, 0.406), (0.229, 0.224, 0.225)),
    ])
    image = Image.fromarray(image)
    image = transform(image).unsqueeze(0)
    return image

def predict_frame(frame, model):
    # Apply Gaussian blur to the frame to reduce noise
    frame = cv2.GaussianBlur(frame, (5, 5), 0)

    image = preprocess_image(frame)
    with torch.no_grad():
        outputs = model(image)
        _, predicted = torch.max(outputs, 1)

    class_names = ["green_circle", "no_circles", "red_circle"]
    predicted_class = class_names[predicted.item()]
    confidence = torch.softmax(outputs, 1)[0][predicted].item()
    if confidence == 1.0 and predicted_class == "no_circles":
        predicted_class = "red_circle"

    return predicted_class, confidence



import socket
import time
time.sleep(5)

# Define the host and port of the C# program
host = '127.0.0.1'  # Replace with the C# program's IP address
port = 12345        # Replace with the C# program's port

# Create a socket object
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Connect to the C# program
s.connect((host, port))

stream = urllib.request.urlopen('http://127.0.0.1:9000')
bytes = bytes()
frame_counter = 0

while True:
    bytes += stream.read(1024)
    a = bytes.find(b'\xff\xd8')
    b = bytes.find(b'\xff\xd9')
    
    if a != -1 and b != -1:
        jpg = bytes[a:b+2]
        bytes = bytes[b+2:]
        frame = cv2.imdecode(np.fromstring(jpg, dtype=np.uint8), cv2.IMREAD_COLOR)
        
        frame_counter += 1
        
        # Analyze every 10 frames
        if frame_counter % 10 == 0:
            predicted_class, confidence = predict_frame(frame, classifier)
            
            # Send the predicted_class to the C# program with a newline delimiter
            message = f"{predicted_class}\n"
            s.send(message.encode())

        cv2.imshow('frame', frame)
        
        if cv2.waitKey(1) == 27:
            s.close()  # Close the socket before exiting
            exit(0)


