using System;
using System.Collections.Generic;
using System.Net;
#if __ANDROID__
using Android.Graphics;
#endif
using HtmlAgilityPack;

namespace ReCollect
{
    public partial class ReText
    {
        public int FontSize = 18;
        public int ImageSize = 100;
#if __ANDROID__
        public Typeface Font = null;
#endif
#if __IOS__
        public string FontName = "HelveticaNeue";
        public string ItalicFontName = "HelveticaNeue-Italic";
        public string BoldFontName = "HelveticaNeue-Bold";
        public string BoldItalicFontName = "HelveticaNeue-BoldItalic";
#endif

        string Html;

        public ReColor LinkColor = ReColor.Blue;  // Default link colour
        public ReColor TextColor = ReColor.Black; // Default text colour

        public ReText(string html)
        {
            // Decode any HTML entities
            Html = System.Web.HttpUtility.HtmlDecode(html.Replace("\n", ""));
        }

        TextWithStyles ParseHtml()
        {
            // Parse HTML
            var doc = new HtmlDocument();
            doc.LoadHtml(Html);

            // Emit Text with Attributes
            return EmitTextWithAttrs(doc.DocumentNode, FontSize);
        }

        class TextWithStyles
        {
            public string Text = "";
            public Stack<StyleWithRange> Styles = new Stack<StyleWithRange> { };
        }

        class StyleWithRange
        {
            public int Offset;
            public int Length;
            public TextStyle Style;
            public void Shift(int offset)
            {
                Offset += offset;
            }
        }

        bool HasParent(HtmlNode node, string name1, string name2 = "")
        {
            foreach (var ancestor in node.AncestorsAndSelf())
            {
                if (ancestor.Name == name1 || ancestor.Name == name2)
                    return true;
            }
            return false;
        }

        TextWithStyles EmitTextWithAttrs(HtmlNode node, int fontSize)
        {
            var node_text = new TextWithStyles();
            TextStyle node_style = null;

            /**
			 * Block level nodes:
			 */
            switch (node.Name)
            {
                case "p":
                case "div":
                case "br":
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                    // Switch <p></p> into strings with newlines UNLESS this is the last <p> in the document
                    if (node.ParentNode.NodeType != HtmlNodeType.Document || node.NextSibling != null)
                    {
                        node.InnerHtml = node.InnerHtml + "\n";
                    }
                    break;
                case "img":
                    node.InnerHtml = node.InnerHtml + " img ";
                    break;
                case "li":
                    node.InnerHtml = "\u2022 " + node.InnerHtml;
                    break;
            }

            // Add to the array of attributes
            switch (node.Name)
            {
                case "#document":
#if __ANDROID__
                    node_style = new FontStyle (Font, this);
#endif
#if __IOS__
                    node_style = new FontStyle(FontName, this);
#endif
                    break;
                case "h1":
                    node_style = new HeaderStyle(1.5f, this);
                    break;
                case "h2":
                    node_style = new HeaderStyle(1.4f, this);
                    break;
                case "h3":
                    node_style = new HeaderStyle(1.3f, this);
                    break;
                case "h4":
                    node_style = new HeaderStyle(1.2f, this);
                    break;
                case "h5":
                    node_style = new HeaderStyle(1.1f, this);
                    break;
                case "a":
                    node_style = new LinkStyle(node.GetAttributeValue("href", ""), this);
                    break;
                case "img":
                    node_style = new ImageStyle(node.GetAttributeValue("src", ""), this);
                    break;
                case "ul":
                    node_style = new UnorderedListStyle(node.InnerHtml, this);
                    break;
                case "i":
                case "em":
                    if (HasParent(node, "b", "strong"))
                        node_style = new BoldItalicStyle(this);
                    else
                        node_style = new ItalicStyle(this);
                    break;
                case "b":
                case "strong":
                    if (HasParent(node, "i", "em"))
                        node_style = new BoldItalicStyle(this);
                    else
                        node_style = new BoldStyle(this);
                    break;
                case "font":
                    foreach (var attr in node.Attributes)
                    {
                        switch (attr.Name)
                        {
                            case "font":
#if __ANDROID__
                                node_style = new FontStyle(Font, this);
#endif
#if __IOS__
                                node_style = new FontStyle(FontName, this);
#endif
                                break;
                            case "color":
                                node_style = new ColorStyle(ReColor.Parse(attr.Value), this);
                                break;
                        }
                    }
                    break;
            }

            foreach (var child in node.ChildNodes)
            {
                switch (child.NodeType)
                {
                    case HtmlNodeType.Comment:
                        break;
                    case HtmlNodeType.Element:
                    case HtmlNodeType.Document:
                        var child_text = EmitTextWithAttrs(child, fontSize);
                        foreach (var ranged_attrs in child_text.Styles)
                        {
                            // Shift the range forward
                            ranged_attrs.Shift(node_text.Text.Length);
                            node_text.Styles.Push(ranged_attrs);
                        }
                        node_text.Text += child_text.Text;
                        break;
                    case HtmlNodeType.Text:
                        node_text.Text += child.InnerText;
                        break;
                }
            }

            // Add the attributes now that we know the text length
            if (node_style != null)
            {
                node_text.Styles.Push(new StyleWithRange()
                {
                    Offset = 0,
                    Length = node_text.Text.Length,
                    Style = node_style
                });
            }

            return node_text;
        }

        partial class TextStyle
        {
            protected ReText Text;
            public TextStyle(ReText text)
            {
                Text = text;
            }
        }

        partial class HeaderStyle : TextStyle
        {
            public HeaderStyle(float factor, ReText text) : base(text)
            {
                Factor = factor;
            }
            public float Factor { get; set; }
        }

        partial class ImageStyle : TextStyle
        {
            public ImageStyle(string src, ReText text) : base(text)
            {
                Src = src;
            }
            public string Src { get; set; }
        }

        partial class LinkStyle : TextStyle
        {
            public LinkStyle(string href, ReText text) : base(text)
            {
                Href = href;
            }
            public string Href { get; set; }
        }

        partial class ItalicStyle : TextStyle
        {
            public ItalicStyle(ReText text) : base(text) { }
        }

        partial class BoldStyle : TextStyle
        {
            public BoldStyle(ReText text) : base(text) { }
        }

        partial class BoldItalicStyle : TextStyle
        {
            public BoldItalicStyle(ReText text) : base(text) { }
        }

        partial class UnorderedListStyle : TextStyle
        {
            public UnorderedListStyle(string item, ReText text) : base(text)
            {
                Item = item;
            }

            public string Item { get; set; }
        }

        partial class ColorStyle : TextStyle
        {
            ReColor Color;
            public ColorStyle(ReColor color, ReText text) : base(text)
            {
                Color = color;
            }
        }

        partial class FontStyle : TextStyle
        {
#if __ANDROID__
			protected Typeface Font;
			public FontStyle (Typeface font, ReText text) : base (text) {
                Font = font;
			}
#endif
#if __IOS__
            protected string FontName;
            public FontStyle(string fName, ReText text) : base(text)
            {
                FontName = fName;
            }
#endif
        }
    }

}

