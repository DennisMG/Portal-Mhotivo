﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mhotivo.Data.Entities
{
    public enum NotificationType
    {
        [Description("General")]
        General = 1, //No destination ID needed
        [Description("Nivel de Educación")]
        EducationLevel = 2, //EducationLevel ID needed
        [Description("Grado")]
        Grade = 3, //Grade ID needed
        [Description("Sección")]
        Section= 4, //AcademicGrade ID needed
        [Description("Materia")]
        Course = 5, //AcademicCourse ID needed
        [Description("Tutor")]
        Student = 6, //Student ID needed
        [Description("Personal")]
        Personal = 7, //Personal Message
    }

    public class Notification
    {

        public Notification()
        {
            NotificationComments = new HashSet<NotificationComment>();
            RecipientUsers = new HashSet<User>();
            CreationDate = DateTime.UtcNow;
        }

        

        public Notification(string subject, string message, PeopleWithUser @from,PeopleWithUser to, NotificationType personal, AcademicYear academicYear)
        {
            AcademicYear = academicYear;
            Title = subject;
            Message = message;
            NotificationCreator = @from;
            To = to;
            NotificationType = personal;
            NotificationComments = new HashSet<NotificationComment>();
            RecipientUsers = new HashSet<User>();
            CreationDate = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string PeopleDirectedTo { get; set; }
        public NotificationType NotificationType { get; set; }
        public long DestinationId { get; set; }
        public virtual PeopleWithUser To { get; set; }
        public virtual PeopleWithUser NotificationCreator { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Approved { get; set; }
        public bool Sent { get; set; }
        public bool SendEmail { get; set; }
        public virtual AcademicYear AcademicYear { get; set; } //used to show only pertinent 
        public virtual ICollection<NotificationComment> NotificationComments { get; set; }
        public virtual ICollection<User> RecipientUsers { get; set; }
    }
}
