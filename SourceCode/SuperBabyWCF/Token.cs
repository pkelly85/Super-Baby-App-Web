using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Portal.Model;
using Portal.Repository;
using System.Runtime.Serialization;

namespace SuperBabyWCF
{
    public class Token : ApiController
    {
        #region Variable  Declaration
        private string _user;
        private string _tokenvalue;
        private DateTime _tokenExpiretime;
        #endregion

        #region Constructor
        public Token() { }

        public Token(string token)
        {
            GetUserToken(token);
        }
        //public Token(IMagellanoEntities dataContext)
        //{
        //    DbContext = dataContext;
        //}
        #endregion

        #region Property

        public string User
        {
            get { return _user; }
            set { _user = value; }
        }


        public string Email
        {
            get;
            set;
        }

        public string TokenValue
        {
            get { return _tokenvalue; }
            set { _tokenvalue = value; }
        }

        public DateTime TokenExpiretime
        {
            get { return _tokenExpiretime; }
            set { _tokenExpiretime = value; }
        }

        #endregion

        #region Public method
        /// <summary>
        /// Get the Existing Token details
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string GetUserToken(string token)
        {
            //Descript Token
            return string.Empty;
        }

        /// <summary>
        /// Set the token Details
        /// </summary>
        /// <param name="token"></param>
        ///  <param name="tokenexpiretime">Token Expire Time in Minutes</param>
        /// <returns></returns>
        public string SetUserToken(tbl_TokenInfo tbluser, string tokenexpiretime)
        {
            int tokenTime = Convert.ToInt16(tokenexpiretime);
            LoginStatus objLogin = new LoginStatus();
            Token objToken = objLogin.GetUserToken(tbluser.EmailID);

            string token = EncryptionDecryption.Encoding.GetEncrypt(EncryptionDecryption.Encoding.SerializeToXmlString<tbl_TokenInfo>(tbluser));
            //string tokenID=   EncryptionDecryption.GetEncrypt(EncryptionDecryption.SerializeToXmlString<UserInfo>(userName));

            objLogin = null;

            if (!string.IsNullOrEmpty(objToken.Email))
            {
                this.TokenExpiretime = DateTime.Now.AddMinutes(tokenTime);
                this.TokenValue = token;
                //this.GUID = objToken.GUID;
                //this.User = objToken.User;
                //this.UserType = objToken.UserType;
                this.Email = objToken.Email.Trim();

                var db = new UnitOfWork();
                List<User> lstUser = db.User.Get(l => l.Email == objToken.Email).ToList();
                if (lstUser.Count > 0)
                {
                    User obj = lstUser.FirstOrDefault();
                    //tblUser obj = db.tblUsers.Get().FirstOrDefault(l => l.Token == this.TokenValue);
                    //if (obj == null)
                    //{
                    //    throw new Exception("Token To Be Fetched is not Found");
                    //}
                    //if (obj != null)
                    //{
                    obj.TokenExpireTime = this.TokenExpiretime;
                    // obj.PkServiceTokenId = this.GUID;
                    obj.Token = this.TokenValue;
                    db.User.Update(obj);
                    db.SaveChanges();
                    //}

                }
            }
            else
            {
                this.TokenExpiretime = DateTime.Now.AddMinutes(tokenTime);
                this.TokenValue = token;
                this.Email = tbluser.EmailID.Trim();

                var db = new UnitOfWork();
                List<User> lstUser = db.User.Get(l => l.Email == tbluser.EmailID).ToList();
                if (lstUser.Count > 0)
                {
                    User obj = lstUser.FirstOrDefault();
                    if (obj != null)
                    {
                        obj.TokenExpireTime = this.TokenExpiretime;
                        obj.Token = this.TokenValue;
                        db.User.Update(obj);
                        db.SaveChanges();
                    }
                }
            }
            objToken = null;
            return token;
        }


        #endregion

        [Serializable()]
        [DataContract]
        public class tbl_TokenInfo
        {
            //[DataMember]
            //public List<tbl_UserInfoToken> UserInfoToken { get; set; }

            [DataMember]
            public string EmailID { get; set; }

        }

        [Serializable()]
        [DataContract]
        public class tbl_UserInfoToken
        {
            //[DataMember]
            //public int servicecentreID { get; set; }

            [DataMember]
            public int UserID { get; set; }

        }

    }
}