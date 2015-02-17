using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using rirf.orm.Dao;

namespace rirf.orm.Dao
{
    public interface IDao : IDisposable
    {
        #region "Int32 insert(E entity);"
        /// <summary>
        /// Inserta la informacion de un objeto dentro de su entidad correspondiente en BD
        /// </summary>
        /// <param name="entity">Objeto que contiene la informacion a insertar sobre la BD</param>
        /// <returns>El id del registro que fue creado en BD, en caso de no existir ningun sequence asociado al Id se retorna un -1</returns>
        Int32 insert<E>(E entity);
        #endregion
        
        #region "Int32 delete(E entity);"
        /// <summary>
        /// Realiza un delete sobre la BD en base a la informacion del objeto recibido
        /// </summary>
        /// <param name="entity">Objeto que contiene la informacion con la que genera el Delete en BD</param>
        /// <returns>El total de registros afectados por el Delete</returns>
        Int32 delete<E>(E entity);
        #endregion

        #region "Int32 update(E entity);"
        /// <summary>
        /// Actualiza la informacion de un registro en BD
        /// </summary>
        /// <param name="entity">El objeto que contiene la informacion a actualizar</param>
        /// <returns>El total de objetos afectados</returns>
        Int32 update<E>(E entity);
        #endregion

        #region "Int32 insertOrUpdate<E>(E entity);"
        /// <summary>
        /// Inserta o actualiza la informacion de un registro en BD
        /// </summary>
        /// <param name="entity">El objeto que contiene la informacion a nisertar o actualizar</param>
        /// <returns>El total de registros afectados durante la operacion</returns>
        Int32 insertOrUpdate<E>(E entity);
        #endregion

        #region "Int32 executeNonQuery(String sqlQuery);"
        /// <summary>
        /// Ejecuta sentencias INSERT, UPDATE, DELETE predefinidas por el usuario.
        /// </summary>
        /// <param name="sqlQuery">Query a ejecutar</param>
        /// <returns>E total de registros afectados por la ejecucion de la sentencia en BD</returns>
        Int32 executeNonQuery(String sqlQuery);
        #endregion

        #region "DataSet executeDataSet(String sqlQuery);"
        /// <summary>
        /// Ejecuta una sentencia SQL devolviendo el resultado dentro de un DataSet
        /// </summary>
        /// <param name="sqlQuery">La sentencia a ejecutar</param>
        /// <returns>Un dataSet con la informacion obtenida como resultado de la sentencia</returns>
        DataSet executeDataSet(String sqlQuery);
        #endregion

        #region "IDataReader executeReader(String sqlQuery);"
        /// <summary>
        /// Ejecuta una sentencia SQL devolviendo el resultado dentro de un DataReader
        /// </summary>
        /// <param name="sqlQuery">La sentencia SQL a ejecutar</param>
        /// <returns>Un DataReader con la informacion obtenida como resultado de la sentencia</returns>
        IDataReader executeReader(String sqlQuery);
        #endregion

        #region "IList<E> list(Type entityType);"
        /// <summary>
        /// Lista la informacion contenida dentro de la entidad asociada al tipo de objeto recibido
        /// </summary>
        /// <param name="entityType">El tipo de objeto entidad al que se desea acceder en BD</param>
        /// <returns>Una lista de objetos del tipo recibido como parametro</returns>
        IList<E> list<E>();
        #endregion

        #region "E uniqueResult(E entity);"
        /// <summary>
        /// Obtiene un Objeto del tipo Especificado en base a los valores capturados dentro del objeto recibido como parametro, 
        /// si la sentencia genera mas de un registro como resultado se devuelve el primer registro recibido
        /// </summary>
        /// <param name="entity">El objeto que contiene los valores con el cual se filtra la informacion</param>
        /// <returns>Un objeto del tipo especificado con la informacion contenida en BD</returns>
        E uniqueResult<E>(E entity);
        #endregion

        #region "Criteria createCriteria(Type tipoObjeto);"
        /// <summary>
        /// Crea un objeto criteria asociando la conexion con la que se esta trabajando actualmente.
        /// </summary>
        /// <param name="tipoObjeto">El tipo de objeto con el cual trabajare Criteria</param>
        /// <returns>Un objeto de tipo Criteria</returns>
        Criteria<T> createCriteria<T>();
        #endregion

        #region "void executeStoreProcedure();"
        /// <summary>
        /// Ejecuta un Store Procedure dentro de la BD con los paretros especificados
        /// </summary>
        /// <param name="nombre">Nombre del Store Procedure a ejecutar</param>
        /// <param name="parametros">Parametros utilizados para la ejecucion del Store Procedure</param>
        /// <returns>Una lista con los parametros de salida arrojados por el Store</returns>
        void executeStoreProcedure(String nombre, IList<Parameter> parametros);
        #endregion

        #region "void BeginTransaction();"
        /// <summary>
        /// En caso de no existir una transaccion activa genera una nueva transaccion.
        /// </summary>
        void BeginTransaction();
        #endregion

        #region "void Commit();"
        /// <summary>
        /// En caso de existir una transaccion activa aplica Commit a las operaciones asociadas a la transaccion.
        /// </summary>
        void Commit();
        #endregion

        #region "void RollBack();"
        /// <summary>
        /// En caso de existir una transaccion activa aplica un RollBack a las operaciones asociadas a dicha transaccion.
        /// </summary>
        void RollBack();
        #endregion

        bool isDisposed();
    }
}
