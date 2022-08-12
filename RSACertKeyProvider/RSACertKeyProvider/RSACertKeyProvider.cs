using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

using KeePass.Plugins;
using KeePass.Forms;
using KeePass.Resources;
using KeePassLib.Keys;
using KeePassLib.Cryptography;
using KeePassLib.Serialization;
// for UI
using KeePass.UI;
using KeePassLib.Utility;

namespace RSACertKeyProvider
{
  public sealed class RSACertKeyProviderExt : Plugin
  {
    private IPluginHost m_host = null;
    private RSAKeyProvider m_prov = new RSAKeyProvider();

    public override bool Initialize(IPluginHost host)
    {
      m_host = host;
      m_host.KeyProviderPool.Add(m_prov);
      return true;
    }

    public override void Terminate()
    {
    m_host.KeyProviderPool.Remove(m_prov);
    }
  }

  public sealed class RSAKeyProvider : KeyProvider
  {
    public override string Name
    {
      get { return "RSA Certificate Key Provider"; }
    }

    public override byte[] GetKey(KeyProviderQueryContext ctx)
    {
      try
      {
        X509Store store = new X509Store("MY",StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

        X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
        X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid,DateTime.Now,false);
        X509Certificate2Collection scollection = X509Certificate2UI.SelectFromCollection(fcollection, "Test Certificate Select","Select a certificate from the following list to get information on that certificate",X509SelectionFlag.SingleSelection);

        if (scollection.Count == 0) {
            throw new CryptographicException("No Certificate selected.");
        }

        X509Certificate2 certificate = scollection[0];
        store.Close();

        //HasPrivateKey
        if(certificate.HasPrivateKey) {
          RSACryptoServiceProvider rsaObj = (RSACryptoServiceProvider)certificate.PrivateKey;
          RSAParameters rsaParams = rsaObj.ExportParameters(true);
          //PrivateKey
          return rsaParams.D;
        } else
          throw new CryptographicException("No Private key contained within certificate.");
      }
      catch (CryptographicException ex)
      {
        MessageBox.Show(ex.Message);
        return null;
      }
    }
  }
}
