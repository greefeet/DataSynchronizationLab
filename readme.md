# DataSynchronizationLab

## Single Thread Synchronization

### Abstract
Single thread read/write storage

### Conclusion :
1. to work with single thread to prevent conflict going to the bottleneck at processing time, such as a storage execution
2. if block thread is faulty, the data and operation will be extremely conflict

### Sample Results :

```
BlockThread_SimultaneousMessageTest
Storage Read Time       : 10 ms
Storage Write Time      : 40 ms
Sampling                : 104 t
Prepairing Time         : 0.0014542 s
Process Time            : 6.3844561 s
Transaction per Seconds : 16.2895630216644 t/s
```

## Single Thread with Block Synchronization

### Abstract
Single thread read and use block to store to reduce write time

## Alternative Algorithm
-