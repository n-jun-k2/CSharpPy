import numpy as np
import chainer
import chainer.functions as F
import chainer.links as L
from chainer.training import extensions
from chainer.backends import cuda
from chainer import datasets
from chainer.datasets import mnist
from chainer import Function, FunctionNode, gradient_check, report, training, utils, Variable
from chainer import datasets, initializers, iterators, optimizers, serializers
from chainer import Link, Chain, ChainList
from chainer import Sequential

gpu_id = 0
n_mid_units = 100
n_out = 10
batchsize = 128
max_epoch = 10


mlp_model =Sequential(
    L.Linear(None, n_mid_units),
    F.relu,
    L.Linear(None, n_mid_units),
    F.relu,
    L.Linear(None, n_out)
    ) 

mlp_model.to_gpu(gpu_id)

def ministFunc() :
    print("call minist....")
    #1.データセット準備
    train,test = mnist.get_mnist()
    #2. データセットの反復準備
    train_itr = iterators.SerialIterator(train,batchsize)
    test_itr = iterators.SerialIterator(test,batchsize,False,False)
    #3. 分類器レイヤーでモデルをラップ
    model = L.Classifier(mlp_model)
    #4. 最適化アルゴリズムを選択
    optimizer = optimizers.MomentumSGD()
    #5. 最適化アルゴリズムモデルへの登録
    optimizer.setup(model)
    #6. 更新処理オブジェクトの生成・設定
    updator = training.updaters.StandardUpdater(train_itr,optimizer, device=gpu_id)
    #7. 学習ループ処理オブジェクトの生成・設定
    trainer = training.Trainer(updator,(max_epoch, 'epoch'),out='mist_result')

    trainer.extend(extensions.LogReport())
    trainer.extend(extensions.snapshot(filename='snapshot_epoch-{.updater.epoch}'))
    trainer.extend(extensions.snapshot_object(model.predictor, filename='model_epoch-{.updater.epoch}'))
    trainer.extend(extensions.Evaluator(test_itr, model, device=gpu_id))
    trainer.extend(extensions.PrintReport(['epoch', 'main/loss', 'main/accuracy', 'validation/main/loss', 'validation/main/accuracy', 'elapsed_time']))
    trainer.extend(extensions.PlotReport(['main/loss', 'validation/main/loss'], x_key='epoch', file_name='loss.png'))
    trainer.extend(extensions.PlotReport(['main/accuracy', 'validation/main/accuracy'], x_key='epoch', file_name='accuracy.png'))
    trainer.extend(extensions.DumpGraph('main/loss'))

    #8. 学習開始
    trainer.run()
    #9. セーブオブジェクト
    serializers.save_npz('minist.model',mlp_model)