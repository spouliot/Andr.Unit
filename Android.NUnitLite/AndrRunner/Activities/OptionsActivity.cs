using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

using MonoDroid.Dialog;

namespace Android.NUnitLite.UI {
	
	[Activity (Label = "Options")]			
	public class OptionsActivity : Activity {
		BooleanElement remote;
		//StringElement host_name;
		EntryElement host_name;
		StringElement host_port;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			remote = new BooleanElement ("Remote Server", false);
			// FIXME: EntryElement does not work
			//host_name = new StringElement ("HostName");
			host_name = new EntryElement ("HostName", String.Empty);
			host_port = new StringElement ("Port");
			
			if (bundle != null) {
				remote.Value = bundle.GetBoolean ("remote");
				host_name.Value = bundle.GetString ("hostName") ?? String.Empty;
				host_port.Value = bundle.GetInt ("hostPort").ToString ();
			} else {
				ISharedPreferences prefs = GetSharedPreferences ("options", FileCreationMode.Private);
				remote.Value = prefs.GetBoolean ("remote", false);
				host_name.Value = prefs.GetString ("hostName", String.Empty);
				host_port.Value = prefs.GetInt ("hostPort", -1).ToString ();
			}
			
			var root = new RootElement ("Options") {
				new Section () { remote, host_name, host_port }
			};

			var lv = new ListView (this) {
				Adapter = new DialogAdapter (this, root)
			};
			SetContentView (lv);
		}
		
		protected override void OnPause ()
		{
			ISharedPreferences prefs = GetSharedPreferences ("options", FileCreationMode.Private);
			var edit = prefs.Edit ();
			edit.PutBoolean ("remote", remote.Value);
			edit.PutString ("hostName", host_name.Value);
			int port = -1;
			ushort p;
			if (UInt16.TryParse (host_port.Value, out p))
				port = p;
			else
				port = -1;
			edit.PutInt ("hostPort", port);
			edit.Commit ();
			base.OnPause ();
		}
		
		protected override void OnSaveInstanceState (Bundle outState)
		{
			outState.PutBoolean ("remoteServer", remote.Value);
			outState.PutString ("hostName", "10.0.1.2"); //host_name);
			outState.PutInt ("hostPort", 16384); //host_port);
			base.OnSaveInstanceState (outState);
		}
	}
}