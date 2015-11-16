using System;
using System.Timers;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace BMN
{
	[Activity (Theme = "@style/Theme.Main", Label = "BMN", MainLauncher = false, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			Button butt = FindViewById<Button>(Resource.Id.MainButton);

			butt.Click += (object sender, EventArgs e) => {

				StartActivity(typeof(BMN.ConnectActivity));
			};
		}
	}
}


