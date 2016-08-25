using System;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Geodesy.Views.Debugging
{
	public class CommandToken
	{
		public const int FLOAT = 1;
		public const int INT = 2;
		public const int ID = 3;
		public const int BOOL = 4;

		public static Regex INT_Regex = new Regex (@"\d+");
		public static Regex FLOAT_Regex = new Regex (@"\d+\.\d+");
		public static Regex ID_Regex = new Regex (@"[a-zA-Z_]\w+");
		public static Regex TRUTH_Regex = new Regex (@"(true|false|on|off)");

		public object Value { get; set; }

		public bool Bool { get { return (bool)Value; } }

		public int Int { get { return (int)Value; } }

		public float Float { get { return (float)Value; } }

		public string Id { get { return (string)Value; } }

		public int TokenType { get; set; }

		public CommandToken (int type, object value)
		{
			this.TokenType = type;
			this.Value = value;
		}

		public override string ToString ()
		{
			string typeStr = "";
			switch (TokenType)
			{
				case INT:
					typeStr = "int";
					break;
				case FLOAT:
					typeStr = "float";
					break;
				case BOOL:
					typeStr = "truth";
					break;
				case ID:
					typeStr = "ident";
					break;
				default:
					throw new NotImplementedException ();
			}

			return string.Format ("({0}, <{1}>)", typeStr, Value);
		}

		public static CommandToken Tokenize (string word)
		{
			if (FLOAT_Regex.IsMatch (word))
				return new CommandToken (FLOAT, float.Parse (word, CultureInfo.InvariantCulture));

			if (INT_Regex.IsMatch (word))
				return new CommandToken (INT, int.Parse (word));

			if (TRUTH_Regex.IsMatch (word))
				return new CommandToken (BOOL, word == "on" || word == "true");

			if (ID_Regex.IsMatch (word))
				return new CommandToken (ID, word);

			throw new NotImplementedException ("Invalid token: " + word);
		}
	}
}

