
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
		private const int REQUEST_ENABLE_BT = 1;

		//declare variables to hold the adapter, the paired devices and new devices
		BluetoothAdapter adapter;
		protected ArrayAdapter<string> pairedDevicesAA;
		protected static ArrayAdapter<string> newDevicesAA;
		protected Receiver receiver;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			adapter = BluetoothAdapter.DefaultAdapter;

			if (!adapter.IsEnabled) {


				Intent enableIntent = new Intent (BluetoothAdapter.ActionRequestEnable);
				StartActivityForResult (enableIntent, REQUEST_ENABLE_BT);

			}

			SetContentView (Resource.Layout.ConnectView);

			Intent discoverIntent = new Intent (BluetoothAdapter.ActionRequestDiscoverable);
			discoverIntent.PutExtra (BluetoothAdapter.ExtraDiscoverableDuration, 0);
			StartActivity (discoverIntent);

			DoDiscovery ();

			this.pairedDevicesAA = new ArrayAdapter<string>(this, Resource.Layout.BluetoothTextView);
			newDevicesAA = new ArrayAdapter<string>(this, Resource.Layout.BluetoothTextView);

			var pairedListView = FindViewById<ListView> (Resource.Id.PairedListView);
			pairedListView.Adapter = this.pairedDevicesAA;
			pairedListView.ItemClick += DeviceListClick;

			var newListView = FindViewById<ListView> (Resource.Id.NewListView);
			newListView.Adapter = newDevicesAA;
			newListView.ItemClick += DeviceListClick;

			var refreshButton = FindViewById<Button> (Resource.Id.refresh);
			refreshButton.Click += (object sender, EventArgs e) => {

				newDevicesAA.Clear();

				DoDiscovery();
			};

			var doneButton = FindViewById<Button> (Resource.Id.DoneButton);
			doneButton.Click += (object sender, EventArgs e) => {

				Finish();
			};

			receiver = new Receiver (this);
			var filter = new IntentFilter (BluetoothDevice.ActionFound);
			RegisterReceiver (receiver, filter);

			// Register for broadcasts when discovery has finished
			filter = new IntentFilter (BluetoothAdapter.ActionDiscoveryFinished);
			RegisterReceiver (receiver, filter);


			var pairedDevices = adapter.BondedDevices;

			// If there are paired devices, add each one to the ArrayAdapter
			if (pairedDevices.Count > 0) {
				FindViewById<View> (Resource.Id.title).Visibility = ViewStates.Visible;
				foreach (var device in pairedDevices) {
					this.pairedDevicesAA.Add (device.Name + "\n" + device.Address);
				}
			} else {
				String noDevices = Resources.GetText (Resource.String.none_paired);
				this.pairedDevicesAA.Add (noDevices);	
			}
		}

		private void DoDiscovery(){
		


			Log.Debug (TAG, "doDiscovery()");

			// Indicate scanning in the title
			//SetProgressBarIndeterminateVisibility (true);
			//SetTitle (Resource.String.scanning);

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

			Android.App.AlertDialog.Builder builder = new AlertDialog.Builder (this, 5);
			AlertDialog alert = builder.Create ();
			alert.SetTitle ("Pair?");
			alert.SetMessage ("Do you want to pair the selected device?\n" + info.Substring(0,info.Length-17));

			alert.SetButton ("Yes", (s, ev) => {


			});
			alert.SetButton2 ("No", (s, ev) => {


			});	

			alert.Show ();

			Intent intent = new Intent ();
			intent.PutExtra (EXTRA_DEVICE_ADDRESS, address);

			// Set result and finish this Activity
			SetResult (Result.Ok, intent);
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
					newDevicesAA.Add (device.Name + "\n" + device.Address);
				}
				// When discovery is finished, change the Activity title
			} else if (action == BluetoothAdapter.ActionDiscoveryFinished) {
				_chat.SetProgressBarIndeterminateVisibility (false);
				_chat.SetTitle (Resource.String.select_device);
				if (newDevicesAA.Count == 0) {
					var noDevices = _chat.Resources.GetText (Resource.String.none_found).ToString ();
					newDevicesAA.Add (noDevices);
				}
			}
		}
	}

	}
}