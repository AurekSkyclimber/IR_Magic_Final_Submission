import numpy as np
import hyperparameters as hp
import gzip
from skimage import io
from skimage.color import rgb2gray
from skimage.transform import resize
from model import Model
import os
import sys
import argparse
import time

os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'

def load_data_scene(search_path, categories, size):
    images = np.zeros((size * hp.scene_class_count, hp.img_size * hp.img_size))
    labels = np.zeros((size * hp.scene_class_count,), dtype = np.int8)
    for label_no in range(hp.scene_class_count):
        img_path = search_path + categories[label_no]
        img_names = [f for f in os.listdir(img_path) if ".png" in f]
        for i in range(size):
            im = rgb2gray(io.imread(img_path + "/" + img_names[i]))
            im_vector = resize(im, (hp.img_size, hp.img_size)).reshape(1, hp.img_size * hp.img_size)
            index = size * label_no + i
            images[index, :] = im_vector
            labels[index] = label_no
    return images, labels

def get_categories_scene(search_path):
    dir_list = []
    for filename in os.listdir(search_path):
        if os.path.isdir(os.path.join(search_path, filename)):
            dir_list.append(filename)
    return dir_list

def format_data_scene_rec():
    train_path = "../data/train/"
    test_path = "../data/test/"
    categories = get_categories_scene(train_path)    
    train_images, train_labels = load_data_scene(train_path, categories, hp.num_train_per_category)
    test_images, test_labels = load_data_scene(test_path, categories, hp.num_test_per_category)
    return train_images, train_labels, test_images, test_labels

def main():
    train_images, train_labels, test_images, test_labels = format_data_scene_rec()
    num_classes = hp.scene_class_count

    model = Model(train_images, train_labels, num_classes)

    startTime = time.time()
    model.train_nn()
    print('runtime: ' + str(time.time() - startTime))
    accuracy = model.accuracy_nn(test_images, test_labels)
    print ('nn model training accuracy: {:.0%}'.format(accuracy))

if __name__ == '__main__':
    main()
