using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using rirf.orm.Dao;

namespace rirf.orm.Dao
{
    public abstract class DaoBase : IDao
    {
        #region IDao<Entity> Members

        public abstract int insert<T>(T entity);
        public abstract Int32 delete<T>(T entity);
        public abstract Int32 update<T>(T entity);
        public abstract int insertOrUpdate<T>(T entity);
        public abstract Int32 executeNonQuery(String sqlQuery);
        public abstract DataSet executeDataSet(string sqlQuery);
        public abstract IDataReader executeReader(string sqlQuery);
        public abstract IList<T> list<T>();
        public abstract T uniqueResult<T>(T entity);
        public abstract void BeginTransaction();
        public abstract void Commit();
        public abstract void RollBack();
        public abstract void Dispose();
        public abstract Criteria<E> createCriteria<E>();
        public abstract void executeStoreProcedure(string nombre, IList<Parameter> parametros);
        public abstract bool isDisposed();
        #endregion
    }
}
