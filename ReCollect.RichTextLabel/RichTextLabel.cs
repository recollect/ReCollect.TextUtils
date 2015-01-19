using System;
using System.Collections.Generic;
using UIKit;
using Foundation;
using CoreGraphics;

namespace ReCollect
{
	public class RichTextLabel : UILabel
	{
		public RichTextLabel (CGRect bounds) : base (bounds)
		{
			UserInteractionEnabled = true;

			AddGestureRecognizer (new UITapGestureRecognizer (tap => {
				var point = tap.LocationOfTouch (0, this);
				OpenLinkAtPoint (new CGPoint (point.X - Bounds.X, point.Y - Bounds.Y));
			}));
		}

		void OpenLinkAtPoint (CGPoint point) {
			// Build the list of links
			AttributedText.EnumerateAttribute (
				new NSString ("NSLink"),
				new Foundation.NSRange (0, AttributedText.Length),
				Foundation.NSAttributedStringEnumeration.None,
				delegate (NSObject attr, NSRange range, ref bool stop) {
					if (attr != null) {
						var bounds = BoundingRectForCharacterRange (range);
						if (bounds.Contains (point)) {
							// Open the url if we can
							var url = (NSUrl) attr;
							if (UIApplication.SharedApplication.CanOpenUrl (url)) {
								UIApplication.SharedApplication.OpenUrl (url);
							}
						}
					}
				}
			);
		}

		CGRect BoundingRectForCharacterRange (NSRange range) {
			var textStorage = new NSTextStorage ();
			textStorage.SetString (AttributedText);

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

