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
    public class ArticleData
    {
        public string inputArticleTitle { get; set; }
        public string inputArticleText { get; set; }
        public bool isPublished { get; set; }
        public string[] tagList { get; set; }
    }

    public class ArticleController : Controller
    {
        public IEnumerable<ArticleModel> getArticles(string currentPerson, bool isPublsihed)
        {
            List<ArticleModel> listOfArticles = new List<ArticleModel>();
            using (CustomDbContext db = new CustomDbContext())
            {
                listOfArticles.AddRange(db.ArticleModel.Where(x => x.createdBy == currentPerson && x.isPublished == isPublsihed));
            }
            
            foreach(ArticleModel article in listOfArticles)
            {
                if (article.tagList != "" && article.tagList != null)
                {
                    article.chosenTags = new List<String>();
                    article.chosenTags.AddRange(article.tagList.Split('|'));
                }
            }
            
            return listOfArticles;
        }

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
            
            IEnumerable<ArticleModel> myModel = getArticles(currentPerson, false);
            if (Request.IsAjaxRequest())
                return PartialView("_ArticlesPartial", myModel);
            return View("MentorArticle", myModel);
        }

        [HttpGet]
        public ActionResult EditArticle(ArticleModel model)
        {
            return View("CreateArticle", model);
        }


        public void LoadArtcileIntoForm(ArticleModel model, string currentPerson, bool onlyPublished)
        {
            //FacadePattern.Facade f = new FacadePattern.Facade(model, currentPerson, onlyPublished);
            //f.workWithSelectedTeg();
            //f.workWithDb();


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
        public ActionResult SaveArticle(ArticleData articleData)//ArticleModel model
        {
            //Decorator.SavaArticle d = new Decorator.SaveToDraft(new Decorator.ImplementSave());
            //d.saveToDb(model);
            //Singleton s = Singleton.Instance;
            string currentPerson;// = s.user;
            string tegString = "";
            IEnumerable<string> selectedTegs = articleData.tagList;
            foreach (var item in selectedTegs)
            {
                tegString += item + "|";
                Console.WriteLine(item);
            }
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
                            tagList = tegString,
                            dateOfCreation = DateTime.Now,
                            dateOfLastEditing = DateTime.Now,
                            isPublished = articleData.isPublished,
                        });
                        db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("Index");
        }

        
        public ActionResult PublishArticle(string articleId)//ArticleModel model
        {
            //Decorator.SavaArticle d = new Decorator.Publish(new Decorator.ImplementSave());
            //d.saveToDb(model);
            using (CustomDbContext db = new CustomDbContext())
            {
                var myModel = db.ArticleModel.FirstOrDefault(x => x.articleID == articleId);
                myModel.isPublished = true;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        
        public ActionResult DeleteArticle(string articleId)
        {
            //using (CustomDbContext db = new CustomDbContext())
            //{
            //    var modelToDelete = db.ArticleModel.FirstOrDefault(x => x.articleID == articleId);
            //    db.ArticleModel.Remove(modelToDelete);
            //    db.SaveChanges();
            //}
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ShowArticles(bool isPublsihed)
        {
            //string currentMent;
            //if (Request.Cookies["MentorId"] != null)
            //    currentMent = Convert.ToString(Request.Cookies["MentorId"].Value);
            //else currentMent = "";
            string currentPerson;// = s.user;
            IEnumerable<ArticleModel> myModel = null;
            if (Request.Cookies["UserId"] != null) //если данные о пользователе записаны в куки
            {
                currentPerson = Convert.ToString(Request.Cookies["UserId"].Value);
                myModel = getArticles(currentPerson, false);
                if (Request.IsAjaxRequest())
                    return PartialView("_ArticlesPartial", myModel);
                return View("MentorArticle", myModel);
            }
            return View("MentorArticle", myModel);
            //if (currentMent != "")
            //{
            //    LoadArtcileIntoForm(model, currentMent, true);
            //    model.createdBy = currentMent;

            //    List<SelectListItem> listSelectListItems = new List<SelectListItem>();
            //    SelectListItem selectList = new SelectListItem()
            //    {
            //        Text = "Подобається",
            //        Value = "1",
            //        Selected = true

            //    };
            //    listSelectListItems.Add(selectList);
            //    selectList = new SelectListItem()
            //    {
            //        Text = "Не подобається",
            //        Value = "2"

            //    };
            //    listSelectListItems.Add(selectList);

            //    model.TegList = listSelectListItems;
            //    LikeDislike(model);
            //}


            //return View("ShowArticles", model);
        }
        //public ActionResult ShowArticles()
        //{
        //    ArticleModel model;
        //    string currentMent;
        //    if (Request.Cookies["MentorId"] != null)
        //        currentMent = Convert.ToString(Request.Cookies["MentorId"].Value);
        //    else currentMent = "";
        //    if (currentMent != "")
        //    {
        //        model = getArticles(currentMent, true);

        //        List<SelectListItem> listSelectListItems = new List<SelectListItem>();
        //        SelectListItem selectList = new SelectListItem()
        //        {
        //            Text = "Подобається",
        //            Value = "1",
        //            Selected = true

        //        };
        //        listSelectListItems.Add(selectList);
        //        selectList = new SelectListItem()
        //        {
        //            Text = "Не подобається",
        //            Value = "2"

        //        };
        //        listSelectListItems.Add(selectList);

        //        model.TegList = listSelectListItems;
        //    }
        //    else
        //        model = new ArticleModel() { };
        //    return View("ShowArticles", model);
        //}

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