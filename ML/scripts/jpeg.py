#pip install pdf2image
#pdf2image requires poppler library, which can be downloaded online as zip file.

from pdf2image import convert_from_path
import re
import os

#Convert pdf files to image and save to folder. Only 1 page with the section/form view is saved.
def saveImage(filename, pdf_folder = "ShopTicketData\\pdfs\\", des_folder = "ShopTicketData\\images\\"):
    x = re.search(r"(?:_P)(\d+)", filename)
    if x:
        x=int(x[1])
    else:
        print("no match")
    print("\nConverting file: " + pdf_folder + filename + "\npage number: " + x)

    images = convert_from_path(pdf_folder + filename, poppler_path=".venv\\Lib\\Release-25.07.0-0\\poppler-25.07.0\\Library\\bin")

    images[x].save(f"{des_folder}{filename[:-4]}.jpg","JPEG")
    

files = os.listdir("ShopTicketData\\pdfs\\")
for image in files:
    saveImage(image, "ShopTicketData\\pdfs\\", "ShopTicketData\\images\\")