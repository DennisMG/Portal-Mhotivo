using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mhotivo.Models
{
    public class MessageCommentDisplayModel
    {
        public long Id { get; set; }

        [Display(Name = "Usuario")]
        public string Commenter { get; set; }

        [Display(Name = "Comentario")]
        public string CommentText { get; set; }

        [Display(Name = "Fecha")]
        public string CreationDate { get; set; }
    }

    public class MessageCommentRegisterModel
    {

        [Display(Name = "Usuario")]
        public long Commenter { get; set; }

        [Display(Name = "Comentario")]
        public string CommentText { get; set; }

        public long Notification { get; set; }
    }

    public class MessageCommentEditModel
    {
        public long Id { get; set; }

        [Display(Name = "Comentario")]
        public string CommentText { get; set; }
    }
}