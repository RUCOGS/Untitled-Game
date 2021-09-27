using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class ConsoleLogger : MonoBehaviour
{


	public static void debug(string i) {
#if UNITY_EDITOR
		UnityEngine.Debug.Log(CreateTagLine(new StackTrace().GetFrame(1).GetMethod().DeclaringType.Name) + i);
#endif
	}

	public static void debug(string tag, string i) {
#if UNITY_EDITOR
		UnityEngine.Debug.Log(CreateTagLine(tag) + i);
#endif
	}

	public static void err(string tag, string i) {
		UnityEngine.Debug.LogError("[Error]" + CreateTagLine(tag) + i);
		UnityEngine.Debug.LogError("[Error]" + CreateTagLine(tag) + new StackTrace(1, true).ToString());
	}


	public static void err(string i) {
		UnityEngine.Debug.LogError("[Error]" + CreateTagLine(new StackTrace().GetFrame(1).GetMethod().DeclaringType.Name) + i);
		UnityEngine.Debug.LogError("[Error]" + CreateTagLine(new StackTrace().GetFrame(1).GetMethod().DeclaringType.Name) + new StackTrace(1, true).ToString());
	}

	public static string CreateTagLine(string tag) {
		return  "[" + tag + "][" + DateTime.Now.ToString("h:mm:ss.fff tt") + "]" + ": ";
	}
}
