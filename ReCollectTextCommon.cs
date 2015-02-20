using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace ReCollect
{
	public partial class ReCollectText
	{
		public int    FontSize        = 18;
		public string FontName        = "Helvetica";
		public string ItalicsFontName = "Helvetica-Italic";
		public string BoldFontName    = "Helvetica-Bold";

		string Html;

		public Color LinkColor = Color.Blue;  // Default link colour
		public Color TextColor = Color.Black; // Default text colour

		public ReCollectText (string html)
		{
			// Decode any HTML entities
			Html = System.Web.HttpUtility.HtmlDecode (html.Replace ("\n", ""));
		}

		TextWithStyles ParseHtml () {
			// Parse HTML
			var doc = new HtmlDocument ();
			doc.LoadHtml (Html);

			// Emit Text with Attributes
			return EmitTextWithAttrs (doc.DocumentNode, FontSize);
		}

		class TextWithStyles {
			public string Text = "";
			public Stack<StyleWithRange> Styles = new Stack<StyleWithRange> { };
		}

		class StyleWithRange {
			public int Offset;
			public int Length;
			public TextStyle Style;
			public void Shift (int offset) {
				Offset += offset;
			}
		}

		TextWithStyles EmitTextWithAttrs (HtmlNode node, int fontSize) {
			var node_text = new TextWithStyles ();
			TextStyle node_style = null;

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
				node_style = new FontStyle (FontName, this);
				break;
			case "h1":
				node_style = new HeaderStyle (1.5f, this);
				break;
			case "h2":
				node_style = new HeaderStyle (1.4f, this);
				break;
			case "h3":
				node_style = new HeaderStyle (1.3f, this);
				break;
			case "h4":
				node_style = new HeaderStyle (1.2f, this);
				break;
			case "h5":
				node_style = new HeaderStyle (1.1f, this);
				break;
			case "a":
				node_style = new LinkStyle (node.GetAttributeValue ("href", ""), this);
				break;
			case "i":
			case "em":
				node_style = new ItalicStyle (this);
				break;
			case "b":
			case "strong":
				node_style = new BoldStyle (this);
				break;
			case "font":
				foreach (var attr in node.Attributes) {
					switch (attr.Name) {
					case "font":
						node_style = new FontStyle (attr.Value, this);
						break;
					case "color":
						node_style = new ColorStyle (Color.Parse (attr.Value), this);
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
					foreach (var ranged_attrs in child_text.Styles) {
						// Shift the range forward
						ranged_attrs.Shift (node_text.Text.Length);
						node_text.Styles.Push (ranged_attrs);
					}
					node_text.Text += child_text.Text;
					break;
				case HtmlNodeType.Text:
					node_text.Text += child.InnerText;
					break;
				}
			}

			// Add the attributes now that we know the text length
			if (node_style != null) {
				node_text.Styles.Push (new StyleWithRange () {
					Offset = 0,
					Length = node_text.Text.Length,
					Style = node_style
				});
			}

			return node_text;
		}

		partial class TextStyle {
			protected ReCollectText Text;
			public TextStyle (ReCollectText text) {
				Text = text;
			}
		}

		partial class HeaderStyle : TextStyle {
			public HeaderStyle (float factor, ReCollectText text) : base (text) {
				Factor = factor;
			}
			public float Factor { get; set; }
		}

		partial class LinkStyle : TextStyle {
			public LinkStyle (string href, ReCollectText text) : base (text) {
				Href = href;
			}
			public string Href { get; set; }
		}

		partial class ItalicStyle : TextStyle {
			public ItalicStyle (ReCollectText text) : base (text) {}
		}

		partial class BoldStyle : TextStyle {
			public BoldStyle (ReCollectText text) : base (text) {}
		}

		partial class ColorStyle : TextStyle {
			Color Color;
			public ColorStyle (Color color, ReCollectText text) : base (text) {
				Color = color;
			}
		}

		partial class FontStyle : TextStyle {
			protected string FontName;
			public FontStyle (string fname, ReCollectText text) : base (text) {
				FontName = fname;
			}
		}
	}
}

