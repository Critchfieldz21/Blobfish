import os
import shutil


source_folder = "ShopTicketData\\"

files = os.listdir("ShopTicketData\\images\\")

#move 400 images to validation folder. Total images: 2069
for i in range(400):
    image = files[i]
    
    shutil.move(f"{source_folder}images\\{image}", f"{source_folder}valid\\images\\{image}")
    shutil.move(f"{source_folder}labels\\{image[:-4]}.txt", f"{source_folder}valid\\labels\\{image[:-4]}.txt")