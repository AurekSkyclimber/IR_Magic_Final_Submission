import numpy as np
from skimage import io
from skimage.color import rgb2gray
from skimage.transform import resize

class Model:

    def __init__(self):
        self.num_classes = 5
        self.labels = np.load('labels.npy')

        # sets up weights and biases...
        self.W = np.load('weights.npy')
        self.b = np.load('biases.npy')
        
    def find_match(self):
        """
        Find a match for the image with the trained neural network model.
        """
        try:
            im = rgb2gray(io.imread("../shape.png"))
            im_vector = resize(im, (28, 28)).reshape(1, 28 * 28)
            
            score = np.dot(im_vector, self.W) + self.b
            predicted_class = np.argmax(score, axis=1)
            return predicted_class
        except Exception as e:
            print(e)
        return 0