
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using Android.Util;

namespace BMN
{
	[Activity (Theme = "@style/Theme.Main",Label = "ConnectActivity")]			
	public class ConnectActivity : Activity
	{
		public const string EXTRA_DEVICE_ADDRESS = "device_address";
		private const string TAG = "DeviceListActivity";

		BluetoothAdapter adapter;
		protected ArrayAdapter<string> pairedDevices;
		protected static ArrayAdapter<string> newDevices;
		protected Receiver receiver;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.ConnectView);

			adapter = BluetoothAdapter.DefaultAdapter;

			if (!adapter.IsEnabled) {

				adapter.Enable ();
			}

			DoDiscovery ();

			this.pairedDevices = new ArrayAdapter<string>(this, Resource.Layout.ConnectView);
			newDevices = new ArrayAdapter<string>(this, Resource.Layout.ConnectView);

			var pairedListView = FindViewById<ListView> (Resource.Id.PairedListView);
			pairedListView.Adapter = this.pairedDevices;
			pairedListView.ItemClick += DeviceListClick;

			var newListView = FindViewById<ListView> (Resource.Id.NewListView);
			newListView.Adapter = newDevices;
			newListView.ItemClick += DeviceListClick;

			receiver = new Receiver (this);
			var filter = new IntentFilter (BluetoothDevice.ActionFound);
			RegisterReceiver (receiver, filter);

			// Register for broadcasts when discovery has finished
			filter = new IntentFilter (BluetoothAdapter.ActionDiscoveryFinished);
			RegisterReceiver (receiver, filter);



			if (adapter == null) {

				Console.WriteLine ("There is no adapter.");
				Finish ();
			}

			Console.WriteLine (adapter);

			var pairedDevices = adapter.BondedDevices;

			// If there are paired devices, add each one to the ArrayAdapter
			if (pairedDevices.Count > 0) {
				FindViewById<View> (Resource.Id.title).Visibility = ViewStates.Visible;
				foreach (var device in pairedDevices) {
					this.pairedDevices.Add (device.Name + "\n" + device.Address);
				}
			} else {
				String noDevices = Resources.GetText (Resource.String.none_paired);
				this.pairedDevices.Add (noDevices);	
			}
		}

		private void DoDiscovery(){
		


			Log.Debug (TAG, "doDiscovery()");

			// Indicate scanning in the title
			SetProgressBarIndeterminateVisibility (true);
			SetTitle (Resource.String.scanning);

			// Turn on sub-title for new devices
			FindViewById<View> (Resource.Id.title).Visibility = ViewStates.Visible;	

			if (adapter == null) {

				this.Finish ();
			}

			if (adapter.IsDiscovering) {
				adapter.CancelDiscovery ();
			}

			// Request discover from BluetoothAdapter
			adapter.StartDiscovery ();
		}

		private void DeviceListClick(object sender, AdapterView.ItemClickEventArgs e){

			adapter.CancelDiscovery ();

			var info = (e.View as TextView).Text.ToString ();
			var address = info.Substring (info.Length - 17);

			Intent intent = new Intent ();
			intent.PutExtra (EXTRA_DEVICE_ADDRESS, address);

			// Set result and finish this Activity
			SetResult (Result.Ok, intent);
			Finish ();
		}
	

	public class Receiver : BroadcastReceiver
	{ 
		Activity _chat;

		public Receiver (Activity chat)
		{
			_chat = chat;
		}

		public override void OnReceive (Context context, Intent intent)
		{ 
			string action = intent.Action;

			// When discovery finds a device
			if (action == BluetoothDevice.ActionFound) {
				// Get the BluetoothDevice object from the Intent
				BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra (BluetoothDevice.ExtraDevice);
				// If it's already paired, skip it, because it's been listed already
				if (device.BondState != Bond.Bonded) {
					newDevices.Add (device.Name + "\n" + device.Address);
				}
				// When discovery is finished, change the Activity title
			} else if (action == BluetoothAdapter.ActionDiscoveryFinished) {
				_chat.SetProgressBarIndeterminateVisibility (false);
				_chat.SetTitle (Resource.String.select_device);
				if (newDevices.Count == 0) {
					var noDevices = _chat.Resources.GetText (Resource.String.none_found).ToString ();
					newDevices.Add (noDevices);
				}
			}
		} 
	}
	}
}

