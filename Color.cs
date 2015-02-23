using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ReCollect {
	public partial class ReColor {
		int R = 0;
		int G = 0;
		int B = 0;
		int A = 0;

		public ReColor (int r=0, int g=0, int b=0, int a=0) {
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public static ReColor Black {
			get { 
				return new ReColor ();
			}
		}

		public static ReColor Blue {
			get { 
				return new ReColor (0, 0, 255);
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

		public static ReColor Parse (string color_str)
		{
			try {
				var hex_match = Regex.Match (color_str, "^#([0-9a-fA-F]{1,2}){3}$");
				if (hex_match.Success) {
					var captures = hex_match.Groups [1].Captures;
					return new ReColor (
						parse_hex_color_val (captures [0].Value),
						parse_hex_color_val (captures [1].Value),
						parse_hex_color_val (captures [2].Value)
					);
				}

				var rgb_match = Regex.Match (color_str, "^rgb\\((\\d+),(\\d+),(\\d+)\\)$");
				if (rgb_match.Success) {
					return new ReColor (
						parse_int_color_val (rgb_match.Groups [1].Value),
						parse_int_color_val (rgb_match.Groups [2].Value),
						parse_int_color_val (rgb_match.Groups [3].Value)
					);
				}
			}
			catch (Exception ex) {
				Console.WriteLine ("Error parsing color parameter {0}: {1}", color_str, ex.Message);
			}

			return ReColor.Black;
		}
	}
}