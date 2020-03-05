# mq-dotnet-samples
## XMS .NET Core and Framework Performance Samples

This README file gives information on how to run XMS .NET Core and Framework Performance Samples.

### XMS .NET Performance Samples

XMS .NET applications are developed using both .NET Core and .NET Framework. We have 3 scenarios:

1. Asynchronous Consumer
2. 'n' number of threads putting messages onto a Single Queue
3. Sync Point (Unit of Work)

#### Asynchronous Consumer

A XMS .NET Consumer Application which is a single threaded application which uses message listener to asynchronously consume 10k messages from a queue.

Following are the parameters that have to be passed to the application:

```sh                                                                      
-qm  : QueueManager                                                                  
-q   : QueueName                                                                      
-h   : remotehost where queuemanager is running                                       
-p   : Port on which listener is running                                              
-l   : Channel  
```

###### How to run .NET Core Sample

```sh
dotnet AsyncConsumer.dll -qm Test1 -q Q1 -h remotehost -p 1414 -l SYSTEM.DEF.SVRCONN
```

###### Hwo to run.NET Framework Sample

```sh
AsyncConsumer.exe -qm Test1 -q Q1 -h remotehost -p 1414 -l SYSTEM.DEF.SVRCONN
```

#### 'n' number of threads putting messages onto a Single Queue

XMS .NET multi-threaded application which connects to a Queue Manager and puts messages onto a single queue using 10 threads. Each thread puts 5k messages as a warmup and then each thread puts 10k messages onto the queue to capture performance statistics


Following are the parameters that have to be passed to the application: 

```sh
-qm       : QueueManager                                                              
-q        : QueueName                                                                     
-h        : remotehost where queuemanager is running                                      
-p        : Port on which listener is running                                             
-l        : Channel                                                                       
-msgType  : Persistence or Non-Persistence                                          
-n        : No of messages                                                                
-t        : No of Threads                                                                 
-msgSize  : Size of the message in bytes                                            
-shareCnv : Sharing Conversations 
```

###### How to run .NET Core Sample

```sh
dotnet XMSCoreOneQueue.dll -q Q1 -h remotehost -p 1414 -l SYSTEM.DEF.SVRCONN -msgType 1 -n 10000 -msgSize 256 -qm Test1 -t 10 -shareCnv false 
```

###### How to run .NET Framework Sample

```sh
XMSOneQueue.exe -q Q1 -h remotehost -p 1414 -l SYSTEM.DEF.SVRCONN -msgType 1 -n 10000 -msgSize 256 -qm Test1 -t 10 -shareCnv false
```

#### Sync Point 

XMS .NET multi-threaded application which connects to a Queue Manager and puts messages onto a single queue using 10 threads. This scenario covers messages put under sync point. A commit being issued after every 100 messages. Each thread puts 5k messages as a warmup. And then each thread puts 10k messages onto the queue to capture performance statistics.

Following are the parameters that have to be passed to the application:

```sh
-qm       : QueueManager                                                            
-q        : QueueName                                                                
-h        : remotehost where queuemanager is running                                 
-p        : Port on which listener is running                                        
-l        : Channel                                                                       
-msgType  : Persistence or Non-Persistence                                          
-n        : No of messages                                                          
-t        : No of Threads                                                           
-msgSize  : Size of the message in bytes                                           
-shareCnv : Sharing Conversations  
```

###### How to run .NET Core Sample

```sh
dotnet XMSLocalTransaction.dll -q Q1 -qm Test1 -h remotehost -p 1414 -l SYSTEM.DEF.SVRCONN -n 10000 -t 10 -msgType 0 -msgSize 256 -shareCnv false
```

###### How to run .NET Framework Sample

```sh
XMSLocalTransaction.exe -q Q1 -qm Test1 -h remotehost -p 1414 -l SYSTEM.DEF.SVRCONN -n 10000 -t 10 -msgType 0 -msgSize 256 -shareCnv false
```
