/*
 * Created by SharpDevelop.
 * User: pravin
 * Date: 1/22/2006
 * Time: 2:08 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace OpenDesktop.Properties
{
	/// <summary>
	/// Description of Resource.
	/// </summary>
	public class Resources
	{
		public static string TrayMenuExit
		{
			get
			{
				return "&Exit";
			}
		}
		
		public static string TrayMenuOpen
		{
			get
			{
				return "&Open";
			}
		}
		
		public static string TrayMenuSettings
		{
			get
			{
				return "&Settings";
			}
		}
		
		public static string FileNotFound
		{
			get
			{
				return "404 - File Not Found";
			}
		}
		
		public static string NoSearchResults
		{
			get
			{
				return "No Search Results";
			}
		}
		
		public static string SearchResultsHeader
		{
			get
			{
				// ex. Results 1 - 10 of about 100 for OpenDesktop
				return "Results {0} - {1} of about {2} for {3}";
			}
		}
		
		public static string SearchResultsNext
		{
			get
			{
				return "Next";
			}
		}
		
		public static string SearchResultsPrev
		{
			get
			{
				return "Prev";
			}
		}
		
		public static string ServerString
		{
			get
			{
				return "Open Desktop Search 0.1";
			}
		}
		
		public static System.Drawing.Icon MainIcon
		{
			get
			{
				System.Resources.ResourceManager rm = new System.Resources.ResourceManager("OpenDesktop.OpenDesktop", typeof(Resources).Assembly);
				System.Drawing.Icon icon = rm.GetObject("main") as System.Drawing.Icon;
				return icon;
			}
		}
	}
}
