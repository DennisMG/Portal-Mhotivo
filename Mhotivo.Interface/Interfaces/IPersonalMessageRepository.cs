using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mhotivo.Data.Entities;

namespace Mhotivo.Interface.Interfaces
{
    public interface IPersonalMessageRepository
    {
        PersonalMessage GetById(long id);
        PersonalMessage Create(PersonalMessage itemToCreate);
        IQueryable<PersonalMessage> Query(Expression<Func<PersonalMessage, PersonalMessage>> expression);
        IQueryable<PersonalMessage> Filter(Expression<Func<PersonalMessage, bool>> expression);
        PersonalMessage Update(PersonalMessage itemToUpdate);
        PersonalMessage Delete(PersonalMessage itemToDelete);
        PersonalMessage Delete(long id);
        IEnumerable<PersonalMessage> GetAllPersonalMessages();
    }
}