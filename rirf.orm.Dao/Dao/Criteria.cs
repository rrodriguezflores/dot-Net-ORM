using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using rirf.orm.Dao.Attributes;
using System.Data.Common;
using System.Data;
using System.ComponentModel;

namespace rirf.orm.Dao
{
    /// <summary>
    /// Clase utilizada para crear un criterio de ejecucion para las consultas a BD
    /// </summary>
    /// <typeparam name="T">Tipo del objeto con el que se desea trabajar, el objeto enviado debe heredar de la clase Entity o LogicEntity</typeparam>
    public class Criteria<T>
    {
        /// <summary>
        /// Objeto dao utilizado para la comunicacion hacia BD
        /// </summary>
        private IDao dao;

        /// <summary>
        /// Commando utilizado para la ejecucion de sentencias en BD
        /// </summary>
        private DbCommand cmd;

        /// <summary>
        /// Connexion utilizada para comunicarse a la BD
        /// </summary>
        private DbConnection conn;

        /// <summary>
        /// Query de seleccion generado
        /// </summary>
        private StringBuilder querySelect;

        /// <summary>
        /// Query de criterio generado
        /// </summary>
        private StringBuilder queryCriteria;

        /// <summary>
        /// Lista de campos obtenidos para el criterio con su correspondiente tipo de dato
        /// </summary>
        private List<KeyValuePair<String, Type>> campos;

        #region "public Criteria(DbConnection conexion, IDao dao)"
        /// <summary>
        /// Crea una nueva instancia del objeto Criteria
        /// </summary>
        /// <param name="conexion">La conexion utilizada para el acceso a datos</param>
        /// <param name="dao">El objeto dao a partir del cual se genero la instancia</param>
        public Criteria(DbConnection conexion, IDao dao, DbTransaction tx)
        {
            this.dao = dao;
            this.conn = conexion;
            cmd = conn.CreateCommand();
            if (tx != null)
                cmd.Transaction = tx;
            this.querySelect = new StringBuilder();
            this.queryCriteria = new StringBuilder();
            campos = new List<KeyValuePair<string, Type>>();
            createSelectQuery(typeof(T));
        }
        #endregion

        #region "public Criteria<T> addWhere(String columna, Object valor)"
        /// <summary>
        /// Agrega una condicion de tipo WHERE
        /// </summary>
        /// <param name="columna">La columna con la cual trabajara la condicion</param>
        /// <param name="valor">El valor utilizado para realizar la comparacion</param>
        /// <returns>El objeto Criteria con la condicion asociada</returns>
        public Criteria<T> addWhere(String columna, Object valor) 
        {
            try
            {
                addCondicion(" WHERE ", columna, valor);
                return this;
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Error al agregar la clausula WHERE para Criteria de tipo <<" + typeof(T).ToString() + ">>, error: " + ex.Message + ", " + ex.StackTrace);
                throw ex;
            }
        }
        #endregion

        #region "public Criteria<T> addAnd(String columna, Object valor)"
        /// <summary>
        /// Agrega una condicion de tipo AND
        /// </summary>
        /// <param name="columna">La columna con la cual trabajara la condicion</param>
        /// <param name="valor">El valor utilizado para realizar la comparacion</param>
        /// <returns>El objeto Criteria con la condicion asociada</returns>
        public Criteria<T> addAnd(String columna, Object valor)
        {
            try
            {
                addCondicion(" AND ", columna, valor);
                return this;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al agregar la clausula AND para Criteria de tipo <<" + typeof(T).ToString() + ">>, error: " + ex.Message + ", " + ex.StackTrace);
                throw ex;
            }
        }
        #endregion

        #region "public Criteria<T> addOr(String columna, Object valor)"
        /// <summary>
        /// Agrega una condicion de tipo OR
        /// </summary>
        /// <param name="columna">La columna con la cual trabajara la condicion</param>
        /// <param name="valor">El valor utilizado para realizar la comparacion</param>
        /// <returns>El objeto Criteria con la condicion asociada</returns>
        public Criteria<T> addOr(String columna, Object valor)
        {
            try
            {
                addCondicion(" OR ", columna, valor);
                return this;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al agregar la clausula OR para Criteria de tipo <<" + typeof(T).ToString() + ">>, error: " + ex.Message + ", " + ex.StackTrace);
                throw ex;
            }
        }
        #endregion

        #region "private void addCondicion(String tipoCondicion, String columna, Object valor)"
        /// <summary>
        /// Agrega una condicion para la seleccion del objeto
        /// </summary>
        /// <param name="tipoCondicion">Indica el tipo de condicion que sera agregada</param>
        /// <param name="columna">La columna para la cual aplica la condicion</param>
        /// <param name="valor">El valor a comparar por la condicion</param>
        private void addCondicion(String tipoCondicion, String columna, Object valor)
        {
            FieldInfo info = null;
            Attribute[] atributos = null;

            try
            {
                info = typeof(T).GetField(columna, BindingFlags.Instance | BindingFlags.NonPublic);
                if (info == null)
                    throw new Exception("El atributo <<" + columna + ">> no existe dentro del tipo <<" + typeof(T) + ">> especificado");

                atributos = System.Attribute.GetCustomAttributes(info);
                if (atributos == null || atributos.Length == 0)
                {
                    queryCriteria.Append(tipoCondicion).Append(columna).Append(" = :").Append(columna).Append(" ");
                    setParameterToCommand(cmd, columna, DbType.AnsiString, valor);
                }
                else
                {
                    foreach (Attribute attr in atributos)
                        if (attr is Column || attr is Id)
                        {
                            queryCriteria.Append(tipoCondicion).Append(((DBMapping)attr).DbName).Append(" = :").Append(columna).Append(" ");
                            setParameterToCommand(cmd, columna, ((Column)attr).TipoDato, valor);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al agregar la clausula WHERE para Criteria de tipo <<" + typeof(T).ToString() + ">>, error: " + ex.Message + ", " + ex.StackTrace);
                throw ex;
            }
        }
        #endregion

        #region "private void createSelectQuery(Type entity)"
        /// <summary>
        /// Crea el query de seleccion asociado al objeto especificado
        /// </summary>
        /// <param name="entity">La entidad para la cual se generara el query</param>
        private void createSelectQuery(Type entity)
        {
            FieldInfo[] attributos = null;
            System.Attribute[] attrs = null;

            querySelect.Append(" SELECT ");
            attributos = entity.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            if (attributos == null || attributos.Length == 0)
                throw new Exception("El objeto <<" + entity.GetType().Name + ">> no cuenta con ningun campo que pueda ser mapeado a la BD");

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
            querySelect.Append(" FROM ").Append(getNombreEntidad(typeof(T))).Append(" ");
        }
        #endregion

        #region "private void setParameterToCommand(DbCommand command, String nombre, DbType tipoDato, Object valor)"
        /// <summary>
        /// Asigna el valor recibido para la condicion a la lista de parametros del comando
        /// </summary>
        /// <param name="command">El comando al cual se agregara el parametro</param>
        /// <param name="nombre">Nombre que sera asignado al parametro</param>
        /// <param name="tipoDato">Tipo de dato al que pertenece el parametro</param>
        /// <param name="valor">Valor asociado al parametro</param>
        private void setParameterToCommand(DbCommand command, String nombre, DbType tipoDato, Object valor)
        {
            DbParameter param = cmd.CreateParameter();
            param.DbType = tipoDato;
            param.Direction = ParameterDirection.Input;
            param.ParameterName = nombre;
            param.Value = valor;

            cmd.Parameters.Add(param);
        }
        #endregion

        #region "private String getNombreEntidad(Type entity)"
        /// <summary>
        /// Determina a partir del objeto que se desea mapear a BD el nombre de la entidad correspondiente para realizar el mapeo.
        /// </summary>
        /// <param name="entity">El objeto entidad que desea ser mapeado a la BD</param>
        /// <returns>El nombre de la entidad correspondiente en BD</returns>
        private String getNombreEntidad(Type entity)
        {
            String nombreEntidad = null;
            object[] attributos = null;

            //Se obtienen los atributos asociados al objeto 
            attributos = entity.GetCustomAttributes(typeof(DBEntity), true);
            //En caso de contener un atributo de tipo DBMapping se obtiene el valor de DBName
            if (attributos != null && attributos.Length > 0)
                nombreEntidad = ((DBEntity)attributos[0]).DbName;
            //En caso contrario lo que se retorna es el nombre de la Clase
            else
                nombreEntidad = entity.Name;

            return nombreEntidad;
        }
        #endregion

        #region "public IList<Entity> list(Type entityType)"
        /// <summary>
        /// Lista la informacion contenida dentro de la entidad asociada al tipo de objeto recibido
        /// </summary>
        /// <param name="entityType">El tipo de objeto entidad al que se desea acceder en BD</param>
        /// <returns>Una lista de objetos del tipo recibido como parametro</returns>
        public IList<T> list()
        {
            T aux = default(T);
            IList<T> resultado = new List<T>();

            System.Console.WriteLine(querySelect.ToString() + queryCriteria.ToString());
            
            //Se ejecuta el comando sobre BD para obtener la informacion de la entidad
            cmd.CommandText = querySelect.ToString() + queryCriteria.ToString();
            
            using (IDataReader datareader = cmd.ExecuteReader())
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
                                .SetValue(aux, dao.uniqueResult(referencia));
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

        #region "public Entity uniqueResult()"
        /// <summary>
        /// Obtiene un Objeto del tipo Especificado en base a los valores capturados dentro del objeto recibido como parametro, 
        /// si la sentencia genera mas de un registro como resultado se devuelve el primer registro recibido
        /// </summary>
        /// <param name="entity">El objeto que contiene los valores con el cual se filtra la informacion</param>
        /// <returns>Un objeto del tipo especificado con la informacion contenida en BD</returns>
        public T uniqueResult()
        {
            int contador = 0;
            object[] attributos = null;
            System.Attribute[] attrs = null;
            T resultado = default(T);

            System.Console.WriteLine(querySelect.ToString() + queryCriteria.ToString());

            //Se ejecuta el comando sobre BD para obtener la informacion de la entidad
            cmd.CommandText = querySelect.ToString() + queryCriteria.ToString();
            using (IDataReader datareader = cmd.ExecuteReader())
            {
                while (datareader.Read())
                {
                    if (contador > 0)
                        throw new Exception("La consulta solicitada entrego mas de un resultado");

                    resultado = (T)Activator.CreateInstance(typeof(T));
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
                                .SetValue(resultado, dao.uniqueResult(referencia));
                            }
                            else
                            {
                                if (datareader[i].GetType().Equals(campos[i].Value))
                                    typeof(T).GetField(campos[i].Key, BindingFlags.Instance | BindingFlags.NonPublic)
                                    .SetValue(resultado, datareader[i]);
                                else
                                    typeof(T).GetField(campos[i].Key, BindingFlags.Instance | BindingFlags.NonPublic)
                                        .SetValue(resultado, TypeDescriptor.GetConverter(datareader[i].GetType()).ConvertTo(datareader[i], campos[i].Value));
                            }
                        }
                        //Se marca el objeto como cargado de BD
                        typeof(T).GetField("isFromDB", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(resultado, true);
                    }
                }
                contador++;
            }
            return resultado;
        }
        #endregion

        #region "public Int32 delete()"
        /// <summary>
        /// Elimina los registros afectados por la sentencia generada
        /// </summary>
        /// <returns>El total de registros eliminados</returns>
        public Int32 delete()
        {
            Int32 resultado = 0;
            System.Console.WriteLine(querySelect.ToString() + queryCriteria.ToString());

            //Se ejecuta el comando sobre BD para eliminar el registro
            cmd.CommandText = "DELETE FROM " + getNombreEntidad(typeof(T)) + " " + queryCriteria.ToString();
            resultado = cmd.ExecuteNonQuery();
            return resultado;
        }
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
