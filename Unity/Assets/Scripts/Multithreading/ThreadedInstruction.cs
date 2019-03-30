using System.Threading;
using UnityEngine;


public sealed class ThreadedInstruction : CustomYieldInstruction
{
	readonly Thread _thread;
	
	public ThreadedInstruction(
		ParameterizedThreadStart thread,
		object parameter)
		: this(new Thread(thread), parameter) { }
	
	public ThreadedInstruction(
		ParameterizedThreadStart thread,
		int maxStackSize,
		object parameter)
		: this(new Thread(thread, maxStackSize), parameter) { }
	
	public ThreadedInstruction(ThreadStart thread)
		: this(new Thread(thread)) { }
	
	public ThreadedInstruction(ThreadStart thread, int maxStackSize)
		: this(new Thread(thread, maxStackSize)) { }
	
	public ThreadedInstruction(Thread thread)
	{
		_thread = thread;
		_thread.Start();
	}
	
	public ThreadedInstruction(Thread thread, object parameter)
	{
		_thread = thread;
		_thread.Start(parameter);
	}
	
	/// <inheritdoc />
	public override bool keepWaiting { get { return _thread.IsAlive; } }
}
