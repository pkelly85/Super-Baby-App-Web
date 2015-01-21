using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Portal.Model;

namespace Portal.Repository
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        private readonly SuperBabyEntities _context;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(SuperBabyEntities context)
        { 
            //Comment
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            try
            {
                IQueryable<TEntity> query = _dbSet;

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                query = includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Aggregate(query,
                                                    (current, includeProperty) => current.Include(includeProperty));

                return orderBy != null ? orderBy(query).ToList() : query.ToList();
            }
            catch (Exception)
            {
                throw new Exception("Database is not available");
            }
        }

        public virtual TEntity Find(object id)
        {
            return _dbSet.Find(id);
        }

        public virtual void Add(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Remove(object id)
        {
            TEntity entityToDelete = _dbSet.Find(id);
            Remove(entityToDelete);
        }

        public virtual void Remove(TEntity entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            // _dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual void Save()
        {
            _context.SaveChanges();
        }

    }    
}
