using System.Web.Security;
using System.Text.RegularExpressions;
using System;

namespace SuperBabyWCF
{
    public class Security
    {
        public static string CreateSalt(int prmSize)
        {
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] buffer = null;
            buffer = new byte[prmSize + 1];
            rng.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        public static string GetPasswordHash(string prmPW, string prmSalt)
        {
            string strSaltAndPW = string.Concat(prmPW, prmSalt);
            string HashedPW = FormsAuthentication.HashPasswordForStoringInConfigFile(strSaltAndPW, "sha1");
            return HashedPW;
        }

        /// <summary>
        /// Encrypt String
        /// </summary>
        /// <param name="prmPWD"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string EncryptString(string prmPWD)
        {
            byte[] b = System.Text.ASCIIEncoding.ASCII.GetBytes(prmPWD);
            return Convert.ToBase64String(b);
        }
        /// <summary>
        /// Decrypt String
        /// </summary>
        /// <param name="prmPWD"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string DecryptString(string prmPWD)
        {
            byte[] b = Convert.FromBase64String(prmPWD);
            return System.Text.ASCIIEncoding.ASCII.GetString(b);
        }

        /// <summary>
        /// Validate Password
        /// Password shold contains atleast 8 character
        /// Passowrd should contains atleast 3 from below condition
        /// one Upper case
        /// one Lower case
        /// one Number 
        /// one Symbol
        /// </summary>
        /// <param name="prmPW"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool ValidatePassword(string prmPW)
        {

            bool isValidPassword = false;

            int intValidCount = 0;

            string regExUpperCase = "[A-Z]";
            string regExLowerCase = "[a-z]";
            string regExNumber = "[0-9]";
            string regExSymbols = "[\\!\\@\\#\\$\\%\\^\\&\\*\\(\\)\\-\\+\\=\\,\\>\\<\\.\\/\\?\\;\\:\\\\]";


            isValidPassword = Regex.IsMatch(prmPW, regExUpperCase);
            if ((isValidPassword))
            {
                intValidCount = intValidCount + 1;
            }
            isValidPassword = Regex.IsMatch(prmPW, regExLowerCase);
            if ((isValidPassword))
            {
                intValidCount = intValidCount + 1;
            }
            isValidPassword = Regex.IsMatch(prmPW, regExNumber, RegexOptions.IgnoreCase);
            if ((isValidPassword))
            {
                intValidCount = intValidCount + 1;
            }
            isValidPassword = Regex.IsMatch(prmPW, regExSymbols);
            if ((isValidPassword))
            {
                intValidCount = intValidCount + 1;
            }

            if ((intValidCount >= 3 & prmPW.Trim().Length >= 8))
            {
                isValidPassword = true;
            }
            else
            {
                isValidPassword = false;
            }

            return isValidPassword;
        }
    }
}