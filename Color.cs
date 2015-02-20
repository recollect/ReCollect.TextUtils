using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ReCollect {
	public partial class Color {
		public int R = 0;
		public int G = 0;
		public int B = 0;

		public static Color FromRGB (int r, int g, int b) {
			return new Color () {
				R = r,
				G = g,
				B = b
			};
		}

		public static Color Black {
			get { 
				return new Color ();
			}
		}

		public static Color Blue {
			get { 
				return new Color () {
					B = 255
				};
			}
		}

		static int parse_hex_color_val (string hex)
		{
			if (hex.Length == 1)
				hex = hex + hex;
			return int.Parse (hex, System.Globalization.NumberStyles.HexNumber);
		}

		static int parse_int_color_val (string val)
		{
			return int.Parse (val);
		}

		public static Color Parse (string color_str)
		{
			try {
				var hex_match = Regex.Match (color_str, "^#([0-9a-fA-F]{1,2}){3}$");
				if (hex_match.Success) {
					var captures = hex_match.Groups [1].Captures;
					return new Color () {
						R = parse_hex_color_val (captures [0].Value),
						G = parse_hex_color_val (captures [1].Value),
						B = parse_hex_color_val (captures [2].Value)
					};
				}

				var rgb_match = Regex.Match (color_str, "^rgb\\((\\d+),(\\d+),(\\d+)\\)$");
				if (rgb_match.Success) {
					return new Color () {
						R = parse_int_color_val (rgb_match.Groups [1].Value),
						G = parse_int_color_val (rgb_match.Groups [2].Value),
						B = parse_int_color_val (rgb_match.Groups [3].Value)
					};
				}
			}
			catch (Exception ex) {
				Console.WriteLine ("Error parsing color parameter {0}: {1}", color_str, ex.Message);
			}

			return Color.Black;
		}
	}
}