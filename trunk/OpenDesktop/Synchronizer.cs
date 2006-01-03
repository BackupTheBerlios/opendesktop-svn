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
using System.Collections.Generic;
using System.Text;

namespace OpenDesktop
{
    /// <summary>
    /// This will be a common class to synchronize events like
    /// 1. Obtaining a lock before moving a index-dir and releasing it after done.
    /// </summary>
    class Synchronizer
    {
        #region Member Vars
        private static Synchronizer _instance;
        private static object m_lock = new object();
        private object m_indexerLock;
        private bool m_bIndexLocked = false;
        #endregion

        #region Constructor
        private Synchronizer()
        {
            m_indexerLock = new object();
        }
        #endregion

        #region Standard Singletone Implementation
        public static Synchronizer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (m_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Synchronizer();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Index Synchronization
        /// <summary>
        /// Lock the index to tell everybody that index is going to change
        /// </summary>
        public void LockIndex()
        {
            lock (m_indexerLock)
            {
                m_bIndexLocked = true;
            }
        }

        /// <summary>
        /// Release the lock
        /// </summary>
        public void ReleaseIndex()
        {
            lock (m_indexerLock)
            {
                m_bIndexLocked = false;
            }
        }

        /// <summary>
        /// Gets the lock state of the index
        /// </summary>
        public bool IndexIsLocked
        {
            get { return m_bIndexLocked; }
        }
        #endregion
    }
}
