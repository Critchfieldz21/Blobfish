import os

def iou(x1, y1, x2, y2, w1, h1, w2, h2):
    box1_area = w1 * h1
    box2_area = w2 * h2
    w1 = w1/2
    h1 = h1/2
    w2 = w2/2
    h2 = h2/2
    
    #intersection rectangle
    left = max(x1 - w1, x2 - w2)
    right = min(x1 + w1, x2 + w2)
    top = max(y1 - h1, y2 - h2)
    bot = min(y1 + h1, y2 + h2)
    
    if right < left or bot < top:
        return 0.0
    
    intersection_area = (right - left) * (bot - top)
    iou = float(intersection_area) / float(box1_area + box2_area - intersection_area)
    return iou

def perimage(label_path, predicted_path):
    x1 = y1 = x2 = y2 = w1 = h1 = w2 = h2 = 0
    with open(label_path, "r") as rf:
        cl, x1, y1, w1, h1 = [float(fl) for fl in rf.readline().strip().split()]
    with open(predicted_path, "r") as rf:
        cl, x2, y2, w2, h2 = [float(fl) for fl in rf.readline().strip().split()]
    return iou(x1, y1, x2, y2, w1, h1, w2, h2)


label_path = "scripts\\data\\valid\\labels\\"
predicted_path = "scripts\\data\\predicted\\labels\\"

images = os.listdir(label_path)
ious = []
for img in images:
    ious.append(perimage(label_path+img, predicted_path+img))
print(min(ious), max(ious), sum(ious), len(ious), sum(ious)/len(ious))