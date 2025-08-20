using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMgmt.DAL.Interfaces
{
    public interface IRepo<T> where T : class
    {
        IEnumerable<T> GetAll();
        T? Get(int id);
        bool Create(T entity);
        bool Update(T entity);
        bool Delete(T entity);
        bool Exists(Expression<Func<T, bool>> predicate);
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        public int Count(Expression<Func<T, bool>>? predicate = null);
    }
}
