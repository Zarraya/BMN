using System;

using Android.Bluetooth;
using Android.OS;
using Android.Content;

namespace BMN
{
	
	public class MessageController
	{

		private static String TAG = "MessageController";

		private static readonly String NAME_SECURE = "ControllerSecure";
		private static readonly String NAME_INSECURE = "ControllerInsecure";

		private readonly BluetoothAdapter _adapter;
		private readonly Handler _handler;
		private AcceptThread _secureAcceptThread;
		private AcceptThread _insecureAcceptThread;
		private ConnectThread _connectThread;
		private ConnectedThread _connectedThread;
		private int _state;

		private static readonly Java.Util.UUID MY_UUID_SECURE = Java.Util.UUID.FromString("faksljfhioxbyuib946785389kjfhdfg");
		private static readonly Java.Util.UUID MY_UUID_INSECURE = Java.Util.UUID.FromString("fakslj92347yuih94685389kjfhdfg");

		protected Object mutex = new Object();

		public static readonly int STATE_NONE = 0;
		public static readonly int STATE_LISTEN = 1;
		public static readonly int STATE_CONNECTING = 2;
		public static readonly int STATE_CONNECTED = 3;

		public MessageController(Context context, Handler handler){

			this._handler = handler;
			_state = STATE_NONE;
			_adapter = BluetoothAdapter.DefaultAdapter;
		}

		private void setState(int state){

			lock (mutex) {

				_state = state;

				//give message to target to allow UI to update
				_handler.ObtainMessage (Utility.MESSAGE_STATE_CHANGE, state, -1).SendToTarget ();
			}

		}


		/// <summary>
		/// Gets the state. potatoes!!!!!!!
		/// </summary>
		private int getState(){

			lock (mutex) {

				return _state;
			}
		}


		public void start(){

			lock (mutex) {

				if (_connectThread != null) {

					_connectThread.cancel ();
					_connectThread = null;
				}

				if (_connectedThread != null) {
				
					_connectedThread.cancel ();
					_connectedThread = null;
				}

				setState (STATE_LISTEN);

				if (_secureAcceptThread == null) {
				
					_secureAcceptThread = new AcceptThread (true);
					//_secureAcceptThread.Start ();
				}

				if (_insecureAcceptThread == null) {

					_insecureAcceptThread = new AcceptThread (false);
					//_insecureAcceptThread.Start ();
				}
			}
		}


		public void connect(BluetoothDevice device, bool secure){

			lock (mutex) {

				if (_state == STATE_CONNECTING) {

					if (_connectThread != null) {

						_connectThread.cancel ();
						_connectThread = null;
					}
				}

				if (_connectedThread != null) {

					_connectedThread.cancel ();
					_connectedThread = null;
				}

				_connectThread = new ConnectThread (device, secure);
				//_connectThread.Start ();
				setState (STATE_CONNECTING);
			}
		}

		public void connected(BluetoothSocket socket, BluetoothDevice device, String socketType){

			lock (mutex) {
				if (_connectThread != null) {

					_connectThread.cancel ();
					_connectThread = null;
				}

				if (_connectedThread != null) {

					_connectedThread.cancel ();
					_connectedThread = null;
				}

				if (_secureAcceptThread != null) {

					_secureAcceptThread.cancel ();
					_secureAcceptThread = null;
				}

				if (_insecureAcceptThread != null) {

					_insecureAcceptThread.cancel ();
					_insecureAcceptThread = null;
				}

				_connectedThread = new ConnectedThread (socket, socketType);
				//_connectThread.start ();

				Message msg = _handler.ObtainMessage (Utility.MESSAGE_DEVICE_NAME);
				Bundle bundle = new Bundle ();
				bundle.PutString (Utility.DEVICE_NAME, device.Name);
				msg.Data = bundle;
				_handler.SendMessage (msg);

				setState (STATE_CONNECTED);
			}
		}

		public void stop(){
			lock (mutex) {

				if (_connectThread != null) {

					_connectThread.cancel ();
					_connectThread = null;
				}

				if (_connectedThread != null) {

					_connectedThread.cancel ();
					_connectedThread = null;
				}

				if (_secureAcceptThread != null) {

					_secureAcceptThread.cancel ();
					_secureAcceptThread = null;
				}

				if (_insecureAcceptThread != null) {

					_insecureAcceptThread.cancel ();
					_insecureAcceptThread = null;
				}

				setState (STATE_NONE);
			}
		}

		public void write(byte[] thing){

			ConnectedThread r;

			lock (mutex) {

				if (_state != STATE_CONNECTED)
					return;
				r = _connectedThread;
			}

			r.write (thing);
		}

		private void connectionFailed(){

			Message msg = _handler.ObtainMessage (Utility.MESSAGE_TOAST);
			Bundle bundle = new Bundle ();
			bundle.PutString (Utility.TOAST, "Unable to connect device.");
			msg.Data = bundle;
			_handler.SendMessage (msg);
		}

		private void connectionLost(){

			Message msg = _handler.ObtainMessage (Utility.MESSAGE_TOAST);
			Bundle bundle = new Bundle ();
			bundle.PutString (Utility.TOAST, "Device connection lost.");
			msg.Data = bundle;
			_handler.SendMessage (msg);
		}


	




		public class AcceptThread{

			private readonly BluetoothServerSocket _serverSocket;
			private string _socketType;
			private BluetoothAdapter _adapter = BluetoothAdapter.DefaultAdapter;

			public AcceptThread(bool secure, MessageController controller){

				BluetoothServerSocket temp = null;
				_socketType = secure ? "Secure" : "Insecure";

				try{

					if(secure){

							temp = _adapter.ListenUsingRfcommWithServiceRecord(NAME_SECURE, MY_UUID_SECURE);

						}else{

							temp = _adapter.ListenUsingInsecureRfcommWithServiceRecord(NAME_INSECURE, MY_UUID_INSECURE);
						}
				}
				catch(Java.IO.IOException e){

						Console.WriteLine(TAG + "Socket type: "+ _socketType + " listen() failed.");
				}

					_serverSocket = temp;
			}

			public void run(){

				BluetoothSocket socket = null;

				while (_state != STATE_CONNECTED) {

					try{

						socket = _serverSocket.Accept();
					}
					catch(Java.IO.IOException e){
					}

					if (socket != null) {

						lock()
					}
				}
			}

			public void cancel(){


			}
		}




		public class ConnectThread{

			public ConnectThread(BluetoothDevice device, bool secure){


			}



			public void cancel(){


			}


		}




		public class ConnectedThread{

			public ConnectedThread(BluetoothSocket socket, String socketType){


			}

			public void cancel(){


			}


			public void write(byte[] thing){


			}
		}
	}
}

