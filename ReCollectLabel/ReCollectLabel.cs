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

		Dictionary <string,HtmlLink> link_map = new Dictionary<string, HtmlLink> { };
		HtmlLink LinkAtPoint (CGPoint point) {
			if (link_map.ContainsKey (point.ToString ()))
				return link_map [point.ToString ()];

			var abs_point = new CGPoint (point.X - Bounds.X, point.Y - Bounds.Y);
			foreach (var link in HtmlLinks) {
				var bounds = BoundingRectForCharacterRange (_rich_text.AttributedText, link.Range);
				if (bounds.Contains (abs_point)) {
					link_map.Add (point.ToString (), link);
					return link;
				}
			}
			return null;
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

		// List of links that have been touched and will need to be unstyled
		List <HtmlLink> touched_links = new List<HtmlLink> { };

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			foreach (var t in touches) {
				var touch = t as UITouch;
				var link = LinkAtPoint (touch.LocationInView (this));

				if (link != null) {
					// Store this link to clear the styles later
					touched_links.Add (link);

					// Style this link
					_rich_text.AttributedText.AddAttributes (
						new UIStringAttributes () {
							BackgroundColor = UIColor.FromRGB (0.85f, 0.85f, 0.85f)
						}, link.Range
					);
					base.AttributedText = _rich_text.AttributedText;
					SetNeedsDisplay ();
				}
			}
		}
			
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);

			// Clear the touch styles on the currently touched links
			foreach (var link in touched_links) {
				_rich_text.AttributedText.RemoveAttribute (UIStringAttributeKey.BackgroundColor, link.Range);
				base.AttributedText = _rich_text.AttributedText;
				SetNeedsDisplay ();
			}
			touched_links.Clear ();

			foreach (var t in touches) {
				var touch = t as UITouch;
				var link = LinkAtPoint (touch.LocationInView (this));
				if (link != null) {
					// Open the url if we can
					if (UIApplication.SharedApplication.CanOpenUrl (link.Url)) {
						UIApplication.SharedApplication.OpenUrl (link.Url);
					}
				}
			}
		}

		class HtmlLink {
			public NSRange Range;
			public NSUrl Url;
		}
	}
}

