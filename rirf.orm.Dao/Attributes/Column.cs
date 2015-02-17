using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace rirf.orm.Dao.Attributes
{
    /// <summary>
    /// Clase que es utilizada para establecer como columna de BD un atributo de una clase
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class Column : DBMapping
    {
        /// <summary>
        /// Indica el tipo de dato de la columna
        /// </summary>
        DbType tipoDato;

        /// <summary>
        /// Construye un nuevo objeto de tipo Column
        /// </summary>
        /// <param name="dBname">Nombre de la columna en BD</param>
        /// <param name="tipoDato">Tipo de dato de la columna</param>
        public Column(String dBname, DbType tipoDato)
            : base(dBname)
        {
            this.tipoDato = tipoDato;
        }

        /// <summary>
        /// Devuelve el tipo de dato de la columna
        /// </summary>
        public DbType TipoDato
        {
            get { return tipoDato; }
        }
    }
}
