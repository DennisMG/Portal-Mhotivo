using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mhotivo.Data.Entities;

namespace Mhotivo.Interface.Interfaces
{
    public interface IProfileRepository
    {
        Profile GetById(long id);
        Profile Create(Profile itemToCreate);
        IQueryable<Profile> Query(Expression<Func<Profile, Profile>> expression);
        IQueryable<Profile> Filter(Expression<Func<Profile, bool>> expression);
        Profile Update(Profile itemToUpdate);
        Profile Delete(Profile itemToDelete);
        Profile Delete(long id);
        IEnumerable<Profile> GetAllProfiles();
    }
}