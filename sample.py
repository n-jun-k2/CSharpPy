import random
import numpy as np

from PyQt5.QtWidgets import QApplication
import matplotlib.pyplot as plt
                                                                           
idata = 9
fdata = 5.5
narray = np.zeros((5,5))
                                                      
#QApplication.addLibraryPath("C:/ProgramData/Anaconda3/Library/plugins")
#QApplication.addLibraryPath(" 'C:/ProgramData/Anaconda3")

def func():
    x=np.random.randn(30)
    y=np.random.randn(30)
    plt.plot(x,y)
    plt.show()

def func2(data : float):
    print("sampleFunc2 {0}".format(data))
    return data

class SampleClass:
    def __init__(self):
        print("Sample Instance")

    def __call__(self):
        print("call function")

    def __del__(self):
        print("Sample Delete")

    def Func(self):#self error
        print("Sample Func")

class SampleClass2(SampleClass)  :
    def __init__(self):
       super().__init__()
       print("Sample Instance 2")

    def __del__(self):
        super().__del__()
        print("Sample Delete 2")


