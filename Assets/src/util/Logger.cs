using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Logger {

	private static Logger instance;

	private const string filepath = "log.txt";
	public static Logger getInstance() {
		if (instance == null) {
			instance = new Logger();
		}
		return instance;
	}

	public void log(string message) {
		/*
		if (!File.Exists(filepath)) {
			using (StreamWriter sw = File.CreateText(filepath)) {
				sw.WriteLine(message);
			}
		} else {
			using (StreamWriter sw = File.AppendText(filepath)) {
				sw.WriteLine(message);
			}
		}
		*/
		using (StreamWriter sw = new StreamWriter(filepath, true)) {
			sw.WriteLine(message);
		}
		/*
		 string path = @"c:\temp\MyTest.txt";
			// This text is added only once to the file.
			if (!File.Exists(path))
			{
				// Create a file to write to.
				using (StreamWriter sw = File.CreateText(path))
				{
					sw.WriteLine("Hello");
					sw.WriteLine("And");
					sw.WriteLine("Welcome");
				}	
			}

			// This text is always added, making the file longer over time
			// if it is not deleted.
			using (StreamWriter sw = File.AppendText(path))
			{
				sw.WriteLine("This");
				sw.WriteLine("is Extra");
				sw.WriteLine("Text");
			}	

		*/
	}
}
