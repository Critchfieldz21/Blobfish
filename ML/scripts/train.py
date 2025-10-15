#pip install ultralytics

from ultralytics import YOLO

def train():
    model = YOLO("yolo11n.pt")  # load a pretrained model (recommended for training)

    results = model.train(data="ShopTicketData\\data.yaml", epochs=100, imgsz=640, batch=-1, name="trained_model", device=0)

if __name__ == "__main__":
    train()