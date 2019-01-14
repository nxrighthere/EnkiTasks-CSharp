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
// If the main thread is not involved in task, it's possible to check task completion at any time
if (taskScheduler.CheckTaskCompletion(task))
	Console.WriteLine("Task completed!");
```

##### Create a task scheduler with profiler callbacks:
```c#
ProfilerCallback threadStart = (thread) => {
	Console.WriteLine("Thread " + thread + " started!");
};

ProfilerCallback threadStop = (thread) => {
	Console.WriteLine("Thread " + thread + " stopped!");
};

ProfilerCallback waitStart = (thread) => {
	Console.WriteLine("Thread " + thread + " is waiting...");
};

ProfilerCallback waitStop = (thread) => {
	Console.WriteLine("Thread " + thread + " is resumed!");
};

ProfilerCallbacks profilerCallbacks = new ProfilerCallbacks();

profilerCallbacks.threadStart = threadStart.GetPointer();
profilerCallbacks.threadStop = threadStop.GetPointer();
profilerCallbacks.waitStart = waitStart.GetPointer(); 
profilerCallbacks.waitStop = waitStop.GetPointer();

TaskScheduler taskScheduler = new TaskScheduler(profilerCallbacks);
```

API reference
--------
### Delegates
#### Parallel function 
Defines a parallel function for task scheduler.

`TaskExecuteRange(uint start, uint end, uint thread, IntPtr arguments)` holds an implementation of the parallel function which will be executed and processed by the task scheduler. You need to guarantee a lifetime of the delegate while task scheduler is doing its job. The start and end parameters indicate execution range. The thread parameter indicates in which thread the task is executed. The arguments parameter is used for user-supplied data.

#### Profiler callback
Provides per task scheduler events.

`ProfilerCallback(uint thread)` notifies when profiler callback related to a particular thread come up. `ProfilerCallback.GetPointer()` can be used for obtaining a pointer to the callback function.

### Structures
#### ProfilerCallbacks
Contains a managed pointers to the profiler callback functions.

`ProfilerCallbacks.threadStart` indicates when a thread starts.

`ProfilerCallbacks.threadStop` indicates when a thread stops.

`ProfilerCallbacks.waitStart` indicates when a thread waits.

`ProfilerCallbacks.waitStop` indicates when a thread resumes.

### Classes
A single low-level disposable class is used to work with enkiTS.

#### TaskScheduler
Contains a managed pointer to the enkiTS instance and profiler callbacks.

##### Constructors
`TaskScheduler(ProfilerCallbacks profilerCallbacks)` creates task scheduler instance with profiler callbacks.

`TaskScheduler(uint threadsCount, ProfilerCallbacks? profilerCallbacks)` creates task scheduler instance with the specified number of threads and profiler callbacks. The threads count, and profiler callbacks parameters are optional.

##### Properties
`TaskScheduler.Threads` gets the number of threads.

##### Methods
`TaskScheduler.Dispose()` destroys the task scheduler instance and frees allocated memory.

`TaskScheduler.CreateTask(IntPtr taskFunction)` creates a task that can be reused to get allocation occurring on startup. `TaskExecuteRange.GetPointer()` can be used for obtaining a pointer to the task function. Returns a managed pointer to the task.

`TaskScheduler.DeleteTask(IntPtr task)` deletes a task and frees allocated memory.

`TaskScheduler.ScheduleTask(IntPtr task, uint setSize, IntPtr arguments)` schedules a task for execution and processing. The number of operations can be specified using the optional set size parameter. The optional arguments parameter can be freely used for user-supplied data.

`TaskScheduler.ScheduleLongTask(IntPtr task, uint setSize, uint minRange, IntPtr arguments)` schedules a long-running task for execution and processing. The minimum range parameter should be set to a value which results in a computation effort of at least 10,000 clock cycles to minimize task scheduler overhead.

`TaskScheduler.CheckTaskCompletion(IntPtr task)` checks a scheduled task for completion. Returns true if a task completed or false if it's still in progress.

`TaskScheduler.WaitForTask(IntPtr task)` involves the caller thread in a scheduled task and waits until completion.

`TaskScheduler.WaitForAll()` involves the caller thread in all scheduled tasks and waits until completion.
