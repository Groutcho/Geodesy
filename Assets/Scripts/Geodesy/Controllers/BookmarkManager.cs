using System;
using Geodesy.Models;
using System.Collections.Generic;
using Console = Geodesy.Views.Debugging.Console;
using Geodesy.Views.Debugging;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;

namespace Geodesy.Controllers
{
	public class BookmarkManager
	{
		public List<Bookmark> Bookmarks { get; private set; }

		public static BookmarkManager Instance { get; private set; }

		public BookmarkManager ()
		{
			Instance = this;
			Bookmarks = new List<Bookmark> (64);
			Console.Instance.Register ("bookmark", ExecuteBookmarkCommand);
		}

		/// <summary>
		/// Record the current state of the scene into a bookmark and
		/// store this bookmark for future use.
		/// </summary>
		public Bookmark Record (string name)
		{
			LatLon position = ViewpointController.Instance.CurrentPosition;

			Bookmark bookmark = new Bookmark {
				Name = name,
				ObserverPosition = position,
				AtmosphereEnabled = Globe.Instance.AtmosphereEnabled,
				BackgroundVisible = Compositer.Instance.BackgroundVisible
			};

			Bookmarks.Add (bookmark);
			return bookmark;
		}

		/// <summary>
		/// Save the bookmarks into the user's bookmarks.json file.
		/// </summary>
		private void Save ()
		{
			JsonSerializer serializer = new JsonSerializer ();
			serializer.Formatting = Formatting.Indented;
			string json = null;
			using (TextWriter text = new StringWriter ())
			{
				using (JsonTextWriter writer = new JsonTextWriter (text))
				{
					serializer.Serialize (writer, this);
					Debug.Log (text.ToString ());
					json = text.ToString ();
				}
			}

			File.WriteAllText (@"c:\temp\bookmarks.json", json);
		}

		#region Console commands

		private CommandResult ExecuteBookmarkCommand (Command command)
		{
			if (Console.Matches (command, new Token (Token.T_ID, "record"), Token.ID))
			{
				Bookmark created = Record (command.Tokens [1].Id);
				return new CommandResult (created);
			}

			if (Console.Matches (command, new Token (Token.T_ID, "save")))
			{
				Save ();
				return new CommandResult ("Bookmarks saved.");
			}

			throw new CommandException ("bookmark [record|save] [name]");
		}

		#endregion
	}
}

