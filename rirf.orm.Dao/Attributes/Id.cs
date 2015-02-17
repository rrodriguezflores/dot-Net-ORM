using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace rirf.orm.Dao.Attributes
{
    /// <summary>
    /// Clase que indica que el atributo al que es aplicada es utilizada como llave primaria de la entidad
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class Id : Column
    {
        /// <summary>
        /// Nombre del sequence utilizado para generar el valor del Id en caso de existir
        /// </summary>
        private String sequenceGenerator;

        /// <summary>
        /// Crea un nuevo objeto de tipo Id
        /// </summary>
        /// <param name="dBname">Nombre de la columna en BD</param>
        /// <param name="dbType">Tipo de dato de la columna</param>
        public Id(String dBname, DbType dbType) : base(dBname, dbType) { }

        /// <summary>
        /// Crea un nuevo objeto de tipo Id
        /// </summary>
        /// <param name="dbName">Nombre de la columna en BD</param>
        /// <param name="dbType">Tipo de dato de la columna</param>
        /// <param name="sequenceGenerator">Nombre del sequence utilizado para generar el valor del Id en caso de existir</param>
        public Id(String dbName, DbType dbType, String sequenceGenerator) : base(dbName, dbType) 
        {
            this.sequenceGenerator = sequenceGenerator;
        }

        /// <summary>
        /// Devuelve el nombre del sequence utilizado para generar el valor del Id en caso de existir
        /// </summary>
        public String SequenceGenerator{
            get { return sequenceGenerator; }
        }
    }
}
