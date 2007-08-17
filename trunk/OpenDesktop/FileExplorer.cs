// OpenDesktop - A search tool for the Windows desktop
// Copyright (C) 2005-2006, Pravin Paratey (pravinp at gmail dot com)
// http://opendesktop.berlios.de
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

using OpenDesktop.Plugin;
using OpenDesktop.Properties;

namespace OpenDesktop
{
	#region Public Delegates
	/// <summary>
	/// Public delegate which tells how many documents have been indexed
	/// </summary>
	/// <param name="filePath">Document currently indexing</param>
	/// <param name="numDocs">number of documents already indexed</param>
	public delegate void IndexProgressHandler(string filePath, int numDocs);
	public delegate void FileExplorerDoneHandler();
	public delegate void FileExplorerProgressHandler(string filePath);
	#endregion

	class FileExplorer : IDisposable
	{
		#region Private Vars
		private NameObjectCollection m_fileFunctionMap;
		private Thread m_objThread;
		private bool _quit;
		private ListDictionary m_DNV; // Donot visit hashmap
		Indexer m_indexer;
		private int m_iNumDocsDone;
		private bool m_bAgressive = false; // true = dont sleep between documents
		#endregion

		#region Public Events
		public event IndexProgressHandler IndexProgress;
		private void OnIndexProgress(string filePath, int numDocs)
		{
			if (IndexProgress != null)
			{
				IndexProgress(filePath, numDocs);
			}
		}
		
		public event FileExplorerProgressHandler FileExplorerProgress;
		private void OnFileExplorerProgress(string filePath)
		{
			if (FileExplorerProgress != null)
			{
				FileExplorerProgress(filePath);
			}
		}
		
		public event FileExplorerDoneHandler FileExplorerDone;
		private void OnFileExplorerDone()
		{
			if(FileExplorerDone != null)
			{
				FileExplorerDone();
			}
		}
		#endregion

		#region Constructor
		public FileExplorer(NameObjectCollection fileFunctionMap)
		{
			m_fileFunctionMap = fileFunctionMap;
			_quit = false;
			m_DNV = Settings.Instance.DNV;
			m_indexer = new Indexer(Properties.Settings.Instance.TempIndexPath, IndexMode.CREATE);
		}

		public void Dispose()
		{
			if (m_objThread != null)
			{
				m_objThread.Abort();
			}
			if (m_indexer != null)
			{
				m_indexer.Close();
			}
		}
		#endregion

		#region Run/Stop/Pause/Resume
		/// <summary>
		/// Starts the FileExploring process
		/// </summary>
		/// <param name="aggresive">
		/// If true, thread priority isnt set to low and there is no delay after
		/// each file addition
		/// </param>
		public void Run(bool aggressive)
		{
			m_iNumDocsDone = 0;
			
			m_bAgressive = aggressive;

			// Check if an update of the index is due
			TimeSpan objUpdateFrequency = new TimeSpan(Properties.Settings.Instance.IndexUpdateFrequency, 0, 0);
			if (DateTime.Now.Subtract(Properties.Settings.Instance.IndexLastUpdated) < objUpdateFrequency)
			{
				Logger.Instance.LogDebug("Update time not elapsed. Exiting FileExplorer");
				return;
			}

			m_objThread = new Thread(new ThreadStart(Explore));
			if(m_bAgressive)
			{
				m_objThread.Priority = ThreadPriority.Normal;
			}
			else
			{
				m_objThread.Priority = ThreadPriority.Lowest;
			}
			m_objThread.Name = "FileExplorer";
			try
			{
				m_objThread.Start();
			}
			catch (Exception e)
			{
				Logger.Instance.LogException(e);
			}
		}
/*
		public void Pause()
		{
			if (m_objThread != null && m_objThread.ThreadState == ThreadState.Running)
			{
				m_objThread.Suspend();
			}
		}

		public void Resume()
		{
			if (m_objThread != null && m_objThread.ThreadState == ThreadState.Suspended)
			{
				m_objThread.Resume();
			}
		}
*/
		public void Stop()
		{
			_quit = true;
		}
		#endregion

		#region Explore
		private void Explore()
		{
			string [] strDriveList = Environment.GetLogicalDrives();
			foreach (string strDrive in strDriveList)
			{
				DriveInfo info = new DriveInfo(strDrive);
				if (info.IsReady)
				{
					Logger.Instance.LogDebug("Exploring " + strDrive);
					Explore(strDrive);
					break; //FIXME: REMOVE THIS. IT IS TEMP FOR DEBUGGING ONLY
				}
			}

			if (!_quit) // If the exit was natural
			{
				Logger.Instance.LogDebug("Moving index from " +
					Properties.Settings.Instance.IndexPath + " to " +
					Properties.Settings.Instance.TempIndexPath);
				// Close index and move it 
				m_indexer.Close(); m_indexer = null;

				// Lock the index as we are going to be moving it
				Synchronizer.Instance.LockIndex(this);
				try
				{
					if (Directory.Exists(Properties.Settings.Instance.IndexPath))
					{
						Directory.Delete(Properties.Settings.Instance.IndexPath, true);
					}
					Directory.Move(Properties.Settings.Instance.TempIndexPath,
						Properties.Settings.Instance.IndexPath);

					Properties.Settings.Instance.IndexLastUpdated = DateTime.Now;
					Properties.Settings.Instance.Save();
				}
				catch (Exception e)
				{
					Logger.Instance.LogException(e);
				}
				Synchronizer.Instance.ReleaseIndex(this);
			}
			OnFileExplorerDone(); // Signal we are done
		}

		private void Explore(string directory)
		{
			if (_quit)
				return;
			
			OnFileExplorerProgress(directory);

			string[] strDirList;
			string[] strFileList;

			try
			{
				strDirList = Directory.GetDirectories(directory);
				strFileList = Directory.GetFiles(directory);
			}
			catch
			{
				return;
			}

			foreach (string strFile in strFileList)
			{
				if (_quit)
					return;

				string strExtention = Path.GetExtension(strFile).ToLower();
				if (strExtention.Length == 0)
					continue;
				ArrayList arPluginList = m_fileFunctionMap.Get(strExtention);
				if (arPluginList == null)
					continue;

				foreach (IPlugin plugin in arPluginList)
				{
					try
					{
						SearchInfo sInfo = plugin.ProcessFile(strFile);
						if (sInfo != null)
						{
							m_indexer.AddDocument(sInfo);
							OnIndexProgress(strFile, ++m_iNumDocsDone);
						}
					}
					catch (Exception e)
					{
						Logger.Instance.LogException(e);
					}
				}
				
				if(!m_bAgressive)
					Thread.Sleep(100);
			}
			foreach (string strDir in strDirList)
			{
				if (_quit)
					return;
				try
				{
					//FIXME: Make this case-insensitive
					if(!m_DNV.Contains(directory))
						Explore(strDir);
				}
				catch (Exception e) // Out of stack exception?
				{
					Logger.Instance.LogException(e);
				}
			}
		}
		#endregion
	}
}
