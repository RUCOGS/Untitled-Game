using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
	void Pool();
	void Unpool();
	void Clean();

	PoolBehaviour poolParent { get; set; }
}
