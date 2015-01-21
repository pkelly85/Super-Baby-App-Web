using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
//using System.Data.Entity;
using System.Runtime.Serialization;
using Portal.Repository;
using Portal.Model;

namespace SuperBabyWCF
{
    public class LoginStatus : ApiController
    {
        public LoginStatus() { }

        public LoginStatus(string token)
        {

        }
        private string _token = string.Empty;
        private string _message = string.Empty;
        private bool _success = false;

        //[DataMember]
        public bool Success
        {
            get { return _success; }
            set { _success = value; }
        }

        //[DataMember]
        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        //[DataMember]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        /// <summary>
        /// Return Token Object with value
        /// </summary>
        /// <param name="pUseId"></param>
        /// <returns></returns>

        public Token GetUserToken(string PEmail)
        {
            Token objToken = new Token();

            var db = new UnitOfWork();
            User user = new User();
            //user = db.tblUsers.Get().FirstOrDefault(l => l.Email == PEmail.Trim() && (l.TokenExpireTime > DateTime.Now || l.TokenExpireTime == null));
            List<User> lstUser = db.User.Get(l => (l.Email == PEmail.Trim()) && l.TokenExpireTime > DateTime.Now).ToList();
            if (lstUser.Count > 0)
            {
                user = lstUser.FirstOrDefault();
                if (user != null)
                {
                    objToken.Email = Convert.ToString(user.Email);
                    // objToken.UserType = Convert.ToInt16(obj.UserType);
                    objToken.TokenExpiretime = Convert.ToDateTime(user.TokenExpireTime);
                    objToken.TokenValue = Convert.ToString(user.DeviceToken);
                }
            }
            return objToken;
        }


        public static Token.tbl_TokenInfo ValidateToken(string token, string userID)
        {
            var db = new UnitOfWork();
            long lngUserID = Convert.ToInt64(userID);
            List<User> lstUser = db.User.Get(s => s.Token == token && s.ID == lngUserID).ToList();
            if (lstUser.Count > 0)
            {
                User obj = lstUser.FirstOrDefault();
                if (obj != null)
                {
                    //===============Not using token expire time================
                    //if (obj.TokenExpireTime > DateTime.Now)
                    //{
                    obj.TokenExpireTime = DateTime.Now.AddMinutes(30.00);
                    obj.Token = token;
                    db.User.Update(obj);
                    db.SaveChanges();
                    return EncryptionDecryption.Encoding.DeserializeXmlString<SuperBabyWCF.Token.tbl_TokenInfo>(EncryptionDecryption.Encoding.GetDecrypt(token));
                    //}
                    //else
                    //{
                    //    return null;
                    //}
                    //===========================End============================
                }
            }
            return null;
        }


    }
}