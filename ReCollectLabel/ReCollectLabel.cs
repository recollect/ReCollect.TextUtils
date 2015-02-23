using System;
using System.Collections.Generic;
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
			AddGestureRecognizer (new UITapGestureRecognizer () {
				Delegate = new LinkGestureDelegate (this)
			});
		}

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

		bool OpenLinkAtPoint (CGPoint point) {
			foreach (var link in HtmlLinks) {
				var bounds = BoundingRectForCharacterRange (_rich_text.AttributedText, link.Range);
				if (bounds.Contains (point)) {
					// Open the url if we can
					if (UIApplication.SharedApplication.CanOpenUrl (link.Url)) {
						UIApplication.SharedApplication.OpenUrl (link.Url);
					}
					return true;
				}
			}
			return false;
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

		class HtmlLink {
			public NSRange Range;
			public NSUrl Url;
		}

		class LinkGestureDelegate : UIGestureRecognizerDelegate
		{
			ReLabel Label;
			public LinkGestureDelegate (ReLabel label) : base ()
			{
				Label = label;
			}

			public override bool ShouldReceiveTouch (UIGestureRecognizer recognizer, UITouch touch)
			{
				var point = touch.LocationInView (Label);
				return Label.OpenLinkAtPoint (new CGPoint (point.X - Label.Bounds.X, point.Y - Label.Bounds.Y));
			}
		}
	}
}

