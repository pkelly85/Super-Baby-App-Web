using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace SuperBabyWCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        void DoWork();

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "RegisterUser", BodyStyle = WebMessageBodyStyle.Wrapped)]
        GetRegisterUserResult RegisterUser(string EmailAddress, string Password, string DeviceToken);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "Login", BodyStyle = WebMessageBodyStyle.Wrapped)]
        GetLoginResult Login(string EmailAddress, string Password, string DeviceToken);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "LoginWithFacebook", BodyStyle = WebMessageBodyStyle.Wrapped)]
        GetLoginResult LoginWithFacebook(string EmailAddress, string FacebookID, string DeviceToken);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "AddEditBabyInfo", BodyStyle = WebMessageBodyStyle.Wrapped)]
        AddEditBabyInfoResult AddEditBabyInfo(string UserID, string UserToken, string Name, string Birthday, string WeightPounds, string WeightOunces, string Height, string ImageData);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetBabyInformation", BodyStyle = WebMessageBodyStyle.Wrapped)]
        GetBabyInformation GetBabyInformation(string UserID, string UserToken);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "UpdateAccountInfo", BodyStyle = WebMessageBodyStyle.Wrapped)]
        UpdateAccountInfo UpdateAccountInfo(string UserID, string UserToken, string Email, string Password);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "AddTimeline", BodyStyle = WebMessageBodyStyle.Wrapped)]
        AddTimeline AddTimeline(string UserID, string UserToken, string Type, string Message, string MilestoneID, string VideoID, string CompletedStatus);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetTimeline", BodyStyle = WebMessageBodyStyle.Wrapped)]
        GetTimelineResult GetTimeline(string UserID, string UserToken, string PageNumber);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "LogoutUser", BodyStyle = WebMessageBodyStyle.Wrapped)]
        GetLogoutResult LogoutUser(string UserID, string UserToken);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "ForgotPassword", BodyStyle = WebMessageBodyStyle.Wrapped)]
        ResultStatus ForgotPassword(string EmailAddress, string CurrentLocalTime);

        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetCompletedMilestones", BodyStyle = WebMessageBodyStyle.Wrapped)]
        GetCompletedMilestonesResult GetCompletedMilestones(string UserID, string UserToken);

    }

    [Serializable]

    #region GetRegisterUserResult

    [DataContract]
    public class GetRegisterUserResult
    {
        private ResultStatus _Status;
        private GetUserResult _GetUserResult;


        [DataMember]
        public ResultStatus ResultStatus
        {
            get { return _Status; }
            set { _Status = value; }
        }

        [DataMember]
        public GetUserResult GetUserResult
        {
            get { return _GetUserResult; }
            set { _GetUserResult = value; }
        }
    }

    #endregion

    #region GetLoginResult

    [DataContract]
    public class GetLoginResult
    {
        private GetUserResult _GetUserResult;
        private GetBabyResult _BabyInformation;
        private ResultStatus _Status;

        [DataMember]
        public GetUserResult GetUserResult
        {
            get { return _GetUserResult; }
            set { _GetUserResult = value; }
        }

        [DataMember]
        public GetBabyResult BabyInformation
        {
            get { return _BabyInformation; }
            set { _BabyInformation = value; }
        }

        [DataMember]
        public ResultStatus ResultStatus
        {
            get { return _Status; }
            set { _Status = value; }
        }

    }

    #endregion

    #region GetStatus
    [DataContract]
    public class ResultStatus
    {
        private string _status = string.Empty;
        private string _statusMessage = string.Empty;

        [DataMember]
        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        [DataMember]
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { _statusMessage = value; }
        }
    }
    #endregion

    #region GetUserResult

    [DataContract]
    public class GetUserResult
    {
        private string _UserId = string.Empty;
        private string _DateCreated = string.Empty;
        private string _DateModified = string.Empty;
        private string _EmailAddress = string.Empty;
        private string _FacebookId = string.Empty;
        private string _Token = string.Empty;

        [DataMember]
        public string UserID
        {
            get { return _UserId; }
            set { _UserId = value; }
        }

        [DataMember]
        public string DateCreated
        {
            get { return _DateCreated; }
            set { _DateCreated = value; }
        }

        [DataMember]
        public string DateModified
        {
            get { return _DateModified; }
            set { _DateModified = value; }
        }

        [DataMember]
        public string EmailAddress
        {
            get { return _EmailAddress; }
            set { _EmailAddress = value; }
        }

        [DataMember]
        public string FacebookId
        {
            get { return _FacebookId; }
            set { _FacebookId = value; }
        }


        [DataMember]
        public string Token
        {
            get { return _Token; }
            set { _Token = value; }
        }

    }

    #endregion

    #region GetBabyResult

    [DataContract]
    public class GetBabyResult
    {
        private string _BabyID = string.Empty;
        private string _DateCreated = string.Empty;
        private string _DateModified = string.Empty;
        private string _UserID = string.Empty;
        private string _Name = string.Empty;
        private string _Birthday = string.Empty;
        private string _WeightPounds = string.Empty;
        private string _WeightOunces = string.Empty;
        private string _Height = string.Empty;
        private string _ImageURL = string.Empty;


        [DataMember]
        public string BabyID
        {
            get { return _BabyID; }
            set { _BabyID = value; }
        }

        [DataMember]
        public string DateCreated
        {
            get { return _DateCreated; }
            set { _DateCreated = value; }
        }

        [DataMember]
        public string DateModified
        {
            get { return _DateModified; }
            set { _DateModified = value; }
        }

        [DataMember]
        public string UserID
        {
            get { return _UserID; }
            set { _UserID = value; }
        }

        [DataMember]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }


        [DataMember]
        public string Birthday
        {
            get { return _Birthday; }
            set { _Birthday = value; }
        }

        [DataMember]
        public string WeightPounds
        {
            get { return _WeightPounds; }
            set { _WeightPounds = value; }
        }

        [DataMember]
        public string WeightOunces
        {
            get { return _WeightOunces; }
            set { _WeightOunces = value; }
        }

        [DataMember]
        public string Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        [DataMember]
        public string ImageURL
        {
            get { return _ImageURL; }
            set { _ImageURL = value; }
        }

    }

    #endregion

    #region GetTimelineEntryResult

    [DataContract]
    public class GetTimelineEntryResult
    {
        private string _TimelineID = string.Empty;
        private string _DateCreated = string.Empty;
        private string _UserID = string.Empty;
        private string _TypeID = string.Empty;
        private string _Message = string.Empty;
        private string _MilestoneID = string.Empty;
        private string _VideoID = string.Empty;

        [DataMember]
        public string TimelineID
        {
            get { return _TimelineID; }
            set { _TimelineID = value; }
        }

        [DataMember]
        public string DateCreated
        {
            get { return _DateCreated; }
            set { _DateCreated = value; }
        }

        [DataMember]
        public string UserID
        {
            get { return _UserID; }
            set { _UserID = value; }
        }

        [DataMember]
        public string TypeID
        {
            get { return _TypeID; }
            set { _TypeID = value; }
        }


        [DataMember]
        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        [DataMember]
        public string MilestoneID
        {
            get { return _MilestoneID; }
            set { _MilestoneID = value; }
        }

        [DataMember]
        public string VideoID
        {
            get { return _VideoID; }
            set { _VideoID = value; }
        }
    }

    #endregion

    #region AddEditBabyInfoResult

    [DataContract]
    public class AddEditBabyInfoResult
    {
        private GetBabyResult _GetBabyResult;
        private ResultStatus _Status;

        [DataMember]
        public GetBabyResult GetBabyResult
        {
            get { return _GetBabyResult; }
            set { _GetBabyResult = value; }
        }

        [DataMember]
        public ResultStatus ResultStatus
        {
            get { return _Status; }
            set { _Status = value; }
        }

    }

    #endregion

    #region GetBabyInformation

    [DataContract]
    public class GetBabyInformation
    {
        private GetBabyResult _GetBabyResult;
        private ResultStatus _Status;

        [DataMember]
        public GetBabyResult GetBabyResult
        {
            get { return _GetBabyResult; }
            set { _GetBabyResult = value; }
        }

        [DataMember]
        public ResultStatus ResultStatus
        {
            get { return _Status; }
            set { _Status = value; }
        }

    }

    #endregion

    #region UpdateAccountInfo

    [DataContract]
    public class UpdateAccountInfo
    {
        private ResultStatus _Status;
        private GetUserResult _GetUserResult;


        [DataMember]
        public ResultStatus ResultStatus
        {
            get { return _Status; }
            set { _Status = value; }
        }

        [DataMember]
        public GetUserResult GetUserResult
        {
            get { return _GetUserResult; }
            set { _GetUserResult = value; }
        }
    }

    #endregion

    #region AddTimeline

    [DataContract]
    public class AddTimeline
    {
        private ResultStatus _Status;

        [DataMember]
        public ResultStatus ResultStatus
        {
            get { return _Status; }
            set { _Status = value; }
        }

    }

    #endregion

    #region GetLogoutResult
    [DataContract]
    public class GetLogoutResult
    {
        private ResultStatus _ResultStatus;

        [DataMember]
        public ResultStatus ResultStatus
        {
            get { return _ResultStatus; }
            set { _ResultStatus = value; }
        }

    }
    #endregion

    #region GetTimelineResult


    [DataContract]
    public class GetTimelineResult
    {
        private ResultStatus _Status;
        private List<GetTimelineEntryResult> _GetTimelineEntryResult;

        [DataMember]
        public ResultStatus ResultStatus
        {
            get { return _Status; }
            set { _Status = value; }
        }

        [DataMember]
        public List<GetTimelineEntryResult> GetTimelineEntryResult
        {
            get { return _GetTimelineEntryResult; }
            set { _GetTimelineEntryResult = value; }
        }

    }
    #endregion

    #region GetCompletedMilestonesResult


    [DataContract]
    public class GetCompletedMilestonesResult
    {
        private ResultStatus _Status;
        private List<string> _lstCompletedMilestones;

        [DataMember]
        public ResultStatus ResultStatus
        {
            get { return _Status; }
            set { _Status = value; }
        }

        [DataMember]
        public List<string> lstCompletedMilestones
        {
            get { return _lstCompletedMilestones; }
            set { _lstCompletedMilestones = value; }
        }

    }
    #endregion
}
