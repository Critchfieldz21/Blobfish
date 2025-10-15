#pytorch is installed by following the guideline from the official website: https://pytorch.org/get-started/locally/
#Make sure pytorch is installed with cuda support. If installed without it, add "--force-reinstall" to reinstall with cuda support.

#Used to check if gpu is available

import torch

if torch.cuda.is_available():
    print("cuda")
else:
    print("cpu")