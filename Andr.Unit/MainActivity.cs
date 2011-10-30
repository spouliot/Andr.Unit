using System.Reflection;

using Android.App;
using Android.OS;
using Android.NUnitLite.UI;

namespace Andr.Unit {
	
	[Activity (Label = "Xamarin's Andr.Unit", MainLauncher = true)]
	public class MainActivity : RunnerActivity {
		
		protected override void OnCreate (Bundle bundle)
		{
			// tests can be inside the main assembly
			Add (Assembly.GetExecutingAssembly ());
			// or in any reference assemblies			
			Add (typeof (m4a.tests.RunnerTest).Assembly);
			// or in any assembly that you load (since JIT is available)
			
			// you cannot add more assemblies once calling base
			base.OnCreate (bundle);
		}
	}
}