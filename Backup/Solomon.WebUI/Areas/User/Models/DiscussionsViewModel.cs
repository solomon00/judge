using Solomon.Domain.Abstract;
using Solomon.Domain.Entities;
using Solomon.TypesExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using System.Linq;
using Ninject;
using Solomon.WebUI.Infrastructure;

namespace Solomon.WebUI.Areas.User.ViewModels
{
    public class DiscussionsViewModel : UserViewModel
    {
        public IEnumerable<IGrouping<Tournament, Comment>> NewComments { get; set; }
    }

    public class AllCommentsViewModel
    {
        public PaginatedList<Comment> PaginatedCommentList { get; set; }
        public int PageSize { get; set; }
    }

    public class NewEventsJsonResponse
    {
        public int TotalCount { get; set; }
        public int CommentsCount { get; set; }
        public int InvitesCount { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
