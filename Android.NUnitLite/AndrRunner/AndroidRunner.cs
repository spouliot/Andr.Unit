using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

using Android.App;

using NUnitLite;

namespace Android.NUnitLite {
	
	public class AndroidRunner : TestListener {
		
		Options options;
		
		private AndroidRunner ()
		{
		}
		
		public bool AutoStart { get; set; }

		public bool TerminateAfterExecution { get; set; }
		
		public Options Options { 
			get {
				if (options == null)
					options = new Options ();
				return options;
			}
			set { options = value; }
		}
		
		#region writer
		
		public TextWriter Writer { get; set; }
		
		public bool OpenWriter (string message)
		{
			DateTime now = DateTime.Now;
			// let the application provide it's own TextWriter to ease automation with AutoStart property
			if (Writer == null) {
				if (Options.ShowUseNetworkLogger) {
					Console.WriteLine ("[{0}] Sending '{1}' results to {2}:{3}", now, message, Options.HostName, Options.HostPort);
					try {
						Writer = new TcpTextWriter (Options.HostName, Options.HostPort);
					}
					catch (SocketException) {
						/*
						UIAlertView alert = new UIAlertView ("Network Error", 
							String.Format ("Cannot connect to {0}:{1}. Continue on console ?", options.HostName, options.HostPort), 
							null, "Cancel", "Continue");
						int button = -1;
						alert.Clicked += delegate(object sender, UIButtonEventArgs e) {
							button = e.ButtonIndex;
						};
						alert.Show ();
						while (button == -1)
							NSRunLoop.Current.RunUntil (NSDate.FromTimeIntervalSinceNow (0.5));

						Console.WriteLine (button);
						Console.WriteLine ("[Host unreachable: {0}]", button == 0 ? "Execution cancelled" : "Switching to console output");
						if (button == 0)
							return false;
						else*/
							Writer = Console.Out;
					}
				} else {
					Writer = Console.Out;
				}
			}
			
			Writer.WriteLine ("[Runner executing:\t{0}]", message);
#if false
			Writer.WriteLine ("[Mono for Android Version:\t{0}]", MonoTouch.Constants.Version);
			UIDevice device = UIDevice.CurrentDevice;
			Writer.WriteLine ("[{0}:\t{1} v{2}]", device.Model, device.SystemName, device.SystemVersion);
			Writer.WriteLine ("[Device Date/Time:\t{0}]", now); // to match earlier C.WL output
			// FIXME: add more data about the device
			
			Writer.WriteLine ("[Bundle:\t{0}]", NSBundle.MainBundle.BundleIdentifier);
			// FIXME: add data about how the app was compiled (e.g. ARMvX, LLVM, Linker options)
#endif
			return true;
		}
		
		public void CloseWriter ()
		{
			Writer.Close ();
			Writer = null;
		}

		#endregion
		
		public void TestStarted (ITest test)
		{
			if (test is TestSuite) {
				Writer.WriteLine ();
				time.Push (DateTime.UtcNow);
				Writer.WriteLine (test.Name);
			}
		}

		Stack<DateTime> time = new Stack<DateTime> ();
			
		public void TestFinished (TestResult result)
		{
			AndroidRunner.Results [result.Test.FullName ?? result.Test.Name] = result;
			
			if (result.Test is TestSuite) {
				if (!result.IsError && !result.IsFailure && !result.IsSuccess && !result.Executed)
					Writer.WriteLine ("\t[INFO] {0}", result.Message);
				
				var diff = DateTime.UtcNow - time.Pop ();
				Writer.WriteLine ("{0} : {1} ms", result.Test.Name, diff.TotalMilliseconds);
			} else {
				if (result.IsSuccess) {
					Writer.Write ("\t{0} ", result.Executed ? "[PASS]" : "[IGNORED]");
				} else if (result.IsFailure || result.IsError) {
					Writer.Write ("\t[FAIL] ");
				} else {
					Writer.Write ("\t[INFO] ");
				}
				Writer.Write (result.Test.Name);
				
				string message = result.Message;
				if (!String.IsNullOrEmpty (message)) {
					Writer.Write (" : {0}", message.Replace ("\r\n", "\\r\\n"));
				}
				Writer.WriteLine ();
						
				string stacktrace = result.StackTrace;
				if (!String.IsNullOrEmpty (result.StackTrace)) {
					string[] lines = stacktrace.Split (new char [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string line in lines)
						Writer.WriteLine ("\t\t{0}", line);
				}
			}
		}
		
		static AndroidRunner runner = new AndroidRunner ();
		
		static public AndroidRunner Runner {
			get { return runner; }
		}
		
		static List<TestSuite> top = new List<TestSuite> ();
		static Dictionary<string,TestSuite> suites = new Dictionary<string, TestSuite> ();
		static Dictionary<string,TestResult> results = new Dictionary<string, TestResult> ();
		
		static public IList<TestSuite> AssemblyLevel {
			get { return top; }
		}
		
		static public IDictionary<string,TestSuite> Suites {
			get { return suites; }
		}
		
		static public IDictionary<string,TestResult> Results {
			get { return results; }
		}
	}
}