﻿using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OpenTerra.Controllers.Commands;
using OpenTerra.Models;
using UnityEngine;

namespace OpenTerra.Controllers
{
	public class BookmarkManager
	{
		private IList<Bookmark> bookmarks;

		private IGlobe globe;
		private ICompositer compositer;
		private IViewpointController viewpointController;

		public BookmarkManager (IShell shell, IGlobe globe, ICompositer compositer, IViewpointController viewpointController)
		{
			this.globe = globe;
			this.compositer = compositer;
			this.viewpointController = viewpointController;
			bookmarks = new List<Bookmark> (64);
			
			shell.Register ("bookmark", ExecuteBookmarkCommand);
		}

		/// <summary>
		/// Record the current state of the scene into a bookmark and
		/// store this bookmark for future use.
		/// </summary>
		public Bookmark Record (string name)
		{
			LatLon position = viewpointController.ActiveViewpoint.CurrentPosition;

			Bookmark bookmark = new Bookmark {
				Name = name,
				ObserverPosition = position,
				AtmosphereEnabled = globe.AtmosphereEnabled,
				BackgroundVisible = compositer.BackgroundVisible
			};

			bookmarks.Add (bookmark);
			return bookmark;
		}

		private static string GetBookmarkFilename()
		{
			var userDir = Utils.GetUserDirectory();
			string bookmarkFile = Path.Combine(userDir.FullName, "bookmarks.json");

			return bookmarkFile;
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
					json = text.ToString ();
				}
			}

			File.WriteAllText (GetBookmarkFilename(), json);
		}

		#region Console commands

		private Response ExecuteBookmarkCommand (Command command)
		{
			if (Command.Matches (command, new Token (Token.T_ID, "record"), Token.ID))
			{
				Bookmark created = Record (command.Tokens [1].Id);
				return new Response (created, ResponseType.Success);
			}

			if (Command.Matches (command, new Token (Token.T_ID, "save")))
			{
				Save ();
				return new Response("Bookmarks saved.", ResponseType.Success);
			}

			throw new CommandException ("bookmark [record|save] [name]");
		}

		#endregion
	}
}

