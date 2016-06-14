using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mhotivo.Data.Entities;

namespace Mhotivo.Interface.Interfaces
{
    public interface ISliderRepository
    {
        Slider GetById(long id);
        Slider Create(Slider itemToCreate);
        IQueryable<Slider> Query(Expression<Func<Slider, Slider>> expression);
        IQueryable<Slider> Filter(Expression<Func<Slider, bool>> expression);
        Slider Update(Slider itemToUpdate);
        Slider Delete(Slider itemToDelete);
        Slider Delete(long id);
        IEnumerable<Slider> GetAllSliderPhotos();
    }
}