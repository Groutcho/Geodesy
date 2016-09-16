using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenTerra.Controllers
{
	/// <summary>
	/// Occurs when a file could not be imported.
	/// </summary>
	public class ImportException : IOException
	{
		public ImportException(string message) : base(message) { }
	}
}
