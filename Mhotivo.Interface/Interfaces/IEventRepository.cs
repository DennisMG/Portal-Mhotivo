using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mhotivo.Data.Entities;

namespace Mhotivo.Interface.Interfaces
{
    public interface IEventRepository
    {
        Event GetById(long id);
        Event Create(Event itemToCreate);
        IQueryable<Event> Query(Expression<Func<Event, Event>> expression);
        IQueryable<Event> Filter(Expression<Func<Event, bool>> expression);
        Event Update(Event itemToUpdate);
        Event Delete(Event itemToDelete);
        Event Delete(long id);
        IEnumerable<Event> GetAllEvents();
    }
}