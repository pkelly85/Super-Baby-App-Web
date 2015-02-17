using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Portal.Model;
using Portal.Repository;
using System.Net;
using System.Configuration;
using System.Net.Mail;
using System.Drawing;
using System.Globalization;

namespace SuperBabyWCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        public SuperBabyWCF.Token.tbl_TokenInfo objTokenInfo = new SuperBabyWCF.Token.tbl_TokenInfo();

        public void DoWork()
        {
        }

        #region Helper methods

        /// <summary>
        /// Gets the Web response context for the request being received.
        /// </summary>
        public IncomingWebRequestContext Request
        {
            get { return WebOperationContext.Current.IncomingRequest; }
        }

        /// <summary>
        /// Gets the Web request context for the request being sent.
        /// </summary>
        public OutgoingWebResponseContext Response
        {
            get { return WebOperationContext.Current.OutgoingResponse; }
        }

        #endregion

        #region RegisterUser
        /// <summary>
        /// Registered User With Checking Duplication of Email , get Registered user Detail as Response 
        /// By : TTv (Milan.G 20141217)
        /// </summary>
        public GetRegisterUserResult RegisterUser(string EmailAddress, string Password, string DeviceToken)
        {

            #region start
            GetRegisterUserResult RegisterUserResult = new GetRegisterUserResult();
            GetUserResult RegUserResult = new GetUserResult();
            ResultStatus ResultStatus = new ResultStatus();

            try
            {
                writeLog("RegisterUser", "START", EmailAddress, "0");
                //Check for unique Email address
                UnitOfWork db = new UnitOfWork();
                List<User> lstUser = new List<User>();
                User objUser = new User();
                lstUser = db.User.Get(e => e.Email.ToUpper() == EmailAddress.ToUpper()).ToList();
                if (lstUser.Count > 0) //Email is already exist
                {
                    RegUserResult.UserID = string.Empty;
                    RegUserResult.DateCreated = string.Empty;
                    RegUserResult.DateModified = string.Empty;
                    RegUserResult.EmailAddress = string.Empty;
                    RegUserResult.FacebookId = string.Empty;
                    RegUserResult.Token = string.Empty;

                    ResultStatus.Status = "0";
                    ResultStatus.StatusMessage = "Email is already exists!";
                    RegisterUserResult.ResultStatus = ResultStatus;
                    RegisterUserResult.GetUserResult = RegUserResult;
                }
                else
                {
                    User objGetLatestUser = new User();
                    // Add new User
                    objGetLatestUser = RegisterNewUser(EmailAddress, Password, DeviceToken);

                    //Prepare response
                    RegUserResult.UserID = objGetLatestUser.ID.ToString();
                    RegUserResult.DateCreated = objGetLatestUser.Date_Created.ToString("MM/dd/yyyy HH:mm:ss");
                    RegUserResult.DateModified = objGetLatestUser.Date_Modified.ToString("MM/dd/yyyy HH:mm:ss");
                    RegUserResult.EmailAddress = objGetLatestUser.Email;
                    RegUserResult.FacebookId = Convert.ToString(objGetLatestUser.Facebook_ID);
                    RegUserResult.Token = objGetLatestUser.Token;

                    //Set Token value
                    SuperBabyWCF.Token.tbl_TokenInfo tokeninfo = new SuperBabyWCF.Token.tbl_TokenInfo();
                    tokeninfo.EmailID = objGetLatestUser.Email.Trim();
                    Token objToken = new Token();
                    string token = objToken.SetUserToken(tokeninfo, "30");
                    Response.Headers.Add("token", token);
                    RegUserResult.Token = token;

                    ResultStatus.Status = "1";
                    ResultStatus.StatusMessage = "Register user successfullly!";
                    RegisterUserResult.ResultStatus = ResultStatus;
                    RegisterUserResult.GetUserResult = RegUserResult;
                }

            }
            catch (Exception ex)
            {
                RegUserResult.UserID = string.Empty;
                RegUserResult.DateCreated = string.Empty;
                RegUserResult.DateModified = string.Empty;
                RegUserResult.EmailAddress = string.Empty;
                RegUserResult.FacebookId = string.Empty;
                RegUserResult.Token = string.Empty;
                ResultStatus.Status = "0";
                ResultStatus.StatusMessage = ex.Message;
                RegisterUserResult.ResultStatus = ResultStatus;
                RegisterUserResult.GetUserResult = RegUserResult;
            }
            #endregion

            writeLog("RegisterUser", "STOP", EmailAddress, "0");
            return RegisterUserResult;
        }

        public User RegisterNewUser(string EmailAddress, string Password, string DeviceToken)
        {
            User objGetLatestUser = new User();
            try
            {
                //Register user and add new object in tblUser Table
                UnitOfWork db = new UnitOfWork();
                User objUser = new User();
                DateTime CurrLocTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                objUser.Date_Created = CurrLocTime;
                objUser.Date_Modified = CurrLocTime;
                objUser.Email = EmailAddress;
                objUser.Salt = Security.CreateSalt(8);
                string encryptedPassword = Security.GetPasswordHash(Password, objUser.Salt);
                objUser.EncryptedPassword = encryptedPassword;

                //Update device token
                List<User> lstUser = db.User.Get(m => m.DeviceToken == DeviceToken && m.ID != objUser.ID).ToList();
                if (lstUser.Count > 0)
                {
                    User userDeviceToken = new User();
                    userDeviceToken = lstUser.FirstOrDefault();
                    if (userDeviceToken != null)
                    {
                        userDeviceToken.DeviceToken = null;
                        db.User.Update(userDeviceToken);
                        db.SaveChanges();
                    }

                }
                objUser.DeviceToken = DeviceToken;
                db.User.Add(objUser);
                db.SaveChanges();

                List<User> lstLatestUser = db.User.Get(e => e.Email == EmailAddress).ToList();
                if (lstLatestUser.Count > 0)
                {
                    objGetLatestUser = lstLatestUser.FirstOrDefault();
                }

                return objGetLatestUser;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        #endregion

        #region Login
        /// <summary>
        /// checks requested credentials with USERS Object and responses back with User's Detail 
        /// By : TTv (Milan.G 20141217)
        /// </summary>
        public GetLoginResult Login(string EmailAddress, string Password, string DeviceToken)
        {
            GetUserResult RegisterUserResult = new GetUserResult();
            GetLoginResult LoginResult = new GetLoginResult();
            GetBabyResult objBabyResult = new GetBabyResult();
            ResultStatus ResultStatus = new ResultStatus();
            try
            {
                writeLog("Login", "START", EmailAddress, "0");
                //Check for unique Email address
                UnitOfWork db = new UnitOfWork();
                List<User> lstUser = db.User.Get(e => e.Email.ToUpper() == EmailAddress.ToUpper()).ToList();
                if (lstUser.Count > 0)
                {
                    User objUser = new User();
                    objUser = lstUser.FirstOrDefault();
                    if (objUser != null) //Email is already exist
                    {
                        string strsalt = objUser.Salt;
                        string strPassword = Security.GetPasswordHash(Password, strsalt);
                        if (objUser.EncryptedPassword == strPassword)
                        {
                            RegisterUserResult.UserID = Convert.ToString(objUser.ID);
                            RegisterUserResult.DateCreated = objUser.Date_Created.ToString("MM/dd/yyyy HH:mm:ss");
                            RegisterUserResult.DateModified = objUser.Date_Modified.ToString("MM/dd/yyyy HH:mm:ss");
                            RegisterUserResult.EmailAddress = objUser.Email;
                            RegisterUserResult.FacebookId = Convert.ToString(objUser.Facebook_ID);

                            RegisterUserResult.Token = objUser.Token;

                            objUser.DeviceToken = DeviceToken;
                            db.User.Update(objUser);
                            db.SaveChanges();

                            //Update device token
                            List<User> lstUserDeviceToken = db.User.Get(m => m.DeviceToken == DeviceToken && m.ID != objUser.ID).ToList();
                            if (lstUserDeviceToken.Count > 0)
                            {
                                User userDeviceToken = new User();
                                userDeviceToken = lstUserDeviceToken.FirstOrDefault();
                                if (userDeviceToken != null)
                                {
                                    userDeviceToken.DeviceToken = null;
                                    db.User.Update(userDeviceToken);
                                    db.SaveChanges();
                                }
                            }

                            //----------Set Token value--------------
                            SuperBabyWCF.Token.tbl_TokenInfo tokeninfo = new SuperBabyWCF.Token.tbl_TokenInfo();
                            tokeninfo.EmailID = objUser.Email.Trim();
                            Token objToken = new Token();
                            string token = objToken.SetUserToken(tokeninfo, "30");
                            Response.Headers.Add("token", token);
                            RegisterUserResult.Token = token;
                            //-----------------------------------------

                            #region GetBabyInformation
                            // Get Baby Information Using UserID  By Ttv (Milan.G 20141222) (start)
                            long lngUserID = Convert.ToInt64(RegisterUserResult.UserID);
                            List<Baby> lstBabyInformation = db.Baby.Get(x => x.UserID == lngUserID).ToList();
                            if (lstBabyInformation.Count > 0)
                            {
                                Baby ObjBabyInformation = lstBabyInformation.FirstOrDefault();
                                if (ObjBabyInformation != null)
                                {
                                    objBabyResult.BabyID = ObjBabyInformation.ID.ToString();
                                    objBabyResult.Birthday = ObjBabyInformation.Birthday.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.DateCreated = ObjBabyInformation.Date_Created.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.DateModified = ObjBabyInformation.Date_Modified.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.Height = Convert.ToString(ObjBabyInformation.Height);
                                    objBabyResult.ImageURL = ObjBabyInformation.ImageURL;
                                    objBabyResult.Name = ObjBabyInformation.Name;
                                    objBabyResult.UserID = ObjBabyInformation.UserID.ToString();
                                    objBabyResult.WeightOunces = Convert.ToString(ObjBabyInformation.WeightOunces);
                                    objBabyResult.WeightPounds = Convert.ToString(ObjBabyInformation.WeightPounds);
                                }
                            }
                            // Get Baby Information Using UserID  By Ttv (Milan.G 20141222) (End)
                            #endregion

                            LoginResult.GetUserResult = RegisterUserResult;
                            LoginResult.BabyInformation = objBabyResult;
                            ResultStatus.Status = "1";
                            ResultStatus.StatusMessage = "";
                            LoginResult.ResultStatus = ResultStatus;
                        }
                        else
                        {
                            //Prepare response                       
                            RegisterUserResult.UserID = string.Empty;
                            RegisterUserResult.DateCreated = string.Empty;
                            RegisterUserResult.DateModified = string.Empty;
                            RegisterUserResult.EmailAddress = string.Empty;
                            RegisterUserResult.FacebookId = string.Empty;
                            RegisterUserResult.Token = string.Empty;
                            ResultStatus.Status = "0";
                            LoginResult.GetUserResult = RegisterUserResult;
                            ResultStatus.StatusMessage = "Incorrect Password";
                            LoginResult.ResultStatus = ResultStatus;
                            objBabyResult.BabyID = string.Empty;
                            objBabyResult.Birthday = string.Empty;
                            objBabyResult.DateCreated = string.Empty;
                            objBabyResult.DateModified = string.Empty;
                            objBabyResult.Height = string.Empty;
                            objBabyResult.ImageURL = string.Empty;
                            objBabyResult.Name = string.Empty;
                            objBabyResult.UserID = string.Empty;
                            objBabyResult.WeightOunces = string.Empty;
                            objBabyResult.WeightPounds = string.Empty;
                            LoginResult.BabyInformation = objBabyResult;
                        }
                    }
                    else
                    {
                        //Prepare response                    
                        RegisterUserResult.UserID = string.Empty;
                        RegisterUserResult.DateCreated = string.Empty;
                        RegisterUserResult.DateModified = string.Empty;
                        RegisterUserResult.EmailAddress = string.Empty;
                        RegisterUserResult.FacebookId = string.Empty;
                        RegisterUserResult.Token = string.Empty;
                        LoginResult.GetUserResult = RegisterUserResult;
                        ResultStatus.Status = "0";
                        ResultStatus.StatusMessage = "Oops! This email doesn’t exist.";
                        LoginResult.ResultStatus = ResultStatus;
                        objBabyResult.BabyID = string.Empty;
                        objBabyResult.Birthday = string.Empty;
                        objBabyResult.DateCreated = string.Empty;
                        objBabyResult.DateModified = string.Empty;
                        objBabyResult.Height = string.Empty;
                        objBabyResult.ImageURL = string.Empty;
                        objBabyResult.Name = string.Empty;
                        objBabyResult.UserID = string.Empty;
                        objBabyResult.WeightOunces = string.Empty;
                        objBabyResult.WeightPounds = string.Empty;
                        LoginResult.BabyInformation = objBabyResult;

                    }
                }
                else
                {
                    //Prepare response                    
                    RegisterUserResult.UserID = string.Empty;
                    RegisterUserResult.DateCreated = string.Empty;
                    RegisterUserResult.DateModified = string.Empty;
                    RegisterUserResult.EmailAddress = string.Empty;
                    RegisterUserResult.FacebookId = string.Empty;
                    RegisterUserResult.Token = string.Empty;
                    LoginResult.GetUserResult = RegisterUserResult;
                    ResultStatus.Status = "0";
                    ResultStatus.StatusMessage = "Oops! This email doesn’t exist.";
                    LoginResult.ResultStatus = ResultStatus;
                    objBabyResult.BabyID = string.Empty;
                    objBabyResult.Birthday = string.Empty;
                    objBabyResult.DateCreated = string.Empty;
                    objBabyResult.DateModified = string.Empty;
                    objBabyResult.Height = string.Empty;
                    objBabyResult.ImageURL = string.Empty;
                    objBabyResult.Name = string.Empty;
                    objBabyResult.UserID = string.Empty;
                    objBabyResult.WeightOunces = string.Empty;
                    objBabyResult.WeightPounds = string.Empty;
                    LoginResult.BabyInformation = objBabyResult;
                }
            }
            catch (Exception ex)
            {
                RegisterUserResult.UserID = string.Empty;
                RegisterUserResult.DateCreated = string.Empty;
                RegisterUserResult.DateModified = string.Empty;
                RegisterUserResult.EmailAddress = string.Empty;
                RegisterUserResult.FacebookId = string.Empty;
                RegisterUserResult.Token = string.Empty;
                LoginResult.GetUserResult = RegisterUserResult;
                ResultStatus.Status = "0";
                ResultStatus.StatusMessage = ex.Message;
                LoginResult.ResultStatus = ResultStatus;
                objBabyResult.BabyID = string.Empty;
                objBabyResult.Birthday = string.Empty;
                objBabyResult.DateCreated = string.Empty;
                objBabyResult.DateModified = string.Empty;
                objBabyResult.Height = string.Empty;
                objBabyResult.ImageURL = string.Empty;
                objBabyResult.Name = string.Empty;
                objBabyResult.UserID = string.Empty;
                objBabyResult.WeightOunces = string.Empty;
                objBabyResult.WeightPounds = string.Empty;
                LoginResult.BabyInformation = objBabyResult;
            }
            writeLog("Login", "STOP", EmailAddress, "0");
            return LoginResult;
        }
        #endregion

        #region LoginWithFacebook
        /// <summary>
        /// checks requested credentials with USERS Object and responses back with User's Detail 
        /// By : TTv (Milan.G 20141217)
        /// </summary>
        public GetLoginResult LoginWithFacebook(string EmailAddress, string FacebookID, string DeviceToken)
        {
            GetUserResult UserResult = new GetUserResult();
            GetLoginResult LoginWithFacebookResult = new GetLoginResult();
            GetBabyResult objBabyResult = new GetBabyResult();
            ResultStatus ResultStatus = new ResultStatus();
            try
            {
                writeLog("LoginWithFacebook", "START", EmailAddress, "0");
                //Check for unique Email address
                UnitOfWork db = new UnitOfWork();
                User objUser = new User();
                List<User> lstUser = db.User.Get(e => e.Email.ToUpper() == EmailAddress.ToUpper()).ToList();
                if (lstUser.Count > 0)
                {

                    string Password = lstUser.FirstOrDefault().EncryptedPassword;
                    if (Password == string.Empty || Password == null)
                    {
                        objUser = lstUser.FirstOrDefault();

                        if (objUser.Facebook_ID == FacebookID)
                        {
                            objUser.DeviceToken = DeviceToken;
                            db.User.Update(objUser);
                            db.SaveChanges();

                            //Update device token
                            List<User> lstUserDeviceToken = db.User.Get(m => m.DeviceToken == DeviceToken && m.ID != objUser.ID).ToList();
                            if (lstUserDeviceToken.Count > 0)
                            {
                                User userDeviceToken = new User();
                                userDeviceToken = lstUserDeviceToken.FirstOrDefault();
                                if (userDeviceToken != null)
                                {
                                    userDeviceToken.DeviceToken = null;
                                    db.User.Update(userDeviceToken);
                                    db.SaveChanges();
                                }
                            }
                            //prepare response                                
                            UserResult.UserID = Convert.ToString(objUser.ID);
                            UserResult.DateCreated = objUser.Date_Created.ToString("MM/dd/yyyy HH:mm:ss");
                            UserResult.DateModified = objUser.Date_Modified.ToString("MM/dd/yyyy HH:mm:ss");
                            UserResult.EmailAddress = objUser.Email;
                            UserResult.FacebookId = Convert.ToString(objUser.Facebook_ID);

                            UserResult.Token = objUser.Token;

                            //Set Token value
                            SuperBabyWCF.Token.tbl_TokenInfo tokeninfo = new SuperBabyWCF.Token.tbl_TokenInfo();
                            tokeninfo.EmailID = objUser.Email.Trim();
                            Token objToken = new Token();
                            string token = objToken.SetUserToken(tokeninfo, "30");
                            Response.Headers.Add("token", token);
                            UserResult.Token = token;

                            #region GetBabyInformation
                            // Get Baby Information Using UserID  By Ttv (Milan.G 20141222) (start)
                            long lngUserID = Convert.ToInt64(UserResult.UserID);
                            List<Baby> lstBabyInformation = db.Baby.Get(x => x.UserID == lngUserID).ToList();
                            if (lstBabyInformation.Count > 0)
                            {
                                Baby ObjBabyInformation = lstBabyInformation.FirstOrDefault();
                                if (ObjBabyInformation != null)
                                {
                                    objBabyResult.BabyID = ObjBabyInformation.ID.ToString();
                                    objBabyResult.Birthday = ObjBabyInformation.Birthday.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.DateCreated = ObjBabyInformation.Date_Created.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.DateModified = ObjBabyInformation.Date_Modified.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.Height = Convert.ToString(ObjBabyInformation.Height);
                                    objBabyResult.ImageURL = ObjBabyInformation.ImageURL;
                                    objBabyResult.Name = ObjBabyInformation.Name;
                                    objBabyResult.UserID = ObjBabyInformation.UserID.ToString();
                                    objBabyResult.WeightOunces = Convert.ToString(ObjBabyInformation.WeightOunces);
                                    objBabyResult.WeightPounds = Convert.ToString(ObjBabyInformation.WeightPounds);
                                }
                            }
                            // Get Baby Information Using UserID  By Ttv (Milan.G 20141222) (End)
                            #endregion

                            LoginWithFacebookResult.GetUserResult = UserResult;
                            ResultStatus.Status = "1";
                            ResultStatus.StatusMessage = "";
                            LoginWithFacebookResult.ResultStatus = ResultStatus;
                            LoginWithFacebookResult.BabyInformation = objBabyResult;
                        }
                        else
                        {
                            //error response                               
                            UserResult.UserID = string.Empty;
                            UserResult.DateCreated = string.Empty;
                            UserResult.DateModified = string.Empty;
                            UserResult.EmailAddress = string.Empty;
                            UserResult.FacebookId = string.Empty;
                            UserResult.Token = string.Empty;
                            LoginWithFacebookResult.GetUserResult = UserResult;
                            ResultStatus.Status = "0";
                            ResultStatus.StatusMessage = "Username Or password are not correct!";
                            LoginWithFacebookResult.ResultStatus = ResultStatus;
                            objBabyResult.BabyID = string.Empty;
                            objBabyResult.Birthday = string.Empty;
                            objBabyResult.DateCreated = string.Empty;
                            objBabyResult.DateModified = string.Empty;
                            objBabyResult.Height = string.Empty;
                            objBabyResult.ImageURL = string.Empty;
                            objBabyResult.Name = string.Empty;
                            objBabyResult.UserID = string.Empty;
                            objBabyResult.WeightOunces = string.Empty;
                            objBabyResult.WeightPounds = string.Empty;
                            LoginWithFacebookResult.BabyInformation = objBabyResult;
                        }
                    }
                    else
                    {
                        //error response                               
                        UserResult.UserID = string.Empty;
                        UserResult.DateCreated = string.Empty;
                        UserResult.DateModified = string.Empty;
                        UserResult.EmailAddress = string.Empty;
                        UserResult.FacebookId = string.Empty;
                        UserResult.Token = string.Empty;
                        LoginWithFacebookResult.GetUserResult = UserResult;
                        ResultStatus.Status = "0";
                        ResultStatus.StatusMessage = "User already exist!";
                        LoginWithFacebookResult.ResultStatus = ResultStatus;
                        objBabyResult.BabyID = string.Empty;
                        objBabyResult.Birthday = string.Empty;
                        objBabyResult.DateCreated = string.Empty;
                        objBabyResult.DateModified = string.Empty;
                        objBabyResult.Height = string.Empty;
                        objBabyResult.ImageURL = string.Empty;
                        objBabyResult.Name = string.Empty;
                        objBabyResult.UserID = string.Empty;
                        objBabyResult.WeightOunces = string.Empty;
                        objBabyResult.WeightPounds = string.Empty;
                        LoginWithFacebookResult.BabyInformation = objBabyResult;
                    }

                }
                else
                {
                    //add new record with empty password and set facebookid                  
                    User objUserNew = new User();
                    DateTime CurrLocTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                    objUserNew.Date_Created = CurrLocTime;
                    objUserNew.Date_Modified = CurrLocTime;
                    objUserNew.Email = EmailAddress;
                    objUserNew.Facebook_ID = FacebookID;
                    objUserNew.DeviceToken = DeviceToken;

                    db.User.Add(objUserNew);
                    db.SaveChanges();

                    //Update device token
                    User userDeviceToken = new User();
                    List<User> lstUserDeviceToken = db.User.Get(m => m.DeviceToken == DeviceToken && m.ID != objUserNew.ID).ToList();
                    if (lstUserDeviceToken.Count > 0)
                    {
                        userDeviceToken = lstUserDeviceToken.FirstOrDefault();
                        if (userDeviceToken != null)
                        {
                            userDeviceToken.DeviceToken = null;
                            db.User.Update(userDeviceToken);
                            db.SaveChanges();
                        }
                    }

                    //Set Token value
                    SuperBabyWCF.Token.tbl_TokenInfo tokeninfo = new SuperBabyWCF.Token.tbl_TokenInfo();
                    tokeninfo.EmailID = objUserNew.Email.Trim();
                    Token objToken = new Token();
                    string token = objToken.SetUserToken(tokeninfo, "30");
                    Response.Headers.Add("token", token);
                    UserResult.Token = token;

                    //response Prepare Result
                    List<User> lstLatestUser = db.User.Get(e => e.Email == EmailAddress).ToList();
                    if (lstLatestUser.Count > 0)
                    {
                        User objGetLatestUser = new User();
                        objGetLatestUser = lstLatestUser.FirstOrDefault();
                        UserResult.UserID = objGetLatestUser.ID.ToString();
                        UserResult.DateCreated = objGetLatestUser.Date_Created.ToString("MM/dd/yyyy HH:mm:ss");
                        UserResult.DateModified = objGetLatestUser.Date_Modified.ToString("MM/dd/yyyy HH:mm:ss");
                        UserResult.EmailAddress = objGetLatestUser.Email;
                        UserResult.FacebookId = Convert.ToString(objGetLatestUser.Facebook_ID);

                        ResultStatus.Status = "1";
                        ResultStatus.StatusMessage = "Register user successfullly!";
                        LoginWithFacebookResult.ResultStatus = ResultStatus;
                        LoginWithFacebookResult.GetUserResult = UserResult;
                        objBabyResult.BabyID = string.Empty;
                        objBabyResult.Birthday = string.Empty;
                        objBabyResult.DateCreated = string.Empty;
                        objBabyResult.DateModified = string.Empty;
                        objBabyResult.Height = string.Empty;
                        objBabyResult.ImageURL = string.Empty;
                        objBabyResult.Name = string.Empty;
                        objBabyResult.UserID = string.Empty;
                        objBabyResult.WeightOunces = string.Empty;
                        objBabyResult.WeightPounds = string.Empty;
                        LoginWithFacebookResult.BabyInformation = objBabyResult;
                    }
                }
            }

            catch (Exception ex)
            {
                UserResult.UserID = string.Empty;
                UserResult.DateCreated = string.Empty;
                UserResult.DateModified = string.Empty;
                UserResult.EmailAddress = string.Empty;
                UserResult.FacebookId = string.Empty;
                UserResult.Token = string.Empty;
                LoginWithFacebookResult.GetUserResult = UserResult;
                ResultStatus.Status = "0";
                ResultStatus.StatusMessage = ex.Message;
                LoginWithFacebookResult.ResultStatus = ResultStatus;
                objBabyResult.BabyID = string.Empty;
                objBabyResult.Birthday = string.Empty;
                objBabyResult.DateCreated = string.Empty;
                objBabyResult.DateModified = string.Empty;
                objBabyResult.Height = string.Empty;
                objBabyResult.ImageURL = string.Empty;
                objBabyResult.Name = string.Empty;
                objBabyResult.UserID = string.Empty;
                objBabyResult.WeightOunces = string.Empty;
                objBabyResult.WeightPounds = string.Empty;
                LoginWithFacebookResult.BabyInformation = objBabyResult;
            }
            writeLog("LoginWithFacebook", "STOP", EmailAddress, "0");
            return LoginWithFacebookResult;
        }
        #endregion

        #region AddEditBabyInfo
        /// <summary>
        /// AddEdit Baby Information By UserID. if record is not Exits then Add new record else Update record 
        /// By : TTv (Milan.G 20141217)
        /// </summary>
        public AddEditBabyInfoResult AddEditBabyInfo(string UserID, string UserToken, string Name, string Birthday, string WeightPounds, string WeightOunces, string Height, string ImageData)
        {
            AddEditBabyInfoResult objAddEditBabyResult = new AddEditBabyInfoResult();
            GetBabyResult objBabyResult = new GetBabyResult();
            ResultStatus ResultStatus = new ResultStatus();

            objTokenInfo = LoginStatus.ValidateToken(UserToken, UserID);
            try
            {
                writeLog("AddEditBabyInfo", "START", UserID, "0");
                var db = new UnitOfWork();
                if (objTokenInfo != null && objTokenInfo.EmailID != null)
                {
                    long lngUserID = Convert.ToInt64(UserID);

                    List<User> lstUser = db.User.Get(m => m.ID == lngUserID).ToList();
                    if (lstUser.Count > 0)
                    {
                        User objUser = new User();
                        objUser = lstUser.FirstOrDefault();
                        if (objUser != null)
                        {
                            List<Baby> lstBaby = db.Baby.Get(x => x.UserID == lngUserID).ToList();
                            if (lstBaby.Count > 0)
                            {
                                Baby objBaby = lstBaby.FirstOrDefault();
                                if (objBaby != null)
                                {
                                    double lngWeightPounds = 0;
                                    double lngWeightOunces = 0;
                                    double lngHeight = 0;

                                    if (WeightPounds.Length > 0)
                                        lngWeightPounds = Convert.ToDouble(WeightPounds);

                                    if (WeightOunces.Length > 0)
                                        lngWeightOunces = Convert.ToDouble(WeightOunces);

                                    if (Height.Length > 0)
                                        lngHeight = Convert.ToDouble(Height);

                                    bool isWeightUpdates = false;
                                    bool isHeightUpdates = false;

                                    if (lngWeightPounds != objBaby.WeightPounds || lngWeightOunces != objBaby.WeightOunces)
                                        isWeightUpdates = true;

                                    if (lngHeight != objBaby.Height)
                                        isHeightUpdates = true;

                                    //Update Baby Information
                                    objBaby.Date_Modified = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                                    objBaby.UserID = lngUserID;
                                    objBaby.Name = Name;
                                    objBaby.Birthday = DateTime.ParseExact(Birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture); // Format Change As per discussion By Ttv(Milan.G 20141223)
                                    objBaby.WeightPounds = lngWeightPounds;
                                    objBaby.WeightOunces = lngWeightOunces;
                                    objBaby.Height = lngHeight;

                                    //3. If ImageData  length > 0
                                    string filePath = "";
                                    if (ImageData.Length > 0)
                                    {
                                        if (objBaby.ImageURL != null && objBaby.ImageURL != "")
                                        {
                                            string Image_Url = objBaby.ImageURL;
                                            objBaby.ImageURL = null;
                                            //============delete image from database======//
                                            DeleteImage(Image_Url);
                                            //=============================================                              
                                        }

                                        //============code for save image=============//
                                        filePath = SaveImage(ImageData, "Profile", 100, 100);
                                        //=============================================
                                        objBaby.ImageURL = filePath;
                                    }

                                    db.Baby.Update(objBaby);
                                    db.SaveChanges();

                                    // ---------------------------- Create TimeLine Entry if required

                                    if (isWeightUpdates)
                                    {
                                        TimelineEntry objNewTimeLine = new TimelineEntry();
                                        objNewTimeLine.Date_Created = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                                        objNewTimeLine.Message = objBaby.Name + " is " + objBaby.WeightPounds.ToString() + " pounds " + objBaby.WeightOunces.ToString() + " ounces";
                                        objNewTimeLine.MilestoneID = 0;
                                        objNewTimeLine.TypeID = 1;
                                        objNewTimeLine.UserID = lngUserID;
                                        objNewTimeLine.VideoID = 0;
                                        db.TimelineEntry.Add(objNewTimeLine);
                                        db.SaveChanges();
                                    }

                                    if (isHeightUpdates)
                                    {
                                        TimelineEntry objNewTimeLine = new TimelineEntry();
                                        objNewTimeLine.Date_Created = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                                        objNewTimeLine.Message = objBaby.Name + " is " + objBaby.Height.ToString() + " inches tall ";
                                        objNewTimeLine.MilestoneID = 0;
                                        objNewTimeLine.TypeID = 2;
                                        objNewTimeLine.UserID = lngUserID;
                                        objNewTimeLine.VideoID = 0;
                                        db.TimelineEntry.Add(objNewTimeLine);
                                        db.SaveChanges();
                                    }

                                    //

                                    //response Prepare Result
                                    List<Baby> lstlatesBaby = db.Baby.Get(e => e.UserID == lngUserID).ToList();
                                    if (lstlatesBaby.Count > 0)
                                    {
                                        Baby objLatesBaby = new Baby();
                                        objLatesBaby = lstlatesBaby.FirstOrDefault();
                                        objBabyResult.BabyID = objLatesBaby.ID.ToString();
                                        objBabyResult.DateCreated = objLatesBaby.Date_Created.ToString("MM/dd/yyyy HH:mm:ss");
                                        objBabyResult.DateModified = objLatesBaby.Date_Modified.ToString("MM/dd/yyyy HH:mm:ss");
                                        objBabyResult.UserID = objLatesBaby.UserID.ToString();
                                        objBabyResult.Name = objLatesBaby.Name.ToString();
                                        objBabyResult.Birthday = objLatesBaby.Birthday.ToString("MM/dd/yyyy HH:mm:ss");
                                        objBabyResult.WeightPounds = objLatesBaby.WeightPounds.ToString();
                                        objBabyResult.WeightOunces = objLatesBaby.WeightOunces.ToString();
                                        objBabyResult.Height = objLatesBaby.Height.ToString();
                                        objBabyResult.ImageURL = Convert.ToString(objLatesBaby.ImageURL);

                                        ResultStatus.Status = "1";
                                        ResultStatus.StatusMessage = "Record Updated successfullly!";
                                        objAddEditBabyResult.ResultStatus = ResultStatus;
                                        objAddEditBabyResult.GetBabyResult = objBabyResult;
                                    }
                                }
                            }
                            else
                            {
                                // Add New Baby Information
                                Baby ObjBabyNew = new Baby();
                                DateTime CurrLocTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                                ObjBabyNew.Date_Created = CurrLocTime;
                                ObjBabyNew.Date_Modified = CurrLocTime;
                                ObjBabyNew.UserID = lngUserID;
                                ObjBabyNew.Name = Name;
                                ObjBabyNew.Birthday = DateTime.ParseExact(Birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture); // Format Change As per discussion By Ttv(Milan.G 20141223)
                                ObjBabyNew.WeightPounds = WeightPounds == string.Empty ? 0 : Convert.ToDouble(WeightPounds);
                                ObjBabyNew.WeightOunces = WeightOunces == string.Empty ? 0 : Convert.ToDouble(WeightOunces);
                                ObjBabyNew.Height = Height == string.Empty ? 0 : Convert.ToDouble(Height);

                                string filePath = "";
                                if (ImageData.Length > 0)
                                {
                                    //============code for save image=============//
                                    filePath = SaveImage(ImageData, "Profile", 100, 100);
                                    //=============================================
                                    ObjBabyNew.ImageURL = filePath;
                                }

                                db.Baby.Add(ObjBabyNew);
                                db.SaveChanges();

                                //response Prepare Result
                                List<Baby> lstlatesBaby = db.Baby.Get(e => e.UserID == lngUserID).ToList();
                                if (lstlatesBaby.Count > 0)
                                {
                                    Baby objLatesBaby = new Baby();
                                    objLatesBaby = lstlatesBaby.FirstOrDefault();
                                    objBabyResult.BabyID = objLatesBaby.ID.ToString();
                                    objBabyResult.DateCreated = objLatesBaby.Date_Created.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.DateModified = objLatesBaby.Date_Modified.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.UserID = objLatesBaby.UserID.ToString();
                                    objBabyResult.Name = objLatesBaby.Name.ToString();
                                    objBabyResult.Birthday = objLatesBaby.Birthday.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.WeightPounds = objLatesBaby.WeightPounds.ToString();
                                    objBabyResult.WeightOunces = objLatesBaby.WeightOunces.ToString();
                                    objBabyResult.Height = objLatesBaby.Height.ToString();
                                    objBabyResult.ImageURL = Convert.ToString(objLatesBaby.ImageURL);

                                    ResultStatus.Status = "1";
                                    ResultStatus.StatusMessage = "Register Baby successfullly!";
                                    objAddEditBabyResult.ResultStatus = ResultStatus;
                                    objAddEditBabyResult.GetBabyResult = objBabyResult;
                                }
                            }
                        }
                    }
                    else
                    {
                        //response Prepare Result
                        ResultStatus.Status = "0";
                        ResultStatus.StatusMessage = "User not exist!";
                        objAddEditBabyResult.ResultStatus = ResultStatus;
                        objAddEditBabyResult.GetBabyResult = objBabyResult;
                    }
                }
                else
                {
                    throw new WebFaultException<string>("Please enter validate token.", HttpStatusCode.Unauthorized);
                }
            }
            catch (Exception ex)
            {
                ResultStatus.Status = "0";
                ResultStatus.StatusMessage = ex.Message;
                objAddEditBabyResult.ResultStatus = ResultStatus;
                objAddEditBabyResult.GetBabyResult = objBabyResult;
            }

            writeLog("AddEditBabyInfo", "STOP", UserID, "0");
            return objAddEditBabyResult;
        }

        #endregion

        #region GetBabyInformation
        /// <summary>
        /// GetBaby Information By UserID
        /// By : TTv (Milan.G 20141217)
        /// </summary>
        public GetBabyInformation GetBabyInformation(string UserID, string UserToken)
        {
            GetBabyInformation objGetBabyInformationResult = new GetBabyInformation();
            GetBabyResult objBabyResult = new GetBabyResult();
            ResultStatus ResultStatus = new ResultStatus();

            objTokenInfo = LoginStatus.ValidateToken(UserToken, UserID);
            try
            {
                writeLog("GetBabyInformation", "START", UserID, "0");
                var db = new UnitOfWork();
                if (objTokenInfo != null && objTokenInfo.EmailID != null)
                {
                    long lngUserID = Convert.ToInt64(UserID);

                    List<User> lstUser = db.User.Get(m => m.ID == lngUserID).ToList();
                    if (lstUser.Count > 0)
                    {
                        User objUser = new User();
                        objUser = lstUser.FirstOrDefault();
                        if (objUser != null)
                        {
                            List<Baby> lstBaby = db.Baby.Get(x => x.UserID == lngUserID).ToList();
                            if (lstBaby.Count > 0)
                            {
                                Baby objBaby = lstBaby.FirstOrDefault();
                                if (objBaby != null)
                                {
                                    // GetBaby Information
                                    objBabyResult.BabyID = objBaby.ID.ToString();
                                    objBabyResult.DateCreated = objBaby.Date_Created.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.DateModified = objBaby.Date_Modified.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.UserID = objBaby.UserID.ToString();
                                    objBabyResult.Name = objBaby.Name.ToString();
                                    objBabyResult.Birthday = objBaby.Birthday.ToString("MM/dd/yyyy HH:mm:ss");
                                    objBabyResult.WeightPounds = objBaby.WeightPounds.ToString();
                                    objBabyResult.WeightOunces = objBaby.WeightOunces.ToString();
                                    objBabyResult.Height = objBaby.Height.ToString();
                                    objBabyResult.ImageURL = Convert.ToString(objBaby.ImageURL);

                                    ResultStatus.Status = "1";
                                    ResultStatus.StatusMessage = "";
                                    objGetBabyInformationResult.ResultStatus = ResultStatus;
                                    objGetBabyInformationResult.GetBabyResult = objBabyResult;
                                }
                                else
                                {
                                    //response Prepare Result
                                    ResultStatus.Status = "0";
                                    ResultStatus.StatusMessage = "Baby information not found!";
                                    objGetBabyInformationResult.ResultStatus = ResultStatus;
                                    objGetBabyInformationResult.GetBabyResult = objBabyResult;
                                }
                            }
                            else
                            {
                                //response Prepare Result
                                ResultStatus.Status = "0";
                                ResultStatus.StatusMessage = "Baby information not found!";
                                objGetBabyInformationResult.ResultStatus = ResultStatus;
                                objGetBabyInformationResult.GetBabyResult = objBabyResult;
                            }
                        }
                        else
                        {
                            //response Prepare Result
                            ResultStatus.Status = "0";
                            ResultStatus.StatusMessage = "User information not found!";
                            objGetBabyInformationResult.ResultStatus = ResultStatus;
                            objGetBabyInformationResult.GetBabyResult = objBabyResult;
                        }
                    }
                    else
                    {
                        //response Prepare Result
                        ResultStatus.Status = "0";
                        ResultStatus.StatusMessage = "User information not found!";
                        objGetBabyInformationResult.ResultStatus = ResultStatus;
                        objGetBabyInformationResult.GetBabyResult = objBabyResult;
                    }
                }
                else
                {
                    throw new WebFaultException<string>("Please enter validate token.", HttpStatusCode.Unauthorized);
                }
            }
            catch (Exception ex)
            {
                ResultStatus.Status = "0";
                ResultStatus.StatusMessage = ex.Message;
                objGetBabyInformationResult.ResultStatus = ResultStatus;
                objGetBabyInformationResult.GetBabyResult = objBabyResult;
            }

            writeLog("GetBabyInformation", "STOP", UserID, "0");
            return objGetBabyInformationResult;
        }
        #endregion

        #region UpdateAccountInfo
        /// <summary>
        /// Update Account Information With preventing Duplication of EmailID
        /// By : TTv (Milan.G 20141217)
        /// </summary>
        public UpdateAccountInfo UpdateAccountInfo(string UserID, string UserToken, string Email, string Password)
        {
            UpdateAccountInfo UpdateAccountInfoResult = new UpdateAccountInfo();
            GetUserResult objUserResult = new GetUserResult();
            ResultStatus ResultStatus = new ResultStatus();
            objTokenInfo = LoginStatus.ValidateToken(UserToken, UserID);
            try
            {
                writeLog("UpdateAccountInfo", "START", UserID, "0");
                var db = new UnitOfWork();
                if (objTokenInfo != null && objTokenInfo.EmailID != null)
                {
                    long lngUserID = Convert.ToInt64(UserID);
                    List<User> lstUser = db.User.Get(m => m.ID == lngUserID).ToList();
                    if (lstUser.Count > 0)
                    {
                        User objUser = new User();
                        objUser = lstUser.FirstOrDefault();
                        if (objUser != null)
                        {
                            List<User> lstCheckEmail = db.User.Get(n => n.Email == Email && n.ID != lngUserID).ToList();
                            if (lstCheckEmail.Count == 0)
                            {
                                User objCheckEmail = lstCheckEmail.FirstOrDefault();
                                if (objCheckEmail == null)
                                {
                                    //1. Update Email
                                    objUser.Email = Email;

                                    //2. requested Password length > 0, update requested password in USERS object for requested UserID.
                                    if (Password.Length > 0)
                                    {
                                        string encryptedPassword = Security.GetPasswordHash(Password, objUser.Salt);
                                        objUser.EncryptedPassword = encryptedPassword;
                                    }

                                    objUser.Date_Modified = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                                    db.User.Update(objUser);
                                    db.SaveChanges();

                                    List<User> lstUpdatedUser = db.User.Get(n => n.ID == lngUserID).ToList();
                                    if (lstUpdatedUser.Count > 0)
                                    {
                                        User objGetUpdatedUser = new User();
                                        objGetUpdatedUser = lstUpdatedUser.FirstOrDefault();
                                        objUserResult.UserID = objGetUpdatedUser.ID.ToString();
                                        objUserResult.DateCreated = objGetUpdatedUser.Date_Created.ToString("MM/dd/yyyy HH:mm:ss");
                                        objUserResult.DateModified = objGetUpdatedUser.Date_Modified.ToString("MM/dd/yyyy HH:mm:ss");
                                        objUserResult.EmailAddress = objGetUpdatedUser.Email;
                                        objUserResult.FacebookId = Convert.ToString(objGetUpdatedUser.Facebook_ID);
                                        objUserResult.Token = objGetUpdatedUser.Token;

                                        //response Prepare Result
                                        ResultStatus.Status = "1";
                                        ResultStatus.StatusMessage = "Record Updated Successfully!";
                                        UpdateAccountInfoResult.ResultStatus = ResultStatus;
                                        UpdateAccountInfoResult.GetUserResult = objUserResult;
                                    }
                                }
                            }
                            else
                            {
                                //response Prepare Result
                                ResultStatus.Status = "0";
                                ResultStatus.StatusMessage = "Email is already exists!";
                                UpdateAccountInfoResult.ResultStatus = ResultStatus;
                                UpdateAccountInfoResult.GetUserResult = objUserResult;
                            }
                        }
                    }
                    else
                    {
                        //response Prepare Result
                        ResultStatus.Status = "0";
                        ResultStatus.StatusMessage = "User not exist!";
                        UpdateAccountInfoResult.ResultStatus = ResultStatus;
                        UpdateAccountInfoResult.GetUserResult = objUserResult;
                    }
                }
                else
                {
                    throw new WebFaultException<string>("Please enter validate token.", HttpStatusCode.Unauthorized);
                }
            }
            catch (Exception ex)
            {
                ResultStatus.Status = "0";
                ResultStatus.StatusMessage = ex.Message;
                UpdateAccountInfoResult.ResultStatus = ResultStatus;
                UpdateAccountInfoResult.GetUserResult = objUserResult;

            }
            writeLog("UpdateAccountInfo", "STOP", UserID, "0");
            return UpdateAccountInfoResult;
        }
        #endregion

        #region AddTimeline
        /// <summary>
        /// Add Record in TimelineEntry Table 
        /// By : TTv (Milan.G 20141217)
        /// </summary>
        public AddTimeline AddTimeline(string UserID, string UserToken, string Type, string Message, string MilestoneID, string VideoID, string CompletedStatus)
        {
            /* Type detail
            0 - Milestone complete
            1 - Update Weight
            2 - Update Height
            3 - Video Watched 
            */

            AddTimeline AddTimelineResult = new AddTimeline();
            ResultStatus ResultStatus = new ResultStatus();
            objTokenInfo = LoginStatus.ValidateToken(UserToken, UserID);
            try
            {
                writeLog("AddTimeline", "START", UserID, "0");
                var db = new UnitOfWork();
                if (objTokenInfo != null && objTokenInfo.EmailID != null)
                {
                    long lngUserID = Convert.ToInt64(UserID);
                    int intCompletedStatus = 1;
                    long lngMilestoneID = 0;
                    long lngVideoID = 0;
                    int intType = Convert.ToInt32(Type);

                    if (CompletedStatus != null && CompletedStatus.Length > 0 ) intCompletedStatus = Convert.ToInt16(CompletedStatus);
                    if (MilestoneID != null && MilestoneID.Length > 0) lngMilestoneID = Convert.ToInt64(MilestoneID);
                    if (VideoID != null && VideoID.Length > 0) lngVideoID = Convert.ToInt64(VideoID);

                    List<User> lstUser = db.User.Get(m => m.ID == lngUserID).ToList();
                    if (lstUser.Count > 0)
                    {
                        User objUser = new User();
                        objUser = lstUser.FirstOrDefault();
                        if (objUser != null) //Complete milestone
                        {
                            if (intCompletedStatus == 1)
                            {
                                // Add TimelineEntry Data
                                TimelineEntry objTimelineEntry = new TimelineEntry();
                                objTimelineEntry.UserID = lngUserID;
                                objTimelineEntry.Date_Created = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                                objTimelineEntry.TypeID = intType;
                                objTimelineEntry.Message = Message;
                                objTimelineEntry.MilestoneID = lngMilestoneID;
                                objTimelineEntry.VideoID = lngVideoID;
                                objTimelineEntry.CompletedStatus = intCompletedStatus;

                                db.TimelineEntry.Add(objTimelineEntry);
                                db.SaveChanges();
                            }
                            else if (intCompletedStatus == 0)
                            {
                                if (intType == 0 || intType == 4) //Milestone complete
                                {
                                    List<TimelineEntry> lstTimeLineEntry = db.TimelineEntry.Get(n => n.TypeID == intType && n.MilestoneID == lngMilestoneID).ToList();
                                    if (lstTimeLineEntry.Count > 0)
                                    {
                                        foreach (TimelineEntry objTimeLineEntry in lstTimeLineEntry)
                                        {
                                            objTimeLineEntry.CompletedStatus = intCompletedStatus;
                                            db.TimelineEntry.Update(objTimeLineEntry);

                                            db.SaveChanges();
                                        }
                                    }

                                }
                            }
                            ResultStatus.Status = "1";
                            ResultStatus.StatusMessage = "Record Save Successfully";
                            AddTimelineResult.ResultStatus = ResultStatus;
                        }

                    }
                    else
                    {
                        //response Prepare Result
                        ResultStatus.Status = "0";
                        ResultStatus.StatusMessage = "User not exist!";
                        AddTimelineResult.ResultStatus = ResultStatus;
                    }
                }
                else
                {
                    throw new WebFaultException<string>("Please enter validate token.", HttpStatusCode.Unauthorized);
                }
            }
            catch (Exception ex)
            {
                ResultStatus.Status = "0";
                ResultStatus.StatusMessage = ex.Message;
                AddTimelineResult.ResultStatus = ResultStatus;
            }

            writeLog("AddTimeline", "STOP", UserID, "0");
            return AddTimelineResult;
        }
        #endregion

        #region GetTimeline
        /// <summary>
        /// Get List of TimelineEntry record by userID
        /// By : TTv (Milan.G 20141217)
        /// </summary>
        public GetTimelineResult GetTimeline(string UserID, string UserToken, string PageNumber)
        {
            GetTimelineResult objGetTimelineResult = new GetTimelineResult();
            List<GetTimelineEntryResult> objlstTimelineEntryResult = new List<GetTimelineEntryResult>();
            ResultStatus ResultStatus = new ResultStatus();

            objTokenInfo = LoginStatus.ValidateToken(UserToken, UserID);
            try
            {
                writeLog("GetTimeline", "START", UserID, "0");
                var db = new UnitOfWork();
                if (objTokenInfo != null && objTokenInfo.EmailID != null)
                {
                    List<TimelineEntry> objTimelineEntries = new List<TimelineEntry>();

                    long lngUserID = Convert.ToInt64(UserID);
                    objTimelineEntries = db.TimelineEntry.Get(n => n.UserID == lngUserID).OrderByDescending(n=>n.ID).ToList();

                    if (objTimelineEntries.Count > 0)
                    {
                        foreach (var objTimelineEntry in objTimelineEntries)
                        {
                            // Get List of TimelineEntry Details
                            GetTimelineEntryResult objTimelineEntryResult = new GetTimelineEntryResult();
                            objTimelineEntryResult.TimelineID = objTimelineEntry.ID.ToString();
                            objTimelineEntryResult.UserID = objTimelineEntry.UserID.ToString();
                            objTimelineEntryResult.TypeID = objTimelineEntry.TypeID.ToString();
                            objTimelineEntryResult.Message = objTimelineEntry.Message.ToString();
                            objTimelineEntryResult.DateCreated = objTimelineEntry.Date_Created.ToString("MM/dd/yyyy HH:mm:ss");
                            objTimelineEntryResult.MilestoneID = Convert.ToString(objTimelineEntry.MilestoneID);
                            objTimelineEntryResult.VideoID = Convert.ToString(objTimelineEntry.VideoID);
                            objlstTimelineEntryResult.Add(objTimelineEntryResult);
                        }

                        //Paging parameters
                        int pagesize = 10;
                        int currentpage = Convert.ToInt32(PageNumber);
                        int currentsize = pagesize;
                        int skipcount = currentsize * (currentpage - 1);
                        int takecount = currentsize * currentpage;

                        if (objlstTimelineEntryResult.Count > 0)
                        {
                            objlstTimelineEntryResult = objlstTimelineEntryResult.Take(takecount).Skip(skipcount).ToList();
                            if (objlstTimelineEntryResult.Count > 0)
                            {
                                ResultStatus.Status = "1";
                                ResultStatus.StatusMessage = "";
                                objGetTimelineResult.ResultStatus = ResultStatus;
                                objGetTimelineResult.GetTimelineEntryResult = objlstTimelineEntryResult;
                            }
                            else
                            {
                                //response Prepare Result
                                ResultStatus.Status = "1";
                                ResultStatus.StatusMessage = "No TimelineData On this PageNumber!";
                                objGetTimelineResult.ResultStatus = ResultStatus;
                                objGetTimelineResult.GetTimelineEntryResult = objlstTimelineEntryResult;
                            }
                        }
                        else
                        {
                            //response Prepare Result
                            ResultStatus.Status = "1";
                            ResultStatus.StatusMessage = "No TimelineData Exist!";
                            objGetTimelineResult.ResultStatus = ResultStatus;
                            objGetTimelineResult.GetTimelineEntryResult = objlstTimelineEntryResult;
                        }
                    }
                }
                else
                {
                    throw new WebFaultException<string>("Please enter validate token.", HttpStatusCode.Unauthorized);
                }

            }
            catch (Exception ex)
            {
                ResultStatus.Status = "0";
                ResultStatus.StatusMessage = ex.Message;
                objGetTimelineResult.ResultStatus = ResultStatus;
            }
            writeLog("GetTimeline", "STOP", UserID, "0");
            return objGetTimelineResult;
        }
        #endregion

        public GetCompletedMilestonesResult GetCompletedMilestones(string UserID, string UserToken, string Type)
        {
            GetCompletedMilestonesResult objCompletedMilestonesResult = new GetCompletedMilestonesResult();
            List<string> lstCompletedMilestones = new List<string>();
            ResultStatus ResultStatus = new ResultStatus();

            objTokenInfo = LoginStatus.ValidateToken(UserToken, UserID);
            try
            {
                writeLog("GetCompletedMilestones", "START", UserID, "0");
                var db = new UnitOfWork();

                if (Type == null || Type.Length <= 0) Type = "0";
                int intType = Convert.ToInt32(Type);

                if (objTokenInfo != null && objTokenInfo.EmailID != null)
                {
                    long lngUserID = Convert.ToInt64(UserID);
                    List<TimelineEntry> lstCompletedMilestone = db.TimelineEntry.Get(n => n.UserID == lngUserID && n.TypeID == intType && n.MilestoneID != null && n.CompletedStatus == 1).ToList();

                    if (lstCompletedMilestone.Count > 0)
                    {
                        foreach (var objTimeLineEntry in lstCompletedMilestone)
                        {
                            if (lstCompletedMilestones.Contains(objTimeLineEntry.MilestoneID.ToString())) continue;

                            lstCompletedMilestones.Add(objTimeLineEntry.MilestoneID.ToString());
                        }
                    }

                    ResultStatus.Status = "1";
                    ResultStatus.StatusMessage = "";
                    objCompletedMilestonesResult.ResultStatus = ResultStatus;
                    objCompletedMilestonesResult.lstCompletedMilestones = lstCompletedMilestones;
                }
                else
                {
                    throw new WebFaultException<string>("Please enter validate token.", HttpStatusCode.Unauthorized);
                }

            }
            catch (Exception ex)
            {
                ResultStatus.Status = "0";
                ResultStatus.StatusMessage = ex.Message;
                objCompletedMilestonesResult.ResultStatus = ResultStatus;
                objCompletedMilestonesResult.lstCompletedMilestones = lstCompletedMilestones;
            }
            writeLog("GetCompletedMilestones", "STOP", UserID, "0");
            return objCompletedMilestonesResult;
        }



        #region LogoutUser
        public GetLogoutResult LogoutUser(string UserID, string UserToken)
        {
            GetLogoutResult objLogoutResult = new GetLogoutResult();
            ResultStatus objResultStatus = new ResultStatus();
            try
            {
                writeLog("LogoutUser", "START", UserID, "0");
                var db = new UnitOfWork();
                if (UserID != null && UserID != "")
                {
                    long lngUserID = Convert.ToInt64(UserID);
                    List<User> lstUser = db.User.Get(e => e.ID == lngUserID).ToList();
                    if (lstUser.Count > 0)
                    {
                        User objUser = lstUser.FirstOrDefault();
                        if (objUser != null)
                        {
                            if (objUser.Token == UserToken)
                            {
                                objUser.Token = null;
                                objUser.TokenExpireTime = null;
                                objUser.DeviceToken = null;
                                db.User.Update(objUser);
                                db.SaveChanges();

                                objResultStatus.Status = "1";
                                objResultStatus.StatusMessage = "Logout User Successfully!";
                                objLogoutResult.ResultStatus = objResultStatus;
                            }
                            else
                            {
                                throw new WebFaultException<string>("Please enter validate token.", HttpStatusCode.Unauthorized);
                            }
                        }
                    }
                    else
                    {
                        objResultStatus.Status = "0";//as per rajendra's instruction in mail on dated-13082014
                        objResultStatus.StatusMessage = "User not found!!";
                        objLogoutResult.ResultStatus = objResultStatus;
                    }
                }
                writeLog("LogoutUser", "STOP", UserID, "0");
                return objLogoutResult;
            }
            catch (Exception ex)
            {
                objResultStatus.Status = "0";
                objResultStatus.StatusMessage = ex.Message;
                objLogoutResult.ResultStatus = objResultStatus;
                return objLogoutResult;
            }
        }
        #endregion

        #region ForgotPassword

        public ResultStatus ForgotPassword(string EmailAddress, string CurrentLocalTime)
        {
            ResultStatus ResultStatus = new ResultStatus();

            try
            {
                writeLog("ForgotPassword", "START", EmailAddress, "0");
                var db = new UnitOfWork();
                User objUser = new User();
                if (EmailAddress != null && EmailAddress != "")
                {
                    List<User> lstUser = db.User.Get(e => e.Email.ToUpper() == EmailAddress.ToUpper()).ToList();
                    if (lstUser.Count > 0)
                    {
                        objUser = lstUser.FirstOrDefault();

                        if (objUser != null)
                        {
                            //Send Email to E-mail Address with Re-set password link here.
                            #region Send-Email

                            string Password = objUser.EncryptedPassword;

                            Guid strResetPassKey = Guid.NewGuid();

                            string SuperBabyWebUrl = ConfigurationManager.AppSettings["SuperBabyWebUrl"];
                            string LoginLink = SuperBabyWebUrl + "ChangePassword/Index?UR=" + Password + "&UT=" + Security.GetPasswordHash(Convert.ToString(1), Security.CreateSalt(8)) + "&initiallogin=true&Uid=" + objUser.ID.ToString() + "&Redirect=mobapp";
                            //string LoginLink = "http://192.168.0.6/RudderWeb/ChangePassword/Index?UR=" + Password + "&UT=" + Security.GetPasswordHash(Convert.ToString(1), Security.CreateSalt(8)) + "&initiallogin=true&Uid=" + objUser.ID.ToString() + "&Redirect=mobapp";// EncryptionHandler.Encode(strResetPassKey);

                            string MailBody = string.Empty;
                            StringBuilder stringBuilderText = new StringBuilder();


                            string mailContent = "<html><head><title></title></head><body><table><tr><td>Dear @user@,<br /><br /></td></tr><tr><td>You requested that we reset your password for your account with Superbaby on @datetimecreated@. <br /> <br /> Please visit this link within 24 hours to reset your password: <br /></td></tr><tr><td><a href='@loginlink@'>@loginlink@</a></td></tr><tr><td>&nbsp;</td></tr><tr><td>Thanks!<br/> The Superbaby Team</td></tr></table></body></html>";

                            mailContent = mailContent.Replace("@user@", "User");
                            mailContent = mailContent.Replace("@loginlink@", LoginLink);
                            if (CurrentLocalTime != null && CurrentLocalTime.Length > 0)
                                mailContent = mailContent.Replace("@datetimecreated@", CurrentLocalTime);
                            else
                                mailContent = mailContent.Replace("@datetimecreated@", DateTime.Now.ToString());

                            stringBuilderText.Append(mailContent);
                            MailBody = stringBuilderText.ToString();


                            SendMail(objUser.Email.Trim(), "", "", "[SupperBaby] Your Password Reset Request", MailBody, string.Empty, true, "", true, "Recognized App");
                            #endregion

                            #region AddToDatabase

                            User_Reset_Password objNewUserPassword = new User_Reset_Password();
                            objNewUserPassword.UserID = objUser.ID;
                            objNewUserPassword.Password_Reset_Key = Convert.ToString(strResetPassKey);
                            objNewUserPassword.Date_Created = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                            objNewUserPassword.Date_Modified = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                            objNewUserPassword.Is_Active = true;
                            db.User_Reset_Password.Add(objNewUserPassword);

                            db.SaveChanges();

                            #endregion

                            ResultStatus.Status = "1";
                            ResultStatus.StatusMessage = "E-mail has been sent successfully!";
                        }
                        else
                        {
                            ResultStatus.Status = "0";
                            ResultStatus.StatusMessage = "Oops! This email doesn’t exist.";
                        }
                    }
                }
                else
                {
                    ResultStatus.Status = "0";
                    ResultStatus.StatusMessage = "Invalid Parameter!";
                }
                writeLog("ForgotPassword", "STOP", EmailAddress, "0");
                return ResultStatus;
            }
            catch (Exception ex)
            {
                ResultStatus.Status = "0";
                ResultStatus.StatusMessage = ex.Message;
                return ResultStatus;
            }
        }

        #endregion

        #region CommonMethods

        private void writeLog(string strMethodName, string strMessage, string strUserID, string strEventID)
        {
            try
            {
                string isWriteLog = System.Configuration.ConfigurationManager.AppSettings["writeLog"];
                if (isWriteLog.ToLower() == "true")
                {
                    string strPath = System.Web.HttpContext.Current.Server.MapPath("WCFLog");
                    string filePath = strPath + "\\WSLog.txt";
                    TextWriter tsw3 = new StreamWriter(filePath, true);
                    string strLog = "Date: " + DateTime.Now.ToString() + " ::: Method name: " + strMethodName + " ::: UserID: " + strUserID + " ::: EventID: " + strEventID + " ::: Message: " + strMessage;
                    tsw3.WriteLine(strLog);
                    tsw3.Close();
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public string FindFilePath()
        {
            return System.Web.HttpContext.Current.Server.MapPath("ImageUpload");
        }

        public string SaveImage(string base64stringImage, string AppendPrefix, int intThumbWidth, int intThumbHeight)
        {
            string filePath = "";
            string _fileName = "";

            try
            {
                if (!Directory.Exists(FindFilePath()))
                {
                    Directory.CreateDirectory(FindFilePath());
                }
                _fileName = AppendPrefix + Guid.NewGuid().ToString() + ".PNG";
                filePath = Path.Combine(FindFilePath()) + "\\" + _fileName;
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }


                var bytes = Convert.FromBase64String(base64stringImage);
                using (var imageFile = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.Write(bytes, 0, bytes.Length);
                    imageFile.Flush();
                    imageFile.Close();
                }

                //--------- Check for source image have required orientation
                Image SourceImage = Image.FromFile(filePath);
                bool IsReqOrientation = false;
                if (SourceImage.PropertyIdList.Contains(0x112)) //0x112 = Orientation
                {
                    var prop = SourceImage.GetPropertyItem(0x112);
                    if (prop.Type == 3 && prop.Len == 2)
                    {
                        UInt16 orientationExif = BitConverter.ToUInt16(SourceImage.GetPropertyItem(0x112).Value, 0);
                        if (orientationExif == 8)
                        {
                            IsReqOrientation = true;
                        }
                        else if (orientationExif == 3)
                        {
                            IsReqOrientation = true;
                        }
                        else if (orientationExif == 6)
                        {
                            IsReqOrientation = true;
                        }
                    }
                }
                //--------- Close Check for source image have required orientation

                //--------- If image required orientation then crop with 100*100 size otherwise do same as before.
                Image image1 = SourceImage;
                if (IsReqOrientation)
                {
                    image1 = getCroppedWithOrientation(filePath, SourceImage, image1.Width, image1.Width);
                }
                else
                {
                    image1 = getCroppedImage(filePath);
                }
                //-----------------------------------------------------------------------

                //Create Thumbnail  
                Image thumb = image1.GetThumbnailImage(intThumbWidth, intThumbHeight, () => false, IntPtr.Zero);

                //-------------- If cropped image has orientation then give rotation flip as required.
                //if (image1.PropertyIdList.Contains(0x112)) //0x112 = Orientation
                //{
                //    var prop = image1.GetPropertyItem(0x112);
                //    if (prop.Type == 3 && prop.Len == 2)
                //    {
                //        UInt16 orientationExif = BitConverter.ToUInt16(image1.GetPropertyItem(0x112).Value, 0);
                //        if (orientationExif == 8)
                //        {
                //            thumb.RotateFlip(RotateFlipType.Rotate270FlipNone);
                //        }
                //        else if (orientationExif == 3)
                //        {
                //            thumb.RotateFlip(RotateFlipType.Rotate180FlipNone);
                //        }
                //        else if (orientationExif == 6)
                //        {
                //            thumb.RotateFlip(RotateFlipType.Rotate90FlipNone);
                //        }
                //    }
                //}
                //----------------------------------

                thumb.Save(filePath.Replace(".PNG", "_Thumb.PNG"));
                image1.Dispose();
                thumb.Dispose();
                SourceImage.Dispose();
                return _fileName;
            }
            catch (Exception e)
            {
                return _fileName;
            }
        }

        private Image getCroppedWithOrientation(string path, Image image1, int reqWidth, int reqHeight)
        {
            Bitmap bmpImage = new Bitmap(path);


            if (image1.PropertyIdList.Contains(0x112)) //0x112 = Orientation
            {
                var prop = image1.GetPropertyItem(0x112);
                if (prop.Type == 3 && prop.Len == 2)
                {
                    UInt16 orientationExif = BitConverter.ToUInt16(image1.GetPropertyItem(0x112).Value, 0);
                    if (orientationExif == 8)
                    {
                        bmpImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    }
                    else if (orientationExif == 3)
                    {
                        bmpImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    }
                    else if (orientationExif == 6)
                    {
                        bmpImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    }
                }
            }

            int OrgX = bmpImage.Width;
            int OrgY = bmpImage.Height;

            int RectX = 0;
            int RectY = 0;

            int center = Convert.ToInt32(OrgX / 2);
            RectX = center - (OrgX / 2);



            int centery = Convert.ToInt32(OrgY / 2);
            RectY = centery - (OrgX / 2);


            Bitmap image = bmpImage.Clone(new Rectangle(RectX, RectY, OrgX, OrgX), bmpImage.PixelFormat);
            bmpImage.Dispose();
            return image;
        }

        private Image getCroppedImage(string path)
        {
            Bitmap bmpImage = new Bitmap(path);

            int rectX = 0;
            int rectY = 0;
            int rectWidthHeight = bmpImage.Width;

            if (bmpImage.Width > bmpImage.Height)
            {
                rectX = (bmpImage.Width - bmpImage.Height) / 2;
                rectY = 0;
                rectWidthHeight = bmpImage.Height;
            }
            else if (bmpImage.Width < bmpImage.Height)
            {
                rectX = 0;
                rectY = (bmpImage.Height - bmpImage.Width) / 2;
                rectWidthHeight = bmpImage.Width;
            }

            Bitmap image = bmpImage.Clone(new Rectangle(rectX, rectY, rectWidthHeight, rectWidthHeight), bmpImage.PixelFormat);
            bmpImage.Dispose();
            return image;
        }

        public void DeleteImage(string ImageName)
        {
            string filePath = Path.Combine(FindFilePath()) + "\\" + ImageName;

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        public static bool SendMail(string MailTo, string MailCC, string MailBCC, string Subject, string Body, string Attachment, bool IsBodyHtml, string EmailEncoding, bool UseSSL, string FromName)
        {
            MailMessage message = new MailMessage();

            string strMailFrom = ConfigurationManager.AppSettings["MailFrom"];
            string strSMTPServer = ConfigurationManager.AppSettings["SMTPServer"];
            string strSMTPUserName = ConfigurationManager.AppSettings["SMTPUserName"];
            string strSMTPPassword = ConfigurationManager.AppSettings["SMTPPassword"];

            message.From = new MailAddress(strMailFrom);//"tatvatestdev2@gmail.com"
            message.To.Add(new MailAddress(MailTo));//magtstestdev1@gmail.com

            if (MailCC != string.Empty)
            {
                string[] mailCC = MailCC.Split(';');
                foreach (string email in mailCC)
                    message.CC.Add(email);
            }

            if (MailBCC != string.Empty)
            {
                MailBCC = MailBCC.Replace(";", ",");
                message.Bcc.Add(MailBCC);
            }

            if (!string.IsNullOrEmpty(Attachment))
            {
                string[] attachment = Attachment.Split(';');
                foreach (string attachFile in attachment)
                {
                    try
                    {
                        Attachment attach = new Attachment(attachFile);
                        message.Attachments.Add(attach);
                    }
                    catch (Exception)
                    { }
                }
            }

            message.Subject = Subject;
            message.Body = Body;
            message.IsBodyHtml = IsBodyHtml;//true;

            // finaly send the email:
            SmtpClient smtp = new SmtpClient();

            //smtp.Host = "192.168.0.6";// "smtp.gmail.com";   
            if (ConfigurationManager.AppSettings["IsProduction"].ToString().ToUpper() == "true".ToUpper())
            {
                smtp.Host = strSMTPServer;
                smtp.EnableSsl = UseSSL;
            }
            else
            {
                smtp.Host = "192.168.0.6";
            }

            smtp.Credentials = new System.Net.NetworkCredential(strSMTPUserName, strSMTPPassword);//"tatvatestdev2@gmail.com", "tatvasoft@123"
            try
            {
                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                message.Dispose();
                throw new Exception("Mail is not send due to following error:- " + ex.Message.ToString());
            }
        }
        #endregion
    }
}
