using System;
using System.Collections.Generic;
using System.Reflection;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using MonoDroid.Dialog;

using NUnitLite;
using NUnitLite.Runner;

namespace Android.NUnitLite.UI {

    public class RunnerActivity : Activity {
		
		Section main;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
			AndroidRunner.Initialized = true;
			
			var menu = new RootElement ("Test Runner");
			
			main = new Section ("Test Suites");
			foreach (TestSuite suite in AndroidRunner.AssemblyLevel) {
				main.Add (new TestSuiteElement (suite));
			}
			menu.Add (main);

			Section options = new Section () {
				new ActionElement ("Run Everything", Run),
				new ActivityElement ("Options", typeof (OptionsActivity)),
				new ActivityElement ("Credits", typeof (CreditsActivity))
			};
			menu.Add (options);

			var lv = new ListView (this) {
				Adapter = new DialogAdapter (this, menu)
			};
			SetContentView (lv);
        }
		
		public void Add (Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");
			
			// this can be called many times but we only want to load them
			// once since we need to share them across most activities
			if (!AndroidRunner.Initialized) {
				// TestLoader.Load always return a TestSuite so we can avoid casting many times
				TestSuite ts = TestLoader.Load (assembly) as TestSuite;
				AndroidRunner.AssemblyLevel.Add (ts);
				Add (ts);
			}
		}
		
		void Add (TestSuite suite)
		{
			AndroidRunner.Suites.Add (suite.FullName ?? suite.Name, suite);
			foreach (ITest test in suite.Tests) {
				TestSuite ts = (test as TestSuite);
				if (ts != null)
					Add (ts);
			}
		}

		void Run ()
		{
			AndroidRunner runner = AndroidRunner.Runner;
			if (!runner.OpenWriter ("Run Everything"))
				return;
			
			try {
				foreach (TestSuite suite in AndroidRunner.AssemblyLevel) {
					suite.Run (runner);
				}
			}
			finally {
				runner.CloseWriter ();
			}
			
			foreach (TestElement te in main) {
				te.Update ();
			}
		}
    }
}