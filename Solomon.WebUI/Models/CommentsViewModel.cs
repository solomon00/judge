using Solomon.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Solomon.WebUI.ViewModels
{
    public class CommentViewModel : Comment
    {
        // Highlighted comments
        public bool IsNewComment { get; set; }

        public CommentViewModel(Comment comment, bool IsNewComment = false)
        {
            this.ChildComments = comment.ChildComments;
            this.CommentID = comment.CommentID;
            this.Date = comment.Date;
            this.EditedByUser = comment.EditedByUser;
            this.EditedByUserID = comment.EditedByUserID;
            this.EditingReason = comment.EditingReason;
            this.IsRead = comment.IsRead;
            this.Level = comment.Level;
            this.OldValue = comment.OldValue;
            this.ParentComment = comment.ParentComment;
            this.ParentCommentID = comment.ParentCommentID;
            this.Problem = comment.Problem;
            this.ProblemID = comment.ProblemID;
            this.Public = comment.Public;
            this.Tournament = comment.Tournament;
            this.TournamentID = comment.TournamentID;
            this.User = comment.User;
            this.UserID = comment.UserID;
            this.Value = comment.Value;
            this.IsNewComment = IsNewComment;
        }
    }

    public class CommentsViewModel
    {
        public int ProblemID { get; set; }
        public int TournamentID { get; set; }
        public bool ShowAll { get; set; }
        public List<CommentViewModel> Comments { get; set; }
    }

    public class AddCommentViewModel
    {
        public int ProblemID { get; set; }
        public int TournamentID { get; set; }
        public int ParentCommentID { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "Обязательное поле")]
        public string Value { get; set; }
    }
}