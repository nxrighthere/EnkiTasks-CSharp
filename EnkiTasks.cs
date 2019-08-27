/*
 *  Managed C# wrapper for Enki Task Scheduler by Doug Binks
 *  Copyright (c) 2019 Stanislav Denisov
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Enki.Tasks {
	public struct TaskSet {
		public IntPtr pointer;

		public bool IsCreated {
			get {
				return pointer != IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ProfilerCallbacks {
		public ProfilerCallback threadStart;
		public ProfilerCallback threadStop;
		public ProfilerCallback waitStart;
		public ProfilerCallback waitStop;
	}

	public delegate void TaskExecuteRange(uint start, uint end, uint thread, IntPtr arguments);
	public delegate void ProfilerCallback(uint thread);

	public class TaskScheduler : IDisposable {
		private IntPtr nativeScheduler;
		private IntPtr nativeCallbacks;

		public TaskScheduler(ProfilerCallbacks profilerCallbacks) {
			NewTaskScheduler(0, profilerCallbacks);
		}

		public TaskScheduler(uint threadsCount = 0, ProfilerCallbacks? profilerCallbacks = null) {
			NewTaskScheduler(threadsCount, profilerCallbacks);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (nativeScheduler != IntPtr.Zero) {
				Native.enkiDeleteTaskScheduler(nativeScheduler);
				nativeScheduler = IntPtr.Zero;
			}
		}

		~TaskScheduler() {
			Dispose(false);
		}

		public uint Threads {
			get {
				return Native.enkiGetNumTaskThreads(nativeScheduler);
			}
		}

		private void NewTaskScheduler(uint threadsCount, ProfilerCallbacks? profilerCallbacks) {
			nativeScheduler = Native.enkiNewTaskScheduler();

			if (nativeScheduler == IntPtr.Zero)
				throw new InvalidOperationException("TaskSet Scheduler not created");

			if (profilerCallbacks != null) {
				nativeCallbacks = Native.enkiGetProfilerCallbacks(nativeScheduler);

				if (nativeCallbacks == IntPtr.Zero)
					throw new InvalidOperationException("Profiler callbacks pointer was not obtained");

				Marshal.StructureToPtr(profilerCallbacks, nativeCallbacks, false);
			}

			if (threadsCount == 0)
				Native.enkiInitTaskScheduler(nativeScheduler);
			else
				Native.enkiInitTaskSchedulerNumThreads(nativeScheduler, threadsCount);
		}

		public TaskSet CreateTask(TaskExecuteRange taskFunction) {
			TaskSet task = Native.enkiCreateTaskSet(nativeScheduler, taskFunction);

			if (!task.IsCreated)
				throw new InvalidOperationException("TaskSet creation failed");

			return task;
		}

		public void DeleteTask(ref TaskSet task) {
			Native.enkiDeleteTaskSet(task.pointer);

			task.pointer = IntPtr.Zero;
		}

		public void ScheduleTask(TaskSet task, uint setSize = 1) {
			ScheduleTask(task, setSize, IntPtr.Zero);
		}

		public void ScheduleTask(TaskSet task, uint setSize, IntPtr arguments) {
			if (setSize == 0)
				throw new ArgumentOutOfRangeException();

			Native.enkiAddTaskSetToPipe(nativeScheduler, task, arguments, setSize);
		}

		public void ScheduleLongTask(TaskSet task, uint setSize = 1, uint minRange = 1) {
			ScheduleLongTask(task, setSize, minRange, IntPtr.Zero);
		}

		public void ScheduleLongTask(TaskSet task, uint setSize, uint minRange, IntPtr arguments) {
			if (setSize == 0 || minRange == 0)
				throw new ArgumentOutOfRangeException();

			Native.enkiAddTaskSetToPipeMinRange(nativeScheduler, task, arguments, setSize, minRange);
		}

		public bool CheckTaskCompletion(TaskSet task) {
			return Native.enkiIsTaskSetComplete(nativeScheduler, task) == 1;
		}

		public void WaitForTask(TaskSet task) {
			Native.enkiWaitForTaskSet(nativeScheduler, task);
		}

		public void WaitForAll() {
			Native.enkiWaitForAll(nativeScheduler);
		}
	}

	[SuppressUnmanagedCodeSecurity]
	internal static class Native {
		private const string nativeLibrary = "enkiTS";

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enkiNewTaskScheduler();

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enkiInitTaskScheduler(IntPtr scheduler);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enkiInitTaskSchedulerNumThreads(IntPtr scheduler, uint threadsCount);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enkiDeleteTaskScheduler(IntPtr scheduler);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern TaskSet enkiCreateTaskSet(IntPtr scheduler, TaskExecuteRange taskFunction);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enkiDeleteTaskSet(IntPtr task);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enkiAddTaskSetToPipe(IntPtr scheduler, TaskSet task, IntPtr arguments, uint setSize);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enkiAddTaskSetToPipeMinRange(IntPtr scheduler, TaskSet task, IntPtr arguments, uint setSize, uint minRange);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int enkiIsTaskSetComplete(IntPtr scheduler, TaskSet task);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enkiWaitForTaskSet(IntPtr scheduler, TaskSet task);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void enkiWaitForAll(IntPtr scheduler);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern uint enkiGetNumTaskThreads(IntPtr scheduler);

		[DllImport(nativeLibrary, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr enkiGetProfilerCallbacks(IntPtr scheduler);
	}
}
