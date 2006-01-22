/*
 * Created by SharpDevelop.
 * User: pravin
 * Date: 1/22/2006
 * Time: 2:08 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Text;
using System.Collections.Specialized;

namespace OpenDesktop.Properties
{
	/// <summary>
	/// Description of Settings.
	/// </summary>
	public class Settings
	{
		#region Member Variables
		private static object m_lock = new object();
		private static Settings _instance = null;
		
		private string m_strAppData;
		private string m_strIndexPath;
		private string m_strTempIndexPath;
		private DateTime m_dtIndexLastUpdated;
		private int m_indexUpdateFreq;
		#endregion
		
		/// <summary>
		/// Constructor
		/// </summary>
		private Settings()
		{
			// TODO: If Settings is empty, fill it up with default values and save
			m_strAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenDesktop");
			
			ReadConfig();
		}
		
		
		#region Standard Singleton Implementation
		public static Settings Instance
		{
			get
			{
				if(_instance == null)
				{
					lock(m_lock)
					{
						if(_instance == null)
						{
							_instance = new Settings();
						}
					}
				}
				return _instance;
			}
		}
		#endregion
		
		public string IndexPath
		{
			get
			{
				return m_strIndexPath;
			}
			set
			{
				;
			}
		}
		
		public string TempIndexPath
		{
			get
			{
				return m_strTempIndexPath;
			}
			set
			{
				;
			}
		}
		
		public DateTime IndexLastUpdated
		{
			get
			{
				return m_dtIndexLastUpdated;
			}
			set
			{
				m_dtIndexLastUpdated = value;
			}
		}
		
		public int IndexUpdateFrequency
		{
			get
			{
				return m_indexUpdateFreq;
			}
		}
		
		private void ReadConfig()
		{
			// Set Default values
			m_strIndexPath = Path.Combine(m_strAppData, "Index");
			m_strTempIndexPath = Path.Combine(m_strAppData, "TempIndex");
			m_dtIndexLastUpdated = DateTime.MinValue;
			m_indexUpdateFreq = 24;
			
			string strConfigFilePath = Path.Combine(m_strAppData, "config.ini");
			if(!File.Exists(strConfigFilePath))
				return;
			StreamReader reader = new StreamReader(strConfigFilePath);
			while(true)
			{
				string strLine = reader.ReadLine();
				if(strLine == null)
					break;
				
				string []strNameValue = strLine.Split('=');
				if(strNameValue.Length != 2)
					continue;
				
				switch(strNameValue[0].Trim())
				{
					case "IndexPath":
						m_strIndexPath = strNameValue[1].Trim();
						break;
					case "TempIndexPath":
						m_strTempIndexPath = strNameValue[1].Trim();
						break;
					case "IndexLastUpdated":
						try
						{
							m_dtIndexLastUpdated = DateTime.Parse(strNameValue[1].Trim());
						}
						catch
						{
							m_dtIndexLastUpdated = DateTime.MinValue;
						}
						break;
					case "IndexUpdateFreq":
						m_indexUpdateFreq = Int32.Parse(strNameValue[1].Trim());
						break;
				}
				
			}
			reader.Close();
		}
		
		/// <summary>
		/// Saves the settings to disk
		/// </summary>
		public void Save()
		{
			
		}
		
		public string ProcessTag(NameValueCollection queryString)
		{
			StringBuilder sbResult = new StringBuilder();
			if (queryString != null)
			{
				string strFirstTime = queryString["firstTime"];
				if (strFirstTime != null)
				{
					// First time user

				}
				Settings.Instance.IndexPath = queryString["IndexPath"];
				Settings.Instance.TempIndexPath = queryString["TempIndexPath"];
			}
			sbResult.Append(@"<form action=""config.html"" method=""post"">");
			sbResult.Append(@"<table cellspacing=""2"" cellpadding=""2"" border=""0"">");
			sbResult.Append(@"<tr><td colspan=""2""><b>Index Settings:</b></td></tr>");
			sbResult.Append(@"<tr><td>Index Path:</td><td><input type=""file"" name=""IndexPath"" value=""" + Settings.Instance.IndexPath + @""" /></td></tr>");
			sbResult.Append(@"<tr><td>Temp Path:</td><td><input type=""file"" name=""TempIndexPath"" value=""" + Settings.Instance.TempIndexPath + @""" /></td></tr>");
			sbResult.Append(@"<tr><td>Update Frequency:</td><td><input type=""text"" name=""UpdateFrequency"" size=""4"" value=""" + Settings.Instance.IndexUpdateFrequency + @"""/>hrs</td></tr>");
			sbResult.Append(@"<tr><td colspan=""2""><b>Startup:</b></td></tr>");
			sbResult.Append(@"<tr><td colspan=""2""><input type=""checkbox"" name=""LaunchAtStartup"" /> Launch OpenDesktop when windows starts</td></tr>");
			sbResult.Append(@"<tr><td colspan=""2""><b>Browser Settings:</b></td></tr>");
			sbResult.Append(@"<tr><td colspan=""2""><input type=""checkbox"" name=""LaunchAtStartup"" /> Launch links in new window</td></tr>");
			sbResult.Append(@"</table>");
			sbResult.Append(@"<p align=""center""><input type=""submit"" value=""Save Settings"" /></p>");
			sbResult.Append(@"</form>");

			return sbResult.ToString();
		}
	}
}
