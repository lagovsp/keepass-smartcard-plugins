﻿/*
  CryptokiKeyPrivder - A PKCS#11 Plugin for Keepass
  Copyright (C) 2013 Daniel Pieper <daniel.pieper@implogy.de>

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using CryptokiKeyProvider.Forms;

using KeePass.Plugins;
using KeePassLib.Keys;
using KeePassLib.Utility;
using KeePass.UI;
using cryptotest;
using System.IO;

namespace CryptokiKeyProvider
{

    public sealed class CryptokiKeyProviderExt : Plugin
    {
        private IPluginHost m_host = null;
        private CryptokiKeyProvider m_prov = null;

        public override bool Initialize(IPluginHost host)
        {
            m_prov = new CryptokiKeyProvider(host);
            m_host = host;
            m_host.KeyProviderPool.Add(m_prov);

            return true;
        }

        public override void Terminate()
        {
            m_host.KeyProviderPool.Remove(m_prov);
        }

        public override string UpdateUrl
        {
            get
            {
                return "http://implogy.de/CryptokiKeyProvider/version";
            }
        }
    }

    public sealed class CryptokiKeyProvider : KeyProvider
    {
        public static IPluginHost m_host = null;
        public static string pkcs11_conf_lib = null;
        public static string pkcs11_conf_slot = null;
        public static string pkcs11_conf_label = null;
        public static string database_name = null;

        public override bool DirectKey { get { return true; } }

        public override bool Exclusive { get { return true; } }

        public CryptokiKeyProvider(IPluginHost host)
        {
            m_host = host;
        }


        public override string Name
        {
            get { return "CryptokiKeyProvider"; }
        }

        public override byte[] GetKey(KeyProviderQueryContext ctx)
        {
            try
            {
                CryptokiKeyProvider.database_name = Path.GetFileName(ctx.DatabasePath);
                
                getSettings();

                if (ctx.CreatingNewKey)
                {
                    return Create(ctx);
                }
                else
                {
                    return Open(ctx);
                }
            }
            catch (Exception ex) { MessageService.ShowWarning(ex.Message); }
           
            return null;
        }

        private static byte[] Create(KeyProviderQueryContext ctx)
        {
            CkpCreationForm dialog = new CkpCreationForm(m_host, Path.GetFileName(ctx.DatabasePath));

            if (UIUtil.ShowDialogAndDestroy(dialog) != DialogResult.OK)
                return null;

            getSettings();

            try
            {
                return pkcs11.pkcs11_read_key(pkcs11_conf_lib, pkcs11_conf_label);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return dialog.selected_key;
        }

        public static void getSettings() {

            string configBase = "CryptokiKeyProvider.";
            string strRef =
                Environment.MachineName + "." +
                Environment.UserDomainName + "." +
                Environment.UserName + ".";

                pkcs11_conf_lib = m_host.CustomConfig.GetString(configBase + strRef + CryptokiKeyProvider.database_name + ".pkcs11_library");
                pkcs11_conf_slot = m_host.CustomConfig.GetString(configBase + strRef + CryptokiKeyProvider.database_name + ".pkcs11_slot");
                pkcs11_conf_label = m_host.CustomConfig.GetString(configBase + strRef + CryptokiKeyProvider.database_name + ".pkcs11_label");

        }


        public static void saveSettings(string lib, string slot, string label)
        {
            string configBase = "CryptokiKeyProvider.";
            string strRef =
                Environment.MachineName + "." +
                Environment.UserDomainName + "." +
                Environment.UserName + ".";
            string pkcs11_conf_lib = configBase + strRef + CryptokiKeyProvider.database_name + ".pkcs11_library";
            string pkcs11_conf_slot = configBase + strRef + CryptokiKeyProvider.database_name + ".pkcs11_slot";
            string pkcs11_conf_label = configBase + strRef + CryptokiKeyProvider.database_name + ".pkcs11_label";

            m_host.CustomConfig.SetString(pkcs11_conf_lib, lib);
            m_host.CustomConfig.SetString(pkcs11_conf_slot, slot);
            m_host.CustomConfig.SetString(pkcs11_conf_label, label);
        }

        public static bool ByteArrayToFile(string file, byte[] data)
        {
            try
            {
                System.IO.FileStream stream = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                stream.Write(data, 0, data.Length);
                stream.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught in process: {0}", e.ToString());
            }
            return false;
        }

        private static byte[] Open(KeyProviderQueryContext ctx)
        {
            getSettings();

            if ( (pkcs11_conf_lib == null) || (pkcs11_conf_label == null) )
            {
                CkpPromtForm dialog = new CkpPromtForm(ctx);
                if (UIUtil.ShowDialogAndDestroy(dialog) == DialogResult.OK)
                {
                    getSettings();
                    return pkcs11.pkcs11_read_key(pkcs11_conf_lib, pkcs11_conf_label, dialog.pin.ReadString());
                }
            }
            else
            {

                try
                {
                    CkpPromtForm dialog = new CkpPromtForm(pkcs11_conf_label);
                    if (UIUtil.ShowDialogAndDestroy(dialog) == DialogResult.OK)
                    {
                         return pkcs11.pkcs11_read_key(pkcs11_conf_lib, pkcs11_conf_label, dialog.pin.ReadString());
                    }
                    return null;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            return null;
        }
    }
}
