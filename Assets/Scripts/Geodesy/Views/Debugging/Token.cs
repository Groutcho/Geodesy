using System;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Geodesy.Views.Debugging
{
	public class Token
	{
		public const int T_FLOAT = 1;
		public const int T_INT = 2;
		public const int T_ID = 3;
		public const int T_BOOL = 4;

		public static Token BOOL = new Token (T_BOOL);
		public static Token FLOAT = new Token (T_FLOAT);
		public static Token ID = new Token (T_ID);
		public static Token INT = new Token (T_INT);

		public static Regex INT_Regex = new Regex (@"\d+");
		public static Regex FLOAT_Regex = new Regex (@"\d+\.\d+");
		public static Regex ID_Regex = new Regex (@"[a-zA-Z_]\w+");
		public static Regex TRUTH_Regex = new Regex (@"(true|false|on|off)");

		public object Value { get; set; }

		public bool Bool { get { return (bool)Value; } }

		public int Int { get { return (int)Value; } }

		public float Float { get { return (float)Value; } }

		public string Id { get { return (string)Value; } }

		public int Type { get; set; }

		public Token (int type, object value)
		{
			this.Type = type;
			this.Value = value;
		}

		public Token (int type) : this (type, null)
		{
		}

		public override string ToString ()
		{
			string typeStr = "";
			switch (Type)
			{
				case T_INT:
					typeStr = "int";
					break;
				case T_FLOAT:
					typeStr = "float";
					break;
				case T_BOOL:
					typeStr = "truth";
					break;
				case T_ID:
					typeStr = "ident";
					break;
				default:
					throw new NotImplementedException ();
			}

			return string.Format ("({0}, <{1}>)", typeStr, Value);
		}

		public static Token Tokenize (string word)
		{
			if (FLOAT_Regex.IsMatch (word))
				return new Token (T_FLOAT, float.Parse (word, CultureInfo.InvariantCulture));

			if (INT_Regex.IsMatch (word))
				return new Token (T_INT, int.Parse (word));

			if (TRUTH_Regex.IsMatch (word))
				return new Token (T_BOOL, word == "on" || word == "true");

			if (ID_Regex.IsMatch (word))
				return new Token (T_ID, word);

			throw new NotImplementedException ("Invalid token: " + word);
		}
	}
}

