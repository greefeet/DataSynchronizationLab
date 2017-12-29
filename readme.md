# DataSynchronizationLab

## Single Thread Synchronization

### Abstract
Single thread read/write storage

### Conclusion :
1. to work with single thread to prevent conflict going to the bottleneck at processing time, such as a storage execution
2. if block thread is faulty, the data and operation will be extremely conflict

### Sample Results :

```
SingleThread_SimultaneousMessageTest
Storage Read Time       : 10 ms
Storage Write Time      : 40 ms
Sampling                : 504 t
Prepairing Time         : 0.005569 s
Process Time            : 31.3865214 s
Transaction per Seconds : 16.0578483221145 t/s
```

## Single Thread with Block Synchronization

### Abstract
Single thread read and use block to store to reduce write time

### Sample Results :

```
SingleThread_Block_SimultaneousMessageTest
Storage Read Time       : 10 ms
Storage Write Time      : 40 ms
Block Size              : 100 t
Sampling                : 504 t
Prepairing Time         : 0.007273 s
Process Time            : 8.0311751 s
Transaction per Seconds : 62.755449074943 t/s
```

## Alternative Algorithm
-