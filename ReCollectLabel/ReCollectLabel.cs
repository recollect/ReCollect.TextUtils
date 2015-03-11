using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UIKit;
using Foundation;
using CoreGraphics;

namespace ReCollect
{
	public class ReLabel : UILabel
	{
		List<HtmlLink> HtmlLinks = new List<HtmlLink> {};

		public ReLabel () : base ()
		{
			Setup ();
		}

		public ReLabel (CGRect bounds) : base (bounds)
		{
			Setup ();
		}

		void Setup ()
		{
			UserInteractionEnabled = true;
		}

		public string XCallbackSuccessUrl = "";
		public string XCallbackName = (NSString) NSBundle.MainBundle.InfoDictionary ["CFBundleDisplayName"];

		protected internal new NSAttributedString AttributedText {
			set {
				throw new Exception ("Set RichText instead of AttributedText!");
			}
		}

		ReText _rich_text = null;
		public ReText RichText {
			get {
				return _rich_text;
			}
			set {
				_rich_text = value;
				FindLinks (_rich_text);
				base.AttributedText = _rich_text.AttributedText;
			}
		}

		HtmlLink LinkAtPoint (CGPoint point, nfloat radius) {
			// Construct an area roughly the size of the finger
			var touch_center = new CGPoint (point.X - Bounds.X, point.Y - Bounds.Y);
			var touch_area = new CGRect (
				                 touch_center.X - (radius / 2),
				                 touch_center.Y - (radius / 2),
				                 radius, radius
			                 );

			HtmlLink matching_link = null;
			nfloat shortest = nint.MaxValue;

			foreach (var link in HtmlLinks) {
				var bounds = BoundingRectForCharacterRange (_rich_text.AttributedText, link.Range);
				var link_center = new CGPoint (
					                  bounds.X + (bounds.Width / 2),
					                  bounds.Y + (bounds.Height / 2)
				                  );

				if (bounds.IntersectsWith (touch_area)) {
					var distance = NMath.Sqrt (
						NMath.Pow (link_center.X - touch_center.X, 2) +
						NMath.Pow (link_center.Y - touch_center.Y, 2)
					);
					if (distance < shortest) {
						matching_link = link;
						shortest = distance;
					}
				}
			}
			return matching_link;
		}
			
		void FindLinks (ReText text) {
			// Build the list of links
			HtmlLinks.Clear ();
			text.AttributedText.EnumerateAttribute (
				new NSString ("Link"),
				new Foundation.NSRange (0, _rich_text.AttributedText.Length),
				Foundation.NSAttributedStringEnumeration.None,
				delegate (NSObject attr, NSRange range, ref bool stop) {
					if (attr != null) {
						HtmlLinks.Add (new HtmlLink () {
							XCallbackSuccessUrl = XCallbackSuccessUrl,
							XCallbackName = XCallbackName,
							Range = range,
							Url = (NSUrl) attr
						});
					}
				}
			);
		}

		CGRect BoundingRectForCharacterRange (NSAttributedString str, NSRange range) {
			var textStorage = new NSTextStorage ();
			textStorage.SetString (str);

			var layoutManager = new NSLayoutManager ();
			textStorage.AddLayoutManager (layoutManager);
			var textContainer = new NSTextContainer (Bounds.Size);
			textContainer.LineFragmentPadding = 0;
			layoutManager.AddTextContainer (textContainer);

			NSRange glyphRange = new NSRange ();

			// Convert the range for glyphs.
			layoutManager.CharacterRangeForGlyphRange (range, ref glyphRange);

			return layoutManager.BoundingRectForGlyphRange (glyphRange, textContainer);
		}

		public async override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			foreach (var t in touches) {
				var touch = t as UITouch;
				var radius = (nfloat) 5f;
				if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0))
					radius = touch.MajorRadius;

				var link = LinkAtPoint (touch.LocationInView (this), radius);

				if (link != null) {
					// Style this link
					_rich_text.AttributedText.AddAttributes (
						new UIStringAttributes () {
							BackgroundColor = UIColor.FromRGB (0.85f, 0.85f, 0.85f)
						}, link.Range
					);
					base.AttributedText = _rich_text.AttributedText;
					SetNeedsDisplay ();

					// Wait half a second
					await Task.Delay (500);

					// Remove the selected style
					_rich_text.AttributedText.RemoveAttribute (UIStringAttributeKey.BackgroundColor, link.Range);
					base.AttributedText = _rich_text.AttributedText;
					SetNeedsDisplay ();

					// Open the link
					var app = UIApplication.SharedApplication;
					if (link.ChromeUrl != null && app.CanOpenUrl (link.ChromeUrl)) {
						app.OpenUrl (link.ChromeUrl);
					} else if (app.CanOpenUrl (link.Url)) {
						app.OpenUrl (link.Url);
					}
				}
			}

			base.TouchesBegan (touches, evt);
		}

		class HtmlLink {
			public string XCallbackSuccessUrl = "";
			public string XCallbackName = "";
			public NSRange Range;
			public NSUrl Url;
			public NSUrl ChromeUrl {
				get {
					if (Url.Scheme == "http" || Url.Scheme == "https") {
						return new NSUrl (
							string.Format (
								"googlechrome-x-callback://x-callback-url/open/?x-source={0}&x-success={1}&url={2}",
								Uri.EscapeDataString (XCallbackName),
								Uri.EscapeDataString (XCallbackSuccessUrl),
								Uri.EscapeDataString (Url.AbsoluteString)
							)
						);
					}
					return null;
				}
			}
		}
	}
}

