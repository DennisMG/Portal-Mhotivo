using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Mhotivo.Data.Entities;
using Mhotivo.Implement.Context;
using Mhotivo.Interface.Interfaces;

namespace Mhotivo.Implement.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly MhotivoContext _context;

        public EventRepository(MhotivoContext ctx)
        {
            _context = ctx;
        }

        public Event GetById(long id)
        {
            return _context.Events.FirstOrDefault(x => x.Id == id);
        }

        public Event Create(Event itemToCreate)
        {
            var @event = _context.Events.Add(itemToCreate);
            _context.SaveChanges();
            return @event;
        }

        public IQueryable<Event> Query(Expression<Func<Event, Event>> expression)
        {
            return _context.Events.Select(expression);
        }

        public IQueryable<Event> Filter(Expression<Func<Event, bool>> expression)
        {
            return _context.Events.Where(expression);
        }

        public Event Update(Event itemToUpdate)
        {
            _context.Entry(itemToUpdate).State = EntityState.Modified;
            _context.SaveChanges();
            return itemToUpdate;
        }

        public Event Delete(Event itemToDelete)
        {
            _context.Events.Remove(itemToDelete);
            _context.SaveChanges();
            return itemToDelete;
        }


        public Event Delete(long id)
        {
            var itemToDelete = GetById(id);
            _context.Events.Remove(itemToDelete);
            _context.SaveChanges();
            return itemToDelete;
        }

        public IEnumerable<Event> GetAllEvents()
        {
            return Query(g => g).ToList();
        }
    }
}