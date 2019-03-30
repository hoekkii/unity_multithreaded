using UnityEngine;


public abstract class Controller<T> : MonoBehaviour where T : MonoBehaviour
{
	const string MoreInstancesWarning = "[Controller] There is more than one instance of \"{0}\". \"{1}\" are destroyed to continue.";
	
	const string CreatedInstanceLog = "[Controller] An instance of {0} is needed in the scene, so \"{1}\" was created to continue.";
	
	static T _instance;
	
	public static T instance
	{
		get
		{
			return _instance == null
				? CreateInstance()
				: _instance;
		}
	}
	
	public static bool Instantiated { get; private set; }
	
	public static bool Destroyed { get; private set; }
	
	/// <summary>
	///     Initialize instance
	/// </summary>
	/// <returns></returns>
	public static T CreateInstance(T t = null)
	{
		if (Destroyed)
			return null;
		
		if (_instance != null || Instantiated)
			return _instance;
		
		if (t != null)
		{
			_instance = t;
		}
		else
		{
			// Find object in scene
			var instances = FindObjectsOfType<T>();
			if (instances.Length > 0)
			{
				// There are/is already instance(s) of type
				if (instances.Length > 1)
				{
					// Delete the remaining objects
					Debug.LogWarningFormat(MoreInstancesWarning, typeof(T),
										   instances.Length);
					
					for (var i = 1; i < instances.Length; i++)
						DestroyImmediate(instances[i].gameObject);
				}
				
				_instance = instances[0];
			}
			else
			{
				// Preventing creating multiple objects
				Instantiated = true;
				
				// Create new GameObject with the controller attached to it
				var obj = new GameObject
				{
					name = typeof(T) + " (singleton)",
				};
				
				_instance = obj.AddComponent<T>();
				
#if UNITY_EDITOR
				Debug.LogFormat(CreatedInstanceLog, typeof(T), obj.name);
#endif
				
				// Reset value
				Instantiated = false;
			}
		}
		
		// Finalize
		Destroyed = false;
		DontDestroyOnLoad(_instance.gameObject);
		Instantiated = true;
		return _instance;
	}
	
	/// <summary>
	///     Called when object is going to be destroyed
	/// </summary>
	void OnDestroy()
	{
		// Only mark singleton as destroyed when the current instance gets destroyed
		if (_instance == this)
			Destroyed = true;
		else if (_instance != null)
			return;
		
		Instantiated = false;
		OnDestroying();
	}
	
	
	void OnEnable()
	{
		// Instantiate if not already done
		if (_instance == null)
		{
			if (Instantiated)
				return;
			
			CreateInstance(this as T);
		}
		else if (_instance != this)
		{
			// Destroy object because another one already excists
			DestroyImmediate(gameObject);
			return;
		}
		
		OnEnabling();
	}
	
	/// <summary>
	///     Called before first frame when created
	/// </summary>
	protected virtual void OnEnabling() { }
	
	/// <summary>
	///     Called when object is being destroyed
	/// </summary>
	protected virtual void OnDestroying() { }
}
