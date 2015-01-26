using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using ReCollect;

namespace ReCollect.RichTextText_Test
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
			var label = new RichTextLabel (bounds) {
				BackgroundColor = UIColor.Clear,
				TextColor = UIColor.Black,
				AutosizesSubviews = true,
				Lines = 0
			};
			label.Layer.BorderColor = UIColor.Clear.CGColor;
			var text = new RichText (
				"<h1>Header 1</h1>" +
				"<h2>Header 2</h2>" +
				"<h3>Header 3</h3>" +
				"<h4>Header 4</h4>" +
				"<h5>Header 5</h5>" +
				"<p>Regular</p>" +
				"<p><i>Italics</i></p>" +
				"<b>BOLD! + BR</b><br />" +
				"<p>Link: <a href=\"http://www.recollect.net\">ReCollect</a></p>"
			) {
				LinkColor = UIColor.Orange,
				FontName = "HelveticaNeue-Light",
				BoldFontName = "HelveticaNeue-Bold",
				ItalicsFontName = "HelveticaNeue-Italics",
			};

			label.RichText = text;
			var bounding_rect = text.GetBoundedSize (bounds.Size);
			label.Frame = new CGRect (
				10, 50, window.Bounds.Width - 20, bounding_rect.Height
			);

			// Create a tappable view that demonstrates that we are bubbling taps
			var tappable = new UIView () { BackgroundColor = UIColor.LightGray };
			tappable.Frame = new CGRect (10, 300, 200, 30);
			var btn_label = new RichTextLabel (new CGRect (0, 0, 200, 30)) {
				RichText = new RichText ("<b>Tap to check bubbling</b>")
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