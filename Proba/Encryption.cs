using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Proba
{
    public class Encryption
    {
        private static int szEncryptionKey = 100;
        public static string EncryptDecrypt(string szPlainText)
        {
            StringBuilder szInputStringBuild = new StringBuilder(szPlainText);
            StringBuilder szOutStringBuild = new StringBuilder(szPlainText.Length);
            char Textch;
            for (int iCount = 0; iCount < szPlainText.Length; iCount++)
            {
                Textch = szInputStringBuild[iCount];
                Textch = (char)(Textch ^ szEncryptionKey);
                szOutStringBuild.Append(Textch);
            }
            return szOutStringBuild.ToString();
        }
    }
}