using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using ReCollect;

namespace TextUtils_Test
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;

		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			window.BackgroundColor = UIColor.White;

			// Create a label that shows some supported Html
			var bounds = new CGRect (10, 100, window.Bounds.Width - 20, 20);
			var label = new ReLabel (bounds) {
				BackgroundColor = UIColor.Clear,
				AutosizesSubviews = true,
				Lines = 0,
				XCallbackSuccessUrl = "recollectTextUtils://textutils.recollect.net"
			};
			label.Layer.BorderColor = UIColor.Clear.CGColor;
			var text = new ReText (
				"<h1>Header 1</h1>" +
				"<h2>Header 2</h2>" +
				"<h3>Header 3</h3>" +
				"<h4>Header 4</h4>" +
				"<h5>Header 5</h5>" +
				"<p>Regular</p>" +
				"<p><i>Italics</i></p>" +
				"<b>BOLD! + BR</b><br />" +
				"<font color=\"rgb(233,150,122)\">Salmon</font> " +
					"<font color=\"#6a5acd\">SlateBlue</font> " +
					"<font color=\"#F65\">Tomato</font> " +
					"<font color=\"invalid\">Invalid</font><br/>" +
				"<p>Link: <a href=\"http://www.recollect.net\">ReCollect</a></p>"
			) {
				LinkColor = new ReColor (UIColor.Orange),
				TextColor = new ReColor (52, 52, 52),
				FontName = "HelveticaNeue-Light",
				BoldFontName = "HelveticaNeue-Bold",
				ItalicsFontName = "HelveticaNeue-Italic"
			};

			label.RichText = text;
			var bounding_rect = text.GetBoundedSize (bounds.Size);
			label.Frame = new CGRect (
				10, 50, window.Bounds.Width - 20, bounding_rect.Height
			);

			// Create a tappable view that demonstrates that we are bubbling taps
			var tappable = new UIView () { BackgroundColor = UIColor.LightGray };
			tappable.Frame = new CGRect (10, 350, 200, 30);
			var btn_label = new ReLabel (new CGRect (0, 0, 200, 30)) {
				RichText = new ReText ("<b>Tap to check bubbling</b>")
			};
			tappable.AddSubview (btn_label);
			tappable.AddGestureRecognizer (new UITapGestureRecognizer (tap => {
				var alert = new UIAlertView ("Success", "Tap bubbled.", null, "Close", null);
				alert.Show ();
			}));

			// Add views
			var vc = new UIViewController ();
			vc.Add (label);
			vc.Add (tappable);
			window.RootViewController = vc;
			
			// make the window visible
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}