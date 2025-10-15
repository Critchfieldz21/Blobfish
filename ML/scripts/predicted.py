#pip install ultralytics

from ultralytics import YOLO
from PIL import Image
import os

def predict(img_path, model, dest_path):
    try:
        images = os.listdir(img_path)
    except FileNotFoundError:
        print("no image file found")
    
    
    for image in images:
        width, height = Image.open(img_path+image).size
        results = model.predict(img_path+image)
        boxes = results[0].boxes.xywh.tolist()
        classes = results[0].boxes.cls.tolist()
        confidences = results[0].boxes.conf.tolist()
        
        text_f = ""
        for i, box in enumerate(boxes):
            text_f += f"{int(classes[i])} {box[0]/width} {box[1]/height} {box[2]/width} {box[3]/height}\n"
            if len(boxes) > 1:
                print(confidences[i])
        with open(f"{dest_path}{image[:-4]}.txt", "w") as file_f:
            print(text_f.strip(), file=file_f)

if __name__ == "__main__":
    try:
        model = YOLO("scripts\\best.pt")
        predict("scripts\\data\\valid\\images\\", model, "scripts\\data\\predicted\\labels\\")
    except Exception:
        print("Exception.")
    