﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using MvcApplication1.Filters;
using System.Text;
using System.IO;
using MvcApplication1.Contexts;
using System.Reflection;

namespace MvcApplication1.Controllers
{
    public class RequestsController : Controller
    {
        [HttpGet]
        public ActionResult LoadStudentRequests()
        {
            RequestsList requestsModel = new RequestsList();
            string currentPerson;
            if (Request.Cookies["UserId"] != null)
                currentPerson = Convert.ToString(Request.Cookies["UserId"].Value);
            else currentPerson = "user1";
            using (CustomDbContext db = new CustomDbContext())
            {
                var r = db.RequestsModel.Where(x => x.createdBy == currentPerson && x.requestState == "request not resolved");
                if (r!=null)
                    foreach (var item in r)
                    {
                        requestsModel.reqests.Add(item);
                    }
            }
            
            return View("StudentRequests", requestsModel.reqests);
        }        

        [HttpGet]
        public ActionResult ResolvedRequests()
        {
            RequestsList requestsModel = new RequestsList();
            string currentPerson;
            if (Request.Cookies["UserId"] != null)
                currentPerson = Convert.ToString(Request.Cookies["UserId"].Value);
            else currentPerson = "user1";
            using (CustomDbContext db = new CustomDbContext())
            {
                var r = db.RequestsModel.Where(x => x.createdBy == currentPerson && x.requestState == "request resolved");
                if (r!=null)
                    foreach (var item in r)
                    {
                        requestsModel.reqests.Add(item);
                    }
            }            
            
            return View("StudentRequests", requestsModel.reqests);
        }
        [HttpGet]
        public ActionResult CanceledRequests()
        {
            RequestsList requestsModel = new RequestsList();
            string currentPerson;
            if (Request.Cookies["UserId"] != null)
                currentPerson = Convert.ToString(Request.Cookies["UserId"].Value);
            else currentPerson = "user1";
            using (CustomDbContext db = new CustomDbContext())
            {
                var r = db.RequestsModel.Where(x => x.createdBy == currentPerson && x.requestState == "request canceled");
                if (r!=null)
                    foreach (var item in r)
                    {
                        requestsModel.reqests.Add(item);
                    }
            }
            return View("StudentRequests", requestsModel.reqests);
        }
        [HttpGet]
        public ActionResult MarkAsResolved(RequestsList resolvThisReq, string requestId)
        {
            //RequestsList requestsModel = new RequestsList();
            
            using (CustomDbContext db = new CustomDbContext())
            {
                var r = db.RequestsModel.SingleOrDefault(x => x.requestId == requestId);

                if (r != null)
                    r.requestState = "request resolved";
                db.SaveChanges();
            }
            
            return RedirectToAction("ResolvedRequests");
        }

        [HttpGet]
        public ActionResult MarkAsCanceled(RequestsList resolvThisReq, string requestId)
        {
            using (CustomDbContext db = new CustomDbContext())
            {
                var r = db.RequestsModel.SingleOrDefault(x => x.requestId == requestId);

                if (r != null)
                    r.requestState = "request canceled";
                db.SaveChanges();
            }
            
            return RedirectToAction("CanceledRequests");
            
        }

        //--------------------------Создание/сохранение заявки-----------------------------------------------

        [HttpGet]
        public ActionResult CreateRequest()
        {

            return View("CreateRequest");
        }


        public ActionResult SaveReqest(RequestModel req)
        {
            Singleton s = Singleton.Instance;                  
            string currentPerson;
            currentPerson = s.user;

            using (CustomDbContext db = new CustomDbContext())
            {
                if (db.ProfileModel.SingleOrDefault(x => x.UserName == currentPerson).MyTegs != null)
                {
                    string[] currentPersonTegs = db.ProfileModel.SingleOrDefault(x => x.UserName == currentPerson).MyTegs.Split('|');

                    List<ProfileModel> mentorListWithTegs = new List<ProfileModel>();
                    var mentorList = db.UserProfiles.Where(x => x.Role == "mentor");
                    if (mentorList != null)
                        foreach (var m in mentorList)
                            for (int i = 0; i < currentPersonTegs.Length; i++)
                            {
                                string currentTeg = currentPersonTegs[i];
                                if (currentTeg != "")
                                {
                                    var currentMentor = m.UserName;
                                    using (CustomDbContext db2 = new CustomDbContext())
                                    {
                                        var ment = db2.ProfileModel.SingleOrDefault(x => x.UserName == currentMentor);
                                        if (ment != null)
                                            if (ment.UserName != currentPerson)
                                                if (ment.MyTegs != null)
                                                    if (ment.MyTegs.Contains(currentTeg))
                                                        if (!mentorListWithTegs.Any(x => x.UserName == ment.UserName))
                                                            mentorListWithTegs.Add(ment);
                                    }
                                }
                            }

                }
            }

            using (CustomDbContext db = new CustomDbContext())
            {
                int count = db.RequestsModel.Count(x => x.createdBy == currentPerson);
                if (count == 0)
                {
                    req.requestId = currentPerson + "_1";                    
                }
                else
                {
                    req.requestId = currentPerson + "_" + (count + 1);
                }
                req.requestState = "request not resolved";
                req.createdBy = currentPerson;
                req.createdAt = DateTime.Now;
                if (req.requestName == null)
                    req.requestName = req.requestText.Substring(0, 10);
                db.RequestsModel.Add(
                //    new RequestModel { 
                //    requestId = req.requestId,
                //    requestName = req.requestName,
                //    requestText = req.requestText,
                //    requestState = req.requestState,
                //    createdBy = req.createdBy
                //}
                req
                );
                db.SaveChanges();                
                
            }

            return RedirectToAction("LoadStudentRequests");
        }

        [HttpGet]
        public ActionResult EditRequest(RequestsList resolvThisReq, string requestId)
        {
            //подгрузить инфу из бд для айди req.requestId
            RequestModel req = new RequestModel();            
            
            using (CustomDbContext db = new CustomDbContext())
            {
                req = db.RequestsModel.SingleOrDefault(x => x.requestId == requestId);                
            }
            return View("EditRequest", req);
        }
        
        [HttpPost]
        public ActionResult SaveChangesReqest(RequestModel req)
        {

            //сохранить изменения в бд по айди req.requestId
            return RedirectToAction("LoadStudentRequests");
        }
        //--------------------------Заявки для ментора-----------------------------------------------

        public List<RequestModel> LoadMentorRequests(string  rState)
        {
           RequestsList requestsModel = new RequestsList();
            string currentPerson;
            if (Request.Cookies["UserId"] != null)
                currentPerson = Convert.ToString(Request.Cookies["UserId"].Value);
            else currentPerson = "user1";
            
            using (CustomDbContext db = new CustomDbContext())
            {
                if (db.ProfileModel.SingleOrDefault(x => x.UserName == currentPerson).MyTegs != null)
                {
                    string[] currentPersonTegs = db.ProfileModel.SingleOrDefault(x => x.UserName == currentPerson).MyTegs.Split('|');

                    List<ProfileModel> mentorListWithTegs = new List<ProfileModel>();
                    var mentorList = db.UserProfiles.Where(x => x.Role == "student" || x.Role == "user");
                    if (mentorList != null)
                        foreach (var m in mentorList)
                            for (int i = 0; i < currentPersonTegs.Length; i++)
                            {
                                string currentTeg = currentPersonTegs[i];
                                if (currentTeg != "")
                                {
                                    var currentMentor = m.UserName;
                                    using (CustomDbContext db2 = new CustomDbContext())
                                    {
                                        var ment = db2.ProfileModel.SingleOrDefault(x => x.UserName == currentMentor);
                                        if (ment != null)
                                            if (ment.UserName != currentPerson)
                                                if (ment.MyTegs != null)
                                                    if (ment.MyTegs.Contains(currentTeg))
                                                        if (!mentorListWithTegs.Any(x => x.UserName == ment.UserName))
                                                            mentorListWithTegs.Add(ment);
                                    }
                                }
                            }


                    foreach (var m in mentorListWithTegs)
                    {
                        
                        using (CustomDbContext db3 = new CustomDbContext())
                        {
                            var r = db3.RequestsModel.Where(x => x.createdBy == m.UserName && x.requestState == rState);
                            if (r != null)
                                foreach (var item in r)
                                {
                                    requestsModel.reqests.Add(item);
                                }
                        }                        
                    }
                }
            }
            return requestsModel.reqests;
        }
            

        [HttpGet]
        public ActionResult MentorAvailableRequests()
        {
            RequestsList requestsModel = new RequestsList();
            requestsModel.reqests = LoadMentorRequests("request not resolved");

            return View("MentorRequests", requestsModel.reqests);
        }

        [HttpGet]
        public ActionResult MentorResolvedRequests()
        {
            RequestsList requestsModel = new RequestsList();
            requestsModel.reqests = LoadMentorRequests("request resolved");
            
            return View("MentorRequests", requestsModel.reqests);
        }

        [HttpGet]
        public ActionResult MentorCanceledRequests()
        {
            RequestsList requestsModel = new RequestsList();
            requestsModel.reqests = LoadMentorRequests("request canceled");

            return View("MentorRequests", requestsModel.reqests);
        }
        
        [HttpGet]
        public ActionResult AboutRequest(RequestsList requestsModel, string requestId)
        {
            RequestModel req = new RequestModel(requestId, "Help with smth", "bla-bla", null, "request resolved");
            return View("RequestDetails", req);
        }
    }
}