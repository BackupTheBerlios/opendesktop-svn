/*
 * Created by SharpDevelop.
 * User: pravin
 * Date: 1/22/2006
 * Time: 3:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Threading;

namespace OpenDesktop
{
	/// <summary>
	/// Description of FirstTimeUser.
	/// </summary>
	public class FirstTimeUser
	{
		private FileExplorer m_fileExplorer = null;
		private IndexGeneration m_form = null;
		private FileExplorerDoneHandler m_dlgtDoneHandler = null;
		private FileExplorerProgressHandler m_dlgtProgressHandler = null;
		
		public FirstTimeUser()
		{
		}
		
		/// <summary>
        /// If the user is determined to be a first time user, this function
        /// generates an initial index. This function blocks until the index
        /// is generated
        /// </summary>
        /// <returns>true means new user found</returns>
		public bool IsFirstTimeUser()
		{
        	if(Directory.Exists(Properties.Settings.Instance.IndexPath))
        		return false;
        	
        	Logger.Instance.LogDebug("First time user. Generating initial index");
        	
        	m_form = new IndexGeneration();
        	m_form.Show();
        	
        	m_fileExplorer = new FileExplorer(PluginManager.Instance.RegisteredFileExtensions);
        	m_fileExplorer.Run(true);
        	m_dlgtProgressHandler = new FileExplorerProgressHandler(m_form.ShowProgress);
			m_dlgtDoneHandler = new FileExplorerDoneHandler(FirstTimeDone);
			m_fileExplorer.FileExplorerProgress += m_dlgtProgressHandler;
			m_fileExplorer.FileExplorerDone += m_dlgtDoneHandler;
//			lock(m_firstTimeUserLock)
//			{
//        		Monitor.Wait(m_firstTimeUserLock);
//			}

        	return true;
        }
        
        void FirstTimeDone()
        {
//        	lock(m_firstTimeUserLock)
//        	{
//        		Monitor.Pulse(m_firstTimeUserLock);
//        	}

        	m_fileExplorer.FileExplorerProgress -= m_dlgtProgressHandler;
        	m_fileExplorer.FileExplorerDone -= m_dlgtDoneHandler;
        	m_form.Dispose(); m_form = null;
        	m_fileExplorer.Dispose(); m_fileExplorer = null;
        }
	}
}
