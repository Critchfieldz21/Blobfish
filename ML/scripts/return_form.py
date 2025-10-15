#pip install pdf2image
#pdf2image requires poppler library, which can be downloaded online as zip file.

import argparse
import re
import os
import shutil
import cv2

import numpy as np
from PIL import Image
from ultralytics import YOLO
from pathlib import Path
from pdf2image import convert_from_path

def saveImage(filename, pdf_folder, des_folder, model, poppler_path):
    x = re.search(r"(?:_P)(\d+)", filename)
    if x:
        x=int(x[1])
    else:
        print("no match")
    print("\nConverting file: " + pdf_folder + filename + "\npage number: " + str(x))

    images = convert_from_path(os.path.join(pdf_folder, filename), poppler_path=poppler_path)

    predict(images, x, model, des_folder, filename)

def predict(images, x, model, save_path, file_name):
    for j in range(len(images)):
        if j == x:
            img = images[j]
            width, height = img.size
            results = model.predict(img)
            boxes = results[0].boxes.xywh.tolist()
            classes = results[0].boxes.cls.tolist()
            cls = []
            text_f = ""
            label_thickness = 3
            image_f = cv2.cvtColor(np.array(img), cv2.COLOR_RGB2BGR)
            for i, box in enumerate(boxes):
                if i > 1:
                    if classes[i] in cls:
                        continue
                text_f += f"{int(classes[i])} {box[0]/width} {box[1]/height} {box[2]/width} {box[3]/height}\n"
                color = (255, 255, 0)
                cv2.rectangle(image_f, (round(box[0] - box[2]/2), round(box[1] - box[3]/2)), (round(box[0] + box[2]/2), round(box[1] + box[3]/2)), color, label_thickness)
                cls.append(classes[i])

            with open(os.path.join(save_path, (file_name[:-4] + ".txt")), "w") as file_f:
                print(text_f.strip(), file=file_f)
            print((file_name[:-4] + "_pg" + str(j) + ".jpg"))
            cv2.imwrite(os.path.join(save_path, (file_name[:-4] + "_pg" + str(j) + ".jpg")), image_f)
        else:
            print(j, file_name[:-4] + "_pg" + str(j) + ".jpg")
            images[j].save(os.path.join(save_path, (file_name[:-4] + "_pg" + str(j) + ".jpg")), "JPEG")

def returnForm(file_path):
    
    return file_path

def return_form():
    parser = argparse.ArgumentParser()
    # parser.add_argument("file_path", type=Path)
    parser.add_argument("images_folder", type = str)
    parser.add_argument("model_path", type=str)
    p = parser.parse_args()
    images_folder = p.images_folder.rstrip("/").rstrip("\\")
    print(images_folder)
    try:
        model = YOLO(p.model_path)
    except Exception as e:
        print(e)
        raise FileNotFoundError("Exception while taking in model path.")
    if(not os.path.isdir(images_folder)):
        raise FileNotFoundError("not a directory")
    files = os.listdir(images_folder)
    output_folder = os.path.join(images_folder, "temp/")
    if(os.path.exists(output_folder)):
        shutil.rmtree(output_folder)
        print("temp folder cleared")
    os.mkdir(output_folder)
    for pdf in files:
        if pdf.lower().endswith(".pdf") and not os.path.isdir(pdf):
            saveImage(pdf, images_folder, output_folder, model, poppler_path = "scripts\\poppler\\library\\bin")

if __name__ == "__main__":
    return_form()
    
