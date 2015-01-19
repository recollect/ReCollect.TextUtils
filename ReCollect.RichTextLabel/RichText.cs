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
		nfloat FontSize = 18;
		string Html;

		public UIColor LinkColor = UIColor.Blue; // Default link colour

		public RichText (string html, Nullable<nfloat> fontSize=null)
		{
			// Decode any HTML entities
			Html = System.Web.HttpUtility.HtmlDecode (html);

			if (fontSize.HasValue)
				FontSize = fontSize.Value;

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

			// Add to the array of attributes
			switch (node.Name) {
			case "p":
			case "div":
				// Switch <p></p> into strings with newlines UNLESS this is the last <p> in the document
				if (node.ParentNode.NodeType != HtmlNodeType.Document || node.NextSibling != null) {
					node.InnerHtml = node.InnerHtml + "\n";
				}
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
					Font = UIFont.FromName ("HelveticaNeue-Italic", fontSize)
				});
				break;
			case "b":
			case "strong":
				attributes.Add (new UIStringAttributes () {
					Font = UIFont.FromName ("HelveticaNeue-Bold", fontSize)
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
						var rgb_match = Regex.Match (attr.Value, "^#([0-9A-F]{2}){3}$");
						if (rgb_match.Success) {
							try {
								var captures = rgb_match.Groups [1].Captures;
								attributes.Add (new UIStringAttributes () {
									ForegroundColor =  UIColor.FromRGB(
										int.Parse (captures [0].Value, System.Globalization.NumberStyles.HexNumber) / 255.0f,
										int.Parse (captures [1].Value, System.Globalization.NumberStyles.HexNumber) / 255.0f,
										int.Parse (captures [2].Value, System.Globalization.NumberStyles.HexNumber) / 255.0f
									)
								});
							}
							catch {
								// Meh... keep it the default color
							}
						} else {
							Console.WriteLine ("Unknown color: {0}", attr.Value);
						}
						break;
					default:
						Console.WriteLine ("UNHANDLED font attr={0}, value={1}", attr.Name, attr.Value);
						break;
					}
				}
				break;
			case "#document":
				attributes.Add (new UIStringAttributes () {
					Font = UIFont.FromName ("HelveticaNeue-Light", fontSize)
				});
				break;
			default:
				Console.WriteLine ("UNHANDLED node={0}, text={1}", node.Name, node_text.Text);
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

