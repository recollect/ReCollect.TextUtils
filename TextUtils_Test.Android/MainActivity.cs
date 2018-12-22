using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Net;
using Android.Text.Method;

using ReCollect;
using Android.Graphics;

namespace TextUtils_Test.Android
{
	[Activity (Label = "TextUtils_Test.Android", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			var text = new ReText (
				"<h1>Header 1</h1>" +
				"<h2>Header 2</h2>" +
				"<h3>Header 3</h3>" +
				"<h4>Header 4</h4>" +
				"<h5>Header 5</h5>" +
				"<p>Regular</p>" +
				"<p><i>Italics</i></p>" +
				"<b>BOLD! + BR</b><br />" +
                "<strong>strong + BR</strong><br />" +
                "<font color=\"rgb(233,150,122)\">Salmon</font> " +
				"<font color=\"#6a5acd\">SlateBlue</font> " +
				"<font color=\"#F65\">Tomato</font> " +
				"<font color=\"invalid\">Invalid</font><br/>" +
				"<p>Link: <a href=\"http://www.recollect.net\">Link to ReCollect</a></p>" +
                "<p>Link: <a href=\"http://www.google.com\">Link to Google</a></p>"
			) {
				LinkColor = new ReColor (255, 127, 0),
				TextColor = new ReColor (52, 52, 52),
                Font = Typeface.CreateFromAsset(this.Assets, "Fonts/IndieFlower.ttf"),
                FontSize = 18
			};

			text.HandleClick += (href) => {
				StartActivity (new Intent(Intent.ActionView, global::Android.Net.Uri.Parse (href)));
			};

			// Get our button from the layout resource,
			// and attach an event to it
			var textview = FindViewById<TextView> (Resource.Id.textView1);
			textview.Focusable = true;
			textview.MovementMethod = LinkMovementMethod.Instance;
			textview.TextFormatted = text.FormattedText;
		}
	}
}


