using AutoMapper;
using Mhotivo.Data.Entities;
using Mhotivo.ParentSite.Models;
using Profile = Mhotivo.Data.Entities.Profile;

namespace Mhotivo.ParentSite
{
    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.CreateMap<Notification, NotificationModel>()
                .ForMember(p => p.NotificationCreator, o => o.MapFrom(src => src.NotificationCreator.FullName))
                .ForMember(p => p.CommentsAmount, o => o.MapFrom(src => src.NotificationComments.Count));
            Mapper.CreateMap<NotificationComment, NotificationCommentsModel>()
                .ForMember(p=>p.Tutor, o=>o.MapFrom(src=>src.Commenter.UserOwner.FullName))
                .ReverseMap();
            Mapper.CreateMap<Homework, HomeworkModel>()
                .ForMember(p => p.AcademicCourse, o => o.MapFrom(src => src.AcademicCourse.Course.Name));
            Mapper.CreateMap<Profile, ProfileDisplayModel>();
            Mapper.CreateMap<Event, EventDisplayModel>()
               .ForMember(p => p.EventDate, o => o.MapFrom(src => src.EventDate.ToShortDateString()))
               .ForMember(p => p.ScheduleTime, o => o.MapFrom(src => src.StartTime.ToString() + " - " + src.FinishTime.ToString()));
           
        }
    }
}