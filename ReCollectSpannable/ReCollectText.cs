using System;
using System.Collections.Generic;
using ReCollect;
using Android.Text;
using Android.Text.Style;
using Android.Graphics;
using System.Net;
using System.Threading.Tasks;

namespace ReCollect
{
	public partial class ReColor {
		public Android.Graphics.Color AndroidColor {
			get {
				return new Android.Graphics.Color (R, G, B);
			}
		}
	}

	public partial class StyleWithRange {
	}

	public partial class ReText
	{
		public event Action<string> HandleClick;
		SpannableString _text = null;

        public SpannableString FormattedText {
			get {
				if (_text != null)
					return _text;

				var parsed = ParseHtml ();

				// Apply attributes
				_text = new SpannableString (parsed.Text);
				while (parsed.Styles.Count > 0) {
					var ranged_styles = parsed.Styles.Pop ();
                    if (ranged_styles.Style.Styles != null)
                    {
                        foreach (var style in ranged_styles.Style.Styles)
                        {
                            if (ranged_styles.Length > 0)
                            {
                                _text.SetSpan(
                                    style,
                                    ranged_styles.Offset,
                                    ranged_styles.Offset + ranged_styles.Length,
                                    SpanTypes.ExclusiveExclusive
                                );
                            }
                        }
                    }
                    else 
                    {
                        LoadImage(ranged_styles);
                    }
				}

				return _text;
			}
		}

        void LoadImage(StyleWithRange ranged_styles){
            ImageStyle imageStyle = ranged_styles.Style as ImageStyle;
            if (imageStyle != null)
            {
                var webClient = new WebClient();
                byte[] imageBytes = webClient.DownloadData(imageStyle.Src.Trim());
                Bitmap b = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                Bitmap bitmap = Bitmap.CreateScaledBitmap(b, (int)ImageSize, (int)ImageSize, true);


                _text.SetSpan(new ImageSpan(bitmap, SpanAlign.Baseline), ranged_styles.Offset, ranged_styles.Offset + ranged_styles.Length, SpanTypes.ExclusiveExclusive);
            }
        }

		partial class TextStyle {
			public virtual List<CharacterStyle> Styles { get; set; }
		}

		partial class HeaderStyle : TextStyle {
			override public List<CharacterStyle> Styles {
				get {
					return new List<CharacterStyle> {
						new RelativeSizeSpan (Factor)
					};
				}
			}
		}

		partial class LinkStyle : TextStyle {
			override public List<CharacterStyle> Styles {
				get {
					return new List<CharacterStyle> {
						new ForegroundColorSpan (Text.LinkColor.AndroidColor),
						new UnderlineSpan (),
						new RichTextLinkSpan (Href, Text)
					};
				}
			}
		}

		partial class ItalicStyle : TextStyle {
			override public List<CharacterStyle> Styles {
				get {
					return new List<CharacterStyle> { new StyleSpan (TypefaceStyle.Italic) };
				}
			}
		}

		partial class BoldStyle : TextStyle {
			override public List<CharacterStyle> Styles {
				get {
					return new List<CharacterStyle> { new StyleSpan (TypefaceStyle.Bold) };
				}
			}
		}

		partial class ColorStyle : TextStyle {
			override public List<CharacterStyle> Styles {
				get {
					return new List<CharacterStyle> { new ForegroundColorSpan (Color.AndroidColor) };
				}
			}
		}

		partial class FontStyle : TextStyle {
			override public List<CharacterStyle> Styles {
				get {
                    TypefaceSpan tfs;
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.P)
                    {
                        tfs = new TypefaceSpan(Font);
                    } else {
                        tfs = new CustomTypefaceSpan(Font);
                    }
                    return new List<CharacterStyle> {tfs, new AbsoluteSizeSpan (Text.FontSize, true)};
                }
			}
		}

        class RichTextLinkSpan : URLSpan {
			string Href;
			ReText Text;
            public RichTextLinkSpan (string href, ReText text) : base (href) {
				Href = href;
				Text = text;
			}
			public override void OnClick (Android.Views.View view)
			{
				if (Text.HandleClick != null)
					Text.HandleClick (Href);
			}
		}
	}
}

