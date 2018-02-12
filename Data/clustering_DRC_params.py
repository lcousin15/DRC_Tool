import numpy as np
import pandas as pd
import matplotlib
import matplotlib.pyplot as plt
import sklearn.cluster as clustering
import sklearn.decomposition as decomposition
from mpl_toolkits.mplot3d import Axes3D
import sklearn.preprocessing as preprocess
import sklearn.manifold as manifold
from Python_tsne_CPP import *

if __name__ == '__main__':

    df = pd.read_csv("P:\\DRC_Tool\\Data\\day1_fit_params_Nuc_Red_Green_All_Fitted.csv", na_values='Not Fitted', index_col='CPD_ID')

    print(df.describe())

    df.drop(['R2 Nuclei', 'R2 R/N', 'R2 G/N'], axis=1, inplace=True)

    # df = df.dropna(axis=0, how='any')
    # print(df)

    ### Imputation
    # imputer = preprocess.Imputer(missing_values='NaN', strategy ='mean', axis = 0, verbose = 0, copy = True)
    # df_imputed = imputer.fit_transform(df.values)

    # standardize = preprocess.StandardScaler(copy=True, with_mean=True, with_std=True)
    # df_standardized = standardize.fit_transform(df)

    df_standardized = preprocess.scale(df, axis=0, with_mean=True, with_std=True, copy=True)
    # print(df_standardized)
    print(df_standardized.mean(axis=0), df_standardized.std(axis=0))


    # model = clustering.AgglomerativeClustering(n_clusters=4, affinity='euclidean', linkage='ward')
    # model.fit(df.values)

    #### PCA

    pca = decomposition.PCA(n_components=None, copy=True, whiten=False, svd_solver='full', random_state=None)
    pca.fit(df_standardized)

    X_transform = pca.transform(df_standardized)

    fig = plt.figure(111, figsize=(4, 3))
    ax = Axes3D(fig)

    ax.scatter(X_transform[:,0], X_transform[:,1], X_transform[:,2], c='red', marker='o')
    plt.show()

    print(pca.explained_variance_ratio_)

    #### Kernel PCA

    kpca = decomposition.KernelPCA(kernel="rbf", gamma=0.2)

    X_kernel_PCA = kpca.fit_transform(df_standardized)

    fig = plt.figure(111, figsize=(4, 3))
    ax = Axes3D(fig)
    ax.scatter(X_kernel_PCA[:, 1], X_kernel_PCA[:, 2], X_kernel_PCA[:, 2], c='blue', marker='o')

    fig, ax = plt.subplots()
    # fig = plt.figure(111, figsize=(4, 3))
    # ax = Axes3D(fig)

    # ax.scatter(X_kernel_PCA[:, 0], X_kernel_PCA[:, 1], X_kernel_PCA[:, 2], c='blue', marker='o')
    ax.scatter(X_kernel_PCA[:, 0], X_kernel_PCA[:, 1], c='blue', marker='o')

    plt.show()

    #### T-SNE

    # tsne = manifold.TSNE(n_components=3, perplexity=5.0, early_exaggeration=100.0, learning_rate=100.0, n_iter=10000000, n_iter_without_progress=1000,
    #                      min_grad_norm=1e-07, metric='euclidean', init='pca', verbose=0, random_state=None, method='exact')
    # #
    # X_tsne = tsne.fit_transform(df_standardized)
    X_tsne = tsne(df_standardized, 3, 3)

    fig = plt.figure(111, figsize=(4, 3))
    ax = Axes3D(fig)

    ax.scatter(X_tsne[:,0], X_tsne[:,1], X_tsne[:,2], c='green', marker='o')
    plt.show()

    X_tsne_2d = tsne(X_transform, 2, 2)

    fig, ax = plt.subplots()
    ax.scatter(X_tsne_2d[:, 0], X_tsne_2d[:, 1], c='green', marker='o')

    plt.show()

    ####  LLE

    X_LLE = manifold.LocallyLinearEmbedding(n_components=3, n_neighbors=5, eigen_solver='auto').fit_transform(df_standardized)
    # X_LLE = manifold.LocallyLinearEmbedding(n_components=3, method='ltsa').fit_transform(df_standardized)
    # X_LLE = manifold.LocallyLinearEmbedding(n_components=3, n_neighbors=5,  method='modified').fit_transform(df_standardized)

    fig = plt.figure(111, figsize=(4, 3))
    ax = Axes3D(fig)

    ax.scatter(X_LLE[:,0], X_LLE[:,1], X_LLE[:,2], c='green', marker='o')
    plt.show()

    #### Isomap

    X_Isomap = manifold.Isomap(n_components=3, n_neighbors=5).fit_transform(df_standardized)

    fig = plt.figure(111, figsize=(4, 3))
    ax = Axes3D(fig)

    ax.scatter(X_Isomap[:,0], X_Isomap[:,1], X_Isomap[:,2], c='green', marker='o')
    plt.show()

    #### MDS

    X_MDS = manifold.MDS(n_components=3, metric=True, n_jobs=-1, verbose=10, max_iter=1000).fit_transform(df_standardized)

    fig = plt.figure(111, figsize=(4, 3))
    ax = Axes3D(fig)

    ax.scatter(X_MDS[:,0], X_MDS[:,1], X_MDS[:,2], c='blue', marker='o')
    plt.show()

    #### Spectral Embbeding

    X_Spectral = manifold.SpectralEmbedding(n_components=3, affinity='rbf', gamma=0.1).fit_transform(df_standardized)

    fig = plt.figure(111, figsize=(4, 3))
    ax = Axes3D(fig)

    ax.scatter(X_Spectral[:,0], X_Spectral[:,1], X_Spectral[:,2], c='red', marker='o')
    plt.show()

    #### K-Means on PCA

    model_kmeans = clustering.KMeans(n_clusters=5, init='k-means++', n_init=10, max_iter=500, tol=0.0001, precompute_distances='auto',
                                     verbose=0, random_state=None, copy_x=True, n_jobs=-1, algorithm='auto')

    model_kmeans.fit(X_transform)
    y = model_kmeans.fit_predict(X_transform)

    y = np.choose(y, [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]).astype(np.float)

    fig = plt.figure(111, figsize=(4, 3))
    ax = Axes3D(fig)
    ax.scatter(X_transform[:,0], X_transform[:,1], X_transform[:,2], c=y, marker='o')
    plt.show()