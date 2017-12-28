# DataSynchronizationLab

## Single Thread Synchronization

### Conclusion :
1. to work with single thread to prevent conflict going to the bottleneck at processing time, such as a storage execution
2. if block thread is faulty, the data and operation will be extremely conflict

### Sample Results :
```
BlockThread_SimultaneousMessageTest
Storage Execute Time    : 1 ms
Sampling                : 504 t
Prepairing Time         : 1.0209 ms
Process Time            : 7844.5547 ms
Transaction per Seconds : 64.248388758128 t/s
```

```
BlockThread_SimultaneousMessageTest
Storage Execute Time    : 1 ms
Sampling                : 1004 t
Prepairing Time         : 5.6454 ms
Process Time            : 15656.9428 ms
Transaction per Seconds : 64.1249069390481 t/s
```

```
BlockThread_SimultaneousMessageTest
Storage Execute Time    : 100 ms
Sampling                : 504 t
Prepairing Time         : 1.3874 ms
Process Time            : 54908.3501 ms
Transaction per Seconds : 9.17893178509474 t/s
```

```
BlockThread_SimultaneousMessageTest
Storage Execute Time    : 100 ms
Sampling                : 1004 t
Prepairing Time         : 2.4895 ms
Process Time            : 109598.4448 ms
Transaction per Seconds : 9.16071393013051 t/s
```