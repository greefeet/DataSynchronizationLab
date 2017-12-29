# DataSynchronizationLab

## Statefull Single Thread Synchronization

### Abstract
Single thread read/write storage

### Conclusion :
1. to work with single thread to prevent conflict going to the bottleneck at processing time, such as a storage execution
2. if block thread is faulty, the data and operation will be extremely conflict

### Sample Results :

```
StatefulSingleThreadSynchronization
Storage Read Time       : 10 ms
Storage Write Time      : 40 ms
Sampling                : 500 t
Prepairing Time         : 0.007494 s
Process Time            : 31.2472763 s
Transaction per Seconds : 16.001394655956 t/s
Client Receive          : 500, 500, 500, 500
Client Conflic          : 0, 0, 0, 0
```

## Statefull Single Thread with Block Synchronization

### Abstract
Single thread read and use block to store to reduce write time

### Sample Results :

```
StatefulSingleThreadBlockSynchronization
Storage Read Time       : 10 ms
Storage Write Time      : 40 ms
Block Size              : 100 t
Sampling                : 500 t
Prepairing Time         : 0.0068125 s
Process Time            : 8.0260603 s
Transaction per Seconds : 62.297064974705 t/s
Client Receive          : 500, 500, 500, 500
Client Conflic          : 0, 0, 0, 0
```

## Stateless Conflic Synchronization

```
StatelessConflickSynchronization
Storage Read Time       : 10 ms
Storage Write Time      : 40 ms
Sampling                : 500 t
Prepairing Time         : 0.0045296 s
Process Time            : 0.08447 s
Transaction per Seconds : 5919.26127619273 t/s
Client Receive          : 497, 493, 495, 496
Client Conflic          : 490, 495, 496, 499
```