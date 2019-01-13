<p align="center"> 
  <img src="https://i.imgur.com/Cla0umu.png" alt="alt logo">
</p>

[![PayPal](https://drive.google.com/uc?id=1OQrtNBVJehNVxgPf6T6yX1wIysz1ElLR)](https://www.paypal.me/nxrighthere) [![Bountysource](https://drive.google.com/uc?id=19QRobscL8Ir2RL489IbVjcw3fULfWS_Q)](https://salt.bountysource.com/checkout/amount?team=nxrighthere) [![Coinbase](https://drive.google.com/uc?id=1LckuF-IAod6xmO9yF-jhTjq1m-4f7cgF)](https://commerce.coinbase.com/checkout/03e11816-b6fc-4e14-b974-29a1d0886697) [![Discord](https://discordapp.com/api/guilds/515987760281288707/embed.png)](https://discord.gg/ceaWXVw)

This repository provides a managed C# wrapper for [enkiTS](https://github.com/dougbinks/enkiTS) library which is created and maintained by [Doug Binks](https://github.com/dougbinks). You will need to [build](https://github.com/dougbinks/enkiTS#building) the native library before you get started.

Enki Task Scheduler is a [lightweight](https://www.enkisoftware.com/devlogpost-20150905-1-Internals-of-a-lightweight-task-scheduler), [fast and scalable](https://www.enkisoftware.com/devlogpost-20150822-1-Implementing_a_lightweight_task_scheduler) system, designed for [task-based programming](https://www.threadingbuildingblocks.org/docs/help/tbb_userguide/Task-Based_Programming.html) which is used in [Avoyd](https://www.youtube.com/watch?v=h6ncXx-BQhs) for efficient parallelization of computations and logic.

<p align="center"> 
  <img src="https://i.imgur.com/xIcqPUN.png" alt="alt logo" title="Parallelization of the recursive Fibonacci in .NET with EnkiTasks">
</p>

Usage
--------
##### Create a new task scheduler:

```c#
// The number of threads will be set automatically
TaskScheduler taskScheduler = new TaskScheduler();
```

##### Destroy the task scheduler:
```c#
taskScheduler.Dispose();
````

##### Define a parallel function:
```c#
TaskExecuteRange taskFunction = (start, end, thread, arguments) => {
	Console.WriteLine("Task is running on the thread: " + thread);
};
````

##### Get a pointer to the parallel function:
```c#
// Can be obtained once and reused further
IntPtr function = taskFunction.GetPointer();
```

##### Create a new task:
```c#
IntPtr task = taskScheduler.CreateTask(function);
```

##### Delete the task:
```c#
taskScheduler.DeleteTask(task);
```

##### Schedule the task:
```c#
// 16 operations will be executed and processed
taskScheduler.ScheduleTask(task, 16);
```

##### Handle completion of the task:
```c#
// Main thread is free to continue any logic or may be involved in the task until completion
taskScheduler.WaitForTask(task);

// Involve main thread in all scheduled tasks
taskScheduler.WaitForAll();
```

##### Check completion of the task:
```c#
// If the main thread is not involved in task it's possible to check task completion at any time
taskScheduler.CheckTaskCompletion(task);
```

API reference
--------
### Delegates
#### Parallel function 
Defines a parallel function for task scheduler.

`TaskExecuteRange(uint start, uint end, uint thread, IntPtr arguments)` 

#### Profiler callback
Provides per task scheduler events.

`ProfilerCallback(uint thread)` 

### Structures
#### ProfilerCallbacks
Contains a managed pointers to the profiler callback functions.

`ProfilerCallbacks.threadStart` 

`ProfilerCallbacks.threadStop` 

`ProfilerCallbacks.waitStart` 

`ProfilerCallbacks.waitStop` 

### Classes
A single low-level disposable class is used to work with enkiTS.

#### TaskScheduler
Contains a managed pointer to the enkiTS instance and profiler callbacks.

##### Constructors
`TaskScheduler(ProfilerCallbacks profilerCallbacks)` 

`TaskScheduler(uint threadsCount, ProfilerCallbacks? profilerCallbacks)` 

##### Properties
`TaskScheduler.Threads` 

##### Methods
`TaskScheduler.Dispose()` 

`TaskScheduler.CreateTask(IntPtr task)` 

`TaskScheduler.DeleteTask(IntPtr task)` 

`TaskScheduler.ScheduleTask(IntPtr task, uint setSize, IntPtr arguments)` 

`TaskScheduler.CheckTaskCompletion(IntPtr task)` 

`TaskScheduler.WaitForTask(IntPtr task)` 

`TaskScheduler.WaitForAll()` 
