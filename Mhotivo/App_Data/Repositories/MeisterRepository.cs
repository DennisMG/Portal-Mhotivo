﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Mhotivo.Models;
using System.Data.Entity;

namespace Mhotivo.App_Data.Repositories
{
    public interface IMeisterRepository : IDisposable
    {
        Meister First(Expression<Func<Meister, Meister>> query);
        Meister GetById(long id);
        Meister Create(Meister itemToCreate);
        IQueryable<Meister> Query(Expression<Func<Meister, Meister>> expression);
        IQueryable<Meister> Filter(Expression<Func<Meister, bool>> expression);
        Meister Update(Meister itemToUpdate);
        Meister Delete(long id);
        void SaveChanges();
    }

    public class MeisterRepository : IMeisterRepository
    {
        private readonly MhotivoContext _context;

        public MeisterRepository(MhotivoContext ctx)
        {
            _context = ctx;
        }

        public Meister First(Expression<Func<Meister, Meister>> query)
        {
            var meisters = _context.Meisters.Select(query);
            return meisters.Count() != 0 ? meisters.First() : null;
        }

        public Meister GetById(long id)
        {
            var meisters = _context.Meisters.Where(x => x.PeopleId == id);
            return meisters.Count() != 0 ? meisters.First() : null;
        }

        public Meister Create(Meister itemToCreate)
        {
            var meister = _context.Meisters.Add(itemToCreate);
            _context.SaveChanges();
            return meister;
        }

        public IQueryable<Meister> Query(Expression<Func<Meister, Meister>> expression)
        {
            return _context.Meisters.Select(expression);
        }

        public IQueryable<Meister> Filter(Expression<Func<Meister, bool>> expression)
        {
            return _context.Meisters.Where(expression);

        }

        public Meister Update(Meister itemToUpdate)
        {
            _context.SaveChanges();
            return itemToUpdate;
        }

        public void Detach(Meister meister)
        {
            _context.Entry(meister).State = EntityState.Detached;
        }

        public Meister Delete(long id)
        {
            var itemToDelete = GetById(id);
            _context.Meisters.Remove(itemToDelete);
            _context.SaveChanges();
            return itemToDelete;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}