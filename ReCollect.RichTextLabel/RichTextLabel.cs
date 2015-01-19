using System;
using System.Collections.Generic;
using UIKit;
using Foundation;
using CoreGraphics;

namespace ReCollect
{
	public class RichTextLabel : UILabel
	{
		class HtmlLink {
			public NSRange Range;
			public NSUrl Url;
		}
		List<HtmlLink> HtmlLinks;

		public RichTextLabel (CGRect bounds) : base (bounds)
		{
			UserInteractionEnabled = true;

			AddGestureRecognizer (new UITapGestureRecognizer (tap => {
				var point = tap.LocationOfTouch (0, this);
				OpenLinkAtPoint (new CGPoint (point.X - Bounds.X, point.Y - Bounds.Y));
			}));
		}

		public override NSAttributedString AttributedText {
			get {
				return base.AttributedText;
			}
			set {
				base.AttributedText = value;
				FindLinks (value);
			}
		}

		void OpenLinkAtPoint (CGPoint point) {
			foreach (var link in HtmlLinks) {
				var bounds = BoundingRectForCharacterRange (AttributedText, link.Range);
				Console.WriteLine ("Checking {0} within {1}", point, bounds);
				if (bounds.Contains (point)) {
					Console.WriteLine ("CONTAINS. Launching {0}", link.Url);
					// Open the url if we can
					if (UIApplication.SharedApplication.CanOpenUrl (link.Url)) {
						UIApplication.SharedApplication.OpenUrl (link.Url);
					}
				}
			}
		}
			
		void FindLinks (NSAttributedString str) {
			// Build the list of links
			HtmlLinks = new List<HtmlLink> { };
			str.EnumerateAttribute (
				new NSString ("Link"),
				new Foundation.NSRange (0, AttributedText.Length),
				Foundation.NSAttributedStringEnumeration.None,
				delegate (NSObject attr, NSRange range, ref bool stop) {
					if (attr != null) {
						HtmlLinks.Add (new HtmlLink () {
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
	}
}

