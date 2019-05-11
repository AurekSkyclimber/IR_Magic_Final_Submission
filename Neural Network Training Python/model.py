import numpy as np
import hyperparameters as hp
import random

from sklearn.svm import LinearSVC

class Model:

    def __init__(self, train_images, train_labels, num_classes):
        self.input_size = train_images.shape[1]
        self.num_classes = num_classes
        self.learning_rate = hp.learning_rate
        self.batchSz = hp.batch_size
        self.train_images = train_images
        self.train_labels = train_labels
        self.clf = LinearSVC(multi_class='ovr',dual=False)

        self.W = np.random.rand(self.input_size, self.num_classes)
        self.b = np.zeros((1, self.num_classes))

    def train_nn(self):
        indices = list(range(self.train_images.shape[0]))
        delta_W = np.zeros((self.input_size, self.num_classes))
        delta_b = np.zeros((1, self.num_classes))

        for epoch in range(hp.num_epochs):
            loss_sum = 0
            random.shuffle(indices)

            for index in range(len(indices)):
                i = indices[index]
                img = self.train_images[i]
                gt_label = self.train_labels[i]
                l = np.add(np.dot(img, self.W), self.b)
                l = np.subtract(l, np.max(l))
                p = np.divide(np.exp(l), np.sum(np.exp(l)))
                loss_sum = loss_sum + (0 if p[0,gt_label] == 0 else -np.log(p[0,gt_label]))
                delta_W = np.zeros((self.input_size, self.num_classes))
                delta_b = np.zeros((1, self.num_classes))
                
                for j in range(self.num_classes):
                    delta_b[0,j] = p[0,j] - 1 if j == gt_label else p[0,j]
                    delta_W[:,j] = img * delta_b[0,j]

                self.W = np.subtract(self.W, np.dot(delta_W, self.learning_rate))
                self.b = np.subtract(self.b, np.dot(delta_b, self.learning_rate))
                
            print( "Epoch " + str(epoch) + ": Total loss: " + str(loss_sum) )

    def accuracy_nn(self, test_images, test_labels):
        """
        Computer the accuracy of the neural network model over the test set.
        """
        scores = np.dot(test_images, self.W) + self.b
        predicted_classes = np.argmax(scores, axis=1)
        return np.mean(predicted_classes == test_labels)
