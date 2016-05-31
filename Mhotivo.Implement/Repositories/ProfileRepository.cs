using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Mhotivo.Data.Entities;
using Mhotivo.Implement.Context;
using Mhotivo.Interface.Interfaces;

public class ProfileRepository: IProfileRepository
{
    private readonly MhotivoContext _context;

    public ProfileRepository(MhotivoContext ctx)
    {
        _context = ctx;
    }

    public Profile GetById(long id)
    {
        return _context.Profiles.FirstOrDefault(x => x.Id == id);
    }

    public Profile Create(Profile itemToCreate)
    {
        var profile = _context.Profiles.Add(itemToCreate);
        _context.SaveChanges();
        return profile;
    }

    public IQueryable<Profile> Query(Expression<Func<Profile, Profile>> expression)
    {
        return _context.Profiles.Select(expression);
    }

    public IQueryable<Profile> Filter(Expression<Func<Profile, bool>> expression)
    {
        return _context.Profiles.Where(expression);
    }

    public Profile Update(Profile itemToUpdate)
    {
        _context.Entry(itemToUpdate).State = EntityState.Modified;
        _context.SaveChanges();
        return itemToUpdate;
    }

    public Profile Delete(Profile itemToDelete)
    {
        _context.Profiles.Remove(itemToDelete);
        _context.SaveChanges();
        return itemToDelete;
    }


    public Profile Delete(long id)
    {
        var itemToDelete = GetById(id);
        _context.Profiles.Remove(itemToDelete);
        _context.SaveChanges();
        return itemToDelete;
    }

    public IEnumerable<Profile> GetAllProfiles()
    {
        return Query(g => g).ToList();
    }
}