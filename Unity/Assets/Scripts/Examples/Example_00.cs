using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example_00 : MonoBehaviour
{
	public int Test;
	
	
    void Start()
    {
		// Let a worker thread do the job
        Threading.Push(() => { for (var i = 0; i < 1000; i++) Test += i; });
    }
	
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
			StartCoroutine(Routine());
    }
	
	IEnumerator Routine()
	{
		// Do stuff while the thread is running
		var instruction = new ThreadedInstruction(HeavyCalculation);
		Debug.Log("Running heavy instruction..");
		yield return instruction;
		
		// Or call it directly
		yield return new ThreadedInstruction(HeavyCalculation);
	}
	
	void HeavyCalculation()
	{
		for (var i = 0; i < 10000; i++)
		{
			i = Mathf.Abs(i);
		}
		
		Threading.WaitOnMain(() => transform.position += Vector3.up);
		
		Test /= 2;
	}
}
