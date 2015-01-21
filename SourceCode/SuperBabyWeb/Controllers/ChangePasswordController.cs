using Portal.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Portal.Model;


namespace SuperBabyWeb.Controllers
{
    public class ChangePasswordController : Controller
    {
        //
        // GET: /ChangePassword/

        public ActionResult Index(string Uid, string Redirect, string UR)
        {
            try
            {
                var db = new UnitOfWork();
                ViewBag.Redirect = "mobapp";
                long lngUId = Convert.ToInt64(Uid);
                List<User> lstUser = db.User.Get(m => m.ID == lngUId && m.EncryptedPassword == UR).ToList();
                User user = new User();
                if (lstUser.Count > 0)
                {
                    user = lstUser.FirstOrDefault();
                }
                if (user != null)
                {
                    User_Reset_Password UserReset = db.User_Reset_Password.Get().OrderByDescending(a => a.Date_Created).FirstOrDefault(m => m.UserID == Convert.ToInt64(user.ID) && m.Is_Active == true);
                    if (UserReset != null)
                    {
                        UserReset.Is_Active = false;
                        db.User_Reset_Password.Update(UserReset);
                        DateTime other = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                        if (UserReset.Date_Created.AddHours(24) < other)
                        {
                            ViewBag.SuccessMessage = "Your reset password link has been expired! Please try again from mobile application!";
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.SuccessMessage = "You had not request for reset password. Please try again from mobile application!";
                        return View();
                    }
                }
                else
                {
                    ViewBag.SuccessMessage = "User does not exsit!";
                    return View();
                }
                if (user.Email != "")
                    ViewBag.Email = user.Email;
                else
                    return RedirectToAction("index", "ChangePassword");
                return View();
            }
            catch (Exception e)
            {
                return View();
            }

        }

        [HttpPost]
        public ActionResult Index(SuperBabyWeb.Models.ChangePassword model, string Redirect, string UR, string UserID)
        {
            try
            {
                var db = new UnitOfWork();
                long lngUserId = Convert.ToInt64(UserID);
                List<User> lstUser = db.User.Get(m => m.ID == lngUserId && m.EncryptedPassword == UR).ToList();
                User user = new User();
                if (lstUser.Count > 0)
                {
                    user = lstUser.FirstOrDefault();
                }
                if (user != null)
                {
                    User_Reset_Password UserReset = db.User_Reset_Password.Get().OrderByDescending(a => a.Date_Created).FirstOrDefault(m => m.UserID == Convert.ToInt64(UserID) && m.Is_Active == true);
                    if (UserReset != null)
                    {
                        UserReset.Is_Active = false;
                        db.User_Reset_Password.Update(UserReset);

                        DateTime other = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                        if (UserReset.Date_Created.AddHours(24) < other)
                        {
                            TempData["SuccessMessage"] = "Your reset password link has been expired! Please try again from mobile application!";
                            ViewBag.SuccessMessage = "Your reset password link has been expired! Please try again from mobile application!";

                            return RedirectToAction("ChangePassword", "Login", new { initiallogin = "true", Uid = UserID, Redirect = "mobapp", UR = UR });
                        }
                        var salt = SuperBabyWeb.Common.Security.CreateSalt(8);
                        string pw = SuperBabyWeb.Common.Security.GetPasswordHash(model.NewPassword.ToString(), salt);
                        user.Salt = salt;
                        user.EncryptedPassword = pw;
                        db.User.Update(user);
                        db.SaveChanges();
                        TempData["SuccessMessage"] = " Password has been changed successfully!";
                        ViewBag.SuccessMessage = " Password has been changed successfully!";

                        return View();
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "You had not request for reset password. Please try again from mobile application.";
                        ViewBag.SuccessMessage = "You had not request for reset password. Please try again from mobile application.";
                        return RedirectToAction("ChangePassword", "Login", new { initiallogin = "true", Uid = UserID, Redirect = "mobapp", UR = UR });

                    }
                }
                else
                {
                    TempData["SuccessMessage"] = "User does not exsit!";
                    ViewBag.SuccessMessage = "User does not exsit!";
                    return RedirectToAction("ChangePassword", "Login", new { initiallogin = "true", Uid = UserID, Redirect = "mobapp", UR = UR });

                }
            }
            catch (Exception e)
            {
                return View();
            }
        }

    }
}
