using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PaxtonSync
{
	public class Logger
	{
		// A bit dirty having all this static - but probably OK given the use.
		private static TextWriter _logFile;

		static Logger()
		{
			var fileName = "PaxtonSyncLog-" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm") + ".log";
			_logFile = new StreamWriter(File.OpenWrite(fileName));
		}

		~Logger()
		{
			_logFile.Dispose();
		}

		public static void WriteLine(string format, params object[] args)
		{
			format = DateTime.UtcNow.ToString("s") + ": " + format;
			Console.WriteLine(format, args);
			_logFile.WriteLine(format, args);
		}
	}
}
