using log4net;
using log4net.Config;
using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using Solomon.WebUI.Areas.User.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace Solomon.WebUI.Areas.User.Controllers
{
    [Authorize]
    public class DiscussionsController : Controller
    {
        private IRepository repository;
        private readonly ILog logger = LogManager.GetLogger(typeof(DiscussionsController));
        /// <summary>
        /// Controller constructor.
        /// Initialize repository.
        /// </summary>
        /// <param name="Repository">Repository object</param>
        public DiscussionsController(IRepository Repository)
        {
            XmlConfigurator.Configure();
            repository = Repository;
        }

        public ActionResult Index()
        {
            int userID = WebSecurity.CurrentUserId;
            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);

            IEnumerable<Comment> comments = repository.Comments.Where(c => c.IsRead == 0 && c.UserID != userID).AsEnumerable();

            if (userID != 1 && !Roles.IsUserInRole("Administrator"))
            {
                if (Roles.IsUserInRole("Judge"))
                {
                    comments = comments
                        .Where(c => user.CanModifyProblems.Contains(c.Problem) || user.CanModifyTournaments.Contains(c.Tournament) ||
                                    (c.ParentComment != null ? c.ParentComment.UserID == userID : false));

                }
                else
                {
                    comments = comments
                        .Where(c => c.ParentComment != null ? c.ParentComment.UserID == userID : false);
                }
            }

            DiscussionsViewModel viewModel = new DiscussionsViewModel() { NewComments = comments.GroupBy(c => c.Tournament) };

            return View(viewModel);
        }

        [HttpGet]
        [Authorize]
        public JsonResult GetEventsCount()
        {
            NewEventsJsonResponse response = new NewEventsJsonResponse();
            response.Success = true;

            int userID = WebSecurity.CurrentUserId;
            UserProfile user = repository
                .Users
                .FirstOrDefault(u => u.UserId == userID);

            #region Comments
            IEnumerable<Comment> comments = repository.Comments.Where(c => c.IsRead == 0 && c.UserID != userID).AsEnumerable();

            if (userID != 1 && !Roles.IsUserInRole("Administrator"))
            {
                if (Roles.IsUserInRole("Judge"))
                {
                    comments = comments
                        .Where(c => user.CanModifyProblems.Contains(c.Problem) || user.CanModifyTournaments.Contains(c.Tournament) ||
                                    (c.ParentComment != null ? c.ParentComment.UserID == userID : false));

                }
                else
                {
                    comments = comments
                        .Where(c => c.ParentComment != null ? c.ParentComment.UserID == userID : false);
                }
            }

            response.CommentsCount = comments.Count();
            #endregion

            response.InvitesCount = user.Teams.Count(t => t.Confirm == 0);

            response.TotalCount = response.CommentsCount + response.InvitesCount;

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult AllComments(int Page = 1, int PageSize = 25)
        {
            AllCommentsViewModel viewModel = new AllCommentsViewModel();

            // New search term
            if (System.Web.HttpContext.Current.Request.HttpMethod == "POST")
            {
                Page = 1;
            }

            if (PageSize == 0)
                PageSize = 25;

            viewModel.PageSize = PageSize;

            IQueryable<Comment> comments = repository.Comments;
            viewModel.PaginatedCommentList = comments.OrderByDescending(c => c.Date).ToPaginatedList<Comment>(Page, PageSize);
            
            return View(viewModel);
        }
    }
}
