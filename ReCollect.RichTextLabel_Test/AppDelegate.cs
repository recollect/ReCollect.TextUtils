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

			var bounds = new CGRect (10, 100, window.Bounds.Width - 20, 20);

			var label = new RichTextLabel (bounds) {
				BackgroundColor = UIColor.Clear,
				TextColor = UIColor.Black,
				AutosizesSubviews = true,
				Lines = 0
			};
			label.Layer.BorderColor = UIColor.Clear.CGColor;

			var text = new RichText ("<p>Regular</p><p><i>Italics</i></p><p><b>BOLD!</b></p><p>Link: <a href=\"http://www.recollect.net\">ReCollect</a></p>");
			label.AttributedText = text.AttributedText;

			var bounding_rect = text.GetBoundedSize (bounds.Size);
			label.Bounds = new CGRect (
				10, 1000, window.Bounds.Width - 20, bounding_rect.Height
			);

			var vc = new UIViewController ();
			vc.Add (label);
			window.RootViewController = vc;
			
			// make the window visible
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}