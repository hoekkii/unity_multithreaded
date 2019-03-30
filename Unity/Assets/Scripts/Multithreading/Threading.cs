#if UNITY_EDITOR
#define UNPROTECTED_CALLS
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public sealed class Threading : Controller<Threading>
{
	Thread[] _workers;
	Queue<Action> _tasks;
	List<Action> _dispatched;
	int _mainThread;
	
	
	void Awake()
	{
		Debug.Log("Test");
		
		_tasks = new Queue<Action>();
		_workers = new Thread[Environment.ProcessorCount];
		for (var i = 0; i < _workers.Length; i++)
		{
			var t = new Thread(Work)
			{
				IsBackground = true,
				Name = "Worker",
			};
			
			_workers[i] = t;
			t.Start();
		}
		
		_dispatched = new List<Action>();
		
		var thread = Thread.CurrentThread;
		_mainThread = thread.ManagedThreadId;
	}
	
	/// <inheritdoc />
	void OnDisable()
	{
		if (_workers == null)
			return;
		
		lock (_tasks)
		{
			for (var i = 0; i < _workers.Length; i++)
				_tasks.Enqueue(null);
			
			Monitor.PulseAll(_tasks);
		}
		
		for (var i = 0; i < _workers.Length; i++)
			_workers[i].Join();
		
		_workers = null;
	}
	
	void Update()
	{
		Monitor.Enter(_dispatched);
		
#       if UNPROTECTED_CALLS
		for (var i = 0; i < _dispatched.Count; i++)
			_dispatched[i]();
#       else
		for (var i = 0; i < _dispatched.Count; i++)
		{
			var action = _dispatched[i];
			try
			{
				action.Invoke();
			}
			catch (Exeption e)
			{
				Debug.LogError(e.Message);
				Debug.LogError(e.StackTrace);
			}
		}
#       endif
		
		_dispatched.Clear();
		Monitor.PulseAll(_dispatched);
		Monitor.Exit(_dispatched);
	}
	
	void Work()
	{
		Action item = null;
		for (;;)
		{
			lock (_tasks)
			{
				// Register we are done with the previous task
				//if (item != null)
				//--_buissyCount;
				
				// Wait for new task
				while (_tasks.Count == 0)
					Monitor.Wait(_tasks);
				
				// Register we are busy
				//++_buissyCount;
				
				// Get new task
				item = _tasks.Dequeue();
			}
			
			// We have to stop
			if (item == null)
				break;
			
			// Run actual method
#           if UNPROTECTED_CALLS
			item.Invoke();
#           else
			try
			{
				item.Invoke();
			}
			catch (Exeption e)
			{
				Debug.LogError(e.Message);
				Debug.LogError(e.StackTrace);
			}
#           endif
		}
	}
	
	/// <summary>
	///     Enqueues a tasks that will be called by a worker thread.
	/// </summary>
	public static void Push(Action task)
	{
		if (task == null || Destroyed)
			return;
		
		var tasks = instance._tasks;
		lock (tasks)
		{
			tasks.Enqueue(task);
			Monitor.PulseAll(tasks);
		}
	}
	
	
	/// <summary>
	///     Enqueues the action to be called on the main thread.
	/// </summary>
	/// <remarks>
	///     Don't call this on the main thread.
	///     The current thread will not wait until the action
	///     has been called.
	/// </remarks>
	public static void CallOnMain(Action action)
	{
		if (Destroyed)
			return;
		
		var self = instance;
		if (Thread.CurrentThread.ManagedThreadId == self._mainThread)
			throw new Exception("This should not be called from the main thread");
		
		var dispatched = self._dispatched;
		Monitor.Enter(dispatched);
		dispatched.Add(action);
		Monitor.Exit(dispatched);
	}
	
	
	/// <summary>
	///     Enqueues the action to be called on the main thread.
	///     The current thread will then wait until the action
	///     has been called.
	/// </summary>
	/// <remarks>
	///     Don't call this on the main thread.
	/// </remarks>
	public static void WaitOnMain(Action action)
	{
		if (Destroyed)
			return;
		
		var self = instance;
		if (Thread.CurrentThread.ManagedThreadId == self._mainThread)
			throw new Exception("This should not be called from the main thread");
		
		var dispatched = self._dispatched;
		Monitor.Enter(dispatched);
		dispatched.Add(action);
		Monitor.Wait(dispatched);
		Monitor.Exit(dispatched);
	}
}
