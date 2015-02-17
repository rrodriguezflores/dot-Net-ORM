using System;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;

using Microsoft.Practices.EnterpriseLibrary.Data;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using rirf.orm.Dao.Attributes;
using rirf.orm.Dao;

namespace rirf.orm.Dao
{
    public class DaoImpl : DaoBase
    {
        private DbConnection conn;
        private bool isTransaccionActiva;
        private Database db;
        private DbTransaction tx;
        private bool disposed;

        #region "public Dao()"
        /// <summary>
        /// Constructor por defecto, genera un objeto de tipo Dao generando una conexion a BD configurada por defecto 
        /// dentro del archivo de configuracion de la aplicacion.
        /// </summary>
        public DaoImpl() 
        {
            isTransaccionActiva = false;
            this.db = DatabaseFactory.CreateDatabase();
            conn = db.CreateConnection();
            conn.Open();
        }
        #endregion

        #region "public Dao(String dbConnectionName)"
        /// <summary>
        /// Constructor que genera un objeto de tipo Dao generando una conexion a BD determinada dentro del archivo de 
        /// configuracion de la aplicacion por el nombre recibido como parametro.
        /// </summary>
        /// <param name="dbName">Nombre de la conexion configurada dentro del archivo de configuracion de la aplicacio</param>
        public DaoImpl(String dbConnectionName)
        {
            isTransaccionActiva = false;
            this.db = DatabaseFactory.CreateDatabase(dbConnectionName);
            conn = db.CreateConnection();
            conn.Open();
        }
        #endregion

        #region IDao<Entity> Members

        #region "public Int32 insert(Entity entity)"
        /// <summary>
        /// Inserta la informacion de un objeto dentro de su entidad correspondiente en BD
        /// </summary>
        /// <param name="entity">Objeto que contiene la informacion a insertar sobre la BD</param>
        /// <returns>El id del registro que fue creado en BD, en caso de no existir ningun sequence asociado al Id se retorna un -1</returns>
        public override int insert<T>(T entity)
        {
            int resultado = -1;
            String nombreEntidad = "";
            String campoId = "";
            object[] attributos = null;
            System.Attribute[] attrs = null;
            StringBuilder values = new StringBuilder("VALUES(");
            StringBuilder queryInsert = new StringBuilder("INSERT INTO ");

            //Se agrega el nombre de la entidad a la que sera aplicado el insert
            nombreEntidad = getNombreEntidad(entity);
            queryInsert.Append(nombreEntidad).Append("(");

            //Se recorren cada uno de los atributos que pertenecen a la entidad para determinar los mapeos a BD
            attributos = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            if (attributos == null || attributos.Length == 0)
                throw new Exception("El objeto <<" + entity.GetType().Name + ">> no cuenta con ningun campo que pueda ser mapeado a la BD");

            //Por cada uno de los campos de la entidad se obtienen sus atributos
            foreach (FieldInfo info in attributos)
            {
                if (info.GetValue(entity) != null)
                {
                    attrs = System.Attribute.GetCustomAttributes(info);
                    if (attrs != null && attrs.Length > 0)
                        foreach (Attribute attr in attrs)
                            if (attr is Column || attr is Id)
                            {
                                if (attr is Id)
                                    campoId = info.Name;
                                values.Append(":").Append(info.Name).Append(", ");
                                queryInsert.Append(((DBMapping)attr).DbName).Append(", ");
                            }
                }
            }

            queryInsert = queryInsert.Remove(queryInsert.Length - 2, 2).Append(") ");
            values = values.Remove(values.Length - 2, 2).Append(") ");
            queryInsert.Append(values.ToString());

            System.Console.WriteLine(queryInsert.ToString());

            //Se manda a agregar los valores de los campos a insertar dentro del query de insersion generado.
            DbCommand cmd = db.GetSqlStringCommand(queryInsert.ToString());
            resultado = setParametersToCommand<T>(entity, nombreEntidad, cmd, db, false, false);
            if (isTransaccionActiva)
                cmd.Transaction = tx;
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();

            //Se marca el objeto como cargado de BD
            if(resultado != -1)
                typeof(T).GetField(campoId, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(entity, resultado);
            typeof(T).GetField("isFromDB", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(entity, true);

            return resultado;
        }
        #endregion

        #region "public Int32 delete(Entity entity)"
        /// <summary>
        /// Realiza un delete sobre la BD en base a la informacion del objeto recibido
        /// </summary>
        /// <param name="entity">Objeto que contiene la informacion con la que genera el Delete en BD</param>
        /// <returns>El total de registros afectados por el Delete</returns>
        public override Int32 delete<T>(T entity)
        {
            Int32 resultado = -1;
            object[] attributos = null;
            System.Attribute[] attrs = null;
            String tipoCondicion = "WHERE";
            StringBuilder condicion = new StringBuilder();
            StringBuilder queryDelete = new StringBuilder("DELETE FROM ");

            //Se agrega el nombre de la entidad a la que sera aplicado el insert
            queryDelete.Append(getNombreEntidad(entity)).Append(" ");

            //Se recorren cada uno de los atributos que pertenecen a la entidad para determinar los mapeos a BD
            attributos = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            if (attributos != null || attributos.Length > 0)
                //Por cada uno de los campos de la entidad se obtienen sus atributos
                foreach (FieldInfo info in attributos)
                    if (info.GetValue(entity) != null)
                    {
                        attrs = System.Attribute.GetCustomAttributes(info);
                        if (attrs != null && attrs.Length > 0)
                            foreach (Attribute attr in attrs)
                                if (attr is Column || attr is Id)
                                {
                                    queryDelete.Append(" ").Append(tipoCondicion).Append(" ").Append(((DBMapping)attr).DbName).Append(" = ");
                                    queryDelete.Append(":").Append(info.Name).Append(" ");
                                    tipoCondicion = "AND";
                                }
                    }

            System.Console.WriteLine(queryDelete.ToString());
            //Se manda a agregar los valores de los campos dentro del query de DELETE generado.
            DbCommand cmd = db.GetSqlStringCommand(queryDelete.ToString());
            setParametersToCommand(entity, getNombreEntidad(entity), cmd, db, false, false);
            if (isTransaccionActiva)
                cmd.Transaction = tx;
            cmd.Connection = conn;
            resultado = cmd.ExecuteNonQuery();
            return resultado;
        }
        #endregion

        #region "public Int32 update(Entity entity)"
        /// <summary>
        /// Actualiza la informacion de un registro en BD
        /// </summary>
        /// <param name="entity">El objeto que contiene la informacion a actualizar</param>
        /// <returns>El total de objetos afectados</returns>
        public override Int32 update<T>(T entity)
        {
            Int32 resultado = -1;
            object[] attributos = null;
            System.Attribute[] attrs = null;
            String tipoCondicion = "WHERE";
            StringBuilder condicion = new StringBuilder();
            StringBuilder queryUpdate = new StringBuilder("UPDATE ");

            //Se agrega el nombre de la entidad a la que sera aplicado el insert
            queryUpdate.Append(getNombreEntidad(entity)).Append(" SET ");

            //Se recorren cada uno de los atributos que pertenecen a la entidad para determinar los mapeos a BD
            attributos = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            if (attributos == null || attributos.Length == 0)
                throw new Exception("El objeto <<" + entity.GetType().Name + ">> no cuenta con ningun campo que pueda ser mapeado a la BD");

            //Por cada uno de los campos de la entidad se obtienen sus atributos
            foreach (FieldInfo info in attributos)
            {
                attrs = System.Attribute.GetCustomAttributes(info);
                if (attrs != null && attrs.Length > 0)
                    foreach (Attribute attr in attrs)
                        if (attr is Id || attr is Fk)
                        {
                            condicion.Append(" ").Append(tipoCondicion).Append(" ").Append(((DBMapping)attr).DbName).Append(" = ");
                            condicion.Append(":").Append(info.Name).Append(" ");
                            tipoCondicion = "AND";
                        }
                        else if (attr is Column)
                        {
                            queryUpdate.Append(((DBMapping)attr).DbName).Append(" = ");
                            queryUpdate.Append(":").Append(info.Name).Append(", ");
                        }
            }

            queryUpdate = queryUpdate.Remove(queryUpdate.Length - 2, 2);
            queryUpdate.Append(condicion.ToString());

            System.Console.WriteLine(queryUpdate.ToString());
            //Se manda a actualizar los valores de los campos dentro del query de UPDATE.
            DbCommand cmd = db.GetSqlStringCommand(queryUpdate.ToString());
            setParametersToCommand(entity, getNombreEntidad(entity), cmd, db, false, true);
            if (isTransaccionActiva)
                cmd.Transaction = tx;
            cmd.Connection = conn;
            resultado = cmd.ExecuteNonQuery();
            
            return resultado;
        }
        #endregion

        #region "public override int insertOrUpdate<T>(T entity)"
        /// <summary>
        /// Inserta o actualiza la informacion de un registro en BD
        /// </summary>
        /// <param name="entity">El objeto que contiene la informacion a nisertar o actualizar</param>
        /// <returns>El total de registros afectados por la operacion</returns>
        public override int insertOrUpdate<T>(T entity)
        {
            int resultado = -1;
            //Se valida si el objeto ya existe en BD
            if ( Convert.ToBoolean(typeof(T).GetField("isFromDB", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entity)) )
                //Se actualiza la informacion del objeto en BD
                resultado = this.update(entity);
            else 
            {
                //Se inserta la informacion del objeto en BD
                this.insert(entity);
                resultado = 1;
            }

            return resultado;
        }
        #endregion

        #region "public Int32 executeNonQuery(String sqlQuery)"
        /// <summary>
        /// Ejecuta sentencias INSERT, UPDATE, DELETE predefinidas por el usuario.
        /// </summary>
        /// <param name="sqlQuery">Query a ejecutar</param>
        /// <returns>E total de registros afectados por la ejecucion de la sentencia en BD</returns>
        public override Int32 executeNonQuery(String sqlQuery)
        {
            int resultado = -1;
            DbCommand cmd = db.GetSqlStringCommand(sqlQuery);
            if (isTransaccionActiva)
                cmd.Transaction = tx;
            cmd.Connection = conn;
            resultado = cmd.ExecuteNonQuery();
            return resultado;
        }
        #endregion

        #region "public System.Data.DataSet executeDataSet(string sqlQuery)"
        /// <summary>
        /// Ejecuta una sentencia SQL devolviendo el resultado dentro de un DataSet
        /// </summary>
        /// <param name="sqlQuery">La sentencia a ejecutar</param>
        /// <returns>Un dataSet con la informacion obtenida como resultado de la sentencia</returns>
        public override DataSet executeDataSet(string sqlQuery)
        {
            DataSet ds = null;

            DbCommand cmd = db.GetSqlStringCommand(sqlQuery);
            ds = db.ExecuteDataSet(cmd);
            return ds;
        }
        #endregion

        #region "public System.Data.IDataReader executeReader(string sqlQuery)"
        /// <summary>
        /// Ejecuta una sentencia SQL devolviendo el resultado dentro de un DataReader
        /// </summary>
        /// <param name="sqlQuery">La sentencia SQL a ejecutar</param>
        /// <returns>Un DataReader con la informacion obtenida como resultado de la sentencia</returns>
        public override IDataReader executeReader(string sqlQuery)
        {
            IDataReader dr = null;

            DbCommand cmd = db.GetSqlStringCommand(sqlQuery);
            dr = db.ExecuteReader(cmd);
            return dr;
        }
        #endregion

        #region "public IList<Entity> list(Type entityType)"
        /// <summary>
        /// Lista la informacion contenida dentro de la entidad asociada al tipo de objeto recibido
        /// </summary>
        /// <param name="entityType">El tipo de objeto entidad al que se desea acceder en BD</param>
        /// <returns>Una lista de objetos del tipo recibido como parametro</returns>
        public override IList<T> list<T>()
        {
            T aux = default(T);
            object[] attributos = null;
            System.Attribute[] attrs = null;
            IList<T> resultado = new List<T>();
            StringBuilder querySelect = new StringBuilder("SELECT ");

            //Se genera un objeto del tipo que se desea obtener
            T entity = (T)Activator.CreateInstance(typeof(T));

            //Se recorren cada uno de los atributos que pertenecen a la entidad para determinar los mapeos a BD
            attributos = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            if (attributos == null || attributos.Length == 0)
                throw new Exception("El objeto <<" + entity.GetType().Name + ">> no cuenta con ningun campo que pueda ser mapeado a la BD");

            List<KeyValuePair<String, Type>> campos = new List<KeyValuePair<string, Type>>();
            //Por cada uno de los campos de la entidad se obtienen sus atributos
            foreach (FieldInfo info in attributos)
            {
                attrs = System.Attribute.GetCustomAttributes(info);
                if (attrs != null && attrs.Length > 0)
                    foreach (Attribute attr in attrs)
                        if (attr is Column || attr is Id)
                        {
                            campos.Add(new KeyValuePair<String, Type>(info.Name, info.FieldType));
                            querySelect.Append(((DBMapping)attr).DbName).Append(", ");
                        }
            }
            querySelect = querySelect.Remove(querySelect.Length - 2, 2);
            //Se agrega el nombre de la entidad a la que sera aplicado el SELECT
            querySelect.Append(" FROM ").Append(getNombreEntidad((Entity)Activator.CreateInstance(typeof(T))));

            System.Console.WriteLine(querySelect.ToString());
            
            //Se ejecuta el comando sobre BD para obtener la informacion de la entidad
            DbCommand cmd = db.GetSqlStringCommand(querySelect.ToString());

            using (IDataReader datareader = db.ExecuteReader(cmd))
            {
                while (datareader.Read())
                {
                    aux = (T)Activator.CreateInstance(typeof(T));
                    for (int i = 0; i < datareader.FieldCount; i++)
                    {
                        if (!(datareader[i] is System.DBNull))
                        {
                            if (campos[i].Value.IsSubclassOf(typeof(Entity)))
                            {
                                //Se genera el objeto referencia que sera insertado dentro de la propiedad
                                Entity referencia = (Entity)Activator.CreateInstance(campos[i].Value);
                                //Se asigna el valor del campo ID para que se realice el mapeo de la entidad de referencia
                                this.setIdValueToEntity(referencia, datareader[i]);

                                typeof(T).GetField(campos[i].Key, BindingFlags.Instance | BindingFlags.NonPublic)
                                .SetValue(aux, uniqueResult(referencia));
                            }
                            else
                            {
                                if (datareader[i].GetType().Equals(campos[i].Value))
                                    typeof(T).GetField(campos[i].Key, BindingFlags.Instance | BindingFlags.NonPublic)
                                    .SetValue(aux, datareader[i]);
                                else
                                    typeof(T).GetField(campos[i].Key, BindingFlags.Instance | BindingFlags.NonPublic)
                                        .SetValue(aux, TypeDescriptor.GetConverter(datareader[i].GetType()).ConvertTo(datareader[i], campos[i].Value));
                            }
                        }
                    }
                    typeof(T).GetField("isFromDB", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(aux, true);
                    resultado.Add(aux);
                }
            }

            return resultado;
        }
        #endregion

        #region "public Entity uniqueResult(Entity entity)"
        /// <summary>
        /// Obtiene un Objeto del tipo Especificado en base a los valores capturados dentro del objeto recibido como parametro, 
        /// si la sentencia genera mas de un registro como resultado se devuelve el primer registro recibido
        /// </summary>
        /// <param name="entity">El objeto que contiene los valores con el cual se filtra la informacion</param>
        /// <returns>Un objeto del tipo especificado con la informacion contenida en BD</returns>
        public override T uniqueResult<T>(T entity)
        {
            object[] attributos = null;
            System.Attribute[] attrs = null;
            T resultado = default(T);
            String tipoCondicion = "WHERE";
            StringBuilder condicion = new StringBuilder();
            StringBuilder querySelect = new StringBuilder("SELECT ");

            //Se recorren cada uno de los atributos que pertenecen a la entidad para determinar los mapeos a BD
            attributos = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            if (attributos == null || attributos.Length == 0)
                throw new Exception("El objeto <<" + entity.GetType().Name + ">> no cuenta con ningun campo que pueda ser mapeado a la BD");

            List<KeyValuePair<String, Type>> campos = new List<KeyValuePair<string, Type>>();
            //Por cada uno de los campos de la entidad se obtienen sus atributos
            foreach (FieldInfo info in attributos)
            {
                attrs = System.Attribute.GetCustomAttributes(info);
                if (attrs != null && attrs.Length > 0)
                    foreach (Attribute attr in attrs)
                        if (attr is Column || attr is Id)
                        {
                            campos.Add(new KeyValuePair<String, Type>(info.Name, info.FieldType));
                            querySelect.Append(((DBMapping)attr).DbName).Append(", ");
                            //if (info.GetValue(entity) != null)
                            if (attr is Id || attr is Fk)
                            {
                                condicion.Append(" ").Append(tipoCondicion).Append(" ").Append(((DBMapping)attr).DbName).Append(" = ");
                                condicion.Append(":").Append(info.Name).Append(" ");
                                tipoCondicion = "AND";
                            }
                        }
            }

            //Se agrega el nombre de la entidad a la que sera aplicado el SELECT
            querySelect = querySelect.Remove(querySelect.Length - 2, 2);
            querySelect.Append(" FROM ").Append(getNombreEntidad(entity));
            querySelect.Append(condicion.ToString());

            System.Console.WriteLine(querySelect.ToString());
            //Se ejecuta el comando sobre BD para obtener la informacion de la entidad
            DbCommand cmd = db.GetSqlStringCommand(querySelect.ToString());
            setParametersToCommand(entity, getNombreEntidad(entity), cmd, db, true, false);

            using (DataSet ds = db.ExecuteDataSet(cmd))
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    resultado = (T)Activator.CreateInstance(entity.GetType());
                    for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                    {
                        if (!(ds.Tables[0].Rows[0][i] is System.DBNull))
                        {
                            if (campos[i].Value.BaseType.IsSubclassOf(typeof(Entity)) || campos[i].Value.IsSubclassOf(typeof(Entity)))
                            {
                                //Se genera el objeto referencia que sera insertado dentro de la propiedad
                                Entity referencia = (Entity)Activator.CreateInstance(campos[i].Value);
                                //Se asigna el valor del campo ID para que se realice el mapeo de la entidad de referencia
                                this.setIdValueToEntity(referencia, ds.Tables[0].Rows[0][i]);
                                //Se obtiene la informacion para llenar el objeto y es asignado al campo
                                entity.GetType().GetField(campos[i].Key, BindingFlags.Instance | BindingFlags.NonPublic)
                                .SetValue(resultado, uniqueResult<Entity>(referencia));
                            }
                            else 
                            {
                                if (ds.Tables[0].Columns[i].DataType.Equals(campos[i].Value))
                                    entity.GetType().GetField(campos[i].Key, BindingFlags.Instance | BindingFlags.NonPublic)
                                    .SetValue(resultado, ds.Tables[0].Rows[0][i]);
                                else
                                    entity.GetType().GetField(campos[i].Key, BindingFlags.Instance | BindingFlags.NonPublic)
                                        .SetValue(resultado, TypeDescriptor.GetConverter(ds.Tables[0].Columns[i].DataType).ConvertTo(ds.Tables[0].Rows[0][i], campos[i].Value));
                            }
                        }
                    }
                    //Se marca el objeto como cargado de BD
                    typeof(T).GetField("isFromDB", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(resultado, true);
                }
            }
            return resultado;
        }
        #endregion

        #region "public override void executeStoreProcedure(string nombre, IList<DbParameter> parametros)"
        /// <summary>
        /// Ejecuta un Store Procedure dentro de la BD con los paretros especificados
        /// </summary>
        /// <param name="nombre">Nombre del Store Procedure a ejecutar</param>
        /// <param name="parametros">Parametros utilizados para la ejecucion del Store Procedure</param>
        /// <returns>Una lista con los parametros de salida arrojados por el Store</returns>
        public override void executeStoreProcedure(string nombre, IList<Parameter> parametros)
        {
            DbCommand cmd = db.GetStoredProcCommand(nombre);

            foreach (Parameter parametro in parametros)
                db.AddParameter(cmd, parametro.Nombre, parametro.TipoDato, parametro.Size, parametro.Direccion, parametro.Nullable, parametro.Precision, parametro.Scale, parametro.SourceColumn, parametro.SourceVersion, parametro.Value);
            db.ExecuteNonQuery(cmd);

            for (int i = 0; i < cmd.Parameters.Count; i++)
                parametros[i].Value = cmd.Parameters[i].Value;
        }
        #endregion

        #region "public void BeginTransaction()"
        /// <summary>
        /// En caso de no existir una transaccion activa genera una nueva transaccion.
        /// </summary>
        public override void BeginTransaction()
        {
            try
            {
                if (isTransaccionActiva)
                    throw new Exception("No es posible crear una nueva transaccion debido a que ya existe una transaccion activa");
                tx = conn.BeginTransaction();
                isTransaccionActiva = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrio un error al crear la transaccion, error: " + ex.Message);
            }
        }
        #endregion

        #region "public void Commit()"
        /// <summary>
        /// En caso de existir una transaccion activa aplica Commit a las operaciones asociadas a la transaccion.
        /// </summary>
        public override void Commit()
        {
            if (isTransaccionActiva)
            {
                tx.Commit();
                isTransaccionActiva = false;
            }
            else
                throw new Exception("Error <<Dao.Commit()>>, No existe ninguna transaccion activa.");
        }
        #endregion

        #region "public void RollBack()"
        /// <summary>
        /// En caso de existir una transaccion activa aplica un RollBack a las operaciones asociadas a dicha transaccion.
        /// </summary>
        public override void RollBack()
        {
            if (isTransaccionActiva)
            {
                tx.Rollback();
                isTransaccionActiva = false;
            }
            else
                throw new Exception("Error <<Dao.RollBack()>>, No existe ninguna transaccion activa.");
        }
        #endregion

        #region "public void Dispose()"
        /// <summary>
        /// Realiza las operaciones necesarias para limpiar el objeto.
        /// </summary>
        public override void Dispose()
        {
            if (tx != null)
            {
                tx.Dispose();
                tx = null;
            }
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
                conn = null;
            }
            disposed = true;
        }
        #endregion

        public override bool isDisposed()
        {
            return disposed;
        }

        #region "public override Criteria createCriteria(Type tipoObjeto)"
        /// <summary>
        /// Crea un objeto criteria asociando la conexion con la que se esta trabajando actualmente.
        /// </summary>
        /// <param name="tipoObjeto">El tipo de objeto con el cual trabajare Criteria</param>
        /// <returns>Un objeto de tipo Criteria</returns>
        public override Criteria<E> createCriteria<E>()
        {
            Criteria<E> c;

            if(isTransaccionActiva)
                c = new Criteria<E>(this.conn, this, tx);
            else
                c = new Criteria<E>(this.conn, this, null);
            return c;
        }
        #endregion

        #endregion

        #region "private String getNombreEntidad(T entity)"
        /// <summary>
        /// Determina a partir del objeto que se desea mapear a BD el nombre de la entidad correspondiente para realizar el mapeo.
        /// </summary>
        /// <param name="entity">El objeto entidad que desea ser mapeado a la BD</param>
        /// <returns>El nombre de la entidad correspondiente en BD</returns>
        private String getNombreEntidad(Object entity)
        {
            String nombreEntidad = null;
            object[] attributos = null;

            //Se obtienen los atributos asociados al objeto 
            attributos = entity.GetType().GetCustomAttributes(typeof(DBEntity), true);
            //En caso de contener un atributo de tipo DBMapping se obtiene el valor de DBName
            if (attributos != null && attributos.Length > 0)
                nombreEntidad = ((DBEntity)attributos[0]).DbName;
            //En caso contrario lo que se retorna es el nombre de la Clase
            else
                nombreEntidad = entity.GetType().Name;

            return nombreEntidad;
        }
        #endregion

        #region"private void setParametersToCommand(Entity entity, String dbName, DbCommand cmd, Database db)"
        /// <summary>
        /// Ejecuta un comando dentro de la BD especificada asignando los valores de los campos contenidos para la entidad recibida
        /// </summary>
        /// <param name="entity">Objeto que contiene la informacion a insertar sobre la BD</param>
        /// <param name="dbName">nombre de la Entidad en BD</param>
        /// <param name="cmd">Comando a ejecutar</param>
        /// <param name="db">Base de datos aobre la que se realizara la ejecuci[on del comando</param>
        private Int32 setParametersToCommand<T>(T entity, String dbName, DbCommand cmd, Database db, bool isSelect, bool isUpdate)
        {
            Int32 idValue = -1;
            object[] columnas = null;
            System.Attribute[] attrs = null;

            columnas = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            //Por cada uno de los campos de la entidad
            foreach (FieldInfo info in columnas)
                if (info.GetValue(entity) != null && System.Attribute.GetCustomAttributes(info).Length > 0)
                    if (info.FieldType.IsSubclassOf(typeof(Entity)) && !isSelect)
                        setIdParameterValue((Entity)info.GetValue(entity), info.Name, cmd, db);
                    else
                    {
                        attrs = System.Attribute.GetCustomAttributes(info);
                        foreach (Attribute attr in attrs)
                            if (attr is Column)
                            {
                                if (isSelect && (attr is Id || attr is Fk))
                                    db.AddInParameter(cmd, info.Name, ((Column)attr).TipoDato, info.GetValue(entity));
                                else if (!isSelect)
                                {
                                    if (attr is Id && !isUpdate && ((Id)attr).SequenceGenerator != null && !((Id)attr).SequenceGenerator.Trim().Equals(""))
                                    {
                                        idValue = Convert.ToInt32(executeDataSet("SELECT " + ((Id)attr).SequenceGenerator + ".NEXTVAL FROM DUAL").Tables[0].Rows[0][0].ToString());
                                        db.AddInParameter(cmd, info.Name, ((Column)attr).TipoDato, idValue);
                                    }
                                    else
                                    {
                                        if ((((Column)attr).TipoDato == DbType.DateTime || ((Column)attr).TipoDato == DbType.Date || ((Column)attr).TipoDato == DbType.DateTime2 || ((Column)attr).TipoDato == DbType.DateTimeOffset) && info.GetValue(entity).Equals(default(DateTime)))
                                            db.AddInParameter(cmd, info.Name, ((Column)attr).TipoDato, null);
                                        else
                                            db.AddInParameter(cmd, info.Name, ((Column)attr).TipoDato, info.GetValue(entity));
                                    }
                                }
                            }
                    }
            return idValue;
        }
        #endregion

        #region "private void setIdParameterValue(Entity entity, String fieldName, DbCommand cmd, Database db)"
        /// <summary>
        /// Obtiene el valor del id de una referencia de clase contenida dentro de una entidad agregandolo como parametro al comando a ejecutar sobre la BD
        /// </summary>
        /// <param name="entity">Objeto sobre el cual se buscara el valor</param>
        /// <param name="fieldName">Nombre con el que sera mapeado el valor dentro de los parametros del comando</param>
        /// <param name="cmd">Comando sobre el que se insertara como parametro el valor obtenido</param>
        /// <param name="db">Base de datos sobre la cual se realizara la ejecuci[on del comando</param>
        private void setIdParameterValue(Entity entity, String fieldName, DbCommand cmd, Database db)
        {
            bool hasNext = true;
            FieldInfo info = null;
            FieldInfo[] columnas = null;
            System.Attribute[] attrs = null;

            //Se obtienen las columnas de la entidad
            columnas = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            //Se recorren las columnas para buscar la que corresponde al id de la entidad
            int contador = 0;
            while (hasNext && contador < columnas.Length)
            {
                info = columnas[contador];
                //Se obtienen los atributos
                if (info != null)
                {
                    attrs = System.Attribute.GetCustomAttributes(info);
                    if (attrs != null && attrs.Length > 0)
                        foreach (Attribute attr in attrs)
                            if (attr is Id)
                            {
                                db.AddInParameter(cmd, fieldName, ((Column)attr).TipoDato, info.GetValue(entity));
                                hasNext = false;
                            }
                }
                contador++;
            }
            if (hasNext)
                throw new Exception("Error durante mapeo de el campo referenciado hacia el objeto <<" + entity.GetType().Name + ">>, no existe ningun campo marcado como ID de la entidad");
        }
        #endregion

        #region "private void setIdValueToEntity(Entity entity, Object value)"
        /// <summary>
        /// Asigna un valor recibido dentro del campo ID de la entidad especificada
        /// </summary>
        /// <param name="entity">La entidad a la que se le asignara el valor del ID</param>
        /// <param name="value">El valor a asignar dentro del ID del objeto</param>
        private void setIdValueToEntity(Entity entity, Object value)
        {
            bool hasNext = true;
            FieldInfo info = null;
            FieldInfo[] columnas = null;
            System.Attribute[] attrs = null;

            //Se obtienen las columnas de la entidad
            columnas = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            //Se recorren las columnas para buscar la que corresponde al id de la entidad
            int contador = 0;
            while (hasNext && contador < columnas.Length)
            {
                info = columnas[contador];
                //Se obtienen los atributos
                attrs = System.Attribute.GetCustomAttributes(info);
                if (attrs != null && attrs.Length > 0)
                    foreach (Attribute attr in attrs)
                        if (attr is Id)
                        {
                            info.SetValue(entity, TypeDescriptor.GetConverter(value.GetType()).ConvertTo(value, info.FieldType));
                            hasNext = false;
                        }
                contador++;
            }
            if (hasNext)
                throw new Exception("Error durante mapeo de el campo referenciado hacia el objeto <<" + entity.GetType().Name + ">>, no existe ningun campo marcado como ID de la entidad");
        }
        #endregion
    }
}
