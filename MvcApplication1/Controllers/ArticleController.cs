﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models;
using MvcApplication1.Contexts;
using System.Reflection;

namespace MvcApplication1.Controllers
{
    //[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    //public class MultiButtonAttribute : ActionNameSelectorAttribute
    //{
    //    public string MatchFormKey { get; set; }
    //    public string MatchFormValue { get; set; }
    //    public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
    //    {
    //        return controllerContext.HttpContext.Request[MatchFormKey] != null &&
    //            controllerContext.HttpContext.Request[MatchFormKey] == MatchFormValue;
    //    }
    //}

    public class ArticleData
    {
        public string inputArticleTitle { get; set; }
        public string inputArticleText { get; set; }
        public string[] tagList { get; set; }
    }

    public class ArticleController : Controller
    {
        // GET: Article
        [HttpGet]
        public ActionResult Index()
        {
            string currentPerson;
            if (Request.Cookies["UserId"] != null)
                currentPerson = Convert.ToString(Request.Cookies["UserId"].Value);
            else currentPerson = null;
            //ProxyPattern.Proxy p = new ProxyPattern.Proxy(); //!!!!!!!!!!!!!!!!!!!!!!!
            //ArticleModel myModel = p.getArticle(currentPerson, false);
            ArticleModel myModel = getArticle(currentPerson, false);
            return View("MentorArticle", myModel);
        }

        public ArticleModel getArticle(string currentPerson, bool onlyPublsihed)
        {
            List<SelectListItem> listSelectListItems = new List<SelectListItem>();
            using (CustomDbContext db = new CustomDbContext())
            {
                if (!onlyPublsihed)
                    foreach (var article in db.ArticleModel.Where(x => x.createdBy == currentPerson))
                    {
                        SelectListItem selectList = new SelectListItem()
                        {
                            Text = article.articleTitle,
                            Value = article.articleID
                        };
                        listSelectListItems.Add(selectList);
                    }
                else
                    foreach (var article in db.ArticleModel.Where(x => x.createdBy == currentPerson && x.isPublished == true))
                    {
                        SelectListItem selectList = new SelectListItem()
                        {
                            Text = article.articleTitle,
                            Value = article.articleID
                        };
                        listSelectListItems.Add(selectList);
                    }
            }
            ArticleModel myModel = new ArticleModel()
            {
                articleNames = listSelectListItems
            };

            return myModel;
        }

        [HttpGet]
        public ActionResult CreateArticle(ArticleModel model)
        {
            return View("CreateArticle", model);
        }

        [HttpGet]
        public ActionResult EditArticle(ArticleModel model)
        {
            return View("CreateArticle", model);
        }


        public void LoadArtcileIntoForm(ArticleModel model, string currentPerson, bool onlyPublished)
        {
            FacadePattern.Facade f = new FacadePattern.Facade(model, currentPerson, onlyPublished);
            f.workWithSelectedTeg();
            f.workWithDb();
            //model.articleNames = getArticle(currentPerson, onlyPublished).articleNames;
            //var str = "";
            //if (model.articleName != null)
            //    foreach (string s in model.articleName)
            //    {
            //        str = model.articleNames.FirstOrDefault(x => x.Value == s).Text;
            //        model.articleID = s;
            //    }

            //using (CustomDbContext db = new CustomDbContext())
            //{
            //    var myModel = db.ArticleModel.FirstOrDefault(x => x.createdBy == currentPerson);
            //    if (myModel != null)
            //    {
            //        model.articleTitle = str;
            //        if (str != "")
            //            model.articleText = myModel.articleText;
            //    }

            //}
            //return model;
        }

        [HttpPost]
        public ActionResult EditArticle(ArticleModel model, ProfileModel p)
        {
            string currentPerson;
            if (Request.Cookies["UserId"] != null)
                currentPerson = Convert.ToString(Request.Cookies["UserId"].Value);
            else currentPerson = "user1";
            LoadArtcileIntoForm(model, currentPerson, false);

            return View("CreateArticle", model);
        }

        [HttpPost]
        //[MultiButton(MatchFormKey = "action", MatchFormValue = "saveToDraft")]
        public ActionResult SaveArticleToDraft(ArticleData articleData)//ArticleModel model
        {
            //Decorator.SavaArticle d = new Decorator.SaveToDraft(new Decorator.ImplementSave());
            //d.saveToDb(model);

            //Singleton s = Singleton.Instance;
            string currentPerson;// = s.user;
            if (Request.Cookies["UserId"] != null) //если данные о пользователе записаны в куки
            {
                if (articleData.inputArticleTitle != "" && articleData.inputArticleText != "" && articleData.tagList != null)
                {
                    using (CustomDbContext db = new CustomDbContext())
                    {
                        string articleid;
                        currentPerson = Convert.ToString(Request.Cookies["UserId"].Value);
                        var myModel = db.ArticleModel.FirstOrDefault(x => x.createdBy == currentPerson);//чи створював користувач вже статті
                        if (myModel == null) //если статтей у пользователя не было
                        {
                            articleid = currentPerson + '_' + 1; //якщо ні, то id статті 1, бо ця стаття в нього перша
                        }
                        else //якщо користувач створює нову статтю, проте в нього були вже створені статті
                        {
                            var articleCount = db.ArticleModel.Count(x => x.createdBy == currentPerson);
                            articleid = currentPerson + '_' + (articleCount + 1);
                        }
                        db.ArticleModel.Add(new ArticleModel
                        {
                            articleID = articleid,
                            createdBy = currentPerson,
                            articleTitle = articleData.inputArticleTitle,
                            articleText = articleData.inputArticleText,
                            SelectedTeg = articleData.tagList,
                            isPublished = false
                        });
                        db.SaveChanges();
                        //else if (model.articleID != null)//якщо користувач обрав існуючу статтю для редагування
                        //{
                        //    //articleid = currentPerson + "_" + model.articleID;
                        //    //перевіряємо чи власне є ця стаття у бд
                        //    myModel = db.ArticleModel.FirstOrDefault(x => x.articleID == model.articleID);
                        //    if (myModel != null)//якщо є
                        //    {
                        //        myModel.articleTitle = model.articleTitle;
                        //        myModel.articleText = model.articleText;
                        //        //myModel.isPublished = false;
                        //        db.SaveChanges();
                        //    }
                        //    else
                        //    {

                        //        db.ArticleModel.Add(new ArticleModel { articleID = model.articleID, createdBy = currentPerson, articleTitle = model.articleTitle });
                        //        myModel = db.ArticleModel.FirstOrDefault(x => x.articleID == model.articleID);
                        //        if (myModel != null)
                        //        {
                        //            myModel.articleTitle = model.articleTitle;
                        //            myModel.articleText = model.articleText;
                        //            //myModel.isPublished = false;
                        //        }
                        //        db.SaveChanges();
                        //    }
                        //}

                    }
                }
            }
            return RedirectToAction("MentorArticle");
            //if (model.articleTitle != null)
            //    using (CustomDbContext db = new CustomDbContext())
            //    {
            //        string articleid;
            //        string currentPerson;
            //        if (Request.Cookies["UserId"] != null)
            //            currentPerson = Convert.ToString(Request.Cookies["UserId"].Value);
            //        else currentPerson = "user1";

            //        //чи створював користувач вже статті
            //        var myModel = db.ArticleModel.FirstOrDefault(x => x.createdBy == currentPerson);
            //        if (myModel == null)
            //        {
            //            articleid = currentPerson + '_' + 1;
            //            //якщо ні, то id статті 1, бо ця стаття в нього перша
            //            db.ArticleModel.Add(new ArticleModel { articleID = articleid, createdBy = currentPerson, articleTitle = model.articleTitle });
            //            if (myModel != null)//якщо є
            //            {
            //                myModel.articleTitle = model.articleTitle;
            //                myModel.articleText = model.articleText;
            //                myModel.isPublished = false;
            //            }
            //            db.SaveChanges();

            //        }
            //        else
            //            //якщо користувач обрав існуючу статтю для редагування
            //            if (model.articleID != null)
            //            {
            //                //articleid = currentPerson + "_" + model.articleID;
            //                //перевіряємо чи власне є ця стаття у бд
            //                myModel = db.ArticleModel.FirstOrDefault(x => x.articleID == model.articleID);
            //                if (myModel != null)//якщо є
            //                {
            //                    myModel.articleTitle = model.articleTitle;
            //                    myModel.articleText = model.articleText;
            //                    myModel.isPublished = false;
            //                    db.SaveChanges();
            //                }
            //                else
            //                {

            //                    db.ArticleModel.Add(new ArticleModel { articleID = model.articleID, createdBy = currentPerson, articleTitle = model.articleTitle });
            //                    myModel = db.ArticleModel.FirstOrDefault(x => x.articleID == model.articleID);
            //                    if (myModel != null)
            //                    {
            //                        myModel.articleTitle = model.articleTitle;
            //                        myModel.articleText = model.articleText;
            //                        myModel.isPublished = false;
            //                    }
            //                    db.SaveChanges();
            //                }
            //            }
            //            else //якщо користувач створює нову статтю, проте в нього були вже створені статті
            //            {
            //                var articleCount = db.ArticleModel.Count(x => x.createdBy == currentPerson);
            //                articleid = currentPerson + '_' + (articleCount + 1);
            //                db.ArticleModel.Add(new ArticleModel { articleID = articleid, createdBy = currentPerson, articleTitle = model.articleTitle });
            //                myModel = db.ArticleModel.FirstOrDefault(x => x.articleID == articleid);
            //                if (myModel != null)
            //                {
            //                    myModel.articleTitle = model.articleTitle;
            //                    myModel.articleText = model.articleText;
            //                    myModel.isPublished = false;
            //                }
            //                db.SaveChanges();
            //            }
            //    }
        }

        [HttpPost]
        //[MultiButton(MatchFormKey = "action", MatchFormValue = "publish")]
        public ActionResult PublishArticle(ArticleModel model)
        {
            Decorator.SavaArticle d = new Decorator.Publish(new Decorator.ImplementSave());
            d.saveToDb(model);
            //if (model.articleTitle != null)
            //    using (CustomDbContext db = new CustomDbContext())
            //    {
            //        string articleid;
            //        string currentPerson;
            //        if (Request.Cookies["UserId"] != null)
            //            currentPerson = Convert.ToString(Request.Cookies["UserId"].Value);
            //        else currentPerson = "user1";

            //        //чи створював користувач вже статті
            //        var myModel = db.ArticleModel.FirstOrDefault(x => x.createdBy == currentPerson);
            //        if (myModel == null)
            //        {
            //            articleid = currentPerson + '_' + 1;
            //            //якщо ні, то id статті 1, бо ця стаття в нього перша
            //            db.ArticleModel.Add(new ArticleModel { articleID = articleid, createdBy = currentPerson, articleTitle = model.articleTitle });
            //            myModel = db.ArticleModel.SingleOrDefault(x => x.articleID == articleid);
            //            if (myModel != null)
            //            {
            //                myModel.articleTitle = model.articleTitle;
            //                myModel.articleText = model.articleText;
            //                myModel.isPublished = true;
            //            }

            //        }
            //        else
            //            //якщо користувач обрав існуючу статтю для редагування
            //            if (model.articleID != null)
            //            {
            //                //articleid = currentPerson + "_" + model.articleID;
            //                //перевіряємо чи власне є ця стаття у бд
            //                myModel = db.ArticleModel.SingleOrDefault(x => x.articleID == model.articleID);
            //                if (myModel != null)//якщо є
            //                {
            //                    myModel.articleTitle = model.articleTitle;
            //                    myModel.articleText = model.articleText;
            //                    myModel.isPublished = true;
            //                }
            //                else
            //                {
            //                    db.ArticleModel.Add(new ArticleModel { articleID = model.articleID, createdBy = currentPerson, articleTitle = model.articleTitle });
            //                    myModel = db.ArticleModel.FirstOrDefault(x => x.articleID == model.articleID);
            //                    if (myModel != null)
            //                    {
            //                        myModel.articleTitle = model.articleTitle;
            //                        myModel.articleText = model.articleText;
            //                        myModel.isPublished = true;
            //                    }

            //                }
            //            }
            //            else //якщо користувач створює нову статтю, проте в нього були вже створені статті
            //            {
            //                var articleCount = db.ArticleModel.Count(x => x.createdBy == currentPerson);
            //                articleid = currentPerson + '_' + (articleCount + 1);
            //                db.ArticleModel.Add(new ArticleModel { articleID = articleid, createdBy = currentPerson, articleTitle = model.articleTitle });
            //                myModel = db.ArticleModel.FirstOrDefault(x => x.articleID == articleid);
            //                if (myModel != null)
            //                {
            //                    myModel.articleTitle = model.articleTitle;
            //                    myModel.articleText = model.articleText;
            //                    myModel.isPublished = true;
            //                }

            //            }

            //        db.SaveChanges();
            //    }

            //опубликовать статью
            return RedirectToAction("MentorArticle");
        }

        public ActionResult ShowArticles()
        {
            ArticleModel model;
            string currentMent;
            if (Request.Cookies["MentorId"] != null)
                currentMent = Convert.ToString(Request.Cookies["MentorId"].Value);
            else currentMent = "";
            if (currentMent != "")
            {
                model = getArticle(currentMent, true);

                List<SelectListItem> listSelectListItems = new List<SelectListItem>();
                SelectListItem selectList = new SelectListItem()
                {
                    Text = "Подобається",
                    Value = "1",
                    Selected = true

                };
                listSelectListItems.Add(selectList);
                selectList = new SelectListItem()
                {
                    Text = "Не подобається",
                    Value = "2"

                };
                listSelectListItems.Add(selectList);

                model.TegList = listSelectListItems;
            }
            else
                model = new ArticleModel() { };
            return View("ShowArticles", model);
        }

        [HttpPost]
        public ActionResult ShowArticles(ArticleModel model)
        {
            string currentMent;
            if (Request.Cookies["MentorId"] != null)
                currentMent = Convert.ToString(Request.Cookies["MentorId"].Value);
            else currentMent = "";
            if (currentMent != "")
            {
                LoadArtcileIntoForm(model, currentMent, true);
                model.createdBy = currentMent;

                List<SelectListItem> listSelectListItems = new List<SelectListItem>();
                SelectListItem selectList = new SelectListItem()
                {
                    Text = "Подобається",
                    Value = "1",
                    Selected = true

                };
                listSelectListItems.Add(selectList);
                selectList = new SelectListItem()
                {
                    Text = "Не подобається",
                    Value = "2"

                };
                listSelectListItems.Add(selectList);

                model.TegList = listSelectListItems;
                LikeDislike(model);
            }


            return View("ShowArticles", model);
        }


        public void LikeDislike(ArticleModel model)
        {
            string currentMent;
            if (Request.Cookies["MentorId"] != null)
                currentMent = Convert.ToString(Request.Cookies["MentorId"].Value);
            else currentMent = "";
            if (currentMent != "")
            {
                string currentPerson;
                if (Request.Cookies["UserId"] != null)
                    currentPerson = Convert.ToString(Request.Cookies["UserId"].Value);
                else currentPerson = "user1";

                using (CustomDbContext db = new CustomDbContext())
                {
                    var m = db.ArticleModel.SingleOrDefault(x => x.articleID == model.articleID);
                    if (model.SelectedTeg != null)
                    {
                        if (model.SelectedTeg.FirstOrDefault() == "1")
                        {
                            if (m.whoLikes != null)
                            {
                                var whoLikes = m.whoLikes;
                                if (!m.whoLikes.Contains(currentPerson))
                                    m.whoLikes = whoLikes + currentPerson + " ";
                            }
                            else m.whoLikes = currentPerson + " ";
                            if (m.whoDislikes != null)
                                if (m.whoDislikes.Contains(currentPerson))
                                {
                                    var whoDislikes = m.whoDislikes;
                                    m.whoDislikes = whoDislikes.Replace(currentPerson, " ");
                                }
                        }
                        else if (model.SelectedTeg.FirstOrDefault() == "2")
                        {
                            if (m.whoDislikes != null)
                            {
                                var whoDislikes = m.whoDislikes;
                                if (!m.whoDislikes.Contains(currentPerson))
                                    m.whoDislikes = whoDislikes + currentPerson + " ";
                            }
                            else m.whoDislikes = currentPerson + " ";
                            if (m.whoLikes != null)
                                if (m.whoLikes.Contains(currentPerson))
                                {
                                    var whoLikes = m.whoLikes;
                                    m.whoLikes = whoLikes.Replace(currentPerson, " ");
                                }
                        }
                    }
                    if (m.whoLikes != null)
                        if (m.whoLikes.Contains(currentPerson))
                        {
                            model.youAreLikeThisArticle = true;
                            model.youAreDislikeThisArticle = false;
                        }
                        else model.youAreLikeThisArticle = false;
                    if (m.whoDislikes != null)
                        if (m.whoDislikes.Contains(currentPerson))
                        {
                            model.youAreDislikeThisArticle = true;
                            model.youAreLikeThisArticle = false;
                        }
                        else model.youAreDislikeThisArticle = false;
                    db.SaveChanges();
                }
            }
            //return View("ShowArticles", model);
        }
    }
}