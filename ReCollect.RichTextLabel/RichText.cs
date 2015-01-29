using System;
using System.Collections.Generic;
using UIKit;
using Foundation;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using CoreGraphics;

namespace ReCollect
{
	public class RichText
	{
		public nfloat FontSize        = 18f;
		public string FontName        = "Helvetica";
		public string ItalicsFontName = "Helvetica-Italic";
		public string BoldFontName    = "Helvetica-Bold";

		string Html;

		static float parse_hex_color_val (string hex)
		{
			if (hex.Length == 1)
				hex = hex + hex;
			return int.Parse (hex, System.Globalization.NumberStyles.HexNumber) / 255.0f;
		}

		static float parse_int_color_val (string val)
		{
			return int.Parse (val) / 255.0f;
		}

		public static UIColor ParseColor (string color_str)
		{
			try {
				var hex_match = Regex.Match (color_str, "^#([0-9a-fA-F]{1,2}){3}$");
				if (hex_match.Success) {
					var captures = hex_match.Groups [1].Captures;
					return UIColor.FromRGB (
						parse_hex_color_val (captures [0].Value),
						parse_hex_color_val (captures [1].Value),
						parse_hex_color_val (captures [2].Value)
					);
				}

				var rgb_match = Regex.Match (color_str, "^rgb\\((\\d+),(\\d+),(\\d+)\\)$");
				if (rgb_match.Success) {
					return UIColor.FromRGB (
						parse_int_color_val (rgb_match.Groups [1].Value),
						parse_int_color_val (rgb_match.Groups [2].Value),
						parse_int_color_val (rgb_match.Groups [3].Value)
					);
				}
			}
			catch (Exception ex) {
				Console.WriteLine ("Error parsing color parameter {0}: {1}", color_str, ex.Message);
			}

			return null;
		}

		public UIColor LinkColor = UIColor.Blue; // Default link colour

		public RichText (string html)
		{
			// Decode any HTML entities
			Html = System.Web.HttpUtility.HtmlDecode (html);
		}

		public CGSize GetBoundedSize (CGSize max) {
			return AttributedText.GetBoundingRect (
				max,
				NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading,
				null
			).Size;
		}

		NSMutableAttributedString _AttributedText = null;
		public NSMutableAttributedString AttributedText {
			get {
				if (_AttributedText != null)
					return _AttributedText;

				// Parse HTML
				var doc = new HtmlDocument ();
				doc.LoadHtml (Html.Replace ("\n", ""));

				// Emit Text with Attributes
				var text = EmitTextWithAttrs (doc.DocumentNode, FontSize);

				// Apply attributes
				_AttributedText = new NSMutableAttributedString (text.Text);
				_AttributedText.BeginEditing ();
				while (text.Attributes.Count > 0) {
					var ranged_attrs = text.Attributes.Pop ();
					var attributes = (NSMutableDictionary) ranged_attrs.Attributes.Dictionary;

					if (attributes ["NSLink"] != null) {
						attributes ["Link"] = attributes ["NSLink"];
						attributes.Remove (new NSString ("NSLink"));
					}

					_AttributedText.AddAttributes (attributes, ranged_attrs.Range);
				}
				_AttributedText.EndEditing ();

				return _AttributedText;
			}
		}

		class TextWithAttrs {
			public string Text = "";
			public Stack<TextAttrWithRange> Attributes = new Stack<TextAttrWithRange> { };
		}

		class TextAttrWithRange {
			public NSRange Range;
			public UIStringAttributes Attributes;
			public void Shift (int offset) {
				Range.Location += offset;
			}
		}

		TextWithAttrs EmitTextWithAttrs (HtmlNode node, nfloat fontSize) {
			var node_text = new TextWithAttrs ();
			var attributes = new List<UIStringAttributes> { };

			/**
			 * Block level nodes:
			 */
			switch (node.Name) {
			case "p":
			case "div":
			case "br":
			case "h1":
			case "h2":
			case "h3":
			case "h4":
			case "h5":
				// Switch <p></p> into strings with newlines UNLESS this is the last <p> in the document
				if (node.ParentNode.NodeType != HtmlNodeType.Document || node.NextSibling != null) {
					node.InnerHtml = node.InnerHtml + "\n";
				}
				break;
			}

			// Add to the array of attributes
			switch (node.Name) {
			case "#document":
				attributes.Add (new UIStringAttributes () {
					Font = UIFont.FromName (FontName, fontSize)
				});
				break;
			case "h1":
				attributes.Add (new UIStringAttributes () {
					Font = UIFont.FromName (FontName, fontSize * 1.5f)
				});
				break;
			case "h2":
				attributes.Add (new UIStringAttributes () {
					Font = UIFont.FromName (FontName, fontSize * 1.4f)
				});
				break;
			case "h3":
				attributes.Add (new UIStringAttributes () {
					Font = UIFont.FromName (FontName, fontSize * 1.3f)
				});
				break;
			case "h4":
				attributes.Add (new UIStringAttributes () {
					Font = UIFont.FromName (FontName, fontSize * 1.2f)
				});
				break;
			case "h5":
				attributes.Add (new UIStringAttributes () {
					Font = UIFont.FromName (FontName, fontSize * 1.1f)
				});
				break;
			case "a":
				var href = node.GetAttributeValue ("href", "");
				if (! string.IsNullOrEmpty (href)) {
					attributes.Add (new UIStringAttributes () {
						ForegroundColor = LinkColor,
						UnderlineColor = LinkColor,
						UnderlineStyle = NSUnderlineStyle.Single,
						Link = NSUrl.FromString (href)
					});
				}
				break;
			case "i":
			case "em":
				attributes.Add (new UIStringAttributes () {
					Font = UIFont.FromName (ItalicsFontName, fontSize)
				});
				break;
			case "b":
			case "strong":
				attributes.Add (new UIStringAttributes () {
					Font = UIFont.FromName (BoldFontName, fontSize)
				});
				break;
			case "font":
				foreach (var attr in node.Attributes) {
					switch (attr.Name) {
					case "font":
						attributes.Add (new UIStringAttributes () {
							Font = UIFont.FromName (attr.Value, fontSize)
						});
						break;
					case "color":
						attributes.Add (new UIStringAttributes () {
							ForegroundColor = ParseColor (attr.Value)
						});
						break;
					}
				}
				break;
			}

			foreach (var child in node.ChildNodes) {
				switch (child.NodeType) {
				case HtmlNodeType.Comment:
					break;
				case HtmlNodeType.Element:
				case HtmlNodeType.Document:
					var child_text = EmitTextWithAttrs (child, fontSize);
					foreach (var ranged_attrs in child_text.Attributes) {
						// Shift the range forward
						ranged_attrs.Shift (node_text.Text.Length);
						node_text.Attributes.Push (ranged_attrs);
					}
					node_text.Text += child_text.Text;
					break;
				case HtmlNodeType.Text:
					node_text.Text += child.InnerText;
					break;
				}
			}

			// Add the attributes now that we know the text length
			foreach (var attr in attributes) {
				node_text.Attributes.Push (new TextAttrWithRange () {
					Range = new NSRange (0, node_text.Text.Length),
					Attributes = attr
				});
			}

			return node_text;
		}
	}
}

