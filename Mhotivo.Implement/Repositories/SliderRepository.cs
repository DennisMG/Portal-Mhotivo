using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Mhotivo.Data.Entities;
using Mhotivo.Implement.Context;
using Mhotivo.Interface.Interfaces;

namespace Mhotivo.Implement.Repositories
{
    public class SliderRepository: ISliderRepository
    {
        private readonly MhotivoContext _context;

        public SliderRepository(MhotivoContext ctx)
        {
            _context = ctx;
        }

        public Slider GetById(long id)
        {
            return _context.SliderPhotos.FirstOrDefault(x => x.Id == id);
        }

        public Slider Create(Slider itemToCreate)
        {
            var sliderPhoto = _context.SliderPhotos.Add(itemToCreate);
            _context.SaveChanges();
            return sliderPhoto;
        }

        public IQueryable<Slider> Query(Expression<Func<Slider, Slider>> expression)
        {
            return _context.SliderPhotos.Select(expression);
        }

        public IQueryable<Slider> Filter(Expression<Func<Slider, bool>> expression)
        {
            return _context.SliderPhotos.Where(expression);
        }

        public Slider Update(Slider itemToUpdate)
        {
            _context.Entry(itemToUpdate).State = EntityState.Modified;
            _context.SaveChanges();
            return itemToUpdate;
        }

        public Slider Delete(Slider itemToDelete)
        {
            _context.SliderPhotos.Remove(itemToDelete);
            _context.SaveChanges();
            return itemToDelete;
        }


        public Slider Delete(long id)
        {
            var itemToDelete = GetById(id);
            _context.SliderPhotos.Remove(itemToDelete);
            _context.SaveChanges();
            return itemToDelete;
        }

        public IEnumerable<Slider> GetAllSliderPhotos()
        {
            return Query(g => g).ToList();
        }
    }
}
