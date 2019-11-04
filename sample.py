import random
import numpy as np

idata = 9
fdata = 5.5
narray = np.zeros((5,5))

def func():
    print("sample Func")

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