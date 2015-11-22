﻿using System;

namespace Mhotivo.ParentSite.Models
{
    public class NotificationCommentsModel
    {
        public int Id { get; set; }
        public string CommentText { get; set; }
        public DateTime CreationDate { get; set; }
        public string Tutor { get; set; }
        public byte[] Photo { get; set; }
    }
}